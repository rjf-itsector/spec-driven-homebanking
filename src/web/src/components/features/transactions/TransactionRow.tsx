import { ArrowDownLeft, ArrowUpRight } from 'lucide-react'

import type { TransactionDto } from '@/lib/api/types'
import { formatCurrency, formatDate } from '@/lib/utils/format'
import { cn } from '@/lib/utils'

interface TransactionRowProps {
  transaction: TransactionDto
  currency?: string
}

export function TransactionRow({
  transaction,
  currency = 'EUR',
}: TransactionRowProps) {
  const isCredit = transaction.type === 'Credit'
  const Icon = isCredit ? ArrowDownLeft : ArrowUpRight

  return (
    <div className="flex items-center gap-3 px-4 py-3 hover:bg-accent/50 transition-colors">
      <div
        className={cn(
          'flex h-9 w-9 shrink-0 items-center justify-center rounded-full',
          isCredit
            ? 'bg-green-100 text-green-700 dark:bg-green-950 dark:text-green-400'
            : 'bg-red-100 text-red-700 dark:bg-red-950 dark:text-red-400',
        )}
      >
        <Icon className="h-4 w-4" />
      </div>

      <div className="flex-1 min-w-0">
        <p className="text-sm font-medium truncate">
          {transaction.description}
        </p>
        <p className="text-xs text-muted-foreground">
          {transaction.category} &middot; {formatDate(transaction.timestamp)}
        </p>
      </div>

      <div className="text-right shrink-0">
        <p
          className={cn(
            'text-sm font-semibold',
            isCredit
              ? 'text-green-700 dark:text-green-400'
              : 'text-red-700 dark:text-red-400',
          )}
        >
          {isCredit ? '+' : '-'}
          {formatCurrency(Math.abs(transaction.amount), currency)}
        </p>
        <p className="text-xs text-muted-foreground">
          {formatCurrency(transaction.balanceAfter, currency)}
        </p>
      </div>
    </div>
  )
}
