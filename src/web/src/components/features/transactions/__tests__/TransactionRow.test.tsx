import { render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import { TransactionRow } from '../TransactionRow'
import type { TransactionDto } from '@/lib/api/types'

const creditTransaction: TransactionDto = {
  id: 'tx-1',
  type: 'Credit',
  amount: 150.5,
  balanceAfter: 1250.75,
  description: 'Salary Payment',
  category: 'Bills',
  timestamp: '2025-06-15T10:30:00Z',
  referenceNumber: 'REF-001',
  relatedTransactionId: null,
}

const debitTransaction: TransactionDto = {
  id: 'tx-2',
  type: 'Debit',
  amount: -42.99,
  balanceAfter: 957.01,
  description: 'Coffee Shop',
  category: 'Dining',
  timestamp: '2025-06-16T14:45:00Z',
  referenceNumber: 'REF-002',
  relatedTransactionId: null,
}

describe('TransactionRow', () => {
  it('renders credit transaction with green styling and + prefix', () => {
    const { container } = render(
      <TransactionRow transaction={creditTransaction} />,
    )
    const amountEl = screen.getByText(/^\+/)
    expect(amountEl).toBeInTheDocument()
    expect(amountEl.className).toMatch(/green/)

    const iconWrapper = container.querySelector('.rounded-full')
    expect(iconWrapper?.className).toMatch(/green/)
  })

  it('renders debit transaction with red styling and - prefix', () => {
    const { container } = render(
      <TransactionRow transaction={debitTransaction} />,
    )
    const amountEl = screen.getByText(/^-/)
    expect(amountEl).toBeInTheDocument()
    expect(amountEl.className).toMatch(/red/)

    const iconWrapper = container.querySelector('.rounded-full')
    expect(iconWrapper?.className).toMatch(/red/)
  })

  it('shows formatted amount and balance after', () => {
    render(<TransactionRow transaction={creditTransaction} />)
    // +€150.50
    expect(screen.getByText(/150\.50/)).toBeInTheDocument()
    // Balance after: €1,250.75
    expect(screen.getByText(/1,250\.75/)).toBeInTheDocument()
  })

  it('shows description text', () => {
    render(<TransactionRow transaction={creditTransaction} />)
    expect(screen.getByText('Salary Payment')).toBeInTheDocument()
  })

  it('shows category badge', () => {
    render(<TransactionRow transaction={creditTransaction} />)
    expect(screen.getByText('Bills')).toBeInTheDocument()
  })

  it('shows formatted date', () => {
    render(<TransactionRow transaction={creditTransaction} />)
    // Intl.DateTimeFormat 'en-US' with month:'short' → "Jun 15, 2025"
    expect(screen.getByText(/Jun 15, 2025/)).toBeInTheDocument()
  })

  it('defaults currency to EUR', () => {
    render(<TransactionRow transaction={creditTransaction} />)
    // Both amount and balanceAfter should use EUR symbol €
    const euroElements = screen.getAllByText(/€/)
    expect(euroElements.length).toBeGreaterThanOrEqual(2)
  })

  it('accepts custom currency', () => {
    render(<TransactionRow transaction={creditTransaction} currency="USD" />)
    // Both amount and balanceAfter should use USD symbol $
    const dollarElements = screen.getAllByText(/\$/)
    expect(dollarElements.length).toBeGreaterThanOrEqual(2)
  })
})
