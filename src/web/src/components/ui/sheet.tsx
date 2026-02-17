import { useCallback, useEffect, useRef } from 'react'
import type { ReactNode } from 'react'
import { cn } from '@/lib/utils'
import { X } from 'lucide-react'

interface SheetProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  children: ReactNode
}

export function Sheet({ open, onOpenChange, children }: SheetProps) {
  const overlayRef = useRef<HTMLDivElement>(null)

  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === 'Escape') onOpenChange(false)
    },
    [onOpenChange],
  )

  useEffect(() => {
    if (open) {
      document.addEventListener('keydown', handleKeyDown)
      document.body.style.overflow = 'hidden'
    }
    return () => {
      document.removeEventListener('keydown', handleKeyDown)
      document.body.style.overflow = ''
    }
  }, [open, handleKeyDown])

  if (!open) return null

  return (
    <div className="fixed inset-0 z-50">
      <div
        ref={overlayRef}
        className="fixed inset-0 bg-black/80"
        onClick={() => onOpenChange(false)}
      />
      {children}
    </div>
  )
}

export function SheetContent({
  children,
  className,
  side = 'left',
}: {
  children: ReactNode
  className?: string
  side?: 'left' | 'right'
}) {
  return (
    <div
      className={cn(
        'fixed inset-y-0 z-50 flex h-full w-3/4 max-w-sm flex-col gap-4 bg-background p-6 shadow-lg',
        side === 'left' ? 'left-0' : 'right-0',
        className,
      )}
    >
      {children}
    </div>
  )
}

export function SheetClose({ onClose }: { onClose: () => void }) {
  return (
    <button
      onClick={onClose}
      className="absolute right-4 top-4 rounded-sm opacity-70 ring-offset-background transition-opacity hover:opacity-100 focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2"
      aria-label="Close"
    >
      <X className="h-4 w-4" />
    </button>
  )
}
