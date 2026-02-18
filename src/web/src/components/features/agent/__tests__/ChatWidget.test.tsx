import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect, vi } from 'vitest'
import { ChatWidget } from '../ChatWidget'

// jsdom doesn't implement scrollIntoView
Element.prototype.scrollIntoView = vi.fn()

// Mock the useChatAgent hook
const mockToggleOpen = vi.fn()
const mockSendMessage = vi.fn()
const mockClearChat = vi.fn()

vi.mock('@/lib/hooks/useChatAgent', () => ({
  useChatAgent: vi.fn(() => ({
    messages: [],
    isOpen: false,
    isLoading: false,
    error: null,
    sendMessage: mockSendMessage,
    toggleOpen: mockToggleOpen,
    clearChat: mockClearChat,
  })),
}))

import { useChatAgent } from '@/lib/hooks/useChatAgent'

describe('ChatWidget', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the floating chat button', () => {
    render(<ChatWidget />)
    expect(screen.getByRole('button')).toBeInTheDocument()
  })

  it('shows "Open chat" aria-label initially', () => {
    render(<ChatWidget />)
    expect(screen.getByLabelText('Open chat')).toBeInTheDocument()
  })

  it('clicking opens the chat panel (shows Banking Assistant header)', () => {
    vi.mocked(useChatAgent).mockReturnValue({
      messages: [],
      isOpen: true,
      isLoading: false,
      error: null,
      sendMessage: mockSendMessage,
      toggleOpen: mockToggleOpen,
      clearChat: mockClearChat,
    })

    render(<ChatWidget />)
    expect(screen.getByText('Banking Assistant')).toBeInTheDocument()
  })

  it('clicking the button calls toggleOpen', async () => {
    vi.mocked(useChatAgent).mockReturnValue({
      messages: [],
      isOpen: false,
      isLoading: false,
      error: null,
      sendMessage: mockSendMessage,
      toggleOpen: mockToggleOpen,
      clearChat: mockClearChat,
    })

    render(<ChatWidget />)
    const user = userEvent.setup()
    await user.click(screen.getByLabelText('Open chat'))
    expect(mockToggleOpen).toHaveBeenCalledTimes(1)
  })

  it('shows "Close chat" aria-label when open', () => {
    vi.mocked(useChatAgent).mockReturnValue({
      messages: [],
      isOpen: true,
      isLoading: false,
      error: null,
      sendMessage: mockSendMessage,
      toggleOpen: mockToggleOpen,
      clearChat: mockClearChat,
    })

    render(<ChatWidget />)
    expect(screen.getByLabelText('Close chat')).toBeInTheDocument()
  })
})
