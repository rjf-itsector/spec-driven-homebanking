import { AccountList } from '@/components/features/accounts/AccountList'
import { SpendingChart } from '@/components/features/spending/SpendingChart'

export function DashboardPage() {
  return (
    <div className="space-y-6" data-testid="dashboard">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Dashboard</h1>
        <p className="text-muted-foreground mt-2">
          Overview of your accounts and balances.
        </p>
      </div>
      <AccountList />
      <SpendingChart />
    </div>
  )
}
