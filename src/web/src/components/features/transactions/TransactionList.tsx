import { useEffect, useRef, useCallback } from 'react'
import { Loader2, Inbox } from 'lucide-react'

import { useInfiniteTransactions } from './useInfiniteTransactions'
import { TransactionRow } from './TransactionRow'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Separator } from '@/components/ui/separator'
import { Skeleton } from '@/components/ui/skeleton'

interface TransactionListProps {
  accountId: string
  currency?: string
}

function TransactionRowSkeleton() {
  return (
    <div className="flex items-center gap-3 px-4 py-3">
      <Skeleton className="h-9 w-9 rounded-full shrink-0" />
      <div className="flex-1 space-y-1.5">
        <Skeleton className="h-4 w-40" />
        <Skeleton className="h-3 w-28" />
      </div>
      <div className="space-y-1.5 text-right">
        <Skeleton className="h-4 w-20 ml-auto" />
        <Skeleton className="h-3 w-16 ml-auto" />
      </div>
    </div>
  )
}

export function TransactionList({
  accountId,
  currency = 'EUR',
}: TransactionListProps) {
  const {
    data,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    isLoading,
    isError,
    error,
  } = useInfiniteTransactions(accountId)

  const sentinelRef = useRef<HTMLDivElement>(null)

  const handleIntersect = useCallback(
    (entries: IntersectionObserverEntry[]) => {
      const [entry] = entries
      if (entry.isIntersecting && hasNextPage && !isFetchingNextPage) {
        void fetchNextPage()
      }
    },
    [fetchNextPage, hasNextPage, isFetchingNextPage],
  )

  useEffect(() => {
    const sentinel = sentinelRef.current
    if (!sentinel) return

    const observer = new IntersectionObserver(handleIntersect, {
      rootMargin: '200px',
    })
    observer.observe(sentinel)

    return () => observer.disconnect()
  }, [handleIntersect])

  const transactions = data?.pages.flatMap((page) => page.transactions) ?? []

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Transactions</CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          {Array.from({ length: 5 }, (_, i) => (
            <div key={i}>
              {i > 0 && <Separator />}
              <TransactionRowSkeleton />
            </div>
          ))}
        </CardContent>
      </Card>
    )
  }

  if (isError) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Transactions</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-destructive">
            Failed to load transactions: {error.message}
          </p>
        </CardContent>
      </Card>
    )
  }

  if (transactions.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Transactions</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center py-12 text-muted-foreground">
            <Inbox className="h-12 w-12 mb-3" />
            <p className="text-sm font-medium">No transactions yet</p>
            <p className="text-xs mt-1">
              Transactions will appear here once activity occurs.
            </p>
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg">Transactions</CardTitle>
      </CardHeader>
      <CardContent className="p-0">
        {transactions.map((tx, index) => (
          <div key={tx.id}>
            {index > 0 && <Separator />}
            <TransactionRow transaction={tx} currency={currency} />
          </div>
        ))}

        {/* Sentinel for infinite scroll */}
        <div ref={sentinelRef} className="h-1" />

        {isFetchingNextPage && (
          <div className="flex items-center justify-center py-4">
            <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
            <span className="ml-2 text-sm text-muted-foreground">
              Loading more…
            </span>
          </div>
        )}

        {!hasNextPage && transactions.length > 0 && (
          <div className="py-4 text-center">
            <p className="text-xs text-muted-foreground">
              No more transactions
            </p>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
