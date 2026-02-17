import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { ArrowRightLeft, Loader2 } from 'lucide-react'

import type { AccountDto } from '@/lib/api/types'
import { getAccounts } from '@/lib/api/accounts'
import { createTransfer } from '@/lib/api/transfers'
import { ApiError } from '@/lib/api/errors'
import { formatCurrency, maskAccountNumber } from '@/lib/utils/format'
import { transferSchema, type TransferFormData } from '@/lib/utils/validation'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

function accountLabel(account: AccountDto): string {
  return `${account.type} — ${maskAccountNumber(account.accountNumber)} (${formatCurrency(account.balance, account.currency)})`
}

export function TransferForm() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [apiError, setApiError] = useState<string | null>(null)

  const { data: accounts = [], isLoading: accountsLoading } = useQuery<
    AccountDto[]
  >({
    queryKey: ['accounts'],
    queryFn: getAccounts,
  })

  const {
    register,
    handleSubmit,
    control,
    reset,
    watch,
    formState: { errors },
  } = useForm<TransferFormData>({
    resolver: zodResolver(transferSchema),
    defaultValues: {
      fromAccountId: '',
      toAccountId: '',
      amount: undefined as unknown as number,
      description: '',
    },
  })

  const selectedFromId = watch('fromAccountId')
  const selectedFrom = accounts.find((a) => a.id === selectedFromId)

  const mutation = useMutation({
    mutationFn: createTransfer,
    onMutate: async (request) => {
      // Cancel outgoing account queries to avoid overwrite
      await queryClient.cancelQueries({ queryKey: ['accounts'] })

      // Snapshot previous accounts for rollback
      const previousAccounts = queryClient.getQueryData<AccountDto[]>([
        'accounts',
      ])

      // Optimistically update account balances
      queryClient.setQueryData<AccountDto[]>(['accounts'], (old) => {
        if (!old) return old
        return old.map((account) => {
          if (account.id === request.fromAccountId) {
            return { ...account, balance: account.balance - request.amount }
          }
          if (account.id === request.toAccountId) {
            return { ...account, balance: account.balance + request.amount }
          }
          return account
        })
      })

      // Show immediate success toast
      toast.success('Transfer completed', {
        description: 'Your funds have been transferred successfully.',
      })

      return { previousAccounts }
    },
    onSuccess: () => {
      reset()
      navigate('/dashboard')
    },
    onError: (error: unknown, _variables, context) => {
      // Rollback optimistic update
      if (context?.previousAccounts) {
        queryClient.setQueryData(['accounts'], context.previousAccounts)
      }

      if (error instanceof ApiError) {
        setApiError(error.message)
      } else {
        setApiError('An unexpected error occurred. Please try again.')
      }

      toast.error('Transfer failed', {
        description:
          error instanceof ApiError ? error.message : 'Please try again later.',
      })
    },
    onSettled: () => {
      // Refetch to ensure server consistency
      queryClient.invalidateQueries({ queryKey: ['accounts'] })
      queryClient.invalidateQueries({ queryKey: ['transactions'] })
    },
  })

  const onSubmit = (data: TransferFormData) => {
    setApiError(null)

    // Client-side same-account check
    if (data.fromAccountId === data.toAccountId) {
      setApiError('Cannot transfer to the same account.')
      return
    }

    // Client-side balance check
    if (selectedFrom && data.amount > selectedFrom.balance) {
      setApiError('Insufficient balance in the source account.')
      return
    }

    mutation.mutate({
      fromAccountId: data.fromAccountId,
      toAccountId: data.toAccountId,
      amount: data.amount,
      description: data.description ?? '',
    })
  }

  const activeAccounts = accounts.filter((a) => a.isActive)

  return (
    <Card className="mx-auto max-w-lg" data-testid="transfer-form">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <ArrowRightLeft className="h-5 w-5" />
          New Transfer
        </CardTitle>
        <CardDescription>Transfer funds between your accounts.</CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
          {apiError && (
            <div
              role="alert"
              className="rounded-md border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive"
            >
              {apiError}
            </div>
          )}

          {/* From Account */}
          <div className="space-y-2">
            <Label htmlFor="fromAccountId">From Account</Label>
            <Controller
              control={control}
              name="fromAccountId"
              render={({ field }) => (
                <Select
                  value={field.value}
                  onValueChange={field.onChange}
                  disabled={accountsLoading}
                >
                  <SelectTrigger id="fromAccountId" data-testid="from-account">
                    <SelectValue placeholder="Select source account" />
                  </SelectTrigger>
                  <SelectContent>
                    {activeAccounts.map((account) => (
                      <SelectItem key={account.id} value={account.id}>
                        {accountLabel(account)}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
            {errors.fromAccountId && (
              <p className="text-sm text-destructive">
                {errors.fromAccountId.message}
              </p>
            )}
          </div>

          {/* To Account */}
          <div className="space-y-2">
            <Label htmlFor="toAccountId">To Account</Label>
            <Controller
              control={control}
              name="toAccountId"
              render={({ field }) => (
                <Select
                  value={field.value}
                  onValueChange={field.onChange}
                  disabled={accountsLoading}
                >
                  <SelectTrigger id="toAccountId" data-testid="to-account">
                    <SelectValue placeholder="Select destination account" />
                  </SelectTrigger>
                  <SelectContent>
                    {activeAccounts.map((account) => (
                      <SelectItem key={account.id} value={account.id}>
                        {accountLabel(account)}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
            {errors.toAccountId && (
              <p className="text-sm text-destructive">
                {errors.toAccountId.message}
              </p>
            )}
          </div>

          {/* Amount */}
          <div className="space-y-2">
            <Label htmlFor="amount">Amount (EUR)</Label>
            <Input
              id="amount"
              type="number"
              step="0.01"
              min="0.01"
              placeholder="0.00"
              data-testid="amount"
              {...register('amount', { valueAsNumber: true })}
            />
            {errors.amount && (
              <p className="text-sm text-destructive">
                {errors.amount.message}
              </p>
            )}
            {selectedFrom && (
              <p className="text-xs text-muted-foreground">
                Available balance:{' '}
                {formatCurrency(selectedFrom.balance, selectedFrom.currency)}
              </p>
            )}
          </div>

          {/* Description */}
          <div className="space-y-2">
            <Label htmlFor="description">
              Description{' '}
              <span className="font-normal text-muted-foreground">
                (optional, max 200 chars)
              </span>
            </Label>
            <Textarea
              id="description"
              placeholder="e.g. Rent payment"
              maxLength={200}
              rows={3}
              data-testid="description"
              {...register('description')}
            />
            {errors.description && (
              <p className="text-sm text-destructive">
                {errors.description.message}
              </p>
            )}
          </div>

          {/* Submit */}
          <Button
            type="submit"
            className="w-full"
            disabled={mutation.isPending}
            data-testid="transfer-submit"
          >
            {mutation.isPending ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Transferring…
              </>
            ) : (
              'Transfer Funds'
            )}
          </Button>
        </form>
      </CardContent>
    </Card>
  )
}
