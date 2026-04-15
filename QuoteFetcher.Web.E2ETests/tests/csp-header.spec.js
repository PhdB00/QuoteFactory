const { test, expect } = require('@playwright/test');

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
});
