import { useState, useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import {
  PieChart,
  Pie,
  Cell,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from 'recharts'
import {
  ArrowDownLeft,
  ArrowUpRight,
  Car,
  Film,
  Heart,
  MoreHorizontal,
  PieChart as PieChartIcon,
  ShoppingBag,
  ShoppingCart,
  Utensils,
  Zap,
} from 'lucide-react'
import type { LucideIcon } from 'lucide-react'

import { getSpendingSummary } from '@/lib/api/spending'
import type { SpendingDirection } from '@/lib/api/spending'
import { getAccounts } from '@/lib/api/accounts'
import { formatCurrency } from '@/lib/utils/format'
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
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'

const CATEGORY_COLORS: Record<string, string> = {
  Groceries: '#22c55e',
  Dining: '#f97316',
  Transportation: '#3b82f6',
  Shopping: '#a855f7',
  Bills: '#ef4444',
  Healthcare: '#ec4899',
  Entertainment: '#eab308',
  Other: '#6b7280',
}

const CATEGORY_ICONS: Record<string, LucideIcon> = {
  Groceries: ShoppingCart,
  Dining: Utensils,
  Transportation: Car,
  Shopping: ShoppingBag,
  Bills: Zap,
  Healthcare: Heart,
  Entertainment: Film,
  Other: MoreHorizontal,
}

function getColor(category: string) {
  return CATEGORY_COLORS[category] ?? '#6b7280'
}

function getIcon(category: string) {
  return CATEGORY_ICONS[category] ?? MoreHorizontal
}

interface CustomTooltipProps {
  active?: boolean
  payload?: Array<{
    payload: {
      category: string
      total: number
      percentage: number
      transactionCount: number
    }
  }>
}

function CustomTooltip({ active, payload }: CustomTooltipProps) {
  if (!active || !payload?.length) return null
  const data = payload[0].payload

  return (
    <div className="rounded-lg border bg-popover px-3 py-2 text-sm shadow-md">
      <p className="font-medium">{data.category}</p>
      <p className="text-muted-foreground">
        {formatCurrency(data.total)} ({data.percentage}%)
      </p>
      <p className="text-muted-foreground">
        {data.transactionCount} transaction
        {data.transactionCount !== 1 ? 's' : ''}
      </p>
    </div>
  )
}

function ChartSkeleton() {
  return (
    <Card>
      <CardHeader>
        <Skeleton className="h-5 w-40" />
        <Skeleton className="mt-1 h-4 w-56" />
      </CardHeader>
      <CardContent>
        <div className="flex flex-col items-center gap-6 lg:flex-row">
          <Skeleton className="h-[250px] w-[250px] rounded-full" />
          <div className="flex-1 space-y-3">
            {Array.from({ length: 5 }).map((_, i) => (
              <Skeleton key={i} className="h-8 w-full" />
            ))}
          </div>
        </div>
      </CardContent>
    </Card>
  )
}

const DIRECTION_CONFIG: Record<
  SpendingDirection,
  { label: string; icon: LucideIcon; emptyLabel: string; totalLabel: string }
> = {
  spending: {
    label: 'Spending',
    icon: ArrowUpRight,
    emptyLabel: 'No spending data for the selected period.',
    totalLabel: 'total spent',
  },
  savings: {
    label: 'Savings',
    icon: ArrowDownLeft,
    emptyLabel: 'No savings data for the selected period.',
    totalLabel: 'total saved',
  },
}

export function SpendingChart() {
  const [selectedAccount, setSelectedAccount] = useState<string>('all')
  const [direction, setDirection] = useState<SpendingDirection>('spending')

  const { data: accounts } = useQuery({
    queryKey: ['accounts'],
    queryFn: getAccounts,
  })

  const accountId = selectedAccount === 'all' ? undefined : selectedAccount

  const {
    data: spending,
    isLoading,
    isError,
    error,
  } = useQuery({
    queryKey: ['spending-summary', accountId, direction],
    queryFn: () => getSpendingSummary(accountId, direction),
  })

  const chartData = useMemo(() => {
    if (!spending?.categories) return []
    return spending.categories.map((c) => ({
      category: c.category,
      total: c.total,
      percentage: c.percentage,
      transactionCount: c.transactionCount,
    }))
  }, [spending])

  const dirConfig = DIRECTION_CONFIG[direction]

  if (isLoading) {
    return <ChartSkeleton />
  }

  if (isError) {
    return (
      <Card>
        <CardContent className="p-6">
          <p className="text-sm text-destructive">
            Failed to load data:{' '}
            {error instanceof Error ? error.message : 'Unknown error'}
          </p>
        </CardContent>
      </Card>
    )
  }

  const hasData = chartData.length > 0

  return (
    <Card className="animate-in fade-in slide-in-from-bottom-4 duration-500">
      <CardHeader>
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex items-center gap-3">
            <PieChartIcon className="h-5 w-5 text-muted-foreground" />
            <div>
              <CardTitle className="text-lg">
                {dirConfig.label} by Category
              </CardTitle>
              <CardDescription>
                Last 30 days &middot;{' '}
                {spending ? formatCurrency(spending.totalSpending) : '—'}{' '}
                {dirConfig.totalLabel}
              </CardDescription>
            </div>
          </div>

          <div className="flex items-center gap-2">
            {/* Direction toggle */}
            <div
              className="relative flex items-center rounded-lg border bg-muted p-0.5"
              data-testid="direction-toggle"
            >
              {(
                Object.entries(DIRECTION_CONFIG) as [
                  SpendingDirection,
                  (typeof DIRECTION_CONFIG)[SpendingDirection],
                ][]
              ).map(([key, cfg]) => {
                const Icon = cfg.icon
                const isActive = direction === key
                return (
                  <button
                    key={key}
                    onClick={() => setDirection(key)}
                    className={cn(
                      'relative z-10 flex items-center gap-1.5 rounded-md px-3 py-1.5 text-xs font-medium transition-all duration-200',
                      isActive
                        ? 'bg-background text-foreground shadow-sm'
                        : 'text-muted-foreground hover:text-foreground',
                    )}
                    data-testid={`direction-${key}`}
                  >
                    <Icon className="h-3.5 w-3.5" />
                    {cfg.label}
                  </button>
                )
              })}
            </div>

            {/* Account filter */}
            <Select value={selectedAccount} onValueChange={setSelectedAccount}>
              <SelectTrigger className="w-[200px]" data-testid="account-filter">
                <SelectValue placeholder="All accounts" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All accounts</SelectItem>
                {accounts?.map((account) => (
                  <SelectItem key={account.id} value={account.id}>
                    {account.type} (****{account.accountNumber.slice(-4)})
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
      </CardHeader>

      <CardContent>
        {!hasData ? (
          <div className="flex flex-col items-center justify-center py-12 text-center">
            <PieChartIcon className="mb-3 h-12 w-12 text-muted-foreground/40" />
            <p className="text-sm text-muted-foreground">
              {dirConfig.emptyLabel}
            </p>
          </div>
        ) : (
          <div className="flex flex-col items-center gap-6 lg:flex-row">
            {/* Pie Chart */}
            <div
              key={direction}
              className="h-[280px] w-full max-w-[320px] animate-in zoom-in-75 fade-in duration-500"
            >
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={chartData}
                    dataKey="total"
                    nameKey="category"
                    cx="50%"
                    cy="50%"
                    outerRadius={110}
                    innerRadius={60}
                    paddingAngle={2}
                    animationBegin={0}
                    animationDuration={800}
                    animationEasing="ease-out"
                    stroke="hsl(var(--background))"
                    strokeWidth={2}
                  >
                    {chartData.map((entry) => (
                      <Cell
                        key={entry.category}
                        fill={getColor(entry.category)}
                        className="transition-opacity hover:opacity-80"
                      />
                    ))}
                  </Pie>
                  <Tooltip content={<CustomTooltip />} />
                  <Legend
                    verticalAlign="bottom"
                    height={36}
                    formatter={(value: string) => (
                      <span className="text-xs text-foreground">{value}</span>
                    )}
                  />
                </PieChart>
              </ResponsiveContainer>
            </div>

            {/* Category breakdown list */}
            <div key={`list-${direction}`} className="flex-1 space-y-2 w-full">
              {chartData.map((entry, index) => {
                const Icon = getIcon(entry.category)
                return (
                  <div
                    key={entry.category}
                    className={cn(
                      'flex items-center gap-3 rounded-lg p-2.5 transition-colors hover:bg-accent/50',
                      'animate-in fade-in slide-in-from-right-4',
                    )}
                    style={{
                      animationDelay: `${index * 80}ms`,
                      animationFillMode: 'both',
                    }}
                    data-testid="category-row"
                  >
                    <div
                      className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full"
                      style={{
                        backgroundColor: `${getColor(entry.category)}20`,
                      }}
                    >
                      <Icon
                        className="h-4 w-4"
                        style={{ color: getColor(entry.category) }}
                      />
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center justify-between">
                        <span className="text-sm font-medium">
                          {entry.category}
                        </span>
                        <span className="text-sm font-semibold tabular-nums">
                          {formatCurrency(entry.total)}
                        </span>
                      </div>
                      <div className="mt-1 flex items-center gap-2">
                        <div className="h-1.5 flex-1 rounded-full bg-muted overflow-hidden">
                          <div
                            className="h-full rounded-full transition-all duration-700 ease-out"
                            style={{
                              width: `${entry.percentage}%`,
                              backgroundColor: getColor(entry.category),
                            }}
                          />
                        </div>
                        <span className="text-xs text-muted-foreground tabular-nums w-10 text-right">
                          {entry.percentage}%
                        </span>
                      </div>
                    </div>
                  </div>
                )
              })}

              {spending && (
                <div
                  className="mt-3 flex items-center justify-between border-t pt-3 text-sm animate-in fade-in duration-500"
                  style={{
                    animationDelay: `${chartData.length * 80 + 100}ms`,
                    animationFillMode: 'both',
                  }}
                >
                  <span className="text-muted-foreground">Daily average</span>
                  <span className="font-semibold tabular-nums">
                    {formatCurrency(spending.averageDaily)}
                  </span>
                </div>
              )}
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
