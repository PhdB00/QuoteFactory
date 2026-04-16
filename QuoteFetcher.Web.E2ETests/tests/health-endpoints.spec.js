const { test, expect } = require('@playwright/test');
const path = require('path');
const { spawn } = require('child_process');

const WEB_PROJECT_PATH = path.resolve(__dirname, '..', '..', 'QuoteFetcher.Web', 'QuoteFetcher.Web.csproj');

function startWebServer(port, envOverrides = {}) {
  const env = {
    ...process.env,
    ASPNETCORE_URLS: `http://localhost:${port}`,
    Urls: `http://localhost:${port}`,
    ...envOverrides,
  };

  return spawn('dotnet', ['run', '--project', WEB_PROJECT_PATH], {
    cwd: path.dirname(WEB_PROJECT_PATH),
    env,
    windowsHide: true,
    stdio: ['ignore', 'pipe', 'pipe'],
  });
}

function waitForServerReady(proc, port, timeoutMs = 20000) {
  const readyPattern = new RegExp(`Now listening on:\\s+http://localhost:${port}`);
  return new Promise((resolve, reject) => {
    const timer = setTimeout(() => {
      cleanup();
      reject(new Error(`Server did not start on port ${port} within ${timeoutMs}ms.`));
    }, timeoutMs);

    const onData = (chunk) => {
      const text = chunk.toString();
      if (readyPattern.test(text)) {
        cleanup();
        resolve();
      }
    };

    const onExit = (code) => {
      cleanup();
      reject(new Error(`Server exited early with code ${code}.`));
    };

    const cleanup = () => {
      clearTimeout(timer);
      proc.stdout.off('data', onData);
      proc.stderr.off('data', onData);
      proc.off('exit', onExit);
    };

    proc.stdout.on('data', onData);
    proc.stderr.on('data', onData);
    proc.on('exit', onExit);
  });
}

function stopProcess(proc) {
  if (!proc || proc.killed) {
    return;
  }

  proc.kill();
}

test.describe('Health Endpoint Tests', () => {
  test('should report liveness healthy when app process is running', async ({ request }) => {
    const response = await request.get('/health');
    expect(response.status()).toBe(200);
  });

  test('should report readiness healthy when dependency is reachable', async ({ request }) => {
    const response = await request.get('/health/ready');
    expect(response.status()).toBe(200);
  });

  test('should keep liveness healthy when readiness dependency fails', async ({ playwright }) => {
    const port = 5113;
    const proc = startWebServer(port, {
      ApiBaseUrl: 'http://localhost:59999',
    });

    try {
      await waitForServerReady(proc, port);

      const requestContext = await playwright.request.newContext({
        baseURL: `http://localhost:${port}`
      });

      const liveResponse = await requestContext.get('/health');
      expect(liveResponse.status()).toBe(200);

      const readyResponse = await requestContext.get('/health/ready');
      expect(readyResponse.status()).toBe(503);

      await requestContext.dispose();
    } finally {
      stopProcess(proc);
    }
  });
});
