import type { TransactionDto, PaginatedResponse } from './types'
import apiClient from './client'
import { handleApiError } from './errors'

export async function getTransactions(
  accountId: string,
  cursor?: string,
  limit?: number,
): Promise<PaginatedResponse<TransactionDto>> {
  const params: Record<string, string | number> = {}
  if (cursor) params.cursor = cursor
  if (limit) params.limit = limit

  return handleApiError(
    apiClient
      .get<
        PaginatedResponse<TransactionDto>
      >(`/accounts/${accountId}/transactions`, { params })
      .then((res) => res.data),
  )
}

export async function getTransactionById(id: string): Promise<TransactionDto> {
  return handleApiError(
    apiClient
      .get<TransactionDto>(`/transactions/${id}`)
      .then((res) => res.data),
  )
}
