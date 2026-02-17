import type { AccountDto, AccountDetailDto } from './types'
import apiClient from './client'
import { handleApiError } from './errors'

export async function getAccounts(): Promise<AccountDto[]> {
  return handleApiError(
    apiClient.get<AccountDto[]>('/accounts').then((res) => res.data),
  )
}

export async function getAccountById(id: string): Promise<AccountDetailDto> {
  return handleApiError(
    apiClient.get<AccountDetailDto>(`/accounts/${id}`).then((res) => res.data),
  )
}
