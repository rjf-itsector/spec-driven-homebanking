import { Routes, Route, Navigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { AuthProvider } from '@/lib/hooks/useAuth'
import { ProtectedRoute } from '@/components/features/auth/ProtectedRoute'
import { DashboardLayout } from '@/components/layouts/DashboardLayout'
import { LoginPage } from '@/pages/LoginPage'
import { DashboardPage } from '@/pages/DashboardPage'
import { TransferPage } from '@/pages/TransferPage'
import { AccountDetailPage } from '@/pages/AccountDetailPage'

const queryClient = new QueryClient()

function App() {
  return (
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
      </AuthProvider>
    </QueryClientProvider>
  )
}

export default App
