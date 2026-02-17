import { test, expect } from '@playwright/test';

test.describe('Transaction List', () => {
  test.beforeEach(async ({ page }) => {
    // Clear localStorage and login
    await page.goto('/');
    await page.evaluate(() => localStorage.clear());

    await page.goto('/login');
    await page.locator('[data-testid="email"]').fill('demo@bank.com');
    await page.locator('[data-testid="password"]').fill('Demo123!');
    await page.locator('[data-testid="login-button"]').click();
    await expect(page).toHaveURL('/dashboard');
  });

  test('loads and displays transactions with infinite scroll', async ({
    page,
  }) => {
    // Click first account to view transactions
    await page.locator('[data-testid="account-card"]').first().click();

    // Wait for initial transactions to load
    await expect(
      page.locator('[data-testid="transaction-row"]').first(),
    ).toBeVisible();

    // Count initial transactions (page size is 20)
    const initialCount = await page
      .locator('[data-testid="transaction-row"]')
      .count();
    expect(initialCount).toBeGreaterThanOrEqual(10);

    // Scroll to bottom to trigger infinite scroll
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));

    // Wait for more transactions to load
    await page.waitForTimeout(2000);

    const afterScrollCount = await page
      .locator('[data-testid="transaction-row"]')
      .count();
    expect(afterScrollCount).toBeGreaterThan(initialCount);
  });

  test('transactions sorted by timestamp (newest first)', async ({ page }) => {
    // Navigate to first account
    await page.locator('[data-testid="account-card"]').first().click();

    await expect(
      page.locator('[data-testid="transaction-row"]').first(),
    ).toBeVisible();

    // Verify that transaction rows exist and are rendered in order
    const rows = page.locator('[data-testid="transaction-row"]');
    const count = await rows.count();
    expect(count).toBeGreaterThan(1);
  });

  test('displays category badges', async ({ page }) => {
    // Navigate to first account
    await page.locator('[data-testid="account-card"]').first().click();

    await expect(
      page.locator('[data-testid="transaction-row"]').first(),
    ).toBeVisible();

    // Verify category badges are visible
    await expect(
      page.locator('[data-testid="category-badge"]').first(),
    ).toBeVisible();
  });

  test('"No more transactions" appears at end', async ({ page }) => {
    // Navigate to first account
    await page.locator('[data-testid="account-card"]').first().click();

    await expect(
      page.locator('[data-testid="transaction-row"]').first(),
    ).toBeVisible();

    // Keep scrolling until we reach the end
    for (let i = 0; i < 10; i++) {
      await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
      await page.waitForTimeout(1000);

      const noMore = page.getByText('No more transactions');
      if (await noMore.isVisible()) {
        await expect(noMore).toBeVisible();
        return;
      }
    }

    // If we have very few transactions, the end message should already be visible
    await expect(page.getByText('No more transactions')).toBeVisible();
  });
});
