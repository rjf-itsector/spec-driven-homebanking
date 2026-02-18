import { ChatMessageList } from './ChatMessageList'
import { ChatInput } from './ChatInput'
import type { ChatMessage } from '@/lib/api/types'
import { Button } from '@/components/ui/button'
import { Trash2 } from 'lucide-react'

interface ChatPanelProps {
  messages: ChatMessage[]
  isLoading: boolean
  error: string | null
  onSend: (message: string) => void
  onClose: () => void
  onClear: () => void
}

export function ChatPanel({
  messages,
  isLoading,
  error,
  onSend,
  onClear,
}: ChatPanelProps) {
  return (
    <div className="flex w-[calc(100vw-2rem)] flex-col rounded-lg border border-border bg-background shadow-xl sm:w-[400px] h-[60vh] sm:h-[500px]">
      {/* Header */}
      <div className="flex items-center justify-between border-b border-border px-4 py-3">
        <h2 className="text-sm font-semibold">Banking Assistant</h2>
        <div className="flex items-center gap-1">
          <Button
            variant="ghost"
            size="icon"
            className="h-7 w-7"
            onClick={onClear}
            aria-label="Clear chat"
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* Messages */}
      <ChatMessageList messages={messages} isLoading={isLoading} />

      {/* Error */}
      {error && (
        <div className="px-4 py-2 text-sm text-destructive bg-destructive/10 border-t border-destructive/20">
          {error}
        </div>
      )}

      {/* Input */}
      <ChatInput onSend={onSend} isLoading={isLoading} />
    </div>
  )
}
