import { renderHook, act, waitFor } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useChatAgent } from '../useChatAgent'
import { sendAgentMessage } from '@/lib/api/agent'
import axios from 'axios'

vi.mock('@/lib/api/agent', () => ({
  sendAgentMessage: vi.fn(),
}))

describe('useChatAgent', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('initially: isOpen false, messages empty, isLoading false, error null', () => {
    const { result } = renderHook(() => useChatAgent())
    expect(result.current.isOpen).toBe(false)
    expect(result.current.messages).toEqual([])
    expect(result.current.isLoading).toBe(false)
    expect(result.current.error).toBeNull()
  })

  it('toggleOpen: flips isOpen', () => {
    const { result } = renderHook(() => useChatAgent())
    expect(result.current.isOpen).toBe(false)

    act(() => {
      result.current.toggleOpen()
    })
    expect(result.current.isOpen).toBe(true)

    act(() => {
      result.current.toggleOpen()
    })
    expect(result.current.isOpen).toBe(false)
  })

  it('clearChat: empties messages and clears error', async () => {
    vi.mocked(sendAgentMessage).mockResolvedValueOnce({
      response: 'Hi there',
      toolsUsed: [],
      tokenUsage: { input: 10, output: 20 },
    })

    const { result } = renderHook(() => useChatAgent())

    await act(async () => {
      await result.current.sendMessage('Hello')
    })
    expect(result.current.messages.length).toBeGreaterThan(0)

    act(() => {
      result.current.clearChat()
    })
    expect(result.current.messages).toEqual([])
    expect(result.current.error).toBeNull()
  })

  it('sendMessage: appends user message, calls API, appends assistant response', async () => {
    vi.mocked(sendAgentMessage).mockResolvedValueOnce({
      response: 'I can help with that!',
      toolsUsed: ['getBalance'],
      tokenUsage: { input: 10, output: 20 },
    })

    const { result } = renderHook(() => useChatAgent())

    await act(async () => {
      await result.current.sendMessage('What is my balance?')
    })

    expect(result.current.messages).toHaveLength(2)
    expect(result.current.messages[0].role).toBe('user')
    expect(result.current.messages[0].content).toBe('What is my balance?')
    expect(result.current.messages[1].role).toBe('assistant')
    expect(result.current.messages[1].content).toBe('I can help with that!')
    expect(result.current.messages[1].toolsUsed).toEqual(['getBalance'])
    expect(sendAgentMessage).toHaveBeenCalledOnce()
  })

  it('sendMessage: sets isLoading during request', async () => {
    let resolvePromise: (value: unknown) => void
    const promise = new Promise((resolve) => {
      resolvePromise = resolve
    })
    vi.mocked(sendAgentMessage).mockReturnValueOnce(
      promise as ReturnType<typeof sendAgentMessage>,
    )

    const { result } = renderHook(() => useChatAgent())

    let sendPromise: Promise<void>
    act(() => {
      sendPromise = result.current.sendMessage('Hello')
    })

    await waitFor(() => {
      expect(result.current.isLoading).toBe(true)
    })

    await act(async () => {
      resolvePromise!({
        response: 'Hi',
        toolsUsed: [],
        tokenUsage: { input: 5, output: 5 },
      })
      await sendPromise!
    })

    expect(result.current.isLoading).toBe(false)
  })

  it('sendMessage: handles 503 error', async () => {
    const axiosError = new axios.AxiosError(
      'Service Unavailable',
      '503',
      undefined,
      undefined,
      {
        status: 503,
        data: { message: 'Agent unavailable' },
        statusText: 'Service Unavailable',
        headers: {},
        config: {} as ReturnType<typeof axios.AxiosError.prototype.toJSON> &
          Record<string, unknown>,
      } as unknown as import('axios').AxiosResponse,
    )
    vi.mocked(sendAgentMessage).mockRejectedValueOnce(axiosError)

    const { result } = renderHook(() => useChatAgent())

    await act(async () => {
      await result.current.sendMessage('Hello')
    })

    expect(result.current.error).toBe(
      'Agent is temporarily unavailable. Please try again later.',
    )
    expect(result.current.isLoading).toBe(false)
  })

  it('sendMessage: handles 429 error', async () => {
    const axiosError = new axios.AxiosError(
      'Too Many Requests',
      '429',
      undefined,
      undefined,
      {
        status: 429,
        data: { message: 'Rate limited' },
        statusText: 'Too Many Requests',
        headers: {},
        config: {} as ReturnType<typeof axios.AxiosError.prototype.toJSON> &
          Record<string, unknown>,
      } as unknown as import('axios').AxiosResponse,
    )
    vi.mocked(sendAgentMessage).mockRejectedValueOnce(axiosError)

    const { result } = renderHook(() => useChatAgent())

    await act(async () => {
      await result.current.sendMessage('Hello')
    })

    expect(result.current.error).toBe(
      'Too many requests. Please wait a moment and try again.',
    )
    expect(result.current.isLoading).toBe(false)
  })

  it('sendMessage: ignores empty string', async () => {
    const { result } = renderHook(() => useChatAgent())

    await act(async () => {
      await result.current.sendMessage('')
    })

    expect(result.current.messages).toEqual([])
    expect(sendAgentMessage).not.toHaveBeenCalled()
  })

  it('sendMessage: ignores text > 500 chars', async () => {
    const { result } = renderHook(() => useChatAgent())
    const longText = 'a'.repeat(501)

    await act(async () => {
      await result.current.sendMessage(longText)
    })

    expect(result.current.messages).toEqual([])
    expect(sendAgentMessage).not.toHaveBeenCalled()
  })
})
