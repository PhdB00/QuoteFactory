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

function waitForProcessExit(proc, timeoutMs = 20000) {
  return new Promise((resolve, reject) => {
    const timer = setTimeout(() => reject(new Error(`Process did not exit within ${timeoutMs}ms.`)), timeoutMs);
    proc.on('exit', (code, signal) => {
      clearTimeout(timer);
      resolve({ code, signal });
    });
  });
}

function stopProcess(proc) {
  if (!proc || proc.killed) {
    return;
  }

  proc.kill();
}

test.describe('CSP Header Tests', () => {
  test('should not allow unsafe-inline for script or style', async ({ request }) => {
    const response = await request.get('/');
    expect(response.ok()).toBeTruthy();

    const cspHeader = response.headers()['content-security-policy'];
    expect(cspHeader).toBeTruthy();

    expect(cspHeader).not.toContain("script-src 'unsafe-inline'");
    expect(cspHeader).not.toContain("style-src 'unsafe-inline'");
    expect(cspHeader).toContain("script-src 'self'");
    expect(cspHeader).toContain("style-src 'self'");
  });

  test('should emit configured security headers from app configuration', async ({ request }) => {
    const response = await request.get('/');
    expect(response.ok()).toBeTruthy();

    const headers = response.headers();
    expect(headers['x-frame-options']).toBe('DENY');
    expect(headers['x-content-type-options']).toBe('nosniff');
    expect(headers['referrer-policy']).toBe('strict-origin-when-cross-origin');
    expect(headers['content-security-policy']).toContain("default-src 'self'");
  });

  test('should apply security header overrides from configuration environment variables', async ({ playwright }) => {
    const port = 5111;
    const overriddenHeaders = {
      SecurityHeaders__XFrameOptions: 'SAMEORIGIN',
      SecurityHeaders__XContentTypeOptions: 'nosniff',
      SecurityHeaders__ReferrerPolicy: 'no-referrer',
      SecurityHeaders__ContentSecurityPolicy: "default-src 'self'; script-src 'self'; style-src 'self'; object-src 'none';"
    };

    const proc = startWebServer(port, overriddenHeaders);

    try {
      await waitForServerReady(proc, port);

      const requestContext = await playwright.request.newContext({
        baseURL: `http://localhost:${port}`
      });

      const response = await requestContext.get('/');
      expect(response.ok()).toBeTruthy();

      const headers = response.headers();
      expect(headers['x-frame-options']).toBe('SAMEORIGIN');
      expect(headers['x-content-type-options']).toBe('nosniff');
      expect(headers['referrer-policy']).toBe('no-referrer');
      expect(headers['content-security-policy']).toBe("default-src 'self'; script-src 'self'; style-src 'self'; object-src 'none';");

      await requestContext.dispose();
    } finally {
      stopProcess(proc);
    }
  });

  test('should fail startup for invalid security header configuration', async () => {
    const port = 5112;
    const proc = startWebServer(port, {
      SecurityHeaders__XFrameOptions: 'INVALID_FRAME_OPTION'
    });

    const result = await waitForProcessExit(proc);
    expect(result.code).not.toBe(0);
  });
});
