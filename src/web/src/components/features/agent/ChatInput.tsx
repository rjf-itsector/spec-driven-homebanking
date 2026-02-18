import { useState, useCallback } from 'react'
import type { KeyboardEvent } from 'react'
import { Button } from '@/components/ui/button'
import { Send } from 'lucide-react'
import { cn } from '@/lib/utils'

interface ChatInputProps {
  onSend: (message: string) => void
  isLoading: boolean
}

export function ChatInput({ onSend, isLoading }: ChatInputProps) {
  const [value, setValue] = useState('')

  const handleSend = useCallback(() => {
    const trimmed = value.trim()
    if (!trimmed || isLoading) return
    onSend(trimmed)
    setValue('')
  }, [value, isLoading, onSend])

  const handleKeyDown = useCallback(
    (e: KeyboardEvent<HTMLTextAreaElement>) => {
      if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault()
        handleSend()
      }
    },
    [handleSend],
  )

  const remaining = 500 - value.length

  return (
    <div className="border-t border-border p-3">
      <div className="flex items-end gap-2">
        <div className="relative flex-1">
          <textarea
            value={value}
            onChange={(e) => setValue(e.target.value.slice(0, 500))}
            onKeyDown={handleKeyDown}
            placeholder="Type a message..."
            disabled={isLoading}
            rows={1}
            className="w-full resize-none rounded-md border border-input bg-background px-3 py-2 text-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
            aria-label="Chat message"
          />
          {value.length > 400 && (
            <span
              className={cn(
                'absolute bottom-1 right-2 text-xs',
                remaining <= 0 ? 'text-destructive' : 'text-muted-foreground',
              )}
            >
              {remaining}
            </span>
          )}
        </div>
        <Button
          onClick={handleSend}
          size="icon"
          disabled={isLoading || !value.trim()}
          aria-label="Send message"
          className="h-9 w-9 shrink-0"
        >
          <Send className="h-4 w-4" />
        </Button>
      </div>
    </div>
  )
}
