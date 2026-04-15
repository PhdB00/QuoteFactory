# QuoteFetcher.Web E2E Tests

Comprehensive end-to-end test suite for QuoteFetcher.Web using Playwright. This test suite validates both happy path and error scenarios to ensure the application maintains its functionality through refactoring and modification.

## Prerequisites

- **Node.js** (v18 or higher)
- **npm** (v9 or higher)
- **QuoteFetcher.Api** running on port 5074 (or custom port via environment variable)
- **QuoteFetcher.Web** running on port 5000 (or custom port via environment variable)

## Installation

1. Navigate to the test directory:
   ```bash
   cd QuoteFetcher.Web.E2ETests
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Install Playwright browsers:
   ```bash
   npx playwright install
   ```

## Configuration

The test suite can be configured via environment variables:

- `WEB_BASE_URL` - Base URL for QuoteFetcher.Web (default: `http://localhost:5000`)
- `API_BASE_URL` - Base URL for QuoteFetcher.Api (default: `http://localhost:5074`)
- `CI` - Set to `true` to enable CI mode (enables retries, changes reporters)

### Example Configuration

```bash
# Windows (PowerShell)
$env:WEB_BASE_URL="http://localhost:8080"
$env:API_BASE_URL="http://localhost:8081"
npm test

# Windows (CMD)
set WEB_BASE_URL=http://localhost:8080
set API_BASE_URL=http://localhost:8081
npm test

# Linux/Mac
WEB_BASE_URL=http://localhost:8080 API_BASE_URL=http://localhost:8081 npm test
```

## Running Tests

### Run all tests
```bash
npm test
```

### Run tests in headed mode (see browser)
```bash
npm run test:headed
```

### Run tests in debug mode
```bash
npm run test:debug
```

### Run tests in UI mode (interactive)
```bash
npm run test:ui
```

### Run tests on specific browser
```bash
npm run test:chromium
npm run test:firefox
npm run test:mobile
```

### Run specific test file
```bash
npx playwright test tests/happy-path.spec.js
npx playwright test tests/error-scenarios.spec.js
```

### Run specific test by name
```bash
npx playwright test -g "should load application successfully"
```

## Test Coverage

### Happy Path Tests (`tests/happy-path.spec.js`)

- ✅ Application loads successfully with correct title
- ✅ Bubbles display for all categories from API
- ✅ Bubbles animate across the screen
- ✅ Quote crawl displays when bubble is clicked
- ✅ Multiple crawls can display simultaneously
- ✅ Bubble collision detection and resolution
- ✅ Responsive layout on mobile viewport

### Error Scenario Tests (`tests/error-scenarios.spec.js`)

- ✅ Error when API is unreachable on initialization
- ✅ Error when categories endpoint returns 500
- ✅ Error when categories endpoint returns empty array
- ✅ Error when categories endpoint returns malformed JSON
- ✅ Error when quote endpoint fails on bubble click
- ✅ Error when quote endpoint returns 404
- ✅ Graceful handling of malformed quote response
- ✅ Error auto-hide after 5 seconds
- ✅ Network timeout handling during quote fetch
- ✅ Consecutive failed quote requests

## Test Reports

After running tests, view the HTML report:

```bash
npm run report
```

This opens a detailed report showing:
- Test results (pass/fail)
- Screenshots of failures
- Video recordings (on retry)
- Traces for debugging

## Test Architecture

### Project Structure

```
QuoteFetcher.Web.E2ETests/
├── tests/
│   ├── happy-path.spec.js      # Happy path scenarios
│   ├── error-scenarios.spec.js # Error handling scenarios
│   └── helpers.js              # Reusable test utilities
├── playwright.config.js         # Playwright configuration
├── package.json                 # Dependencies and scripts
├── .gitignore                   # Git ignore rules
└── README.md                    # This file
```

### Helper Utilities (`tests/helpers.js`)

The test suite includes comprehensive helper functions:

- **API Mocking**
  - `mockApiResponse(page, endpoint, response, statusCode)` - Mock API responses
  - `mockApiResponseRaw(page, endpoint, body, statusCode, contentType)` - Mock with raw body
  - `blockApiRequests(page)` - Simulate API unreachable
  - `mockApiTimeout(page, endpoint)` - Simulate network timeout

- **Element Interactions**
  - `waitForBubbles(page, count, timeout)` - Wait for bubbles to appear
  - `getBubbleByCategory(page, category)` - Find bubble by category
  - `clickBubbleAndWaitForQuote(page, category)` - Click and wait for API
  - `waitForCrawl(page, timeout)` - Wait for crawl animation

- **Error Handling**
  - `getErrorMessage(page)` - Get error message text
  - `waitForErrorToDisappear(page, timeout)` - Wait for auto-hide

- **Positioning & Animation**
  - `getBubblePosition(page, bubble)` - Get bubble coordinates
  - `setBubblePositionsForCollision(page, idx1, idx2, distance)` - Control collision
  - `getBubbleVelocity(page, bubbleIndex)` - Get bubble velocity
  - `accelerateCrawlAnimation(page)` - Speed up animations

## Browser Support

The test suite runs on:

- **Chromium** (Desktop: 1280x720)
- **Firefox** (Desktop: 1280x720)
- **Mobile Chrome** (Mobile: 375x667)

WebKit is intentionally excluded per project requirements.

## Performance

- **Test Timeout**: 30 seconds per test
- **Expect Timeout**: 5 seconds for assertions
- **Parallel Workers**: 4 workers (locally), 1 worker (CI)
- **Target Execution Time**: < 2 minutes total

## Reliability

The test suite is designed for zero flakiness:

- All API responses are mocked for deterministic behavior
- Uses explicit waits instead of arbitrary timeouts
- Retries enabled on CI (2 retries)
- Automatic screenshot/video capture on failure

## Debugging Failed Tests

1. **View the HTML report**:
   ```bash
   npm run report
   ```

2. **Run in debug mode**:
   ```bash
   npm run test:debug
   ```

3. **Check screenshots**: Located in `test-results/` directory

4. **View trace**: Click on failed test in HTML report to see detailed trace

5. **Run specific failed test**:
   ```bash
   npx playwright test -g "test name here"
   ```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: E2E Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'

      - name: Install dependencies
        working-directory: ./QuoteFetcher.Web.E2ETests
        run: npm install

      - name: Install Playwright browsers
        working-directory: ./QuoteFetcher.Web.E2ETests
        run: npx playwright install --with-deps

      - name: Start API server
        run: |
          # Start your API server here

      - name: Start Web server
        run: |
          # Start your Web server here

      - name: Run tests
        working-directory: ./QuoteFetcher.Web.E2ETests
        run: npm test
        env:
          CI: true

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: playwright-report
          path: QuoteFetcher.Web.E2ETests/playwright-report/
```

## Troubleshooting

### Tests fail with "Navigation timeout"
- Ensure QuoteFetcher.Web is running on the correct port
- Check `WEB_BASE_URL` environment variable

### Tests fail with "API unreachable"
- Ensure QuoteFetcher.Api is running on the correct port
- Check `API_BASE_URL` environment variable

### Bubbles not appearing
- Check browser console for JavaScript errors
- Verify API is returning valid category data

### Collision test fails
- This test manipulates bubble positions programmatically
- Ensure `window.bubbles` array is accessible in the application

## Future Enhancements

Potential improvements for the test suite:

- Visual regression testing with screenshots
- Performance testing (animation FPS, load times)
- Accessibility testing with axe-core
- Component-level integration tests post-refactoring
- Parallel API server for isolated test environment

## Support

For issues or questions:
1. Check the HTML report for detailed error information
2. Run tests in debug mode (`npm run test:debug`)
3. Review test logs in `test-results/` directory
