import { useEffect, useRef } from 'react'
import { ChatMessage } from './ChatMessage'
import { ChatThinking } from './ChatThinking'
import type { ChatMessage as ChatMessageType } from '@/lib/api/types'

interface ChatMessageListProps {
  messages: ChatMessageType[]
  isLoading: boolean
}

export function ChatMessageList({ messages, isLoading }: ChatMessageListProps) {
  const bottomRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages, isLoading])

  return (
    <div className="flex-1 overflow-y-auto px-4 py-3 space-y-3">
      {messages.length === 0 && !isLoading && (
        <div className="flex h-full items-center justify-center text-sm text-muted-foreground">
          <p>Ask me about your accounts, transactions, or transfers.</p>
        </div>
      )}
      {messages.map((msg) => (
        <ChatMessage key={msg.id} message={msg} />
      ))}
      {isLoading && <ChatThinking />}
      <div ref={bottomRef} />
    </div>
  )
}
