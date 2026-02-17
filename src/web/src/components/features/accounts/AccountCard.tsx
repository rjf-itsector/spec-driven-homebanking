import { Link } from 'react-router-dom'
import { Building2, CreditCard, PiggyBank, Wallet } from 'lucide-react'

import type { AccountDto } from '@/lib/api/types'
import { formatCurrency, maskAccountNumber } from '@/lib/utils/format'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { cn } from '@/lib/utils'

const accountTypeConfig: Record<
  string,
  { label: string; icon: typeof Wallet }
> = {
  Checking: { label: 'Checking', icon: Wallet },
  Savings: { label: 'Savings', icon: PiggyBank },
  CreditCard: { label: 'Credit Card', icon: CreditCard },
  Investment: { label: 'Investment', icon: Building2 },
}

function getAccountTypeConfig(type: string) {
  return accountTypeConfig[type] ?? { label: type, icon: Wallet }
}

interface AccountCardProps {
  account: AccountDto
}

export function AccountCard({ account }: AccountCardProps) {
  const config = getAccountTypeConfig(account.type)
  const Icon = config.icon
  const isNegative = account.balance < 0

  return (
    <Link to={`/accounts/${account.id}`} className="block">
      <Card
        className="transition-colors hover:bg-accent/50 cursor-pointer"
        data-testid="account-card"
      >
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle className="text-sm font-medium">{config.label}</CardTitle>
          <Icon className="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div
            className={cn(
              'text-2xl font-bold',
              isNegative && 'text-destructive',
            )}
          >
            {formatCurrency(account.balance, account.currency)}
          </div>
          <p className="text-xs text-muted-foreground mt-1">
            {maskAccountNumber(account.accountNumber)}
          </p>
        </CardContent>
      </Card>
    </Link>
  )
}
