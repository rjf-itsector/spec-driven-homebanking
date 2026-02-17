import { renderHook, act } from '@testing-library/react'
import { vi, describe, it, expect, beforeEach } from 'vitest'
import type { ReactNode } from 'react'
import type { LoginResponse } from '@/lib/api/types'

vi.mock('@/lib/api/auth', () => ({
  login: vi.fn(),
}))

import { AuthProvider, useAuth } from '../useAuth'
import { login as apiLogin } from '@/lib/api/auth'

const wrapper = ({ children }: { children: ReactNode }) => (
  <AuthProvider>{children}</AuthProvider>
)

const mockUser = {
  id: 'u-1',
  email: 'test@example.com',
  firstName: 'John',
  lastName: 'Doe',
}

const mockLoginResponse: LoginResponse = {
  token: 'jwt-token-123',
  expiresIn: 3600,
  user: mockUser,
}

describe('useAuth', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.clearAllMocks()
  })

  it('throws when used outside AuthProvider', () => {
    expect(() => {
      renderHook(() => useAuth())
    }).toThrow('useAuth must be used within an AuthProvider')
  })

  it('provides unauthenticated state by default', () => {
    const { result } = renderHook(() => useAuth(), { wrapper })

    expect(result.current.user).toBeNull()
    expect(result.current.token).toBeNull()
    expect(result.current.isAuthenticated).toBe(false)
    expect(result.current.isLoading).toBe(false)
  })

  it('loads user from localStorage if available', () => {
    localStorage.setItem('hb_token', 'stored-token')
    localStorage.setItem('hb_user', JSON.stringify(mockUser))

    const { result } = renderHook(() => useAuth(), { wrapper })

    expect(result.current.user).toEqual(mockUser)
    expect(result.current.token).toBe('stored-token')
    expect(result.current.isAuthenticated).toBe(true)
  })

  it('login stores token and user in localStorage and updates state', async () => {
    vi.mocked(apiLogin).mockResolvedValue(mockLoginResponse)

    const { result } = renderHook(() => useAuth(), { wrapper })

    await act(async () => {
      await result.current.login({
        email: 'test@example.com',
        password: 'password123',
      })
    })

    expect(apiLogin).toHaveBeenCalledWith({
      email: 'test@example.com',
      password: 'password123',
    })
    expect(result.current.user).toEqual(mockUser)
    expect(result.current.token).toBe('jwt-token-123')
    expect(result.current.isAuthenticated).toBe(true)
    expect(localStorage.getItem('hb_token')).toBe('jwt-token-123')
    expect(localStorage.getItem('hb_user')).toBe(JSON.stringify(mockUser))
  })

  it('logout removes token and user from localStorage and resets state', async () => {
    vi.mocked(apiLogin).mockResolvedValue(mockLoginResponse)

    const { result } = renderHook(() => useAuth(), { wrapper })

    // Login first
    await act(async () => {
      await result.current.login({
        email: 'test@example.com',
        password: 'password123',
      })
    })

    expect(result.current.isAuthenticated).toBe(true)

    // Then logout
    act(() => {
      result.current.logout()
    })

    expect(result.current.user).toBeNull()
    expect(result.current.token).toBeNull()
    expect(result.current.isAuthenticated).toBe(false)
    expect(localStorage.getItem('hb_token')).toBeNull()
    expect(localStorage.getItem('hb_user')).toBeNull()
  })

  it('handles corrupt JSON in localStorage gracefully', () => {
    localStorage.setItem('hb_user', '{not valid json')
    localStorage.setItem('hb_token', 'some-token')

    const { result } = renderHook(() => useAuth(), { wrapper })

    expect(result.current.user).toBeNull()
    // loadUser clears storage on parse failure
    expect(localStorage.getItem('hb_user')).toBeNull()
    expect(localStorage.getItem('hb_token')).toBeNull()
  })
})
