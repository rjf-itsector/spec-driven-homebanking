import { Component, type ErrorInfo, type ReactNode } from 'react'
import { AlertCircle, RefreshCw } from 'lucide-react'
import { Button } from '@/components/ui/button'

interface ErrorBoundaryProps {
  children: ReactNode
}

interface ErrorBoundaryState {
  hasError: boolean
  error: Error | null
}

export class ErrorBoundary extends Component<
  ErrorBoundaryProps,
  ErrorBoundaryState
> {
  constructor(props: ErrorBoundaryProps) {
    super(props)
    this.state = { hasError: false, error: null }
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error }
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('ErrorBoundary caught an error:', error, errorInfo)
  }

  handleReset = () => {
    this.setState({ hasError: false, error: null })
  }

  handleReturnHome = () => {
    window.location.href = '/'
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="flex min-h-screen flex-col items-center justify-center bg-background p-4 text-foreground">
          <AlertCircle className="mb-4 h-16 w-16 text-destructive" />
          <h1 className="mb-2 text-2xl font-bold">Something went wrong</h1>
          <p className="mb-6 max-w-md text-center text-muted-foreground">
            We&apos;re sorry, but something unexpected happened. Please try
            again or return to the dashboard.
          </p>
          {this.state.error && (
            <pre className="mb-6 max-w-lg overflow-auto rounded-md bg-muted p-4 text-xs text-muted-foreground">
              {this.state.error.message}
            </pre>
          )}
          <div className="flex gap-3">
            <Button variant="outline" onClick={this.handleReset}>
              <RefreshCw className="mr-2 h-4 w-4" />
              Try Again
            </Button>
            <Button onClick={this.handleReturnHome}>Return to Dashboard</Button>
          </div>
        </div>
      )
    }

    return this.props.children
  }
}
