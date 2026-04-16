/**
 * Helper utilities for QuoteFetcher.Web E2E tests
 */

const { expect } = require('@playwright/test');

/**
 * Get the API base URL from configuration
 * @returns {string} API base URL
 */
function getApiBaseUrl() {
  return process.env.API_BASE_URL || 'http://localhost:5074';
}

/**
 * Mock API response for a specific endpoint
 * @param {import('@playwright/test').Page} page - Playwright page object
 * @param {string} endpoint - API endpoint path (e.g., '/quote_category')
 * @param {any} response - Response body to return
 * @param {number} statusCode - HTTP status code (default: 200)
 */
async function mockApiResponse(page, endpoint, response, statusCode = 200) {
  const apiBaseUrl = getApiBaseUrl();
  const fullUrl = `${apiBaseUrl}${endpoint}`;

  await page.route(fullUrl, async (route) => {
    await route.fulfill({
      status: statusCode,
      contentType: 'application/json',
      body: JSON.stringify(response),
    });
  });
}

/**
 * Mock API response with specific content type
 * @param {import('@playwright/test').Page} page - Playwright page object
 * @param {string} endpoint - API endpoint path
 * @param {string} body - Raw response body
 * @param {number} statusCode - HTTP status code
 * @param {string} contentType - Content-Type header
 */
async function mockApiResponseRaw(page, endpoint, body, statusCode = 200, contentType = 'application/json') {
  const apiBaseUrl = getApiBaseUrl();
  const fullUrl = `${apiBaseUrl}${endpoint}`;

  await page.route(fullUrl, async (route) => {
    await route.fulfill({
      status: statusCode,
      contentType: contentType,
      body: body,
    });
  });
}

/**
 * Block all requests to API base URL (simulate API unreachable)
 * @param {import('@playwright/test').Page} page - Playwright page object
 */
async function blockApiRequests(page) {
  const apiBaseUrl = getApiBaseUrl();

  await page.route(`${apiBaseUrl}/**`, async (route) => {
    await route.abort('failed');
  });
}

/**
 * Mock API with network timeout
 * @param {import('@playwright/test').Page} page - Playwright page object
 * @param {string} endpoint - API endpoint path
 */
async function mockApiTimeout(page, endpoint) {
  const apiBaseUrl = getApiBaseUrl();
  const fullUrl = `${apiBaseUrl}${endpoint}`;

  await page.route(fullUrl, async (route) => {
    // Never fulfill - simulates timeout
    // The test timeout will handle this
  });
}

/**
 * Mock API response with an artificial delay
 * @param {import('@playwright/test').Page} page - Playwright page object
 * @param {string} endpoint - API endpoint path
 * @param {any} response - Response body to return
 * @param {number} delayMs - Delay in milliseconds before fulfilling
 * @param {number} statusCode - HTTP status code (default: 200)
 */
async function mockApiDelayedResponse(page, endpoint, response, delayMs, statusCode = 200) {
  const apiBaseUrl = getApiBaseUrl();
  const fullUrl = `${apiBaseUrl}${endpoint}`;

  await page.route(fullUrl, async (route) => {
    await new Promise(resolve => setTimeout(resolve, delayMs));
    await route.fulfill({
      status: statusCode,
      contentType: 'application/json',
      body: JSON.stringify(response),
    });
  });
}

/**
 * Wait for bubble elements to appear
 * @param {import('@playwright/test').Page} page - Playwright page object
 * @param {number} count - Expected number of bubbles
 * @param {number} timeout - Timeout in milliseconds (default: 5000)
 */
async function waitForBubbles(page, count, timeout = 5000) {
  await expect(page.locator('.bubble')).toHaveCount(count, { timeout });
}

/**
 * Get bubble element by category text
 * @param {import('@playwright/test').Page} page - Playwright page object
 * @param {string} category - Category text to search for
 * @returns {import('@playwright/test').Locator} Bubble element locator
 */
function getBubbleByCategory(page, category) {
  return page.locator('.bubble').filter({ hasText: category });
}

/**
 * Click a bubble and wait for quote API request
 * @param {import('@playwright/test').Page} page - Playwright page object
 * @param {string} category - Category of bubble to click
 * @returns {Promise<any>} Promise that resolves with the API response
 */
async function clickBubbleAndWaitForQuote(page, category) {
  const apiBaseUrl = getApiBaseUrl();
  const responsePromise = page.waitForResponse(
    response => response.url().includes(`${apiBaseUrl}/quote?category=${category}`),
    { timeout: 5000 }
  );

  const bubble = getBubbleByCategory(page, category);
  await bubble.click( { force:true } );

  const response = await responsePromise;
  return await response.json();
}

/**
 * Wait for crawl animation element to appear
 * @param {import('@playwright/test').Page} page - Playwright page object
 * @param {number} timeout - Timeout in milliseconds (default: 5000)
 */
async function waitForCrawl(page, timeout = 5000) {
  await page.locator('.crawl').first().waitFor({ state: 'visible', timeout });
}

/**
 * Get error message text if visible
 * @param {import('@playwright/test').Page} page - Playwright page object
 * @returns {Promise<string|null>} Error message text or null if not visible
 */
async function getErrorMessage(page) {
  const errorElement = page.locator('#error-message');
  const isVisible = await errorElement.isVisible();

  if (!isVisible) {
    return null;
  }

  return await errorElement.textContent();
}

/**
 * Wait for error message to disappear (auto-hide)
 * @param {import('@playwright/test').Page} page - Playwright page object
 * @param {number} timeout - Timeout in milliseconds (default: 6000)
 */
async function waitForErrorToDisappear(page, timeout = 7000) {

  // Wait for the element to actually be hidden (not just have the class)
  await page.locator('#error-message').waitFor({ state: 'hidden', timeout });
}

/**
 * Get current position of a bubble element
 * @param {import('@playwright/test').Page} page - Playwright page object
 * @param {import('@playwright/test').Locator} bubble - Bubble element locator
 * @returns {Promise<{x: number, y: number}>} Position object with x and y coordinates
 */
async function getBubblePosition(page, bubble) {
  const box = await bubble.boundingBox();
  return {
    x: box.x,
    y: box.y
  };
}

/**
 * Speed up CSS animations for faster tests
 * @param {import('@playwright/test').Page} page - Playwright page object
 */
async function accelerateCrawlAnimation(page) {
  await page.addStyleTag({
    content: `
      .crawl {
        animation-duration: 0.5s !important;
      }
    `
  });
}

/**
 * Set bubble positions programmatically to control collision testing
 * @param {import('@playwright/test').Page} page - Playwright page object
 * @param {number} bubbleIndex1 - Index of first bubble
 * @param {number} bubbleIndex2 - Index of second bubble
 * @param {number} distance - Distance between bubbles in pixels
 */
async function setBubblePositionsForCollision(page, bubbleIndex1 = 0, bubbleIndex2 = 1, distance = 10) {
  await page.evaluate(({ idx1, idx2, dist }) => {
    const bubbles = document.querySelectorAll('.bubble');
    if (bubbles[idx1] && bubbles[idx2]) {
      // Position first bubble
      const bubble1 = bubbles[idx1];
      bubble1.style.left = '400px';
      bubble1.style.top = '300px';

      // Position second bubble very close to first
      const bubble2 = bubbles[idx2];
      bubble2.style.left = `${400 + dist}px`;
      bubble2.style.top = '300px';

      // Set velocities to make them collide
      if (window.bubbles && window.bubbles[idx1] && window.bubbles[idx2]) {
        window.bubbles[idx1].vx = 2;
        window.bubbles[idx1].vy = 0;
        window.bubbles[idx2].vx = -2;
        window.bubbles[idx2].vy = 0;
      }
    }
  }, { idx1: bubbleIndex1, idx2: bubbleIndex2, dist: distance });
}

/**
 * Get bubble velocity (requires access to window.bubbles array)
 * @param {import('@playwright/test').Page} page - Playwright page object
 * @param {number} bubbleIndex - Index of bubble
 * @returns {Promise<{vx: number, vy: number}|null>} Velocity object or null
 */
async function getBubbleVelocity(page, bubbleIndex) {
  return await page.evaluate((idx) => {
    if (window.bubbles && window.bubbles[idx]) {
      return {
        vx: window.bubbles[idx].vx,
        vy: window.bubbles[idx].vy
      };
    }
    return null;
  }, bubbleIndex);
}

/**
 * Intercept and capture API request
 * @param {import('@playwright/test').Page} page - Playwright page object
 * @param {string} endpoint - Endpoint to intercept
 * @returns {Promise<any>} Promise that resolves with request details
 */
async function interceptApiRequest(page, endpoint) {
  const apiBaseUrl = getApiBaseUrl();
  const fullUrl = `${apiBaseUrl}${endpoint}`;

  return new Promise((resolve) => {
    page.route(fullUrl, async (route) => {
      const request = route.request();
      resolve({
        url: request.url(),
        method: request.method(),
        headers: request.headers(),
      });
      await route.continue();
    });
  });
}

module.exports = {
  getApiBaseUrl,
  mockApiResponse,
  mockApiResponseRaw,
  blockApiRequests,
  mockApiTimeout,
  mockApiDelayedResponse,
  waitForBubbles,
  getBubbleByCategory,
  clickBubbleAndWaitForQuote,
  waitForCrawl,
  getErrorMessage,
  waitForErrorToDisappear,
  getBubblePosition,
  accelerateCrawlAnimation,
  setBubblePositionsForCollision,
  getBubbleVelocity,
  interceptApiRequest,
};
