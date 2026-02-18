# Home Banking AI Agent - Implementation Plan

**Version**: 1.0.0  
**Date**: February 17, 2026  
**Parent Plan**: [plan.md](plan.md)  
**Spec**: [spec-agent.md](spec-agent.md)

---

## Prerequisites

Before starting, ensure these are in place:

- [x] Azure AI Foundry project exists with a model deployment (GPT-4o or GPT-4o-mini)
- [x] Foundry project endpoint and deployment name available
- [x] Existing homebanking app is fully functional (all 30 tasks from plan.md completed)
- [x] .NET 9 SDK installed, `dotnet build src/api` succeeds
- [x] Node.js 20+ installed, `npm run build` in src/web succeeds

---

## Definition of Done (Agent Tasks)

> Inherits from the Global DoD in [plan.md](plan.md) with these additions:

- [x] Agent-specific code compiles with zero warnings
- [x] Agent tools have unit tests with ≥80% coverage
- [x] Agent endpoint returns proper errors when Azure AI Foundry is unavailable
- [x] Chat widget renders correctly on mobile (320px) and desktop (1280px)
- [x] No PII in application logs from agent interactions
- [x] `scripts/validate.sh` passes (existing gates still green)

---

## Phase A1: Agent Backend Foundation

### Task TA01: Add Agent NuGet Packages and Configuration
- **Status**: pending
- **Dependencies**: All plan.md tasks complete
- **Estimate**: S (2-4h)
- **Description**: Install Microsoft Agent Framework NuGet packages and configure Azure AI Foundry connection settings.
- **DoD**:
  - [x] NuGet packages added to HomeBanking.API.csproj:
    - `Microsoft.Agents.AI.AzureAI --prerelease`
    - `Microsoft.Agents.AI.OpenAI --prerelease`
    - `Microsoft.Agents.AI.Workflows --prerelease`
    - `Azure.AI.Projects` (latest stable)
    - `Azure.Identity` (latest stable)
  - [x] `appsettings.json` updated with `AzureAI` configuration section:
    ```json
    {
      "AzureAI": {
        "ProjectEndpoint": "",
        "ModelDeploymentName": ""
      }
    }
    ```
  - [x] `appsettings.Development.json` populated with placeholder values (or real values if available)
  - [x] `.env.example` created documenting required environment variables
  - [x] Solution builds with zero warnings
  - [x] Existing tests still pass
- **Plan changes**: —

---

### Task TA02: Implement Agent Tool Functions
- **Status**: pending
- **Dependencies**: TA01
- **Estimate**: M (4-8h)
- **Description**: Create C# tool functions that the agent model will invoke via function calling. These operate directly on HomeBankingDbContext.
- **DoD**:
  - [x] New folder: `HomeBanking.Core/Services/AgentTools/`
  - [x] `IAgentToolService` interface in `HomeBanking.Core/Interfaces/`
  - [x] `AgentToolService` implementation with methods:
    - `GetAccountBalancesAsync(Guid userId)` — returns all account balances
    - `GetAccountDetailsAsync(Guid userId, string accountType)` — returns single account details with transaction count
    - `SearchTransactionsAsync(Guid userId, TransactionSearchParams)` — search with filters (description, category, amount range, dates, limit)
    - `GetSpendingSummaryAsync(Guid userId, SpendingSummaryParams)` — category aggregation for date range
    - `DetectUnusualTransactionsAsync(Guid userId, decimal threshold)` — finds transactions > threshold × category average
    - `ExecuteTransferAsync(Guid userId, TransferParams)` — creates transfer (reuses existing transfer logic)
  - [x] `TransactionSearchParams`, `SpendingSummaryParams`, `TransferParams` DTOs created in `HomeBanking.Contracts/Requests/`
  - [x] Tool response DTOs created in `HomeBanking.Contracts/Responses/`
  - [x] Service registered in DI container (`Program.cs`)
  - [x] XML doc comments on all public types and methods
  - [x] Solution builds with zero warnings
- **Plan changes**: —

---

### Task TA03: Unit Tests for Agent Tools
- **Status**: pending
- **Dependencies**: TA02
- **Estimate**: M (4-8h)
- **Description**: Comprehensive unit tests for all agent tool functions.
- **DoD**:
  - [x] New test file: `HomeBanking.Core.Tests/AgentToolServiceTests.cs`
  - [x] Tests for `GetAccountBalances`: returns all active accounts, correct balances, empty when no accounts
  - [x] Tests for `GetAccountDetails`: correct account found by type, returns 404-equivalent for missing type
  - [x] Tests for `SearchTransactions`:
    - Filter by description (case-insensitive)
    - Filter by category
    - Filter by amount range (min/max)
    - Filter by date range
    - Combined filters
    - Limit enforcement (default 10, max 20)
    - Empty results
  - [x] Tests for `GetSpendingSummary`:
    - Correct category totals and counts
    - Percentage calculations
    - Default date range (30 days)
    - Custom date range
    - Average daily spending calculation
  - [x] Tests for `DetectUnusualTransactions`:
    - Identifies transactions > 2x category average
    - Custom threshold
    - No unusual transactions returns empty list
    - Single transaction in category (not flagged)
  - [x] Tests for `ExecuteTransfer`:
    - Successful transfer updates both balances
    - Insufficient funds error
    - Same-account error
    - Account not found error
    - Amount validation (> 0, ≤ 1,000,000)
  - [x] All tests pass, ≥80% coverage on AgentToolService
- **Plan changes**: —

---

### Task TA04: BankingAgentService (Orchestration Layer)
- **Status**: pending
- **Dependencies**: TA02
- **Estimate**: M (4-8h)
- **Description**: Create the agent orchestration service that manages the system prompt, tool registration, and conversation flow using Microsoft Agent Framework.
- **DoD**:
  - [x] New file: `HomeBanking.Infrastructure/Services/BankingAgentService.cs`
  - [x] `IBankingAgentService` interface in `HomeBanking.Core/Interfaces/`
  - [x] Service responsibilities:
    - Initialize Azure AI Foundry client with configured endpoint and deployment
    - Register all 6 agent tools with their JSON schemas
    - Manage system prompt (hardcoded server-side, per spec-agent.md)
    - Build message array: system prompt + conversation history + new user message
    - Execute agent turn: send to model → process tool calls → return response
    - Handle multi-step tool calling (model may call multiple tools per turn)
    - Truncate conversation history to 20 messages max
  - [x] Returns `AgentChatResponse` DTO with: response text, tools used list, token usage
  - [x] Handles Azure AI Foundry errors gracefully (timeout, unavailable) → throws typed exception
  - [x] Registered as scoped service in DI container
  - [x] Structured logging: tool calls, response time, token counts (no PII)
  - [x] XML doc comments
  - [x] Solution builds with zero warnings
- **Plan changes**: —

---

### Task TA05: AgentController (REST Endpoint)
- **Status**: pending
- **Dependencies**: TA04
- **Estimate**: S (2-4h)
- **Description**: Create the REST endpoint for agent chat interactions.
- **DoD**:
  - [x] New file: `HomeBanking.API/Controllers/AgentController.cs`
  - [x] Endpoint: `POST /api/v1/agent/chat`
  - [x] Decorated with `[Authorize]` (reuses existing JWT auth)
  - [x] Request validation:
    - `message` required, 1-500 characters
    - `conversationHistory` optional, max 20 items
    - Each history entry: `role` (user/assistant) + `content` (max 2000 chars)
  - [x] Extracts userId from JWT claims (same pattern as other controllers)
  - [x] Calls `IBankingAgentService.ChatAsync(userId, message, history)`
  - [x] Returns 200 with `AgentChatResponse`
  - [x] Returns 400 for validation errors
  - [x] Returns 429 for rate limiting (10 req/min per user)
  - [x] Returns 503 when Azure AI Foundry is unreachable
  - [x] Request/response DTOs in `HomeBanking.Contracts/`
  - [x] XML doc comments, OpenAPI annotations
  - [x] Solution builds with zero warnings
- **Plan changes**: —

---

### Task TA06: Agent Integration Tests
- **Status**: pending
- **Dependencies**: TA05
- **Estimate**: M (4-8h)
- **Description**: Integration tests for the agent endpoint using TestWebApplicationFactory.
- **DoD**:
  - [x] New file: `HomeBanking.API.Tests/AgentControllerIntegrationTests.cs`
  - [x] Tests (with mocked Azure AI Foundry client):
    - `POST /api/v1/agent/chat` returns 401 without JWT
    - `POST /api/v1/agent/chat` returns 400 for empty message
    - `POST /api/v1/agent/chat` returns 400 for message > 500 chars
    - `POST /api/v1/agent/chat` returns 400 for > 20 history items
    - `POST /api/v1/agent/chat` returns 200 with valid request (mocked model response)
    - `POST /api/v1/agent/chat` returns 503 when model client throws
    - Rate limiting returns 429 after 10 requests
  - [x] Agent model/Foundry client mocked at the DI level in TestWebApplicationFactory
  - [x] All existing tests still pass
  - [x] All new tests pass
- **Plan changes**: —

---

## Phase A2: Agent Frontend

### Task TA07: Chat Widget Components
- **Status**: pending
- **Dependencies**: TA05
- **Estimate**: M (4-8h)
- **Description**: Build the React chat widget UI components.
- **DoD**:
  - [x] New folder: `src/web/src/components/features/agent/`
  - [x] Components created:
    - `ChatWidget.tsx` — floating button + panel container, open/close state
    - `ChatPanel.tsx` — panel layout (header, message area, input)
    - `ChatMessageList.tsx` — scrollable message list, auto-scroll to bottom
    - `ChatMessage.tsx` — message bubble, user (right-aligned, blue) vs assistant (left-aligned, gray)
    - `ChatInput.tsx` — text input + send button, Enter to send, disabled while loading
    - `ChatThinking.tsx` — animated dots indicator while agent is processing
  - [x] Markdown rendering in agent messages (react-markdown + remark-gfm)
  - [x] `npm install react-markdown remark-gfm` (new dependencies)
  - [x] Responsive: 400px panel on desktop, full-width on mobile
  - [x] Accessible: ARIA labels, keyboard navigation, focus management
  - [x] Uses shadcn/ui primitives (Button, Card, ScrollArea, Input)
  - [x] Tailwind styling consistent with existing app theme
  - [x] TSDoc on exported components
  - [x] Frontend builds with zero warnings
  - [x] Frontend lint passes
- **Plan changes**: —

---

### Task TA08: useChatAgent Hook and API Integration
- **Status**: pending
- **Dependencies**: TA07
- **Estimate**: S (2-4h)
- **Description**: Create the React hook for agent state management and API communication.
- **DoD**:
  - [x] New file: `src/web/src/lib/hooks/useChatAgent.ts`
  - [x] New file: `src/web/src/lib/api/agent.ts` (API client function)
  - [x] Hook manages:
    - `messages: ChatMessage[]` — accumulated conversation
    - `isOpen: boolean` — widget open/close state
    - `isLoading: boolean` — request in flight
    - `error: string | null` — last error message
  - [x] `sendMessage(text: string)` function:
    - Appends user message to state
    - Calls `POST /api/v1/agent/chat` with message + conversation history
    - Appends assistant response to state
    - Handles errors (network, 429, 503) with user-friendly messages
  - [x] `clearChat()` function — resets messages
  - [x] `toggleOpen()` function — open/close panel
  - [x] Conversation history limited to last 20 messages in API payload
  - [x] API client uses existing axios instance (from `src/web/src/lib/api/client.ts`)
  - [x] TypeScript types for request/response DTOs
  - [x] TSDoc comments
  - [x] Frontend builds with zero warnings
- **Plan changes**: —

---

### Task TA09: Integrate Chat Widget into App Layout
- **Status**: pending
- **Dependencies**: TA08
- **Estimate**: S (2-4h)
- **Description**: Mount the ChatWidget in the authenticated app layout so it appears on all protected pages.
- **DoD**:
  - [x] ChatWidget rendered inside the authenticated layout (DashboardLayout or equivalent)
  - [x] Widget only visible when user is authenticated
  - [x] Widget does NOT appear on the login page
  - [x] Chat state resets on logout
  - [x] Widget does not interfere with existing UI (proper z-index, no layout shift)
  - [x] Tested on mobile (320px), tablet (768px), desktop (1280px) viewports
  - [x] Frontend builds with zero warnings
  - [x] Existing frontend tests still pass
- **Plan changes**: —

---

### Task TA10: Frontend Unit Tests for Agent Components
- **Status**: pending
- **Dependencies**: TA09
- **Estimate**: M (4-8h)
- **Description**: Unit tests for all agent-related React components and hooks.
- **DoD**:
  - [x] New folder: `src/web/src/components/features/agent/__tests__/`
  - [x] Test files:
    - `ChatWidget.test.tsx` — renders button, opens/closes panel on click
    - `ChatMessage.test.tsx` — renders user vs assistant styling, renders markdown
    - `ChatInput.test.tsx` — character limit (500), empty message prevented, send on Enter, disabled when loading
    - `ChatThinking.test.tsx` — renders indicator
    - `useChatAgent.test.ts` — sendMessage appends messages, loading state, error state, clearChat resets, history limit
  - [x] Mocked API calls (MSW or vi.mock)
  - [x] All tests pass
  - [x] Coverage ≥ 80% on agent components
  - [x] Existing frontend tests still pass
- **Plan changes**: —

---

## Phase A3: End-to-End & Polish

### Task TA11: Agent E2E Tests
- **Status**: pending
- **Dependencies**: TA09, TA06
- **Estimate**: M (4-8h)
- **Description**: Playwright E2E tests for the agent chat flow.
- **DoD**:
  - [x] New file: `e2e/tests/agent.spec.ts`
  - [x] Tests:
    - Chat widget button visible on dashboard after login
    - Chat widget NOT visible on login page
    - Open chat → send message → response appears (requires running agent backend with mocked model or real Foundry)
    - Send "What's my checking balance?" → response contains account balance
    - Close and reopen chat → messages preserved (within session)
    - Mobile viewport → chat panel is full-width
  - [x] E2E test configuration handles agent endpoint availability
  - [x] All existing E2E tests still pass
- **Plan changes**: —

---

### Task TA12: Agent Health Check and Error Handling
- **Status**: pending
- **Dependencies**: TA04
- **Estimate**: S (2-4h)
- **Description**: Add agent-specific health checks and ensure graceful degradation.
- **DoD**:
  - [x] Health check for Azure AI Foundry connectivity added to `/health` endpoint
  - [x] Agent health reported as a sub-component (degraded, not failing, if Foundry is down)
  - [x] Main app health check remains `Healthy` even if agent is `Degraded`
  - [x] Chat widget shows "Agent temporarily unavailable" banner when 503 received
  - [x] Widget remains interactive (user can retry) when agent is degraded
  - [x] Structured logging for agent health transitions
  - [x] Unit test for health check behavior
- **Plan changes**: —

---

### Task TA13: Documentation and ADR
- **Status**: pending
- **Dependencies**: TA11, TA12
- **Estimate**: S (2-4h)
- **Description**: Update project documentation for the agent feature.
- **DoD**:
  - [x] `README.md` updated with:
    - Agent feature description
    - Azure AI Foundry configuration instructions
    - Environment variables needed
    - How to run with/without agent
  - [x] `docs/architecture/ADR-002-ai-agent-architecture.md` created with:
    - Decision: .NET in-process agent vs. Python microservice
    - Decision: Stateless conversation (client-side history)
    - Decision: Function calling for tool invocation
    - Decision: Explicit transfer confirmation
  - [x] API documentation updated with agent chat endpoint
  - [x] Inline code comments for non-obvious agent logic
- **Plan changes**: —

---

## Task Dependency Graph

```
TA01 (NuGet + Config)
 └── TA02 (Agent Tools)
      ├── TA03 (Tool Unit Tests)
      └── TA04 (BankingAgentService)
           ├── TA05 (AgentController)
           │    ├── TA06 (Integration Tests)
           │    ├── TA07 (Chat Widget UI)
           │    │    └── TA08 (useChatAgent Hook)
           │    │         └── TA09 (Mount in Layout)
           │    │              └── TA10 (Frontend Tests)
           │    └── TA11 (E2E Tests) ← also needs TA09
           └── TA12 (Health Check)
                └── TA13 (Docs + ADR) ← also needs TA11
```

---

## Estimates Summary

| Task | Phase | Size | Est. Hours |
|------|-------|------|------------|
| TA01 | A1 | S | 2-4h |
| TA02 | A1 | M | 4-8h |
| TA03 | A1 | M | 4-8h |
| TA04 | A1 | M | 4-8h |
| TA05 | A1 | S | 2-4h |
| TA06 | A1 | M | 4-8h |
| TA07 | A2 | M | 4-8h |
| TA08 | A2 | S | 2-4h |
| TA09 | A2 | S | 2-4h |
| TA10 | A2 | M | 4-8h |
| TA11 | A3 | M | 4-8h |
| TA12 | A3 | S | 2-4h |
| TA13 | A3 | S | 2-4h |
| **Total** | | | **38-72h** |

---

## Risk Register

| Risk | Impact | Likelihood | Mitigation |
|------|--------|-----------|------------|
| Microsoft.Agents.AI prerelease API changes | M | H | Pin exact NuGet versions; isolate agent code in separate service classes |
| Azure AI Foundry rate limits / quota | M | M | Implement retry with backoff; use GPT-4o-mini for lower cost |
| Model hallucinations (incorrect tool calls) | M | M | Strong system prompt; validate all tool inputs server-side |
| Token costs in demo environment | L | M | Cap token budget per request; use efficient system prompt |
| Chat widget performance on mobile | L | L | Lazy-load widget; minimize re-renders; virtual scroll if needed |

---

## Plan Change Log

| Date | Source Task | Change Description |
|------|-----------|-------------------|
| 2026-02-17 | — | Initial agent plan created (v1.0.0) |
