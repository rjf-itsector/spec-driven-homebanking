import { render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import { ChatThinking } from '../ChatThinking'

describe('ChatThinking', () => {
  it('renders thinking indicator (data-testid="chat-thinking")', () => {
    render(<ChatThinking />)
    expect(screen.getByTestId('chat-thinking')).toBeInTheDocument()
  })

  it('renders three bouncing dots', () => {
    render(<ChatThinking />)
    const container = screen.getByTestId('chat-thinking')
    const dots = container.querySelectorAll('.animate-bounce')
    expect(dots).toHaveLength(3)
  })
})
