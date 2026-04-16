# QuoteFetcher.Web E2E Tests

End-to-end tests for `QuoteFetcher.Web` using Playwright.

## What is covered

Current suites in `tests/`:

- `happy-path.spec.js` - Core user flows (load app, render bubbles, click bubble, crawl rendering, mobile behavior)
- `error-scenarios.spec.js` - API failure and timeout handling
- `frontend-module-boundaries.spec.js` - Frontend modularization safeguards (no legacy globals, request count behavior)
- `csp-header.spec.js` - Security header behavior and config override validation
- `health-endpoints.spec.js` - Liveness/readiness endpoint behavior

## Prerequisites

- Node.js and npm
- Playwright browsers installed (`npx playwright install`)
- `QuoteFetcher.Web` running (default: `http://localhost:5001`)
- For tests that start the app process (`csp-header.spec.js`, `health-endpoints.spec.js`): .NET SDK + `dotnet` on PATH

Optional environment variables:

- `WEB_BASE_URL` (default: `http://localhost:5001`)
- `API_BASE_URL` (default: `http://localhost:5074`)
- `CI` (`true` enables CI mode: retries + single worker)

## Install

```bash
cd QuoteFetcher.Web.E2ETests
npm install
npx playwright install
```

## Run tests

Run all:

```bash
npm test
```

Useful modes:

```bash
npm run test:headed
npm run test:debug
npm run test:ui
```

Run a specific browser project:

```bash
npm run test:chromium
npm run test:firefox
npm run test:mobile
```

Run a specific spec:

```bash
npx playwright test tests/happy-path.spec.js
npx playwright test tests/error-scenarios.spec.js
npx playwright test tests/frontend-module-boundaries.spec.js
npx playwright test tests/csp-header.spec.js
npx playwright test tests/health-endpoints.spec.js
```

Run a specific test name:

```bash
npx playwright test -g "should load application successfully"
```

## Test behavior notes

- UI tests primarily mock API calls via Playwright route interception.
- CSP tests validate response headers on `/` and include startup validation cases.
- Health tests validate `/health` and `/health/ready`, including degraded readiness scenarios.

## Configuration currently used

From `playwright.config.js`:

- `testDir`: `./tests`
- Test timeout: `30s`
- Expect timeout: `5s`
- `fullyParallel`: `true`
- Retries: `2` on CI, `0` locally
- Workers: `1` on CI, `4` locally
- Browser projects: `chromium`, `firefox`, `mobile-chrome`
- Artifacts: trace on first retry, screenshots on failure, video retained on failure
- `bypassCSP: true` enabled for browser context

## Reports

```bash
npm run report
```

This opens the Playwright HTML report (`playwright-report/`).

## Troubleshooting

- `Navigation timeout`:
  - Confirm `QuoteFetcher.Web` is running at `WEB_BASE_URL` (default `http://localhost:5001`).
- API-related UI failures:
  - Confirm `API_BASE_URL` matches the expected API URL when not fully mocked.
- `dotnet` not found / process-start failures in CSP or health suites:
  - Install .NET SDK and ensure `dotnet` is available in PATH.
- No report opens:
  - Ensure tests have been executed at least once to generate `playwright-report/`.
