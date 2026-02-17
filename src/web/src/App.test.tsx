import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, it, expect, beforeEach } from 'vitest'
import App from './App'

function renderApp(route: string) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[route]}>
        <App />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('App', () => {
  beforeEach(() => {
    localStorage.removeItem('hb_token')
    localStorage.removeItem('hb_user')
  })

  it('redirects unauthenticated users to login page', () => {
    renderApp('/dashboard')
    expect(screen.getByText('Sign in to your account')).toBeInTheDocument()
  })

  it('renders login page with form fields', () => {
    renderApp('/login')
    expect(screen.getByLabelText('Email')).toBeInTheDocument()
    expect(screen.getByLabelText('Password')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument()
  })

  it('shows Home Banking title on login page', () => {
    renderApp('/login')
    expect(screen.getByText('Home Banking')).toBeInTheDocument()
  })

  it('shows dashboard layout when authenticated', () => {
    localStorage.setItem('hb_token', 'test-token')
    localStorage.setItem(
      'hb_user',
      JSON.stringify({
        id: '1',
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
      }),
    )
    renderApp('/dashboard')
    expect(
      screen.getByText('Your accounts will appear here.'),
    ).toBeInTheDocument()
    expect(screen.getAllByText('John Doe').length).toBeGreaterThan(0)
    expect(
      screen.getByRole('heading', { name: 'Dashboard' }),
    ).toBeInTheDocument()
  })

  it('navigates to transfer page when authenticated', () => {
    localStorage.setItem('hb_token', 'test-token')
    localStorage.setItem(
      'hb_user',
      JSON.stringify({
        id: '1',
        email: 'test@example.com',
        firstName: 'Jane',
        lastName: 'Smith',
      }),
    )
    renderApp('/transfer')
    expect(
      screen.getByRole('heading', { name: 'Transfer' }),
    ).toBeInTheDocument()
    expect(screen.getByText('Transfer form coming soon.')).toBeInTheDocument()
  })
})
