// @ts-check
const { defineConfig, devices } = require('@playwright/test');

/**
 * Read environment variables for configuration
 */
const WEB_BASE_URL = process.env.WEB_BASE_URL || 'http://localhost:5001';
const API_BASE_URL = process.env.API_BASE_URL || 'http://localhost:5074';

/**
 * @see https://playwright.dev/docs/test-configuration
 */
module.exports = defineConfig({
  testDir: './tests',

  /* Maximum time one test can run for */
  timeout: 30 * 1000,

  /* Expect timeout for assertions */
  expect: {
    timeout: 5000
  },

  /* Run tests in files in parallel */
  fullyParallel: true,

  /* Fail the build on CI if you accidentally left test.only in the source code */
  forbidOnly: !!process.env.CI,

  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,

  /* Opt out of parallel tests on CI */
  workers: process.env.CI ? 1 : 4,

  /* Reporter to use */
  reporter: process.env.CI
    ? [['line'], ['html']]
    : [['list'], ['html']],

  /* Shared settings for all the projects below */
  use: {
    /* security header (Content-Security-Policy) blocks Playwright's route interception mechanism. */
    bypassCSP: true,
    
    /* Base URL to use in actions like `await page.goto('/')` */
    baseURL: WEB_BASE_URL,

    /* Collect trace on first retry */
    trace: 'on-first-retry',

    /* Screenshot on failure */
    screenshot: 'only-on-failure',

    /* Video on first retry */
    video: 'retain-on-failure',
  },

  /* Configure projects for major browsers */
  projects: [
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        viewport: { width: 1280, height: 720 }
      },
    },

    {
      name: 'firefox',
      use: {
        ...devices['Desktop Firefox'],
        viewport: { width: 1280, height: 720 }
      },
    },

    {
      name: 'mobile-chrome',
      use: {
        ...devices['Pixel 5'],
        viewport: { width: 375, height: 667 }
      },
    },
  ],

  /* Global setup to expose configuration to tests */
  metadata: {
    apiBaseUrl: API_BASE_URL,
    webBaseUrl: WEB_BASE_URL,
  },
});
