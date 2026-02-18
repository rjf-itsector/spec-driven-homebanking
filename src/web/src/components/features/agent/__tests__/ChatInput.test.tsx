import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect, vi } from 'vitest'
import { ChatInput } from '../ChatInput'

describe('ChatInput', () => {
  it('renders input and send button', () => {
    render(<ChatInput onSend={vi.fn()} isLoading={false} />)
    expect(screen.getByLabelText('Chat message')).toBeInTheDocument()
    expect(screen.getByLabelText('Send message')).toBeInTheDocument()
  })

  it('send button disabled when input is empty', () => {
    render(<ChatInput onSend={vi.fn()} isLoading={false} />)
    expect(screen.getByLabelText('Send message')).toBeDisabled()
  })

  it('typing enables send button', async () => {
    const user = userEvent.setup()
    render(<ChatInput onSend={vi.fn()} isLoading={false} />)
    await user.type(screen.getByLabelText('Chat message'), 'Hello')
    expect(screen.getByLabelText('Send message')).toBeEnabled()
  })

  it('clicking send calls onSend with trimmed text and clears input', async () => {
    const user = userEvent.setup()
    const onSend = vi.fn()
    render(<ChatInput onSend={onSend} isLoading={false} />)
    const input = screen.getByLabelText('Chat message')
    await user.type(input, '  Hello world  ')
    await user.click(screen.getByLabelText('Send message'))
    expect(onSend).toHaveBeenCalledWith('Hello world')
    expect(input).toHaveValue('')
  })

  it('pressing Enter calls onSend', async () => {
    const user = userEvent.setup()
    const onSend = vi.fn()
    render(<ChatInput onSend={onSend} isLoading={false} />)
    await user.type(screen.getByLabelText('Chat message'), 'Hello{Enter}')
    expect(onSend).toHaveBeenCalledWith('Hello')
  })

  it('pressing Shift+Enter does NOT send', async () => {
    const user = userEvent.setup()
    const onSend = vi.fn()
    render(<ChatInput onSend={onSend} isLoading={false} />)
    await user.type(
      screen.getByLabelText('Chat message'),
      'Hello{Shift>}{Enter}{/Shift}',
    )
    expect(onSend).not.toHaveBeenCalled()
  })

  it('send button disabled when isLoading is true', async () => {
    render(<ChatInput onSend={vi.fn()} isLoading={true} />)
    // Textarea is disabled when loading, so we can't type. Just check button directly.
    expect(screen.getByLabelText('Send message')).toBeDisabled()
  })

  it('input cannot exceed 500 characters', async () => {
    const user = userEvent.setup()
    render(<ChatInput onSend={vi.fn()} isLoading={false} />)
    const input = screen.getByLabelText('Chat message')
    const longText = 'a'.repeat(510)
    await user.type(input, longText)
    expect((input as HTMLTextAreaElement).value.length).toBeLessThanOrEqual(500)
  })

  it('shows character count when > 400 chars', async () => {
    const user = userEvent.setup()
    render(<ChatInput onSend={vi.fn()} isLoading={false} />)
    const input = screen.getByLabelText('Chat message')
    const text = 'a'.repeat(401)
    await user.type(input, text)
    // Remaining should be 500 - 401 = 99
    expect(screen.getByText('99')).toBeInTheDocument()
  })
})
