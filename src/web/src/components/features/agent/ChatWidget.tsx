import { ChatPanel } from './ChatPanel'
import { useChatAgent } from '@/lib/hooks/useChatAgent'
import { Button } from '@/components/ui/button'
import { MessageCircle, X } from 'lucide-react'

export function ChatWidget() {
  const {
    messages,
    isOpen,
    isLoading,
    error,
    sendMessage,
    toggleOpen,
    clearChat,
  } = useChatAgent()

  return (
    <div className="fixed bottom-4 right-4 z-50 flex flex-col items-end gap-2">
      {isOpen && (
        <ChatPanel
          messages={messages}
          isLoading={isLoading}
          error={error}
          onSend={sendMessage}
          onClose={toggleOpen}
          onClear={clearChat}
        />
      )}
      <Button
        onClick={toggleOpen}
        size="icon"
        className="h-14 w-14 rounded-full shadow-lg"
        aria-label={isOpen ? 'Close chat' : 'Open chat'}
      >
        {isOpen ? (
          <X className="h-6 w-6" />
        ) : (
          <MessageCircle className="h-6 w-6" />
        )}
      </Button>
    </div>
  )
}
