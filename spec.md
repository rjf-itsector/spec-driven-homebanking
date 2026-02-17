# Home Banking Application - Technical Specification

**Version**: 1.0.0  
**Date**: February 17, 2026  
**Status**: Draft

---

## Table of Contents

1. [Overview](#overview)
2. [Functional Requirements](#functional-requirements)
3. [Non-Functional Requirements](#non-functional-requirements)
4. [Architecture Overview](#architecture-overview)
5. [Technology Stack](#technology-stack)
6. [Design Decisions](#design-decisions)
7. [Data Model](#data-model)
8. [API Specification](#api-specification)
9. [Security Considerations](#security-considerations)
10. [Testing Strategy](#testing-strategy)

---

## Overview

A modern home banking web application demonstrating production-ready patterns for financial transaction management. The application provides users with real-time account balance tracking, transaction history with categorization, and secure money transfer capabilities.

**Project Goals:**
- Showcase modern full-stack development practices
- Demonstrate secure authentication and authorization flows
- Implement responsive, accessible financial UI components
- Provide comprehensive test coverage (unit, integration, E2E)
- Establish CI/CD pipeline with automated quality gates

**Target Users:**
- Demo users accessing via login credentials
- Primary device: Mobile (70%+ expected traffic)
- Secondary device: Desktop/tablet

---

## Functional Requirements

### FR-1: Authentication & Authorization

**FR-1.1 User Login**
- Users must authenticate via email/password form
- Mock JWT token issued upon successful authentication
- Token stored securely in httpOnly cookie or localStorage
- Demo credentials: `demo@bank.com` / `Demo123!`
- Invalid credentials return clear error messages

**FR-1.2 Session Management**
- JWT tokens expire after 2 hours
- Expired sessions redirect to login page
- Token automatically attached to API requests
- Logout clears token and returns to login

**FR-1.3 Protected Routes**
- Dashboard and financial pages require valid JWT
- Unauthenticated users redirected to login
- Token validation on every API request

### FR-2: Account Management

**FR-2.1 Account Dashboard**
- Display all user accounts as interactive cards
- Show account type (Checking, Savings, Credit Card, Investment)
- Display current balance with currency formatting
- Show last 4 digits of account number (e.g., "****1234")
- Indicate account status (active/inactive)

**FR-2.2 Account Details**
- View individual account information
- Display account creation date
- Show recent transaction count
- Link to full transaction history

### FR-3: Transaction Management

**FR-3.1 Transaction List**
- Display transactions in reverse chronological order (newest first)
- Show for each transaction:
  - Date and time
  - Description
  - Amount (with +/- indicator)
  - Category badge with icon
  - Running balance after transaction
  - Reference/confirmation number
- Support infinite scroll pagination (load 20 transactions per batch)
- Show loading indicator while fetching additional transactions
- Handle end of list gracefully (no more data message)

**FR-3.2 Transaction Categorization**
- Automatic category assignment based on description patterns
- Visual category badges with color coding:
  - 🛒 Groceries (green)
  - 🍽️ Dining (orange)
  - 🚗 Transportation (blue)
  - 🛍️ Shopping (purple)
  - 💡 Bills (red)
  - 🏥 Healthcare (pink)
  - 🎬 Entertainment (yellow)
  - 📋 Other (gray)
- Category icons for quick visual identification

**FR-3.3 Transaction Filtering** (Optional Enhancement)
- Filter by date range
- Filter by transaction type
- Filter by category
- Search by description

### FR-4: Money Transfers

**FR-4.1 Transfer Form**
- Select source account (from dropdown)
- Select destination account (to dropdown)
- Enter transfer amount with numeric input
- Add optional description (max 200 characters)
- Real-time validation feedback
- Review screen before confirmation

**FR-4.2 Transfer Validation**
- Amount must be greater than 0
- Amount cannot exceed source account balance
- Source and destination accounts must differ
- Amount limited to $1,000,000 per transaction
- Description sanitized for special characters

**FR-4.3 Transfer Execution**
- Create debit transaction in source account
- Create credit transaction in destination account
- Link transactions via reference ID
- Update both account balances atomically
- Show success confirmation with updated balances
- Optimistic UI update (instant feedback)

**FR-4.4 Transfer Error Handling**
- Display user-friendly error messages
- Handle insufficient funds scenario
- Handle network errors with retry option
- Validate form before submission
- Prevent double-submission

### FR-5: User Experience

**FR-5.1 Responsive Design**
- Mobile-first approach (320px - 428px primary)
- Tablet support (768px - 1024px)
- Desktop support (1280px+)
- Touch-optimized interactive elements (min 44x44px)
- Readable typography on all screen sizes

**FR-5.2 Loading States**
- Skeleton loaders for account cards during fetch
- Spinner for transaction list initial load
- Inline loading indicator for infinite scroll
- Button loading state during transfers
- Progress feedback for long operations

**FR-5.3 Error Handling**
- Global error boundary for React crashes
- Toast notifications for transient errors
- Inline form validation errors
- Network error recovery suggestions
- Graceful degradation for failed features

**FR-5.4 Accessibility**
- WCAG 2.1 AA compliance target
- Keyboard navigation support
- Screen reader compatible labels
- Sufficient color contrast (4.5:1 for text)
- Focus visible indicators
- Semantic HTML structure

---

## Non-Functional Requirements

### NFR-1: Performance

**NFR-1.1 Page Load Times**
- Initial page load: < 2 seconds (3G connection)
- Time to Interactive (TTI): < 3 seconds
- First Contentful Paint (FCP): < 1.5 seconds
- Dashboard data load: < 500ms (API response)

**NFR-1.2 API Response Times**
- GET endpoints: < 200ms (p95)
- POST endpoints: < 500ms (p95)
- Transaction list: < 300ms for 20 records
- Health checks: < 50ms

**NFR-1.3 Frontend Performance**
- Bundle size: < 500KB (gzipped)
- Time to parse JS: < 300ms
- Smooth scrolling: 60fps maintained
- No layout shifts (CLS < 0.1)

### NFR-2: Scalability

**NFR-2.1 Data Capacity**
- Support up to 5 accounts per user
- Handle 1000+ transactions per account
- Efficient pagination for large datasets
- Optimized query performance

**NFR-2.2 Concurrent Users**
- Handle 10+ simultaneous users (demo environment)
- No race conditions in transaction processing
- Thread-safe balance updates

### NFR-3: Reliability

**NFR-3.1 Availability**
- 99%+ uptime for demo environment
- Graceful degradation on service errors
- Health check endpoints for monitoring

**NFR-3.2 Data Integrity**
- Balance calculations always accurate
- Transaction history immutable
- Atomic transfer operations (all-or-nothing)
- Consistent state after errors

**NFR-3.3 Error Recovery**
- Automatic retry for transient failures
- Clear user guidance on error resolution
- Logging for debugging and monitoring

### NFR-4: Security

**NFR-4.1 Authentication**
- JWT tokens with secure signing
- Token expiration enforced
- No tokens in URL parameters
- Secure credential transmission (HTTPS assumption)

**NFR-4.2 Input Validation**
- Server-side validation for all inputs
- SQL injection prevention (EF Core parameterized queries)
- XSS prevention (React auto-escaping)
- CSRF protection for state-changing operations

**NFR-4.3 API Security**
- CORS configured for known origins
- Rate limiting on transfer endpoints (10 req/min)
- Authorization checks on all endpoints
- No sensitive data in error messages

### NFR-5: Maintainability

**NFR-5.1 Code Quality**
- Consistent code style (ESLint, dotnet format)
- TypeScript strict mode enabled
- Maximum function complexity: 10
- Test coverage: 70%+ (backend), 60%+ (frontend)

**NFR-5.2 Documentation**
- OpenAPI specification for all endpoints
- README with setup instructions
- Inline code comments for complex logic
- Architecture decision records (ADRs)

**NFR-5.3 Developer Experience**
- Fast local development setup (< 5 minutes)
- Hot module reload (backend and frontend)
- Clear error messages
- Comprehensive test suite

### NFR-6: Accessibility

**NFR-6.1 WCAG 2.1 Level AA**
- Color contrast ratio ≥ 4.5:1 for text
- All functionality keyboard accessible
- Screen reader compatible
- No flashing content
- Skip navigation links

**NFR-6.2 Inclusive Design**
- Clear language (8th grade reading level)
- Error messages with recovery guidance
- Timestamps in user's timezone
- Currency formatting respects locale

---

## Architecture Overview

### System Architecture

```
┌─────────────────────────────────────────────────────┐
│                     Browser                         │
│  ┌───────────────────────────────────────────────┐  │
│  │  React SPA (Port 5173)                        │  │
│  │  - Route Guard (JWT validation)               │  │
│  │  - React Query (API state + cache)            │  │
│  │  - React Hook Form (form state)               │  │
│  └───────────────┬───────────────────────────────┘  │
└──────────────────┼──────────────────────────────────┘
                   │ HTTPS (JWT in Authorization header)
                   │
┌──────────────────▼──────────────────────────────────┐
│            .NET Web API (Port 5000)                 │
│  ┌──────────────────────────────────────────────┐   │
│  │  Controllers (REST endpoints)                │   │
│  │    - AuthController                          │   │
│  │    - AccountsController                      │   │
│  │    - TransactionsController                  │   │
│  │    - TransfersController                     │   │
│  └──────────────┬───────────────────────────────┘   │
│                 │                                    │
│  ┌──────────────▼───────────────────────────────┐   │
│  │  Application Services                       │   │
│  │    - TransferService (business logic)       │   │
│  │    - AuthService (JWT generation)           │   │
│  └──────────────┬───────────────────────────────┘   │
│                 │                                    │
│  ┌──────────────▼───────────────────────────────┐   │
│  │  Domain Models                               │   │
│  │    - Account, Transaction, User              │   │
│  └──────────────┬───────────────────────────────┘   │
│                 │                                    │
│  ┌──────────────▼───────────────────────────────┐   │
│  │  EF Core DbContext                           │   │
│  │    - InMemory Provider                       │   │
│  │    - Demo Data Seeder                        │   │
│  └──────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

### Folder Structure

```
spec-driven-homebanking/
├── .github/
│   └── workflows/
│       ├── backend.yml          # Backend CI (build, test, lint)
│       ├── frontend.yml         # Frontend CI (build, test, lint, typecheck)
│       └── e2e.yml              # E2E tests (Playwright)
│
├── backend/
│   ├── src/
│   │   ├── HomeBanking.API/              # Web API layer
│   │   │   ├── Controllers/              # REST endpoints
│   │   │   ├── Middleware/               # Auth, error handling
│   │   │   ├── Program.cs                # Application entry point
│   │   │   └── appsettings.json          # Configuration
│   │   │
│   │   ├── HomeBanking.Core/             # Domain layer
│   │   │   ├── Entities/                 # Account, Transaction, User
│   │   │   ├── Enums/                    # TransactionType, Category
│   │   │   ├── Interfaces/               # IRepository, IAuthService
│   │   │   └── Services/                 # Business logic
│   │   │
│   │   ├── HomeBanking.Infrastructure/   # Data access layer
│   │   │   ├── Data/
│   │   │   │   ├── HomeBankingDbContext.cs
│   │   │   │   ├── DataSeeder.cs
│   │   │   │   └── Configurations/       # EF Core entity configs
│   │   │   └── Repositories/             # Repository implementations
│   │   │
│   │   └── HomeBanking.Contracts/        # DTOs & API contracts
│   │       ├── Requests/                 # LoginRequest, TransferRequest
│   │       ├── Responses/                # AccountResponse, TokenResponse
│   │       └── Validators/               # FluentValidation rules
│   │
│   ├── tests/
│   │   ├── HomeBanking.API.Tests/        # Integration tests
│   │   │   ├── Controllers/
│   │   │   └── TestWebApplicationFactory.cs
│   │   │
│   │   └── HomeBanking.Core.Tests/       # Unit tests
│   │       ├── Entities/
│   │       └── Services/
│   │
│   └── HomeBanking.sln                   # Solution file
│
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   │   ├── ui/                       # shadcn/ui components
│   │   │   │   ├── button.tsx
│   │   │   │   ├── card.tsx
│   │   │   │   ├── input.tsx
│   │   │   │   └── ...
│   │   │   │
│   │   │   ├── features/                 # Feature components
│   │   │   │   ├── auth/
│   │   │   │   │   ├── LoginForm.tsx
│   │   │   │   │   └── ProtectedRoute.tsx
│   │   │   │   ├── accounts/
│   │   │   │   │   ├── AccountCard.tsx
│   │   │   │   │   └── AccountList.tsx
│   │   │   │   ├── transactions/
│   │   │   │   │   ├── TransactionList.tsx
│   │   │   │   │   ├── TransactionRow.tsx
│   │   │   │   │   ├── CategoryBadge.tsx
│   │   │   │   │   └── useInfiniteTransactions.ts
│   │   │   │   └── transfers/
│   │   │   │       ├── TransferForm.tsx
│   │   │   │       └── TransferConfirmation.tsx
│   │   │   │
│   │   │   └── layouts/
│   │   │       ├── DashboardLayout.tsx
│   │   │       └── ErrorBoundary.tsx
│   │   │
│   │   ├── lib/
│   │   │   ├── api/
│   │   │   │   ├── client.ts             # Axios instance
│   │   │   │   ├── auth.ts               # Auth API calls
│   │   │   │   ├── accounts.ts           # Account API calls
│   │   │   │   └── types.ts              # TypeScript types
│   │   │   ├── hooks/
│   │   │   │   ├── useAuth.ts            # Auth context hook
│   │   │   │   └── useTransfers.ts       # Transfer mutations
│   │   │   └── utils/
│   │   │       ├── format.ts             # Currency, date formatting
│   │   │       └── validation.ts         # Zod schemas
│   │   │
│   │   ├── pages/
│   │   │   ├── LoginPage.tsx
│   │   │   ├── DashboardPage.tsx
│   │   │   └── TransferPage.tsx
│   │   │
│   │   ├── App.tsx                       # Router, providers
│   │   ├── main.tsx                      # Entry point
│   │   └── index.css                     # Global styles
│   │
│   ├── tests/
│   │   └── unit/
│   │       ├── components/
│   │       └── utils/
│   │
│   ├── public/
│   ├── index.html
│   ├── package.json
│   ├── vite.config.ts
│   ├── tailwind.config.js
│   ├── postcss.config.js
│   ├── tsconfig.json
│   └── .eslintrc.json
│
├── e2e/
│   ├── tests/
│   │   ├── auth.spec.ts                  # Login flow
│   │   ├── dashboard.spec.ts             # Dashboard load
│   │   ├── transactions.spec.ts          # Infinite scroll
│   │   └── transfer.spec.ts              # Transfer flow
│   ├── fixtures/
│   │   └── test-data.ts
│   ├── playwright.config.ts
│   └── package.json
│
├── docs/
│   ├── api/                              # API documentation
│   └── architecture/                     # ADRs, diagrams
│
├── spec.md                               # This file
├── plan.md                               # Implementation task plan
├── README.md                             # Setup & usage guide
└── .gitignore
```

### API Contract Structure

All API endpoints follow REST conventions:

```
Base URL: http://localhost:5000/api/v1

Authentication:
POST   /auth/login          # Authenticate user, receive JWT

Accounts:
GET    /accounts            # List all user accounts
GET    /accounts/{id}       # Get account details

Transactions:
GET    /accounts/{id}/transactions?cursor={cursor}&limit=20
                           # Get transactions with pagination
GET    /transactions/{id}  # Get single transaction details

Transfers:
POST   /transfers          # Create new transfer

Health:
GET    /health             # Basic health check
GET    /health/ready       # Readiness probe
```

---

## Technology Stack

### Backend Stack

```yaml
Runtime & Framework:
  - .NET Core: 9.0.x (LTS until November 2026)
  - Language: C# 12 (latest)
  
Data Access:
  - Entity Framework Core: 9.0.*
  - Database Provider: Microsoft.EntityFrameworkCore.InMemory 9.0.*
  
API Documentation:
  - Scalar.AspNetCore: 1.2.*
  - OpenAPI Specification: 3.0
  
Authentication:
  - System.IdentityModel.Tokens.Jwt: 7.5.*
  - Microsoft.AspNetCore.Authentication.JwtBearer: 9.0.*
  
Health Checks:
  - Microsoft.Extensions.Diagnostics.HealthChecks: 9.0.* (built-in)
  - AspNetCore.HealthChecks.UI: 8.0.*
  
Testing:
  - xUnit: 2.9.*
  - xUnit.runner.visualstudio: 2.8.*
  - FluentAssertions: 6.12.*
  - Moq: 4.20.*
  - Microsoft.AspNetCore.Mvc.Testing: 9.0.* (integration tests)
  
Validation:
  - FluentValidation.AspNetCore: 11.3.*
  
Logging:
  - Serilog.AspNetCore: 8.0.*
  - Serilog.Sinks.Console: 5.0.*
```

### Frontend Stack

```yaml
Framework:
  - React: ^18.3.0 (latest stable)
  - React DOM: ^18.3.0
  
Build Tool:
  - Vite: ^5.4.0
  - @vitejs/plugin-react-swc: ^3.7.0 (fast refresh with SWC)
  
Language:
  - TypeScript: ^5.6.0 (strict mode enabled)
  
Routing:
  - react-router-dom: ^6.26.0
  
State Management:
  - @tanstack/react-query: ^5.56.0 (API state)
  
Forms & Validation:
  - react-hook-form: ^7.53.0
  - @hookform/resolvers: ^3.9.0
  - zod: ^3.23.0
  
Styling:
  - Tailwind CSS: ^3.4.0
  - autoprefixer: ^10.4.0
  - postcss: ^8.4.0
  
UI Components:
  - shadcn/ui: latest (component library - copied, not npm package)
  - Radix UI: ^2.0.* (shadcn dependency)
  - lucide-react: ^0.446.0 (icons)
  - class-variance-authority: ^0.7.0 (component variants)
  - clsx: ^2.1.0
  - tailwind-merge: ^2.5.0
  
HTTP Client:
  - axios: ^1.7.0
  
Utilities:
  - date-fns: ^3.6.0 (date formatting)
  
Testing:
  - Vitest: ^2.1.0 (unit tests)
  - @testing-library/react: ^16.0.0
  - @testing-library/jest-dom: ^6.5.0
  - @testing-library/user-event: ^14.5.0
  
Linting & Formatting:
  - ESLint: ^9.11.0
  - Prettier: ^3.3.0
  - @typescript-eslint/parser: ^8.0.0
  - @typescript-eslint/eslint-plugin: ^8.0.0
```

### E2E Testing Stack

```yaml
E2E Framework:
  - @playwright/test: ^1.47.0
  
Accessibility:
  - @axe-core/playwright: ^4.10.0 (a11y testing)
  
Browsers:
  - Chromium, Firefox, WebKit (via Playwright)
```

### DevOps Stack

```yaml
CI/CD:
  - GitHub Actions (workflows for backend, frontend, e2e)
  
Version Control:
  - Git 2.40+
  
Package Management:
  - NuGet (backend)
  - npm 10+ (frontend)
  
Code Coverage:
  - Coverlet (backend coverage)
  - Vitest coverage provider (frontend)
```

### Development Tools

```yaml
IDEs:
  - Visual Studio Code (recommended)
  - Visual Studio 2022+
  - JetBrains Rider
  
VS Code Extensions:
  - C# Dev Kit
  - ESLint
  - Prettier
  - Tailwind CSS IntelliSense
  - REST Client
  
CLI Tools:
  - dotnet CLI 9.0+
  - Node.js 20+ LTS
  - npm 10+
```

---

## Design Decisions

### DD-1: Mock JWT Authentication

**Decision**: Implement JWT-based authentication with hardcoded credentials instead of no authentication or OAuth provider.

**Rationale**:
- **Demonstrates Security Patterns**: Shows proper authorization flow, token handling, and protected routes
- **Simple Setup**: No external dependencies (Auth0, Azure AD) required
- **Realistic**: Mirrors production authentication patterns
- **Testable**: Easy to test both authenticated and unauthenticated scenarios
- **Portfolio Value**: Showcases security understanding

**Trade-offs**:
- (+) Complete auth flow demonstration
- (+) Reusable patterns for real OAuth integration
- (-) Hardcoded credentials (acceptable for demo)
- (-) No password hashing (not needed for demo)

**Alternatives Considered**:
- No authentication: Too simplistic, doesn't show security practices
- OAuth provider: Over-engineered for demo, external dependencies
- Basic auth: Less modern, not suitable for SPA

**Implementation Notes**:
- JWT signed with HS256 and secret key
- Token expiry: 2 hours
- Claims: userId, email, issued timestamp
- Token stored in localStorage (acceptable for demo)

### DD-2: Infinite Scroll for Transactions

**Decision**: Implement infinite scroll pagination instead of page-based or "load all" approach.

**Rationale**:
- **Modern UX**: Matches user expectations from banking apps (Chase, Bank of America)
- **Performance**: Loads data progressively, reducing initial load time
- **Mobile-First**: Better for touch interfaces than pagination buttons
- **Scalability**: Handles 1000+ transactions gracefully
- **Smooth Experience**: No page breaks or jarring transitions

**Trade-offs**:
- (+) Better perceived performance
- (+) Natural interaction model
- (-) Slightly more complex implementation
- (-) Harder to jump to specific page

**Alternatives Considered**:
- Page-based: Too desktop-centric, multiple clicks
- Load all: Poor performance with many transactions
- Virtual scrolling: Over-engineered for 1000 records

**Implementation Notes**:
- Use Intersection Observer API for scroll detection
- Load 20 transactions per batch
- Cursor-based pagination (last transaction ID)
- Show loading spinner during fetch
- "No more transactions" message at end

### DD-3: Mobile-First Responsive Design

**Decision**: Design for mobile screens first (320px-428px), then scale up to desktop.

**Rationale**:
- **Usage Patterns**: 70%+ of banking app usage is on mobile devices
- **Better Constraints**: Designing for small screens forces prioritization
- **Progressive Enhancement**: Easier to add desktop features than remove for mobile
- **Accessibility**: Mobile-first often improves keyboard navigation
- **Modern Standard**: Industry best practice for consumer apps

**Trade-offs**:
- (+) Optimized for majority use case
- (+) Forces focus on essential features
- (-) Desktop might feel less optimized initially
- (-) More testing required across breakpoints

**Breakpoints**:
- Mobile: 320px - 768px (base styles)
- Tablet: 768px - 1024px
- Desktop: 1024px+

### DD-4: InMemory Database (Demo Data Resets)

**Decision**: Use EF Core InMemory provider; data resets on API restart.

**Rationale**:
- **Fast Setup**: No database installation required
- **CI/CD Friendly**: No database provisioning in pipelines
- **E2E Testing**: Clean slate for every test run
- **Demo Clarity**: Clearly signals this is a demonstration
- **Simplicity**: No migration files or schema evolution

**Trade-offs**:
- (+) Zero database configuration
- (+) Fast integration tests
- (+) No data cleanup needed
- (-) Data lost on restart (acceptable for demo)
- (-) Not production-ready (intentionally)

**Alternatives Considered**:
- SQLite file: Adds persistence complexity
- Seed from JSON: Still requires database choice
- SQL Server: Over-engineered, setup friction

**Implementation Notes**:
- Seed data on startup in `Program.cs`
- Include clear documentation: "Data resets on restart"
- Seed realistic demo data (3-5 accounts, 50+ transactions)

### DD-5: Decimal Type for Money

**Decision**: Store and calculate money using `decimal` (C#) and integer cents (TypeScript), never `float`/`double`.

**Rationale**:
- **Financial Precision**: Prevents floating-point errors ($0.30 ≠ 0.2999999)
- **Industry Standard**: All financial systems use fixed-point arithmetic
- **Data Integrity**: Ensures balance calculations always accurate
- **Compliance**: Meets financial data handling standards

**Implementation**:
- **Backend**: C# `decimal` type (128-bit, 28-29 significant digits)
- **API**: Transfer amounts as decimal strings to avoid JS number precision loss
- **Frontend**: Store as integers (cents), display with `Intl.NumberFormat`

```typescript
// Frontend formatting
const formatCurrency = (cents: number) => 
  new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD'
  }).format(cents / 100);
```

### DD-6: TanStack Query (React Query) for API State

**Decision**: Use TanStack Query instead of Redux or plain fetch/axios.

**Rationale**:
- **Purpose-Built**: Designed specifically for server state management
- **Built-in Features**: Caching, refetching, optimistic updates, loading states
- **Less Boilerplate**: No actions, reducers, sagas to write
- **Better DX**: Automatic background refetching, stale-while-revalidate
- **Performance**: Intelligent deduplication, parallel queries

**Trade-offs**:
- (+) Drastically less code
- (+) Better UX (optimistic updates, instant cache)
- (+) Built-in devtools
- (-) Learning curve for team unfamiliar with it
- (-) Less control vs custom Redux

**Alternatives Considered**:
- Redux Toolkit + RTK Query: More boilerplate, overkill for this scope
- Plain axios: Manual cache management, loading states
- SWR: Similar to React Query, but less feature-complete

**Configuration**:
```typescript
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60000, // 1 minute
      cacheTime: 300000, // 5 minutes
      refetchOnWindowFocus: true,
    },
  },
});
```

### DD-7: Monorepo Structure

**Decision**: Single Git repository with `backend/`, `frontend/`, `e2e/` folders instead of separate repositories.

**Rationale**:
- **Atomic Commits**: Single PR for full features (backend + frontend + tests)
- **Shared Types**: Generate TypeScript types from OpenAPI spec
- **Simplified CI**: One workflow file, unified versioning
- **Portfolio Simplicity**: Single clone, single README
- **Dependency Management**: Easier to keep versions in sync

**Trade-offs**:
- (+) Single source of truth
- (+) Easier setup for new developers
- (+) Unified documentation
- (-) Larger repository size
- (-) Requires monorepo-aware CI/CD

**Alternatives Considered**:
- Separate repos: More setup friction, versioning complexity
- Nx/Turborepo: Over-engineered for 2-project setup

### DD-8: Scalar over Swagger UI

**Decision**: Use Scalar for OpenAPI documentation instead of Swagger UI.

**Rationale**:
- **Modern UI**: Better visual design, dark theme support
- **Better DX**: Interactive, supports code generation
- **Performance**: Faster than Swagger UI
- **Try-it-out**: Built-in API testing interface
- **Active Development**: Modern, well-maintained

**Implementation**:
```csharp
builder.Services.AddOpenApi();
app.MapScalarApiReference(); // Available at /scalar/v1
```

### DD-9: Feature-Based Frontend Folders

**Decision**: Organize components by feature (`features/accounts/`, `features/transfers/`) instead of by type (`containers/`, `components/`).

**Rationale**:
- **Colocation**: Related files stay together (component, styles, tests, hooks)
- **Scalability**: Easy to add features without cross-folder changes
- **Mental Model**: Matches user features, easier to navigate
- **Encapsulation**: Feature boundaries clear

**Structure**:
```
features/
  accounts/
    AccountCard.tsx
    AccountCard.test.tsx
    useAccountData.ts
  transfers/
    TransferForm.tsx
    TransferForm.test.tsx
    useTransferMutation.ts
```

### DD-10: Clean Architecture Lite (Backend)

**Decision**: Use lightweight Clean Architecture with Core/Infrastructure/API layers, but not full DDD/CQRS.

**Rationale**:
- **Separation of Concerns**: Business logic isolated from infrastructure
- **Testability**: Core logic doesn't depend on EF Core or ASP.NET
- **Maintainability**: Clear boundaries between layers
- **Right-Sized**: Not over-engineered for demo app scope
- **Educational**: Shows architecture patterns without overwhelming

**Layers**:
1. **API Layer**: Controllers, middleware, DTOs
2. **Core Layer**: Entities, interfaces, business logic
3. **Infrastructure Layer**: EF Core, repositories, external services
4. **Contracts Layer**: Shared DTOs between API and client

**Avoided Complexity**:
- No CQRS (read/write separation not needed)
- No MediatR (command/query bus overkill)
- No event sourcing (transaction log not required)
- No domain events (simple demo scope)

---

## Data Model

### Entity Relationship Diagram

```
┌─────────────────────────┐
│         User            │
│─────────────────────────│
│ Id: Guid (PK)           │
│ Email: string           │
│ PasswordHash: string    │
│ CreatedAt: DateTime     │
└────────┬────────────────┘
         │ 1
         │
         │ *
┌────────▼────────────────┐
│       Account           │
│─────────────────────────│
│ Id: Guid (PK)           │
│ UserId: Guid (FK)       │
│ AccountNumber: string   │
│ Type: AccountType       │
│ Balance: decimal        │
│ Currency: string        │
│ CreatedAt: DateTime     │
│ UpdatedAt: DateTime     │
└────────┬────────────────┘
         │ 1
         │
         │ *
┌────────▼────────────────┐
│     Transaction         │
│─────────────────────────│
│ Id: Guid (PK)           │
│ AccountId: Guid (FK)    │
│ Type: TransactionType   │
│ Amount: decimal         │
│ BalanceAfter: decimal   │
│ Description: string     │
│ Category: Category      │
│ Timestamp: DateTime     │
│ ReferenceNumber: string │
│ RelatedTransactionId?   │
└─────────────────────────┘
```

### Entity Definitions

#### User

```csharp
public class User
{
    public Guid Id { get; set; }
    public required string Email { get; set; } // Unique
    public required string PasswordHash { get; set; }
    public string FirstName { get; set; } = "Demo";
    public string LastName { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
```

#### Account

```csharp
public class Account
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required string AccountNumber { get; set; } // Last 4 digits visible
    public AccountType Type { get; set; }
    public decimal Balance { get; set; } // MUST be decimal
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public User User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public enum AccountType
{
    Checking = 0,
    Savings = 1,
    CreditCard = 2,
    Investment = 3
}
```

#### Transaction

```csharp
public class Transaction
{
    public Guid Id { get; set; }
    public required Guid AccountId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; } // Positive = credit, Negative = debit
    public decimal BalanceAfter { get; set; } // Snapshot for historical accuracy
    public required string Description { get; set; }
    public TransactionCategory Category { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public required string ReferenceNumber { get; set; } // e.g., "TXN-20260217-001234"
    public Guid? RelatedTransactionId { get; set; } // For transfers (links debit/credit)
    
    // Navigation
    public Account Account { get; set; } = null!;
}

public enum TransactionType
{
    Deposit = 0,
    Withdrawal = 1,
    Transfer = 2,
    Payment = 3,
    Fee = 4,
    Interest = 5
}

public enum TransactionCategory
{
    Groceries = 0,
    Dining = 1,
    Transportation = 2,
    Shopping = 3,
    Bills = 4,
    Healthcare = 5,
    Entertainment = 6,
    Other = 7
}
```

### Demo Data Seed

```csharp
// Seed one demo user
User: demo@bank.com / Demo123!

// Seed 4 accounts
1. Checking ****1234: $5,250.00
2. Savings ****5678: $12,800.00
3. Credit Card ****9012: -$1,243.50 (negative balance)
4. Investment ****3456: $28,500.00

// Seed 50-80 transactions per account
- Date range: Last 90 days
- Mix of deposits, withdrawals, transfers
- Realistic descriptions:
  - "Salary Deposit - Acme Corp"
  - "Whole Foods Market #1234"
  - "Netflix Subscription"
  - "Transfer to Savings"
  - "ATM Withdrawal - Main St"
- Appropriate categories for each
- Balance reconciliation: Sum(transactions) = current balance
```

---

## API Specification

### Authentication Endpoints

#### POST /api/v1/auth/login

Authenticate user and receive JWT token.

**Request**:
```json
{
  "email": "demo@bank.com",
  "password": "Demo123!"
}
```

**Response 200 OK**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 7200,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "demo@bank.com",
    "firstName": "Demo",
    "lastName": "User"
  }
}
```

**Response 401 Unauthorized**:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Invalid email or password"
}
```

### Account Endpoints

#### GET /api/v1/accounts

Get all accounts for authenticated user.

**Headers**:
```
Authorization: Bearer {token}
```

**Response 200 OK**:
```json
{
  "accounts": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "accountNumber": "****1234",
      "type": "Checking",
      "balance": 5250.00,
      "currency": "USD",
      "isActive": true,
      "createdAt": "2025-11-01T10:00:00Z",
      "updatedAt": "2026-02-17T08:30:00Z"
    }
  ]
}
```

#### GET /api/v1/accounts/{id}

Get single account details.

**Response 200 OK**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "accountNumber": "****1234",
  "type": "Checking",
  "balance": 5250.00,
  "currency": "USD",
  "isActive": true,
  "transactionCount": 87,
  "createdAt": "2025-11-01T10:00:00Z",
  "updatedAt": "2026-02-17T08:30:00Z"
}
```

**Response 404 Not Found**:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Account not found"
}
```

### Transaction Endpoints

#### GET /api/v1/accounts/{accountId}/transactions

Get paginated transactions for account (infinite scroll support).

**Query Parameters**:
- `cursor` (optional): Last transaction ID from previous page
- `limit` (optional): Number of records (default: 20, max: 100)

**Example**:
```
GET /api/v1/accounts/3fa85f64.../transactions?limit=20
GET /api/v1/accounts/3fa85f64.../transactions?cursor=9b1deb4d...&limit=20
```

**Response 200 OK**:
```json
{
  "transactions": [
    {
      "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
      "type": "Withdrawal",
      "amount": -45.67,
      "balanceAfter": 5250.00,
      "description": "Whole Foods Market #1234",
      "category": "Groceries",
      "timestamp": "2026-02-17T08:15:00Z",
      "referenceNumber": "TXN-20260217-001234"
    }
  ],
  "pagination": {
    "hasMore": true,
    "nextCursor": "8e3cc0bd-..."
  }
}
```

#### GET /api/v1/transactions/{id}

Get single transaction details.

**Response 200 OK**:
```json
{
  "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": "Transfer",
  "amount": -100.00,
  "balanceAfter": 4150.00,
  "description": "Transfer to Savings",
  "category": "Other",
  "timestamp": "2026-02-16T14:30:00Z",
  "referenceNumber": "TXN-20260216-005678",
  "relatedTransactionId": "2d7f88c9-..." // Link to credit transaction
}
```

### Transfer Endpoints

#### POST /api/v1/transfers

Create transfer between accounts.

**Request**:
```json
{
  "fromAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "toAccountId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "amount": 100.00,
  "description": "Savings for vacation"
}
```

**Validation Rules**:
- `amount` > 0
- `amount` <= source account balance
- `fromAccountId` ≠ `toAccountId`
- `amount` <= 1,000,000
- `description` max 200 characters

**Response 201 Created**:
```json
{
  "transferId": "1a2b3c4d-...",
  "debitTransactionId": "5e6f7g8h-...",
  "creditTransactionId": "9i0j1k2l-...",
  "fromAccountBalance": 5150.00,
  "toAccountBalance": 12900.00,
  "timestamp": "2026-02-17T09:00:00Z"
}
```

**Response 400 Bad Request**:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Insufficient funds",
  "errors": {
    "amount": ["Balance insufficient for transfer. Available: $5150.00"]
  }
}
```

### Health Endpoints

#### GET /health

Basic health check.

**Response 200 OK**:
```json
{
  "status": "Healthy",
  "timestamp": "2026-02-17T09:00:00Z"
}
```

#### GET /health/ready

Readiness check (includes database connectivity).

**Response 200 OK**:
```json
{
  "status": "Healthy",
  "checks": {
    "self": "Healthy",
    "database": "Healthy"
  },
  "timestamp": "2026-02-17T09:00:00Z"
}
```

---

## Security Considerations

### Authentication & Authorization

1. **JWT Implementation**:
   - Sign with HMAC-SHA256 (HS256)
   - Secret key: 256-bit random value (stored in appsettings)
   - Expiration: 2 hours
   - Claims: userId, email, iat (issued at)

2. **Token Storage** (Frontend):
   - localStorage (acceptable for demo)
   - Production consideration: httpOnly cookies

3. **Endpoint Protection**:
   - All endpoints except `/auth/login` and `/health` require JWT
   - Validate token signature on every request
   - Return 401 for missing/invalid tokens

### Input Validation

1. **Server-Side Validation** (Primary Defense):
   - Use FluentValidation for DTOs
   - Validate all numeric ranges (amount > 0, amount <= balance)
   - Sanitize string inputs (description, email)
   - Check account ownership (user can only access their accounts)

2. **Client-Side Validation** (UX Enhancement):
   - Zod schemas matching backend rules
   - Real-time feedback during form entry
   - Never trust client validation alone

### API Security

1. **CORS Configuration**:
   ```csharp
   // Development
   builder.Services.AddCors(options => {
       options.AddPolicy("LocalDev", policy => {
           policy.WithOrigins("http://localhost:5173")
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                 .AllowCredentials();
       });
   });
   ```

2. **Rate Limiting**:
   ```csharp
   builder.Services.AddRateLimiter(options => {
       options.AddFixedWindowLimiter("transfers", opt => {
           opt.Window = TimeSpan.FromMinutes(1);
           opt.PermitLimit = 10;
       });
   });
   
   app.MapPost("/api/v1/transfers", ...)
      .RequireRateLimiting("transfers");
   ```

3. **SQL Injection Prevention**:
   - EF Core uses parameterized queries automatically
   - Never use raw SQL with string concatenation

4. **XSS Prevention**:
   - React escapes strings by default
   - Avoid `dangerouslySetInnerHTML`
   - Sanitize HTML in descriptions server-side

### Data Protection

1. **Sensitive Data**:
   - Never log JWT tokens
   - Never log passwords (even hashed)
   - Mask account numbers in logs (show last 4 digits only)

2. **Error Messages**:
   - No stack traces in API responses
   - Generic messages for auth failures ("Invalid credentials" not "User not found")
   - Detailed errors logged server-side only

### Demo Limitations

1. **Explicitly NOT Implemented** (document clearly):
   - Password hashing (uses plain text - demo only)
   - HTTPS enforcement (local development)
   - Refresh tokens (JWT expiry only)
   - Account lockout after failed attempts
   - CSRF tokens (stateless JWT)
   - Content Security Policy headers
   - HSTS headers

2. **Production Recommendations** (in README):
   - Use OAuth 2.0 / OpenID Connect provider
   - Implement proper password hashing (bcrypt, Argon2)
   - Use HTTPS everywhere
   - Add refresh token rotation
   - Implement MFA (2FA)
   - Add comprehensive audit logging
   - Use secrets management (Azure Key Vault, AWS Secrets Manager)

---

## Testing Strategy

### Test Pyramid

```
          /\
         /  \
        / E2E \         5-10 tests (critical flows)
       /──────\
      /        \
     / Integration \    20-30 tests (API endpoints)
    /──────────────\
   /                \
  /   Unit Tests     \  50-80 tests (business logic)
 /────────────────────\
```

### Backend Testing

#### Unit Tests (HomeBanking.Core.Tests)

**Scope**: Domain logic, business rules

**Examples**:
- Account balance calculations
- Transaction validation rules
- Transfer logic (sufficient funds check)
- Category assignment logic
- Date/time calculations

**Tools**: xUnit, FluentAssertions

```csharp
[Fact]
public void Transfer_WithInsufficientFunds_ThrowsException()
{
    // Arrange
    var source = new Account { Balance = 50m };
    var destination = new Account { Balance = 100m };
    
    // Act
    var act = () => TransferService.Execute(source, destination, 100m);
    
    // Assert
    act.Should().Throw<InsufficientFundsException>()
       .WithMessage("*insufficient*");
}
```

#### Integration Tests (HomeBanking.API.Tests)

**Scope**: Controllers, database, full request pipeline

**Examples**:
- POST /auth/login returns JWT token
- GET /accounts returns user's accounts only
- POST /transfers creates two transactions atomically
- Invalid JWT returns 401
- Transfer with insufficient funds returns 400

**Tools**: WebApplicationFactory, xUnit

```csharp
public class TransfersControllerTests : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task PostTransfer_ValidRequest_Returns201()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _testJwt);
        
        var request = new TransferRequest(
            FromAccountId: _accountId1,
            ToAccountId: _accountId2,
            Amount: 100m,
            Description: "Test transfer"
        );
        
        // Act
        var response = await client.PostAsJsonAsync("/api/v1/transfers", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<TransferResponse>();
        result.FromAccountBalance.Should().Be(4900m); // 5000 - 100
    }
}
```

### Frontend Testing

#### Unit Tests (Vitest)

**Scope**: Components, utilities, hooks

**Examples**:
- AccountCard displays formatted balance
- CategoryBadge shows correct icon for category
- formatCurrency utility formats cents correctly
- TransferForm validates amount > 0
- useInfiniteTransactions hook loads paginated data

**Tools**: Vitest, Testing Library

```typescript
describe('AccountCard', () => {
  it('formats balance as currency', () => {
    const account = {
      id: '123',
      accountNumber: '****1234',
      type: 'Checking',
      balance: 525000, // cents
    };
    
    render(<AccountCard account={account} />);
    
    expect(screen.getByText('$5,250.00')).toBeInTheDocument();
  });
  
  it('applies correct styling for negative balance', () => {
    const account = { ...mockAccount, balance: -50000 };
    
    render(<AccountCard account={account} />);
    
    const balance = screen.getByText('-$500.00');
    expect(balance).toHaveClass('text-red-600');
  });
});
```

#### Component Tests (Testing Library)

**Scope**: User interactions, form submissions

**Examples**:
- Login form submits credentials
- Transfer form validates before submit
- Transaction list shows loading state
- Infinite scroll triggers data fetch

```typescript
describe('TransferForm', () => {
  it('prevents transfer to same account', async () => {
    const user = userEvent.setup();
    render(<TransferForm />);
    
    await user.selectOptions(screen.getByLabelText('From Account'), 'account-1');
    await user.selectOptions(screen.getByLabelText('To Account'), 'account-1');
    await user.click(screen.getByRole('button', { name: 'Transfer' }));
    
    expect(await screen.findByText(/cannot transfer to same account/i))
      .toBeInTheDocument();
  });
});
```

### E2E Tests (Playwright)

**Scope**: Critical user journeys, full system integration

**Test Cases**:

1. **auth.spec.ts**:
   - User can log in with valid credentials
   - Invalid credentials show error message
   - Unauthenticated user redirected to login

2. **dashboard.spec.ts**:
   - Dashboard loads after login
   - All accounts display with correct balances
   - Account cards are clickable

3. **transactions.spec.ts**:
   - Transaction list loads initial 20 transactions
   - Infinite scroll loads next batch
   - Category badges display correct icons
   - Transactions sorted by newest first

4. **transfer.spec.ts**:
   - User can complete transfer between accounts
   - Form validation prevents invalid transfers
   - Balances update after transfer
   - Success message displays
   - Transaction appears in both account histories

**Example**:
```typescript
test('complete transfer flow', async ({ page }) => {
  // Login
  await page.goto('http://localhost:5173/login');
  await page.fill('[data-testid="email"]', 'demo@bank.com');
  await page.fill('[data-testid="password"]', 'Demo123!');
  await page.click('[data-testid="login-button"]');
  
  // Wait for dashboard
  await expect(page.locator('[data-testid="dashboard"]')).toBeVisible();
  
  // Navigate to transfer
  await page.click('[data-testid="transfer-button"]');
  
  // Fill form
  await page.selectOption('[data-testid="from-account"]', { label: 'Checking ****1234' });
  await page.selectOption('[data-testid="to-account"]', { label: 'Savings ****5678' });
  await page.fill('[data-testid="amount"]', '100');
  await page.fill('[data-testid="description"]', 'Test transfer');
  
  // Submit
  await page.click('[data-testid="submit-transfer"]');
  
  // Verify success
  await expect(page.locator('[data-testid="success-message"]'))
    .toContainText('Transfer successful');
  
  // Verify balance update
  const checkingBalance = page.locator('[data-testid="account-balance-1"]');
  await expect(checkingBalance).toContainText('$5,150.00'); // 5250 - 100
});
```

### Test Coverage Goals

- **Backend Core**: 80%+ line coverage
- **Backend API**: 70%+ line coverage
- **Frontend Components**: 60%+ line coverage
- **Frontend Utilities**: 80%+ line coverage
- **E2E**: 100% coverage of critical paths (login, view, transfer)

### Testing in CI/CD

```yaml
# .github/workflows/backend.yml
- name: Run tests with coverage
  run: dotnet test --collect:"XPlat Code Coverage"
  
- name: Upload coverage
  uses: codecov/codecov-action@v4
  with:
    files: coverage.cobertura.xml
    
# .github/workflows/frontend.yml
- name: Run tests with coverage
  run: npm run test:coverage
  
- name: Upload coverage
  uses: codecov/codecov-action@v4
  with:
    files: coverage/lcov.info
```

### Accessibility Testing

**Playwright + Axe-Core**:
```typescript
import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test('dashboard should not have accessibility violations', async ({ page }) => {
  await page.goto('http://localhost:5173/dashboard');
  
  const accessibilityScanResults = await new AxeBuilder({ page }).analyze();
  
  expect(accessibilityScanResults.violations).toEqual([]);
});
```

---

## Appendices

### A. Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-02-17 | System | Initial specification |

### B. Glossary

- **JWT**: JSON Web Token, compact token format for auth
- **EF Core**: Entity Framework Core, ORM for .NET
- **SPA**: Single Page Application
- **TTI**: Time to Interactive
- **WCAG**: Web Content Accessibility Guidelines
- **E2E**: End-to-End (testing)
- **DTO**: Data Transfer Object
- **CORS**: Cross-Origin Resource Sharing

### C. References

- [.NET 9 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [React Documentation](https://react.dev/)
- [Tailwind CSS](https://tailwindcss.com/)
- [shadcn/ui](https://ui.shadcn.com/)
- [TanStack Query](https://tanstack.com/query/)
- [Playwright](https://playwright.dev/)
- [JWT.io](https://jwt.io/)

### D. Future Enhancements

**Phase 2 Considerations** (not in current scope):
- Real-time balance updates via WebSockets
- Transaction search and advanced filtering
- Scheduled/recurring transfers
- Export transactions to CSV/PDF
- Mobile app (React Native)
- Multi-currency support
- Bill pay integration
- Budget tracking features
- Notifications system
- Dark mode toggle (currently always dark)

---

**End of Specification**
