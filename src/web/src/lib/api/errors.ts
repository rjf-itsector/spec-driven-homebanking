import axios from 'axios'
import type { ApiErrorResponse } from './types'

export class ApiError extends Error {
  public readonly status: number
  public readonly errors?: Record<string, string[]>
  public readonly originalError: unknown

  constructor(
    message: string,
    status: number,
    errors?: Record<string, string[]>,
    originalError?: unknown,
  ) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.errors = errors
    this.originalError = originalError
  }

  static fromAxiosError(error: unknown): ApiError {
    if (axios.isAxiosError(error)) {
      const data = error.response?.data as ApiErrorResponse | undefined
      const status = error.response?.status ?? 0
      const message =
        data?.message ?? error.message ?? 'An unexpected error occurred'
      const errors = data?.errors

      return new ApiError(message, status, errors, error)
    }

    if (error instanceof Error) {
      return new ApiError(error.message, 0, undefined, error)
    }

    return new ApiError('An unexpected error occurred', 0, undefined, error)
  }
}

export async function handleApiError<T>(promise: Promise<T>): Promise<T> {
  try {
    return await promise
  } catch (error: unknown) {
    throw ApiError.fromAxiosError(error)
  }
}
