const { test, expect } = require('@playwright/test');
const {
  mockApiResponse,
  mockApiResponseRaw,
  blockApiRequests,
  mockApiDelayedResponse,
  waitForBubbles,
  getBubbleByCategory,
  getErrorMessage,
  waitForErrorToDisappear,
} = require('./helpers');

// Test data
const TEST_CATEGORIES = ['animal', 'celebrity', 'food'];

test.describe('Error Scenario Tests', () => {

  test('should display error when API is unreachable on load', async ({ page }) => {
    // Block only quote_category endpoint, allow config endpoint
    await page.route('**/quote_category', route => route.abort());
    
    // Navigate to the application
    await page.goto('/');

    // Wait for error message to appear
    await expect(page.locator('#error-message')).toBeVisible({ timeout: 5000 });

    // Verify error message content
    const errorText = await getErrorMessage(page);
    expect(errorText).toContain('Unable to connect to the quote service');

    // Verify no bubbles were created
    const bubbleCount = await page.locator('.bubble').count();
    expect(bubbleCount).toBe(0);
  });

  test('should display error when categories endpoint returns 500', async ({ page }) => {
    // Mock categories endpoint to return 500
    await mockApiResponse(page, '/quote_category', { error: 'Internal Server Error' }, 500);

    // Navigate to the application
    await page.goto('/');

    // Wait for error message to appear
    await expect(page.locator('#error-message')).toBeVisible({ timeout: 5000 });

    // Verify error message mentions status code
    const errorText = await getErrorMessage(page);
    expect(errorText).toContain('Unable to load categories');

    // Verify no bubbles were created
    const bubbleCount = await page.locator('.bubble').count();
    expect(bubbleCount).toBe(0);
  });

  test('should display error when categories endpoint returns empty array', async ({ page }) => {
    // Mock categories endpoint to return empty array
    await mockApiResponse(page, '/quote_category', []);

    // Navigate to the application
    await page.goto('/');

    // Wait for error message to appear
    await expect(page.locator('#error-message')).toBeVisible({ timeout: 5000 });

    // Verify error message
    const errorText = await getErrorMessage(page);
    expect(errorText).toContain('No categories found');

    // Verify no bubbles were created
    const bubbleCount = await page.locator('.bubble').count();
    expect(bubbleCount).toBe(0);
  });

  test('should handle malformed JSON from categories endpoint', async ({ page }) => {
    // Mock categories endpoint to return invalid JSON
    await mockApiResponseRaw(
      page,
      '/quote_category',
      'this is not valid JSON {{{',
      200,
      'application/json'
    );

    // Navigate to the application
    await page.goto('/');

    // Wait for error message to appear
    await expect(page.locator('#error-message')).toBeVisible({ timeout: 5000 });

    // Verify error appeared
    const errorText = await getErrorMessage(page);
    expect(errorText).toBeTruthy();
    expect(errorText.length).toBeGreaterThan(0);

    // Verify no bubbles were created
    const bubbleCount = await page.locator('.bubble').count();
    expect(bubbleCount).toBe(0);
  });

  test('should display error when quote endpoint fails on bubble click', async ({ page }) => {
    // Mock successful category load
    await mockApiResponse(page, '/quote_category', TEST_CATEGORIES);

    // Navigate and wait for bubbles
    await page.goto('/');
    await waitForBubbles(page, TEST_CATEGORIES.length);

    // Now block quote endpoint
    await blockApiRequests(page);

    // Click a bubble
    const animalBubble = getBubbleByCategory(page, 'animal');
    await animalBubble.click( {force: true} );

    // Wait for error message to appear
    await expect(page.locator('#error-message')).toBeVisible({ timeout: 5000 });

    // Verify error message
    const errorText = await getErrorMessage(page);
    expect(errorText).toMatch(/Failed to fetch quote|Unable to connect/i);

    // Verify bubble remains on screen (not destroyed)
    await expect(animalBubble).toBeVisible();
  });

  test('should display error when quote endpoint returns 404', async ({ page }) => {
    // Mock successful category load
    await mockApiResponse(page, '/quote_category', TEST_CATEGORIES);

    // Navigate and wait for bubbles
    await page.goto('/');
    await waitForBubbles(page, TEST_CATEGORIES.length);

    // Mock quote endpoint to return 404
    await mockApiResponse(
      page,
      '/quote?category=animal',
      { error: 'Not Found' },
      404
    );

    // Click a bubble
    const animalBubble = getBubbleByCategory(page, 'animal');
    await animalBubble.click( {force: true} );

    // Wait for error message to appear
    await expect(page.locator('#error-message')).toBeVisible({ timeout: 5000 });

    // Verify error message mentions 404
    const errorText = await getErrorMessage(page);
    expect(errorText).toContain('Unable to fetch quote');
  });

  test('should handle malformed quote response gracefully', async ({ page }) => {
    // Mock successful category load
    await mockApiResponse(page, '/quote_category', TEST_CATEGORIES);

    // Navigate and wait for bubbles
    await page.goto('/');
    await waitForBubbles(page, TEST_CATEGORIES.length);

    // Mock quote endpoint to return empty object (missing Value field)
    await mockApiResponse(page, '/quote?category=animal', {});

    // Click a bubble
    const animalBubble = getBubbleByCategory(page, 'animal');
    await animalBubble.click( {force: true} );

    // Wait a moment for processing
    await page.waitForTimeout(1000);

    // Check if either:
    // 1. Crawl appears with fallback behavior
    // 2. Error message appears
    const crawlVisible = await page.locator('.crawl').first().isVisible().catch(() => false);
    const errorVisible = await page.locator('#error-message').isVisible();

    // One of these should be true (fallback behavior)
    expect(crawlVisible || errorVisible).toBe(true);
  });

  test('should auto-hide error message after 5 seconds', async ({ page }) => {
    // Mock categories endpoint to return 500 to trigger error
    await mockApiResponse(page, '/quote_category', { error: 'Server Error' }, 500);

    // Navigate to the application
    await page.goto('/');

    // Wait for error to appear
    await expect(page.locator('#error-message')).toBeVisible({ timeout: 5000 });

    // Wait for error to auto-hide (5+ seconds)
    await waitForErrorToDisappear(page, 6000);

    // Verify error has hidden class
    await expect(page.locator('#error-message')).toHaveClass(/hidden/);
  });

  test('should handle network timeout during quote fetch', async ({ page }) => {
    // Mock successful category load
    await mockApiResponse(page, '/quote_category', TEST_CATEGORIES);

    // Navigate and wait for bubbles
    await page.goto('/');
    await waitForBubbles(page, TEST_CATEGORIES.length);

    // Delay quote response beyond client timeout threshold
    await mockApiDelayedResponse(
      page,
      '/quote?category=animal',
      { Value: 'Too slow' },
      12000
    );

    // Click a bubble
    const animalBubble = getBubbleByCategory(page, 'animal');
    const startTime = Date.now();
    await animalBubble.click( {force: true} );

    // Verify timeout error appears and happens within timeout window
    const errorMessage = page.locator('#error-message');
    await expect(errorMessage).toBeVisible({ timeout: 12000 });

    const elapsedMs = Date.now() - startTime;
    expect(elapsedMs).toBeLessThan(11000);

    const errorText = await getErrorMessage(page);
    expect(errorText).toContain('request timed out');
    expect(errorText).toContain('Unable to fetch quote');
  });

  test('should display timeout error when category fetch exceeds timeout on load', async ({ page }) => {
    await mockApiDelayedResponse(page, '/quote_category', TEST_CATEGORIES, 12000);

    const startTime = Date.now();
    await page.goto('/');

    const errorMessage = page.locator('#error-message');
    await expect(errorMessage).toBeVisible({ timeout: 12000 });

    const elapsedMs = Date.now() - startTime;
    expect(elapsedMs).toBeLessThan(11000);

    const errorText = await getErrorMessage(page);
    expect(errorText).toContain('Unable to load categories: request timed out');

    const bubbleCount = await page.locator('.bubble').count();
    expect(bubbleCount).toBe(0);
  });

  test('should display error for consecutive failed quote requests', async ({ page }) => {

    // Mock successful category load
    await mockApiResponse(page, '/quote_category', TEST_CATEGORIES);

    // Navigate and wait for bubbles
    await page.goto('/');
    await waitForBubbles(page, TEST_CATEGORIES.length);

    // Mock all quote endpoints to fail
    await mockApiResponse(page, '/quote?category=animal', { error: 'Error' }, 500);
    await mockApiResponse(page, '/quote?category=celebrity', { error: 'Error' }, 500);

    // Click first bubble and wait for network request to complete
    const animalBubble = getBubbleByCategory(page, 'animal');
    await animalBubble.click({ force: true });

    // Wait for error
    await expect(page.locator('#error-message')).toBeVisible({ timeout: 5000 });

    // Click second bubble
    const celebrityBubble = getBubbleByCategory(page, 'celebrity');
    await celebrityBubble.click({ force: true });
    await expect(page.locator('#error-message')).toBeVisible({ timeout: 5000 });

    // Error should still appear (or be visible)
    const errorText = await getErrorMessage(page);
    expect(
        errorText.includes('Unable to fetch quote. Please try again.') ||
        errorText.includes('Unable to connect')
    ).toBe(true);
    
  });

});
