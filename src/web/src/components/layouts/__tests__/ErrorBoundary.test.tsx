import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import type { ReactNode } from 'react'
import { ErrorBoundary } from '../ErrorBoundary'

function ThrowingComponent({ error }: { error: Error }): ReactNode {
  throw error
}

function GoodComponent() {
  return <p>All good</p>
}

describe('ErrorBoundary', () => {
  beforeEach(() => {
    // Suppress React error boundary console.error noise
    vi.spyOn(console, 'error').mockImplementation(() => {})
  })

  it('renders children when no error occurs', () => {
    render(
      <ErrorBoundary>
        <GoodComponent />
      </ErrorBoundary>,
    )
    expect(screen.getByText('All good')).toBeInTheDocument()
  })

  it('renders fallback UI when a child throws', () => {
    render(
      <ErrorBoundary>
        <ThrowingComponent error={new Error('boom')} />
      </ErrorBoundary>,
    )
    expect(screen.getByText('Something went wrong')).toBeInTheDocument()
    expect(
      screen.getByText(/something unexpected happened/i),
    ).toBeInTheDocument()
  })

  it('displays the error message', () => {
    render(
      <ErrorBoundary>
        <ThrowingComponent error={new Error('Test failure message')} />
      </ErrorBoundary>,
    )
    expect(screen.getByText('Test failure message')).toBeInTheDocument()
  })

  it('shows a Return to Dashboard button', () => {
    render(
      <ErrorBoundary>
        <ThrowingComponent error={new Error('boom')} />
      </ErrorBoundary>,
    )
    expect(
      screen.getByRole('button', { name: /return to dashboard/i }),
    ).toBeInTheDocument()
  })

  it('shows a Try Again button that resets the error state', async () => {
    const user = userEvent.setup()
    const { rerender } = render(
      <ErrorBoundary>
        <ThrowingComponent error={new Error('boom')} />
      </ErrorBoundary>,
    )

    expect(screen.getByText('Something went wrong')).toBeInTheDocument()

    // After clicking Try Again, the boundary resets. Re-render with good child.
    // We need to trick React: clicking "Try Again" resets state, then on re-render
    // we want a non-throwing component. Since the same ThrowingComponent would throw
    // again, we test that the button exists and is clickable.
    const tryAgainBtn = screen.getByRole('button', { name: /try again/i })
    expect(tryAgainBtn).toBeInTheDocument()

    // Rerender with a good component first, then click try again
    rerender(
      <ErrorBoundary>
        <GoodComponent />
      </ErrorBoundary>,
    )

    await user.click(tryAgainBtn)

    // After reset, the good component should render
    expect(screen.getByText('All good')).toBeInTheDocument()
  })
})
