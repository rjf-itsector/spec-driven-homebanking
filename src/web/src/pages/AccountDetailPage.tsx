import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ArrowLeft } from 'lucide-react'

import { getAccountById } from '@/lib/api/accounts'
import { formatCurrency, maskAccountNumber } from '@/lib/utils/format'
import { TransactionList } from '@/components/features/transactions/TransactionList'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'

function AccountDetailSkeleton() {
  return (
    <Card>
      <CardHeader>
        <Skeleton className="h-5 w-32" />
      </CardHeader>
      <CardContent className="space-y-2">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-4 w-24" />
      </CardContent>
    </Card>
  )
}

export function AccountDetailPage() {
  const { accountId } = useParams<{ accountId: string }>()

  const {
    data: account,
    isLoading,
    isError,
    error,
  } = useQuery({
    queryKey: ['account', accountId],
    queryFn: () => getAccountById(accountId!),
    enabled: !!accountId,
  })

  return (
    <div className="space-y-6">
      <div>
        <Button variant="ghost" size="sm" asChild className="-ml-2">
          <Link to="/dashboard">
            <ArrowLeft className="mr-1 h-4 w-4" />
            Back to Dashboard
          </Link>
        </Button>
      </div>

      {isLoading && <AccountDetailSkeleton />}

      {isError && (
        <Card>
          <CardContent className="py-6">
            <p className="text-sm text-destructive">
              Failed to load account: {error.message}
            </p>
          </CardContent>
        </Card>
      )}

      {account && (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">{account.type} Account</CardTitle>
          </CardHeader>
          <CardContent className="space-y-1">
            <div
              className={cn(
                'text-3xl font-bold',
                account.balance < 0 && 'text-destructive',
              )}
            >
              {formatCurrency(account.balance, account.currency)}
            </div>
            <p className="text-sm text-muted-foreground">
              {maskAccountNumber(account.accountNumber)}
            </p>
            <p className="text-xs text-muted-foreground">
              {account.transactionCount} transaction
              {account.transactionCount !== 1 ? 's' : ''}
            </p>
          </CardContent>
        </Card>
      )}

      {accountId && (
        <TransactionList accountId={accountId} currency={account?.currency} />
      )}
    </div>
  )
}
