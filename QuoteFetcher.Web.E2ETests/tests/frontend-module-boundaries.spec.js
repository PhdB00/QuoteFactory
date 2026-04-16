const { test, expect } = require('@playwright/test');
const {
  getApiBaseUrl,
  mockApiResponse,
  waitForBubbles,
  getBubbleByCategory,
  waitForCrawl,
} = require('./helpers');

const TEST_CATEGORIES = ['animal', 'celebrity', 'food'];

test.describe('Frontend Modularization Tests', () => {
  test('should not expose legacy mutable globals on window', async ({ page }) => {
    await mockApiResponse(page, '/quote_category', TEST_CATEGORIES);
    await page.goto('/');
    await waitForBubbles(page, TEST_CATEGORIES.length);

    const legacyGlobalStatus = await page.evaluate(() => {
      const legacyGlobalNames = [
        'bubbles',
        'spatialGrid',
        'API_BASE_URL',
        'initialize',
        'cleanup',
        'animate',
      ];

      return legacyGlobalNames.map((name) => ({
        name,
        exists: Object.prototype.hasOwnProperty.call(window, name),
      }));
    });

    for (const globalInfo of legacyGlobalStatus) {
      expect(globalInfo.exists, `${globalInfo.name} should not be exposed on window`).toBe(false);
    }
  });

  test('should issue exactly one quote request per bubble click', async ({ page }) => {
    const apiBaseUrl = getApiBaseUrl();
    let quoteRequestCount = 0;

    await mockApiResponse(page, '/quote_category', TEST_CATEGORIES);
    await page.route(`${apiBaseUrl}/quote?category=animal`, async (route) => {
      quoteRequestCount += 1;
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ Value: 'Single request quote.' }),
      });
    });

    await page.goto('/');
    await waitForBubbles(page, TEST_CATEGORIES.length);

    const animalBubble = getBubbleByCategory(page, 'animal');
    await animalBubble.click({ force: true });
    await waitForCrawl(page);

    expect(quoteRequestCount).toBe(1);
  });

  test('should preserve click-to-crawl behavior after modular bootstrapping', async ({ page }) => {
    await mockApiResponse(page, '/quote_category', TEST_CATEGORIES);
    await mockApiResponse(page, '/quote?category=animal', {
      Value: 'Crawl text from modular app.',
    });

    await page.goto('/');
    await waitForBubbles(page, TEST_CATEGORIES.length);

    const animalBubble = getBubbleByCategory(page, 'animal');
    await animalBubble.click({ force: true });
    await waitForCrawl(page);

    await expect(page.locator('.crawl').first()).toContainText('Crawl text from modular app.');
  });
});
