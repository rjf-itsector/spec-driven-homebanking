import { describe, it, expect } from 'vitest'
import { transferSchema } from '@/lib/utils/validation'

const validTransfer = {
  fromAccountId: 'acc-1',
  toAccountId: 'acc-2',
  amount: 100,
  description: 'Test transfer',
}

describe('transferSchema', () => {
  it('accepts valid transfer data', () => {
    const result = transferSchema.safeParse(validTransfer)
    expect(result.success).toBe(true)
  })

  it('accepts valid data without optional description', () => {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const { description: _, ...withoutDescription } = validTransfer
    const result = transferSchema.safeParse(withoutDescription)
    expect(result.success).toBe(true)
  })

  it('rejects empty fromAccountId', () => {
    const result = transferSchema.safeParse({
      ...validTransfer,
      fromAccountId: '',
    })
    expect(result.success).toBe(false)
    expect(result.error?.issues[0].message).toBe('Please select source account')
  })

  it('rejects empty toAccountId', () => {
    const result = transferSchema.safeParse({
      ...validTransfer,
      toAccountId: '',
    })
    expect(result.success).toBe(false)
    expect(result.error?.issues[0].message).toBe(
      'Please select destination account',
    )
  })

  it('rejects zero amount', () => {
    const result = transferSchema.safeParse({ ...validTransfer, amount: 0 })
    expect(result.success).toBe(false)
    expect(result.error?.issues[0].message).toBe(
      'Amount must be greater than zero',
    )
  })

  it('rejects negative amount', () => {
    const result = transferSchema.safeParse({ ...validTransfer, amount: -50 })
    expect(result.success).toBe(false)
    expect(result.error?.issues[0].message).toBe(
      'Amount must be greater than zero',
    )
  })

  it('rejects amount exceeding maximum limit', () => {
    const result = transferSchema.safeParse({
      ...validTransfer,
      amount: 1_000_001,
    })
    expect(result.success).toBe(false)
    expect(result.error?.issues[0].message).toBe('Amount exceeds maximum limit')
  })

  it('accepts amount at maximum limit', () => {
    const result = transferSchema.safeParse({
      ...validTransfer,
      amount: 1_000_000,
    })
    expect(result.success).toBe(true)
  })

  it('rejects description over 200 characters', () => {
    const result = transferSchema.safeParse({
      ...validTransfer,
      description: 'a'.repeat(201),
    })
    expect(result.success).toBe(false)
    expect(result.error?.issues[0].message).toBe(
      'Description must be 200 characters or fewer',
    )
  })

  it('accepts description at exactly 200 characters', () => {
    const result = transferSchema.safeParse({
      ...validTransfer,
      description: 'a'.repeat(200),
    })
    expect(result.success).toBe(true)
  })
})
