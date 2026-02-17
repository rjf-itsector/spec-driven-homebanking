import { describe, it, expect } from 'vitest'
import axios from 'axios'
import { ApiError, handleApiError } from '../errors'

describe('ApiError', () => {
  it('constructor sets name, message, status, errors, and originalError', () => {
    const fieldErrors = { email: ['Required'] }
    const original = new Error('orig')
    const err = new ApiError('Bad request', 400, fieldErrors, original)

    expect(err).toBeInstanceOf(Error)
    expect(err.name).toBe('ApiError')
    expect(err.message).toBe('Bad request')
    expect(err.status).toBe(400)
    expect(err.errors).toEqual({ email: ['Required'] })
    expect(err.originalError).toBe(original)
  })

  describe('fromAxiosError', () => {
    it('handles AxiosError with response data', () => {
      const axiosErr = new axios.AxiosError(
        'Request failed',
        '400',
        undefined,
        undefined,
        {
          status: 422,
          data: {
            message: 'Validation failed',
            errors: { email: ['Invalid email'] },
          },
          statusText: 'Unprocessable Entity',
          headers: {},
          config: {} as never,
        },
      )

      const apiErr = ApiError.fromAxiosError(axiosErr)

      expect(apiErr).toBeInstanceOf(ApiError)
      expect(apiErr.message).toBe('Validation failed')
      expect(apiErr.status).toBe(422)
      expect(apiErr.errors).toEqual({ email: ['Invalid email'] })
      expect(apiErr.originalError).toBe(axiosErr)
    })

    it('handles AxiosError without response data', () => {
      const axiosErr = new axios.AxiosError('Network Error', 'ERR_NETWORK')

      const apiErr = ApiError.fromAxiosError(axiosErr)

      expect(apiErr).toBeInstanceOf(ApiError)
      expect(apiErr.message).toBe('Network Error')
      expect(apiErr.status).toBe(0)
      expect(apiErr.errors).toBeUndefined()
      expect(apiErr.originalError).toBe(axiosErr)
    })

    it('handles plain Error', () => {
      const plainErr = new TypeError('Something broke')

      const apiErr = ApiError.fromAxiosError(plainErr)

      expect(apiErr).toBeInstanceOf(ApiError)
      expect(apiErr.message).toBe('Something broke')
      expect(apiErr.status).toBe(0)
      expect(apiErr.errors).toBeUndefined()
      expect(apiErr.originalError).toBe(plainErr)
    })

    it('handles non-Error values', () => {
      const apiErr1 = ApiError.fromAxiosError('string error')
      expect(apiErr1.message).toBe('An unexpected error occurred')
      expect(apiErr1.status).toBe(0)
      expect(apiErr1.originalError).toBe('string error')

      const apiErr2 = ApiError.fromAxiosError(null)
      expect(apiErr2.message).toBe('An unexpected error occurred')
      expect(apiErr2.originalError).toBeNull()
    })
  })
})

describe('handleApiError', () => {
  it('returns value from resolved promise', async () => {
    const result = await handleApiError(Promise.resolve('hello'))
    expect(result).toBe('hello')
  })

  it('throws ApiError from rejected promise', async () => {
    const axiosErr = new axios.AxiosError('fail', '500', undefined, undefined, {
      status: 500,
      data: { message: 'Internal Server Error' },
      statusText: 'Internal Server Error',
      headers: {},
      config: {} as never,
    })

    await expect(handleApiError(Promise.reject(axiosErr))).rejects.toThrow(
      ApiError,
    )
    await expect(
      handleApiError(Promise.reject(axiosErr)),
    ).rejects.toMatchObject({
      message: 'Internal Server Error',
      status: 500,
    })
  })
})
