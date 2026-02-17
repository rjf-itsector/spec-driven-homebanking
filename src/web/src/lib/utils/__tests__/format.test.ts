import { describe, it, expect } from 'vitest'
import {
  formatCurrency,
  formatDate,
  maskAccountNumber,
} from '@/lib/utils/format'

describe('formatCurrency', () => {
  it('formats a positive amount in EUR by default', () => {
    expect(formatCurrency(1234.56)).toBe('€1,234.56')
  })

  it('formats zero as €0.00', () => {
    expect(formatCurrency(0)).toBe('€0.00')
  })

  it('formats a negative amount with minus sign', () => {
    expect(formatCurrency(-500)).toBe('-€500.00')
  })

  it('formats with two decimal places', () => {
    expect(formatCurrency(10)).toBe('€10.00')
  })

  it('formats large amounts with thousands separator', () => {
    expect(formatCurrency(1_000_000)).toBe('€1,000,000.00')
  })

  it('formats amount in USD when specified', () => {
    expect(formatCurrency(1234.56, 'USD')).toBe('$1,234.56')
  })

  it('formats amount in GBP when specified', () => {
    expect(formatCurrency(99.99, 'GBP')).toBe('£99.99')
  })
})

describe('formatDate', () => {
  it('formats an ISO date string', () => {
    const result = formatDate('2024-01-15T10:30:00Z')
    expect(result).toContain('Jan')
    expect(result).toContain('15')
    expect(result).toContain('2024')
  })

  it('returns a string matching Intl.DateTimeFormat output', () => {
    const input = '2024-06-20T14:00:00Z'
    const expected = new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(new Date(input))
    expect(formatDate(input)).toBe(expected)
  })

  it('handles different months correctly', () => {
    const result = formatDate('2024-12-25T00:00:00Z')
    expect(result).toContain('Dec')
    expect(result).toContain('2024')
  })
})

describe('maskAccountNumber', () => {
  it('masks all but last 4 characters', () => {
    expect(maskAccountNumber('PT12345678')).toBe('****5678')
  })

  it('masks a short account number', () => {
    expect(maskAccountNumber('1234')).toBe('****1234')
  })

  it('masks a long account number', () => {
    expect(maskAccountNumber('PT50000201231234567890154')).toBe('****0154')
  })
})
