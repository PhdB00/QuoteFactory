const { test, expect } = require('@playwright/test');
const {
  mockApiResponse,
  waitForBubbles,
  getBubbleByCategory,
  clickBubbleAndWaitForQuote,
  waitForCrawl,
  getBubblePosition,
  setBubblePositionsForCollision,
  getBubbleVelocity,
} = require('./helpers');

// Test data - deterministic categories for testing
const TEST_CATEGORIES = ['animal', 'celebrity', 'food', 'science', 'sport'];

const TEST_QUOTE_RESPONSE = {
  icon_url: 'https://localhost.invalid.com/test.jpg',
  Value: 'This is a test quote from the test suite.'
};

async function observeClickedClassAppliedOnce(bubbleLocator, timeoutMs = 1500) {
  return bubbleLocator.evaluate((el, timeout) => {
    return new Promise((resolve) => {
      if (el.classList.contains('clicked')) {
        resolve(true);
        return;
      }

      const observer = new MutationObserver(() => {
        if (el.classList.contains('clicked')) {
          observer.disconnect();
          resolve(true);
        }
      });

      observer.observe(el, { attributes: true, attributeFilter: ['class'] });

      setTimeout(() => {
        observer.disconnect();
        resolve(false);
      }, timeout);
    });
  }, timeoutMs);
}

async function waitForExplosionFragments(page, timeoutMs = 2000) {
  await expect.poll(async () => {
    return await page.locator('.bubble-fragment').count();
  }, { timeout: timeoutMs }).toBeGreaterThan(0);
}

test.describe('Happy Path Tests', () => {

  test('should load application successfully', async ({ page }) => {
    // Mock the categories API to return test data
    await mockApiResponse(page, '/quote_category', TEST_CATEGORIES);

    // Navigate to the application
    await page.goto('/');

    // Verify page title
    await expect(page).toHaveTitle('Quote Bubbles');

    // Verify error message is hidden
    await expect(page.locator('#error-message')).toHaveClass(/hidden/);

    // Verify bubble container exists and bubbles are loaded
    await waitForBubbles(page, TEST_CATEGORIES.length);
  });

  test('should display bubbles for all categories returned by API', async ({ page }) => {
    // Mock the categories API
    await mockApiResponse(page, '/quote_category', TEST_CATEGORIES);

    // Navigate to the application
    await page.goto('/');

    // Wait for bubbles to appear
    await waitForBubbles(page, TEST_CATEGORIES.length);

    // Verify each category appears in a bubble
    for (const category of TEST_CATEGORIES) {
      const bubble = getBubbleByCategory(page, category);
      await expect(bubble).toBeVisible();
    }
  });

  test('should animate bubbles across the screen', async ({ page }) => {
    // Mock the categories API
    await mockApiResponse(page, '/quote_category', TEST_CATEGORIES);

    // Navigate and wait for bubbles
    await page.goto('/');
    await waitForBubbles(page, TEST_CATEGORIES.length);

    // Get initial position of first bubble
    const firstBubble = page.locator('.bubble').first();
    const initialPosition = await getBubblePosition(page, firstBubble);

    // Wait for animation to occur
    await page.waitForTimeout(500);

    // Get new position
    const newPosition = await getBubblePosition(page, firstBubble);

    // Verify position has changed (either x or y)
    const hasMovedX = Math.abs(newPosition.x - initialPosition.x) > 1;
    const hasMovedY = Math.abs(newPosition.y - initialPosition.y) > 1;

    expect(hasMovedX || hasMovedY).toBe(true);
  });

  test('should display quote crawl when bubble is clicked', async ({ page }) => {
    // Mock APIs using helper function for consistency
    await mockApiResponse(page, '/quote_category', TEST_CATEGORIES);
    await mockApiResponse(page, '/quote?category=animal', TEST_QUOTE_RESPONSE);

    // Navigate and wait for bubbles
    await page.goto('/');
    await waitForBubbles(page, TEST_CATEGORIES.length);

    // Click the animal bubble
    const animalBubble = getBubbleByCategory(page, 'animal');
    const clickedClassObserved = observeClickedClassAppliedOnce(animalBubble, 1500);
    await animalBubble.click( {force: true} );

    // Verify bubble click feedback is applied (transient class)
    expect(await clickedClassObserved).toBe(true);
    await waitForExplosionFragments(page);

    // Wait for crawl to appear
    await waitForCrawl(page);

    // Verify crawl element exists and has correct class
    const crawl = page.locator('.crawl').first();
    await expect(crawl).toBeVisible();

    // Verify crawl contains the quote text
    await expect(crawl).toContainText(TEST_QUOTE_RESPONSE.Value);

    // Verify the same category bubble respawns after explosion completes
    const respawnedAnimalBubble = getBubbleByCategory(page, 'animal');
    await expect(respawnedAnimalBubble).toBeVisible({ timeout: 5000 });
    await expect.poll(async () => {
      return await page.locator('.bubble-fragment').count();
    }, { timeout: 5000 }).toBe(0);
  });

  test('should apply temporary clicked visual state before explosion and respawn', async ({ page }) => {
    // Mock APIs using helper function for consistency
    await mockApiResponse(page, '/quote_category', TEST_CATEGORIES);
    await mockApiResponse(page, '/quote?category=animal', TEST_QUOTE_RESPONSE);

    // Navigate and wait for bubbles
    await page.goto('/');
    await waitForBubbles(page, TEST_CATEGORIES.length);

    // Click the animal bubble
    const animalBubble = getBubbleByCategory(page, 'animal');
    const clickedClassObserved = observeClickedClassAppliedOnce(animalBubble, 1500);
    await animalBubble.click({ force: true });

    // Verify clicked class appears quickly
    expect(await clickedClassObserved).toBe(true);

    // Verify explosion happens and the bubble category returns after respawn
    await waitForExplosionFragments(page, 2500);
    const respawnedAnimalBubble = getBubbleByCategory(page, 'animal');
    await expect(respawnedAnimalBubble).toBeVisible({ timeout: 5000 });
  });

  test('should display multiple crawls when multiple bubbles are clicked', async ({ page }) => {
    // Mock APIs
    await mockApiResponse(page, '/quote_category', TEST_CATEGORIES);

    // Mock quote responses for different categories
    await mockApiResponse(page, '/quote?category=animal', {
      icon_url: 'https://localhost.invalid.com/animal.jpg',
      Value: 'First quote about animals.'
    });
    await mockApiResponse(page, '/quote?category=celebrity', {
      icon_url: 'https://localhost.invalid.com/celebrity.jpg',
      Value: 'Second quote about celebrities.'
    });

    // Navigate and wait for bubbles
    await page.goto('/');
    await waitForBubbles(page, TEST_CATEGORIES.length);

    // Click first bubble
    const animalBubble = getBubbleByCategory(page, 'animal');
    await animalBubble.click({force: true} );
    await waitForCrawl(page);

    // Click second bubble
    const celebrityBubble = getBubbleByCategory(page, 'celebrity');
    await celebrityBubble.click({force: true} );

    // Wait a moment for second crawl
    await page.waitForTimeout(500);

    // Verify two crawl elements exist
    const crawlCount = await page.locator('.crawl').count();
    expect(crawlCount).toBe(2);
  });
  
  test('should work on mobile viewport', async ({ page, browserName }) => {
    // Skip for mobile-chrome project (it's already mobile)
    // This test runs on all projects, so we'll set viewport for non-mobile
    if (browserName === 'chromium' || browserName === 'firefox') {
      await page.setViewportSize({ width: 375, height: 667 });
    }

    // Mock APIs using helper function for consistency
    await mockApiResponse(page, '/quote_category', TEST_CATEGORIES);
    await mockApiResponse(page, '/quote?category=animal', TEST_QUOTE_RESPONSE);

    // Navigate and wait for bubbles
    await page.goto('/');
    await waitForBubbles(page, TEST_CATEGORIES.length);

    // Verify bubbles appear
    const bubbleCount = await page.locator('.bubble').count();
    expect(bubbleCount).toBe(TEST_CATEGORIES.length);

    // Verify bubbles stay within viewport bounds
    const bubbles = await page.locator('.bubble').all();
    for (const bubble of bubbles) {
      const box = await bubble.boundingBox();
      expect(box.x).toBeGreaterThanOrEqual(0);
      expect(box.x + box.width).toBeLessThanOrEqual(375);
    }

    // Click a bubble
    const animalBubble = getBubbleByCategory(page, 'animal');
    await animalBubble.click({force: true} );

    // Wait for crawl
    await waitForCrawl(page);

    // Verify crawl displays correctly
    const crawl = page.locator('.crawl').first();
    await expect(crawl).toBeVisible();
    await expect(crawl).toContainText(TEST_QUOTE_RESPONSE.Value);
  });

});
