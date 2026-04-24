import { test, expect } from '@playwright/test';

const BASE_URL = process.env.E2E_BASE_URL || 'http://localhost:3000';

test.describe('Speaking Feedback Flow', () => {
  test('learner can submit speaking attempt and receive feedback', async ({ page }) => {
    await loginAsTestLearner(page);

    // Navigate to speaking practice
    await page.click('[data-testid="speaking-nav"]');
    await page.click('[data-testid="start-speaking-drill"]');

    // Select a phrase to practice
    await page.click('[data-testid="phrase-option"]');

    // Simulate recording (in real test, would upload audio)
    // For E2E, we use fake audio URL
    await page.fill('[data-testid="audio-url-input"]', 'https://example.com/test-audio.mp3');
    await page.click('[data-testid="submit-attempt"]');

    // Wait for processing
    await page.waitForSelector('[data-testid="attempt-status"]', { timeout: 10000 });

    // Verify feedback is displayed
    await expect(page.locator('[data-testid="feedback-section"]')).toBeVisible();
    await expect(page.locator('[data-testid="pronunciation-score"]')).toBeVisible();
    await expect(page.locator('[data-testid="fluency-score"]')).toBeVisible();
    await expect(page.locator('[data-testid="overall-feedback"]')).toBeVisible();
  });

  test('attempt fails gracefully when provider is unavailable', async ({ page }) => {
    await loginAsTestLearner(page);

    await page.click('[data-testid="speaking-nav"]');
    await page.click('[data-testid="start-speaking-drill"]');
    await page.click('[data-testid="phrase-option"]');

    await page.fill('[data-testid="audio-url-input"]', 'https://example.com/test-audio.mp3');
    await page.click('[data-testid="submit-attempt"]');

    // Should show retryable state
    await expect(page.locator('[data-testid="attempt-failed"]')).toBeVisible({ timeout: 15000 });
    await expect(page.locator('[data-testid="retry-button"]')).toBeVisible();
  });
});

async function loginAsTestLearner(page: any) {
  await page.goto(`${BASE_URL}/login`);
  await page.fill('[data-testid="email-input"]', process.env.E2E_TEST_EMAIL || 'test@example.com');
  await page.fill('[data-testid="password-input"]', process.env.E2E_TEST_PASSWORD || 'testpassword');
  await page.click('[data-testid="login-button"]');
  await page.waitForURL(/dashboard/);
}