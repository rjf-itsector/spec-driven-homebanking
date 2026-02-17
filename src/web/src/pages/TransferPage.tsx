import { TransferForm } from '@/components/features/transfers/TransferForm'

export function TransferPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Transfer</h1>
        <p className="text-muted-foreground mt-1">
          Move funds between your accounts.
        </p>
      </div>
      <TransferForm />
    </div>
  )
}
