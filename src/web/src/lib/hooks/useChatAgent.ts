import { useState, useCallback } from 'react'
import { sendAgentMessage } from '@/lib/api/agent'
import type { ChatMessage } from '@/lib/api/types'
import axios from 'axios'

export function useChatAgent() {
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [isOpen, setIsOpen] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const toggleOpen = useCallback(() => {
    setIsOpen((prev) => !prev)
  }, [])

  const clearChat = useCallback(() => {
    setMessages([])
    setError(null)
  }, [])

  const sendMessage = useCallback(
    async (text: string) => {
      const trimmed = text.trim()
      if (!trimmed || trimmed.length > 500) return

      setError(null)

      const userMessage: ChatMessage = {
        id: crypto.randomUUID(),
        role: 'user',
        content: trimmed,
        timestamp: new Date(),
      }

      setMessages((prev) => [...prev, userMessage])
      setIsLoading(true)

      try {
        // Build conversation history from last 20 messages
        const history = [...messages, userMessage]
          .slice(-20)
          .map((m) => ({ role: m.role, content: m.content }))

        const response = await sendAgentMessage({
          message: trimmed,
          conversationHistory: history.slice(0, -1), // Exclude the current message from history
        })

        const assistantMessage: ChatMessage = {
          id: crypto.randomUUID(),
          role: 'assistant',
          content: response.response,
          timestamp: new Date(),
          toolsUsed: response.toolsUsed,
        }

        setMessages((prev) => [...prev, assistantMessage])
      } catch (err: unknown) {
        if (axios.isAxiosError(err)) {
          if (err.response?.status === 429) {
            setError('Too many requests. Please wait a moment and try again.')
          } else if (err.response?.status === 503) {
            setError(
              'Agent is temporarily unavailable. Please try again later.',
            )
          } else {
            setError('Something went wrong. Please try again.')
          }
        } else {
          setError('Something went wrong. Please try again.')
        }
      } finally {
        setIsLoading(false)
      }
    },
    [messages],
  )

  return {
    messages,
    isOpen,
    isLoading,
    error,
    sendMessage,
    toggleOpen,
    clearChat,
  }
}
