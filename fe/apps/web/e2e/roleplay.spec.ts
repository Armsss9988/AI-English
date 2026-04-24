import { test, expect } from '@playwright/test';

const BASE_URL = process.env.E2E_BASE_URL || 'http://localhost:3000';

test.describe('Roleplay Finalization', () => {
  test('can complete a roleplay session and see coaching summary', async ({ page }) => {
    await loginAsTestLearner(page);

    // Start roleplay
    await page.click('[data-testid="roleplay-nav"]');
    await page.click('[data-testid="start-roleplay"]');

    // Select scenario
    await page.click('[data-testid="scenario-option"]');

    // Complete turns
    for (let i = 0; i < 3; i++) {
      await page.fill('[data-testid="learner-input"]', `This is turn ${i + 1}`);
      await page.click('[data-testid="send-message"]');
      await page.waitForTimeout(500); // Wait for AI response
    }

    // Finalize session
    await page.click('[data-testid="finalize-session"]');

    // Verify summary displayed
    await expect(page.locator('[data-testid="coaching-summary"]')).toBeVisible();
    await expect(page.locator('[data-testid="total-turns"]')).toContainText('3');

    // Verify success criteria evaluated
    await expect(page.locator('[data-testid="criteria-evaluated"]')).toBeVisible();
  });

  test('roleplay failure shows retryable state', async ({ page }) => {
    await loginAsTestLearner(page);

    await page.click('[data-testid="roleplay-nav"]');
    await page.click('[data-testid="start-roleplay"]');
    await page.click('[data-testid="scenario-option"]');

    await page.fill('[data-testid="learner-input"]', 'Test message');
    await page.click('[data-testid="send-message"]');

    // Should eventually show error state
    await expect(page.locator('[data-testid="session-error"]')).toBeVisible({ timeout: 20000 });
    await expect(page.locator('[data-testid="retry-session"]')).toBeVisible();
  });
});

async function loginAsTestLearner(page: any) {
  await page.goto(`${BASE_URL}/login`);
  await page.fill('[data-testid="email-input"]', process.env.E2E_TEST_EMAIL || 'test@example.com');
  await page.fill('[data-testid="password-input"]', process.env.E2E_TEST_PASSWORD || 'testpassword');
  await page.click('[data-testid="login-button"]');
  await page.waitForURL(/dashboard/);
}