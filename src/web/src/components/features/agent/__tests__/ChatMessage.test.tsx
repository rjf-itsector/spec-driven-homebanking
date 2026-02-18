import { render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import type { ChatMessage as ChatMessageType } from '@/lib/api/types'
import { ChatMessage } from '../ChatMessage'

const userMessage: ChatMessageType = {
  id: 'msg-1',
  role: 'user',
  content: 'Hello, how are you?',
  timestamp: new Date(),
}

const assistantMessage: ChatMessageType = {
  id: 'msg-2',
  role: 'assistant',
  content: 'I am **bold** assistant',
  timestamp: new Date(),
}

describe('ChatMessage', () => {
  it('renders user message with correct content', () => {
    render(<ChatMessage message={userMessage} />)
    expect(screen.getByText('Hello, how are you?')).toBeInTheDocument()
  })

  it('renders user message right-aligned (data-testid="chat-message-user")', () => {
    render(<ChatMessage message={userMessage} />)
    const el = screen.getByTestId('chat-message-user')
    expect(el).toBeInTheDocument()
    expect(el).toHaveClass('justify-end')
  })

  it('renders assistant message left-aligned (data-testid="chat-message-assistant")', () => {
    render(<ChatMessage message={assistantMessage} />)
    const el = screen.getByTestId('chat-message-assistant')
    expect(el).toBeInTheDocument()
    expect(el).toHaveClass('justify-start')
  })

  it('renders markdown in assistant messages (bold text)', () => {
    render(<ChatMessage message={assistantMessage} />)
    const boldEl = screen.getByText('bold')
    expect(boldEl.tagName.toLowerCase()).toBe('strong')
  })

  it('renders plain text for user messages', () => {
    render(<ChatMessage message={userMessage} />)
    const container = screen.getByTestId('chat-message-user')
    const paragraph = container.querySelector('p')
    expect(paragraph).toBeInTheDocument()
    expect(paragraph).toHaveTextContent('Hello, how are you?')
  })
})
