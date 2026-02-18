// API request/response types matching backend C# records (camelCase serialization)

export interface LoginRequest {
  email: string
  password: string
}

export interface UserDto {
  id: string
  email: string
  firstName: string
  lastName: string
}

export interface LoginResponse {
  token: string
  expiresIn: number
  user: UserDto
}

export interface AccountDto {
  id: string
  accountNumber: string
  type: string
  balance: number
  currency: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface AccountDetailDto {
  id: string
  accountNumber: string
  type: string
  balance: number
  currency: string
  isActive: boolean
  transactionCount: number
  createdAt: string
  updatedAt: string
}

export interface TransactionDto {
  id: string
  type: string
  amount: number
  balanceAfter: number
  description: string
  category: string
  timestamp: string
  referenceNumber: string
  relatedTransactionId: string | null
}

export interface TransferRequest {
  fromAccountId: string
  toAccountId: string
  amount: number
  description: string
}

export interface TransferResponse {
  transferId: string
  debitTransactionId: string
  creditTransactionId: string
  fromAccountBalance: number
  toAccountBalance: number
  timestamp: string
}

export interface PaginatedResponse<T> {
  transactions: T[]
  pagination: {
    hasMore: boolean
    nextCursor: string | null
  }
}

export interface ApiErrorResponse {
  message: string
  errors?: Record<string, string[]>
  status?: number
}

// Agent chat types
export interface AgentChatRequest {
  message: string
  conversationHistory?: ConversationHistoryMessage[]
}

export interface ConversationHistoryMessage {
  role: 'user' | 'assistant'
  content: string
}

export interface AgentChatResponse {
  response: string
  toolsUsed: string[]
  tokenUsage: { input: number; output: number }
}

export interface ChatMessage {
  id: string
  role: 'user' | 'assistant'
  content: string
  timestamp: Date
  toolsUsed?: string[]
}

// Spending analytics types
export interface CategorySpendingDto {
  category: string
  total: number
  transactionCount: number
  percentage: number
}

export interface SpendingSummaryDto {
  fromDate: string
  toDate: string
  totalSpending: number
  categories: CategorySpendingDto[]
  averageDaily: number
}
