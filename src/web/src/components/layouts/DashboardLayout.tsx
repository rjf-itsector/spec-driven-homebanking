import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '@/lib/hooks/useAuth'
import { ChatWidget } from '@/components/features/agent/ChatWidget'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import { Sheet, SheetContent, SheetClose } from '@/components/ui/sheet'
import { cn } from '@/lib/utils'
import {
  LayoutDashboard,
  ArrowLeftRight,
  LogOut,
  Menu,
  Landmark,
} from 'lucide-react'
import { useState, useCallback } from 'react'

const NAV_ITEMS = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/transfer', label: 'Transfer', icon: ArrowLeftRight },
] as const

function NavLinks({ onClick }: { onClick?: () => void }) {
  return (
    <nav className="flex flex-col gap-1">
      {NAV_ITEMS.map(({ to, label, icon: Icon }) => (
        <NavLink
          key={to}
          to={to}
          onClick={onClick}
          className={({ isActive }) =>
            cn(
              'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
              isActive
                ? 'bg-accent text-accent-foreground'
                : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
            )
          }
        >
          <Icon className="h-4 w-4" />
          {label}
        </NavLink>
      ))}
    </nav>
  )
}

function DesktopSidebar() {
  return (
    <aside className="hidden md:flex md:w-64 md:flex-col md:border-r md:border-border">
      <div className="flex h-14 items-center gap-2 px-4">
        <Landmark className="h-5 w-5 text-primary" />
        <span className="text-lg font-semibold">Home Banking</span>
      </div>
      <Separator />
      <div className="flex-1 px-3 py-4">
        <NavLinks />
      </div>
    </aside>
  )
}

function MobileHeader({
  onMenuClick,
  userName,
  onLogout,
}: {
  onMenuClick: () => void
  userName: string
  onLogout: () => void
}) {
  return (
    <header className="flex h-14 items-center justify-between border-b border-border px-4 md:hidden">
      <div className="flex items-center gap-2">
        <Button
          variant="ghost"
          size="icon"
          onClick={onMenuClick}
          aria-label="Open menu"
        >
          <Menu className="h-5 w-5" />
        </Button>
        <Landmark className="h-5 w-5 text-primary" />
        <span className="text-lg font-semibold">Home Banking</span>
      </div>
      <div className="flex items-center gap-2">
        <span className="text-sm text-muted-foreground">{userName}</span>
        <Button
          variant="ghost"
          size="icon"
          onClick={onLogout}
          aria-label="Logout"
        >
          <LogOut className="h-4 w-4" />
        </Button>
      </div>
    </header>
  )
}

function DesktopHeader({
  userName,
  onLogout,
}: {
  userName: string
  onLogout: () => void
}) {
  return (
    <header className="hidden md:flex h-14 items-center justify-end border-b border-border px-6">
      <div className="flex items-center gap-3">
        <span className="text-sm text-muted-foreground">{userName}</span>
        <Button variant="ghost" size="sm" onClick={onLogout}>
          <LogOut className="mr-2 h-4 w-4" />
          Logout
        </Button>
      </div>
    </header>
  )
}

export function DashboardLayout() {
  const { user, logout } = useAuth()
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)

  const userName = user ? `${user.firstName} ${user.lastName}` : ''

  const handleLogout = useCallback(() => {
    logout()
  }, [logout])

  const closeMobileMenu = useCallback(() => {
    setMobileMenuOpen(false)
  }, [])

  return (
    <div className="flex h-screen flex-col bg-background text-foreground">
      {/* Mobile header */}
      <MobileHeader
        onMenuClick={() => setMobileMenuOpen(true)}
        userName={userName}
        onLogout={handleLogout}
      />

      {/* Mobile sidebar sheet */}
      <Sheet open={mobileMenuOpen} onOpenChange={setMobileMenuOpen}>
        <SheetContent side="left">
          <SheetClose onClose={closeMobileMenu} />
          <div className="flex items-center gap-2 mb-6">
            <Landmark className="h-5 w-5 text-primary" />
            <span className="text-lg font-semibold">Home Banking</span>
          </div>
          <NavLinks onClick={closeMobileMenu} />
        </SheetContent>
      </Sheet>

      <div className="flex flex-1 overflow-hidden">
        {/* Desktop sidebar */}
        <DesktopSidebar />

        {/* Main content */}
        <div className="flex flex-1 flex-col overflow-hidden">
          {/* Desktop header */}
          <DesktopHeader userName={userName} onLogout={handleLogout} />

          <main className="flex-1 overflow-y-auto p-4 md:p-6">
            <Outlet />
          </main>
        </div>
      </div>

      {/* AI Chat Widget */}
      <ChatWidget />
    </div>
  )
}
