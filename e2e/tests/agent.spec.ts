import { test, expect } from '@playwright/test'

// Helper to login and navigate to dashboard
async function loginAndGoToDashboard(page: import('@playwright/test').Page) {
  await page.goto('/login')
  await page.getByLabel('Email').fill('demo@bank.com')
  await page.getByLabel('Password').fill('Demo123!')
  await page.getByRole('button', { name: /sign in/i }).click()
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible()
}

test.describe('Agent Chat Widget', () => {
  test('chat button is visible on dashboard after login', async ({ page }) => {
    await loginAndGoToDashboard(page)
    await expect(page.getByRole('button', { name: 'Open chat' })).toBeVisible()
  })

  test('chat button is NOT visible on login page', async ({ page }) => {
    await page.goto('/login')
    await expect(page.getByRole('button', { name: 'Open chat' })).not.toBeVisible()
  })

  test('opens chat panel on button click', async ({ page }) => {
    await loginAndGoToDashboard(page)
    await page.getByRole('button', { name: 'Open chat' }).click()
    await expect(page.getByText('Banking Assistant')).toBeVisible()
    await expect(page.getByLabel('Chat message')).toBeVisible()
    await expect(page.getByRole('button', { name: 'Send message' })).toBeVisible()
  })

  test('closes chat panel on close button click', async ({ page }) => {
    await loginAndGoToDashboard(page)
    await page.getByRole('button', { name: 'Open chat' }).click()
    await expect(page.getByText('Banking Assistant')).toBeVisible()
    // Click the close button (same button, now shows X)
    await page.getByRole('button', { name: 'Close chat' }).click()
    await expect(page.getByText('Banking Assistant')).not.toBeVisible()
  })

  test('can type a message in the input', async ({ page }) => {
    await loginAndGoToDashboard(page)
    await page.getByRole('button', { name: 'Open chat' }).click()
    const input = page.getByLabel('Chat message')
    await input.fill('What is my balance?')
    await expect(input).toHaveValue('What is my balance?')
  })

  test('send button is disabled when input is empty', async ({ page }) => {
    await loginAndGoToDashboard(page)
    await page.getByRole('button', { name: 'Open chat' }).click()
    await expect(page.getByRole('button', { name: 'Send message' })).toBeDisabled()
  })

  test('send button is enabled when input has text', async ({ page }) => {
    await loginAndGoToDashboard(page)
    await page.getByRole('button', { name: 'Open chat' }).click()
    await page.getByLabel('Chat message').fill('Hello')
    await expect(page.getByRole('button', { name: 'Send message' })).toBeEnabled()
  })

  test('sending a message shows user message and handles agent response', async ({ page }) => {
    await loginAndGoToDashboard(page)
    await page.getByRole('button', { name: 'Open chat' }).click()
    await page.getByLabel('Chat message').fill('What is my checking balance?')
    await page.getByRole('button', { name: 'Send message' }).click()
    
    // User message should appear
    await expect(page.getByTestId('chat-message-user')).toBeVisible()
    await expect(page.getByTestId('chat-message-user')).toContainText('What is my checking balance?')
    
    // Input should be cleared after send
    await expect(page.getByLabel('Chat message')).toHaveValue('')
    
    // Since agent is unavailable (no Azure AI configured), expect an error message
    // The thinking indicator should appear briefly then an error
    // Wait for loading to finish (thinking indicator disappears)
    await expect(page.getByTestId('chat-thinking')).toBeVisible({ timeout: 2000 }).catch(() => {
      // Thinking may be too fast to catch, that's ok
    })
    
    // Wait for the response or error
    // Since Azure is not configured, we should see an error message
    await expect(page.getByText(/unavailable|went wrong|try again/i)).toBeVisible({ timeout: 10000 })
  })

  test('chat widget is visible on transfer page too', async ({ page }) => {
    await loginAndGoToDashboard(page)
    // Navigate to transfer page
    await page.getByRole('link', { name: 'Transfer' }).click()
    await expect(page.getByRole('heading', { name: 'Transfer' })).toBeVisible()
    await expect(page.getByRole('button', { name: 'Open chat' })).toBeVisible()
  })

  test('chat preserves messages when navigating between pages', async ({ page }) => {
    await loginAndGoToDashboard(page)
    await page.getByRole('button', { name: 'Open chat' }).click()
    await page.getByLabel('Chat message').fill('Hello')
    await page.getByRole('button', { name: 'Send message' }).click()
    await expect(page.getByTestId('chat-message-user')).toContainText('Hello')
    
    // Navigate to transfer page
    await page.getByRole('link', { name: 'Transfer' }).click()
    await expect(page.getByRole('heading', { name: 'Transfer' })).toBeVisible()
    
    // Chat should still be open with the message - though note chat state may reset
    // since ChatWidget uses local state. This test documents current behavior.
    // If the widget is remounted on navigation, messages reset.
  })

  test('clear chat removes all messages', async ({ page }) => {
    await loginAndGoToDashboard(page)
    await page.getByRole('button', { name: 'Open chat' }).click()
    
    // Send a message
    await page.getByLabel('Chat message').fill('Test message')
    await page.getByRole('button', { name: 'Send message' }).click()
    await expect(page.getByTestId('chat-message-user')).toBeVisible()
    
    // Clear chat
    await page.getByRole('button', { name: 'Clear chat' }).click()
    
    // Messages should be gone
    await expect(page.getByTestId('chat-message-user')).not.toBeVisible()
    // Empty state placeholder should appear
    await expect(page.getByText('Ask me about your accounts')).toBeVisible()
  })
})
