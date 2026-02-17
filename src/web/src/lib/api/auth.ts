import type { LoginRequest, LoginResponse } from './types'
import apiClient from './client'
import { handleApiError } from './errors'

export async function login(credentials: LoginRequest): Promise<LoginResponse> {
  return handleApiError(
    apiClient
      .post<LoginResponse>('/auth/login', credentials)
      .then((res) => res.data),
  )
}
