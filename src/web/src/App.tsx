import { Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from '@/lib/hooks/useAuth'
import { ProtectedRoute } from '@/components/features/auth/ProtectedRoute'
import { LoginPage } from '@/pages/LoginPage'

function DashboardPlaceholder() {
  return (
    <div className="min-h-screen flex items-center justify-center">
      <div className="text-center">
        <h1 className="text-3xl font-bold">Dashboard</h1>
        <p className="text-muted-foreground mt-2">Coming soon...</p>
      </div>
    </div>
  )
}

function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<ProtectedRoute />}>
          <Route path="/dashboard" element={<DashboardPlaceholder />} />
        </Route>
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </AuthProvider>
  )
}

export default App
