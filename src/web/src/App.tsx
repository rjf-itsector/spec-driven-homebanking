import { Routes, Route, Navigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { toast } from 'sonner'
import { AuthProvider } from '@/lib/hooks/useAuth'
import { ProtectedRoute } from '@/components/features/auth/ProtectedRoute'
import { DashboardLayout } from '@/components/layouts/DashboardLayout'
import { ErrorBoundary } from '@/components/layouts/ErrorBoundary'
import { Toaster } from '@/components/ui/sonner'
import { LoginPage } from '@/pages/LoginPage'
import { DashboardPage } from '@/pages/DashboardPage'
import { TransferPage } from '@/pages/TransferPage'
import { AccountDetailPage } from '@/pages/AccountDetailPage'
import { ApiError } from '@/lib/api/errors'

function getErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    return error.message
  }
  if (error instanceof Error) {
    return error.message
  }
  return 'An unexpected error occurred'
}

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
    },
    mutations: {
      onError: (error: Error) => {
        toast.error('Request failed', {
          description: getErrorMessage(error),
        })
      },
    },
  },
})

function App() {
  return (
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route element={<ProtectedRoute />}>
              <Route element={<DashboardLayout />}>
                <Route path="/dashboard" element={<DashboardPage />} />
                <Route path="/transfer" element={<TransferPage />} />
                <Route
                  path="/accounts/:accountId"
                  element={<AccountDetailPage />}
                />
              </Route>
            </Route>
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Routes>
          <Toaster />
        </AuthProvider>
      </QueryClientProvider>
    </ErrorBoundary>
  )
}

export default App
