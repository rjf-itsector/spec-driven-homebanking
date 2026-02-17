import { test, expect } from '@playwright/test';

test.describe('Transfer Flow', () => {
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

  test('login and navigate to transfer page', async ({ page }) => {
    // Click Transfer link in sidebar
    await page.getByRole('link', { name: 'Transfer' }).click();
    await expect(page).toHaveURL('/transfer');
    await expect(page.locator('[data-testid="transfer-form"]')).toBeVisible();
  });

  test('fill form with valid data and submit transfer', async ({ page }) => {
    // Navigate to transfer page
    await page.getByRole('link', { name: 'Transfer' }).click();
    await expect(page).toHaveURL('/transfer');

    // Wait for accounts to load in the dropdowns
    await expect(page.locator('[data-testid="from-account"]')).toBeEnabled();

    // Select From Account (Radix Select: click trigger then option)
    await page.locator('[data-testid="from-account"]').click();
    await page.getByRole('option').first().click();

    // Select To Account (different account)
    await page.locator('[data-testid="to-account"]').click();
    await page.getByRole('option').nth(1).click();

    // Fill amount
    await page.locator('[data-testid="amount"]').fill('10');

    // Fill description
    await page.locator('[data-testid="description"]').fill('E2E test transfer');

    // Submit
    await page.locator('[data-testid="transfer-submit"]').click();

    // Verify success — should redirect to dashboard (onSuccess navigates)
    await expect(page).toHaveURL('/dashboard');
  });

  test('verify success message after transfer', async ({ page }) => {
    await page.getByRole('link', { name: 'Transfer' }).click();
    await expect(page.locator('[data-testid="from-account"]')).toBeEnabled();

    // Select accounts
    await page.locator('[data-testid="from-account"]').click();
    await page.getByRole('option').first().click();

    await page.locator('[data-testid="to-account"]').click();
    await page.getByRole('option').nth(1).click();

    await page.locator('[data-testid="amount"]').fill('5');
    await page.locator('[data-testid="transfer-submit"]').click();

    // Optimistic toast should appear
    await expect(page.getByText('Transfer completed')).toBeVisible();
  });

  test('form validation errors display', async ({ page }) => {
    await page.getByRole('link', { name: 'Transfer' }).click();

    // Submit empty form — should show validation errors
    await page.locator('[data-testid="transfer-submit"]').click();

    // Should show required field errors (stay on transfer page)
    await expect(page).toHaveURL('/transfer');
  });

  test('cannot transfer to same account', async ({ page }) => {
    await page.getByRole('link', { name: 'Transfer' }).click();
    await expect(page.locator('[data-testid="from-account"]')).toBeEnabled();

    // Select same account for both From and To
    await page.locator('[data-testid="from-account"]').click();
    const firstOption = page.getByRole('option').first();
    await firstOption.click();

    await page.locator('[data-testid="to-account"]').click();
    await page.getByRole('option').first().click();

    await page.locator('[data-testid="amount"]').fill('10');
    await page.locator('[data-testid="transfer-submit"]').click();

    // Should show same-account error
    await expect(
      page.getByText('Cannot transfer to the same account'),
    ).toBeVisible();
  });
});
