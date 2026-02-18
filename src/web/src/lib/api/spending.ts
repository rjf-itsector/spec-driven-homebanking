import type { SpendingSummaryDto } from './types'
import apiClient from './client'
import { handleApiError } from './errors'

export type SpendingDirection = 'spending' | 'savings'

export async function getSpendingSummary(
  accountId?: string,
  direction: SpendingDirection = 'spending',
): Promise<SpendingSummaryDto> {
  const params: Record<string, string> = { direction }
  if (accountId) params.accountId = accountId

  return handleApiError(
    apiClient
      .get<SpendingSummaryDto>('/spending/summary', { params })
      .then((res) => res.data),
  )
}
