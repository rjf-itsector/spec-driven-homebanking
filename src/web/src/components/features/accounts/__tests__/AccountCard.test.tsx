import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, it, expect } from 'vitest'
import type { AccountDto } from '@/lib/api/types'
import { AccountCard } from '../AccountCard'

const mockAccount: AccountDto = {
  id: 'acc-1',
  accountNumber: 'PT12345678',
  type: 'Checking',
  balance: 1234.56,
  currency: 'EUR',
  isActive: true,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
}

function renderAccountCard(account: AccountDto) {
  return render(
    <MemoryRouter>
      <AccountCard account={account} />
    </MemoryRouter>,
  )
}

describe('AccountCard', () => {
  it('renders the account type label', () => {
    renderAccountCard(mockAccount)
    expect(screen.getByText('Checking')).toBeInTheDocument()
  })

  it('renders the formatted balance', () => {
    renderAccountCard(mockAccount)
    expect(screen.getByText('€1,234.56')).toBeInTheDocument()
  })

  it('applies text-destructive class for negative balance', () => {
    renderAccountCard({ ...mockAccount, balance: -500 })
    const balanceEl = screen.getByText('-€500.00')
    expect(balanceEl).toHaveClass('text-destructive')
  })

  it('does not apply text-destructive class for positive balance', () => {
    renderAccountCard(mockAccount)
    const balanceEl = screen.getByText('€1,234.56')
    expect(balanceEl).not.toHaveClass('text-destructive')
  })

  it('renders the masked account number', () => {
    renderAccountCard(mockAccount)
    expect(screen.getByText('****5678')).toBeInTheDocument()
  })

  it('links to the correct account detail page', () => {
    renderAccountCard(mockAccount)
    const link = screen.getByRole('link')
    expect(link).toHaveAttribute('href', '/accounts/acc-1')
  })

  it('renders Savings type label correctly', () => {
    renderAccountCard({ ...mockAccount, type: 'Savings' })
    expect(screen.getByText('Savings')).toBeInTheDocument()
  })

  it('falls back to the raw type for unknown account types', () => {
    renderAccountCard({ ...mockAccount, type: 'Business' })
    expect(screen.getByText('Business')).toBeInTheDocument()
  })
})
