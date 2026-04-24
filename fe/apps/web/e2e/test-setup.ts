import { test, expect } from '@playwright/test';

const BASE_URL = process.env.E2E_BASE_URL || 'http://localhost:3000';

test.describe('Critical E2E Flows', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto(BASE_URL);
  });

  test('onboarding - new learner can complete profile setup', async ({ page }) => {
    // Navigate to onboarding
    await page.click('[data-testid="start-onboarding"]');

    // Fill profile
    await page.fill('[data-testid="name-input"]', 'Test Learner');
    await page.selectOption('[data-testid="level-select"]', 'intermediate');

    // Complete onboarding
    await page.click('[data-testid="submit-profile"]');

    // Verify redirect to dashboard
    await expect(page).toHaveURL(/dashboard/);
    await expect(page.locator('[data-testid="welcome-message"]')).toContainText('Test Learner');
  });

  test('due review - learner can complete review items', async ({ page }) => {
    // Login first
    await loginAsTestLearner(page);

    // Navigate to daily mission
    await page.click('[data-testid="daily-mission-nav"]');

    // Verify review section exists
    await expect(page.locator('[data-testid="reviews-section"]')).toBeVisible();

    // Click first review item
    const reviewItems = page.locator('[data-testid="review-item"]');
    const count = await reviewItems.count();
    expect(count).toBeGreaterThan(0);

    await reviewItems.first().click();

    // Submit review response
    await page.fill('[data-testid="review-response"]', 'Good morning team');
    await page.click('[data-testid="submit-review"]');

    // Verify completion
    await expect(page.locator('[data-testid="review-completed"]')).toBeVisible();
  });

});

async function loginAsTestLearner(page: any) {
  // Use test credentials from environment
  const testEmail = process.env.E2E_TEST_EMAIL || 'test@example.com';
  const testPassword = process.env.E2E_TEST_PASSWORD || 'testpassword';

  await page.goto(`${BASE_URL}/login`);
  await page.fill('[data-testid="email-input"]', testEmail);
  await page.fill('[data-testid="password-input"]', testPassword);
  await page.click('[data-testid="login-button"]');
  await page.waitForURL(/dashboard/);
}