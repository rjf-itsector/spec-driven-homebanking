import { test, expect } from '@playwright/test';

test.describe('Authentication', () => {
  test.beforeEach(async ({ page }) => {
    // Clear localStorage before each test
    await page.goto('/');
    await page.evaluate(() => localStorage.clear());
  });

  test('navigate to login page', async ({ page }) => {
    await page.goto('/login');

    await expect(page.locator('[data-testid="email"]')).toBeVisible();
    await expect(page.locator('[data-testid="password"]')).toBeVisible();
    await expect(page.locator('[data-testid="login-button"]')).toBeVisible();
    await expect(page.getByText('Home Banking')).toBeVisible();
    await expect(page.getByText('Sign in to your account')).toBeVisible();
  });

  test('user can log in with valid credentials', async ({ page }) => {
    await page.goto('/login');

    await page.locator('[data-testid="email"]').fill('demo@bank.com');
    await page.locator('[data-testid="password"]').fill('Demo123!');
    await page.locator('[data-testid="login-button"]').click();

    // Should redirect to dashboard
    await expect(page).toHaveURL('/dashboard');
    await expect(page.locator('[data-testid="dashboard"]')).toBeVisible();
  });

  test('verify account cards visible after login', async ({ page }) => {
    await page.goto('/login');

    await page.locator('[data-testid="email"]').fill('demo@bank.com');
    await page.locator('[data-testid="password"]').fill('Demo123!');
    await page.locator('[data-testid="login-button"]').click();

    await expect(page).toHaveURL('/dashboard');

    // Wait for accounts to load and verify at least one card is visible
    await expect(
      page.locator('[data-testid="account-card"]').first(),
    ).toBeVisible();

    const cardCount = await page
      .locator('[data-testid="account-card"]')
      .count();
    expect(cardCount).toBeGreaterThan(0);
  });

  test('shows error with invalid credentials', async ({ page }) => {
    await page.goto('/login');

    await page.locator('[data-testid="email"]').fill('wrong@example.com');
    await page.locator('[data-testid="password"]').fill('WrongPassword1!');
    await page.locator('[data-testid="login-button"]').click();

    // Should show error and stay on login page
    await expect(page.locator('[role="alert"]')).toBeVisible();
    await expect(page).toHaveURL('/login');
  });

  test('redirects unauthenticated user to login', async ({ page }) => {
    await page.goto('/dashboard');

    // Should redirect to login
    await expect(page).toHaveURL('/login');
  });
});
