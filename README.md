# Home Banking Application

![Backend CI](https://github.com/rjf-itsector/spec-driven-homebanking/actions/workflows/backend.yml/badge.svg)
![Frontend CI](https://github.com/rjf-itsector/spec-driven-homebanking/actions/workflows/frontend.yml/badge.svg)
![E2E Tests](https://github.com/rjf-itsector/spec-driven-homebanking/actions/workflows/e2e.yml/badge.svg)

A modern, full-stack home banking application built with a **spec-driven development** approach, demonstrating production-ready patterns with .NET 9, React 19, and TypeScript.

## Features

- **Secure Authentication** — Mock JWT-based auth with protected routes and token management
- **Account Management** — View multiple account types (Checking, Savings, Credit Card, Investment)
- **Transaction History** — Infinite scroll pagination with colour-coded category badges
- **Money Transfers** — Validated transfers between accounts with optimistic UI updates
- **Responsive Design** — Mobile-first layout with Tailwind CSS v4 and shadcn/ui
- **Error Handling** — Error boundaries, global mutation error toasts, and API error mapping
- **Comprehensive Testing** — Unit, integration, and E2E tests (Vitest + Playwright)
- **CI/CD Pipeline** — Automated testing with GitHub Actions
- **AI Banking Agent** — Conversational assistant powered by Azure AI Foundry (GPT-4o)

## AI Banking Agent

The application includes an AI-powered banking assistant that provides conversational access to account information, transaction search, spending insights, and transfer operations.

### Agent Capabilities

- **Account Inquiries**: "What's my checking account balance?" / "Show me all my accounts"
- **Transaction Search**: "Find my transactions from Whole Foods" / "Show me all transactions over $100"
- **Spending Insights**: "Break down my spending by category" / "What's my biggest spending category?"
- **Anomaly Detection**: "Any unusual transactions recently?"
- **Transfer Assistance**: "Transfer $100 from checking to savings" (requires confirmation)

### Configuration

The agent requires an Azure AI Foundry project with a GPT-4o (or compatible) model deployment.

1. Create an Azure AI Foundry project at [ai.azure.com](https://ai.azure.com)
2. Deploy a GPT-4o or GPT-4o-mini model
3. Configure the connection in `src/api/HomeBanking.API/appsettings.json`:

```json
{
  "AzureAI": {
    "ProjectEndpoint": "https://your-project.services.ai.azure.com",
    "ModelDeploymentName": "gpt-4o"
  }
}
```

### Running Without Azure AI

The application works fully without Azure AI configured — the agent chat endpoint will return 503, and the chat widget will display "Agent temporarily unavailable." All other features (accounts, transactions, transfers) work normally.

### Health Checks

- `GET /health` — Overall application health (Healthy even if agent is Degraded)
- `GET /health/agent` — Agent subsystem health (Healthy/Degraded with details)

## Tech Stack

| Layer    | Technology                              | Path       |
|----------|-----------------------------------------|------------|
| Backend  | .NET 9 Web API (Clean Architecture)     | `src/api/` |
| Frontend | React 19 + Vite 7 + TypeScript 5.9      | `src/web/` |
| E2E      | Playwright                              | `e2e/`     |

### Backend Libraries

- Entity Framework Core 9 (InMemory provider)
- BCrypt.Net for password hashing
- Scalar for API documentation UI

### Frontend Libraries

- @tanstack/react-query 5 — Server state management
- react-router-dom 7 — Client-side routing
- react-hook-form 7 + zod 4 — Form handling and validation
- Tailwind CSS v4 + shadcn/ui (new-york) — UI framework
- sonner — Toast notifications
- lucide-react — Icons

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- Git

## Setup & Running

### Clone Repository

```bash
git clone git@github.com:rjf-itsector/spec-driven-homebanking.git
cd spec-driven-homebanking
```

### Backend

```bash
cd src/api
dotnet restore
dotnet build
dotnet run --project HomeBanking.API
```

Backend runs at: **http://localhost:5000**
API docs (Scalar): **http://localhost:5000/scalar/v1**
Health check: **http://localhost:5000/health**

### Frontend

```bash
cd src/web
npm install
npm run dev
```

Frontend runs at: **http://localhost:5173**

## Demo Credentials

| Field    | Value         |
|----------|---------------|
| Email    | demo@bank.com |
| Password | Demo123!      |

## Architecture

The backend follows **Clean Architecture** with four layers:

```
HomeBanking.API           → Controllers, middleware, DI configuration
HomeBanking.Core          → Entities, interfaces, business services
HomeBanking.Infrastructure → EF Core DbContext, repositories, data seeder
HomeBanking.Contracts     → Request/Response DTOs (shared boundary)
```

The frontend uses a **feature-based** structure:

```
src/
├── components/
│   ├── features/       → Domain components (accounts, transactions, transfers)
│   ├── layouts/        → DashboardLayout, ErrorBoundary, ProtectedRoute
│   └── ui/             → shadcn/ui primitives
├── lib/
│   ├── api/            → API client, typed endpoints, error handling
│   ├── hooks/          → useAuth, useInfiniteTransactions
│   └── utils/          → Formatters, validators
└── pages/              → Route-level page components
```

See [docs/architecture/](docs/architecture/) for architectural decisions:
- [ADR-001: Mock JWT Auth](docs/architecture/ADR-001-mock-jwt-auth.md)
- [ADR-002: AI Agent Architecture](docs/architecture/ADR-002-ai-agent-architecture.md)

## Testing

### Backend Tests

```bash
cd src/api
dotnet test --verbosity normal
dotnet test --collect:"XPlat Code Coverage"   # with coverage
```

### Frontend Unit Tests

```bash
cd src/web
npx vitest run                  # all tests
npx vitest run --coverage       # with coverage report
```

### E2E Tests

```bash
cd e2e
npm install
npx playwright install --with-deps
npx playwright test             # requires backend + frontend running
```

### Validation Script

```bash
./scripts/validate.sh           # all checks
./scripts/validate.sh backend   # backend only
./scripts/validate.sh frontend  # frontend only
```

## Project Structure

```
├── .github/workflows/    # CI pipelines (backend, frontend, e2e)
├── src/
│   ├── api/              # .NET 9 Web API (Clean Architecture)
│   └── web/              # React + Vite + TypeScript SPA
├── e2e/                  # Playwright E2E tests
├── docs/
│   ├── api/              # API documentation
│   └── architecture/     # Architecture Decision Records
├── scripts/
│   └── validate.sh       # Quality-gate script
├── spec.md               # Application specification
└── plan.md               # Implementation plan (30 tasks)
```

## Limitations

- **InMemory Database** — Data resets on API restart (by design for demo)
- **Mock Authentication** — Hardcoded demo credentials, no real user management
- **Single Currency** — All accounts use EUR
- **No Persistence** — Not suitable for production use

## Documentation

- [Application Specification](spec.md)
- [Agent Specification](spec-agent.md)
- [Implementation Plan](plan.md)
- [Agent Implementation Plan](plan-agent.md)
- [Architecture Decisions](docs/architecture/)
- [Agent Chat API](docs/api/agent-chat.md)
