import type { TransferRequest, TransferResponse } from './types'
import apiClient from './client'
import { handleApiError } from './errors'

export async function createTransfer(
  request: TransferRequest,
): Promise<TransferResponse> {
  return handleApiError(
    apiClient
      .post<TransferResponse>('/transfers', request)
      .then((res) => res.data),
  )
}

export async function getTransferById(id: string): Promise<TransferResponse> {
  return handleApiError(
    apiClient.get<TransferResponse>(`/transfers/${id}`).then((res) => res.data),
  )
}
