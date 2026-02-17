import { render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import { CategoryBadge } from '../CategoryBadge'

const CATEGORIES = [
  'Groceries',
  'Dining',
  'Transportation',
  'Shopping',
  'Bills',
  'Healthcare',
  'Entertainment',
  'Other',
] as const

describe('CategoryBadge', () => {
  it.each(CATEGORIES)('renders label for %s category', (category) => {
    render(<CategoryBadge category={category} />)
    expect(screen.getByText(category)).toBeInTheDocument()
  })

  it.each(CATEGORIES)('has correct aria-label for %s category', (category) => {
    render(<CategoryBadge category={category} />)
    expect(screen.getByLabelText(category)).toBeInTheDocument()
  })

  it('falls back to Other for an unknown category', () => {
    render(<CategoryBadge category="UnknownCategory" />)
    expect(screen.getByText('Other')).toBeInTheDocument()
    expect(screen.getByLabelText('Other')).toBeInTheDocument()
  })

  it('falls back to Other for an empty string category', () => {
    render(<CategoryBadge category="" />)
    expect(screen.getByText('Other')).toBeInTheDocument()
  })

  it('accepts an additional className prop', () => {
    const { container } = render(
      <CategoryBadge category="Groceries" className="extra-class" />,
    )
    expect(container.firstChild).toHaveClass('extra-class')
  })
})
