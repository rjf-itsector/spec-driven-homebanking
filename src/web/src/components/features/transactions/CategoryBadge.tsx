import {
  Car,
  Film,
  Heart,
  MoreHorizontal,
  ShoppingBag,
  ShoppingCart,
  Utensils,
  Zap,
} from 'lucide-react'
import type { LucideIcon } from 'lucide-react'

import { Badge } from '@/components/ui/badge'
import { cn } from '@/lib/utils'

interface CategoryConfig {
  icon: LucideIcon
  label: string
  className: string
}

const CATEGORY_MAP: Record<string, CategoryConfig> = {
  Groceries: {
    icon: ShoppingCart,
    label: 'Groceries',
    className:
      'border-green-200 bg-green-50 text-green-700 dark:border-green-800 dark:bg-green-950 dark:text-green-400',
  },
  Dining: {
    icon: Utensils,
    label: 'Dining',
    className:
      'border-orange-200 bg-orange-50 text-orange-700 dark:border-orange-800 dark:bg-orange-950 dark:text-orange-400',
  },
  Transportation: {
    icon: Car,
    label: 'Transportation',
    className:
      'border-blue-200 bg-blue-50 text-blue-700 dark:border-blue-800 dark:bg-blue-950 dark:text-blue-400',
  },
  Shopping: {
    icon: ShoppingBag,
    label: 'Shopping',
    className:
      'border-purple-200 bg-purple-50 text-purple-700 dark:border-purple-800 dark:bg-purple-950 dark:text-purple-400',
  },
  Bills: {
    icon: Zap,
    label: 'Bills',
    className:
      'border-red-200 bg-red-50 text-red-700 dark:border-red-800 dark:bg-red-950 dark:text-red-400',
  },
  Healthcare: {
    icon: Heart,
    label: 'Healthcare',
    className:
      'border-pink-200 bg-pink-50 text-pink-700 dark:border-pink-800 dark:bg-pink-950 dark:text-pink-400',
  },
  Entertainment: {
    icon: Film,
    label: 'Entertainment',
    className:
      'border-yellow-200 bg-yellow-50 text-yellow-700 dark:border-yellow-800 dark:bg-yellow-950 dark:text-yellow-400',
  },
  Other: {
    icon: MoreHorizontal,
    label: 'Other',
    className:
      'border-gray-200 bg-gray-50 text-gray-700 dark:border-gray-800 dark:bg-gray-950 dark:text-gray-400',
  },
}

const DEFAULT_CONFIG: CategoryConfig = CATEGORY_MAP.Other

interface CategoryBadgeProps {
  category: string
  className?: string
}

export function CategoryBadge({ category, className }: CategoryBadgeProps) {
  const config = CATEGORY_MAP[category] ?? DEFAULT_CONFIG
  const Icon = config.icon

  return (
    <Badge
      variant="outline"
      aria-label={config.label}
      data-testid="category-badge"
      className={cn(
        'gap-1 px-1.5 py-0 text-[10px] font-medium leading-5',
        config.className,
        className,
      )}
    >
      <Icon className="h-3 w-3" />
      {config.label}
    </Badge>
  )
}
