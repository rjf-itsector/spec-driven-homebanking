# ADR-002: AI Agent Architecture

**Status**: Accepted  
**Date**: 2026-02-17  
**Deciders**: Engineering Team

## Context

The Home Banking application needed an AI-powered conversational assistant to help users with account inquiries, transaction search, spending insights, and transfer operations. Key decisions were required on architecture, state management, tool invocation pattern, and safety mechanisms.

## Decisions

### D1: In-Process .NET Agent (vs. Python Microservice)

**Decision**: Integrate the agent directly into the existing .NET API using Azure.AI.Projects SDK.

**Rationale**:
- **Shared data access**: Agent tools query HomeBankingDbContext directly — zero network overhead
- **Single deployment**: No additional service to host, monitor, or deploy
- **Shared auth**: Reuses existing JWT authentication middleware
- **Code reuse**: Same entity models, validation logic, and error patterns
- **Simpler CI/CD**: One build pipeline, one artifact

**Trade-offs**:
- (+) In-process tool execution is fast and reliable
- (+) Consistent logging and error handling
- (-) Agent lifecycle coupled to API lifecycle
- (-) Azure.AI.Projects SDK is prerelease (API may change)

### D2: Stateless Conversation (Client-Side History)

**Decision**: Conversation history is maintained client-side and sent with each request. Server stores no session state.

**Rationale**:
- **Horizontal scaling**: Any API instance can handle any request
- **Privacy**: Conversation data lives only in the user's browser
- **Simplicity**: No session store, cache, or sticky sessions needed
- **Consistency**: InMemory database is already ephemeral by design

**Implementation**:
- Client sends conversation history (max 20 messages) with each request
- Server truncates to 20 messages if exceeded
- System prompt is server-side only, never exposed to client
- History cleared on page refresh or logout

### D3: Function Calling for Tool Invocation

**Decision**: Use Azure AI Foundry's native function calling to let the model decide which tools to invoke.

**Rationale**:
- **Natural routing**: Model determines tool based on user intent
- **Multi-tool turns**: Model can call multiple tools in sequence per turn
- **Type safety**: Tool parameters defined as JSON schemas, validated server-side
- **Extensibility**: New tools added by registering functions without changing orchestration

**Tools implemented**:
| Tool | Purpose |
|------|---------|
| `get_account_balances` | Retrieve all account balances |
| `get_account_details` | Get details for a specific account type |
| `search_transactions` | Search with filters (description, category, amount, dates) |
| `get_spending_summary` | Category-wise spending aggregation |
| `detect_unusual_transactions` | Flag transactions above category average |
| `execute_transfer` | Create transfers between accounts (requires confirmation) |

### D4: Explicit Transfer Confirmation

**Decision**: Agent always requires explicit user confirmation before executing any transfer.

**Rationale**:
- **Safety**: Financial operations must never be accidental
- **Trust**: Users maintain full control over money movement
- **Auditability**: Two-step flow creates clear intent record
- **Industry standard**: All banking chatbots require confirmation for transfers

**Implementation**: System prompt instructs the model to always present transfer details and ask for confirmation before calling `execute_transfer`.

### D5: Graceful Degradation

**Decision**: Agent features degrade gracefully when Azure AI Foundry is unavailable. Core banking functionality is never affected.

**Rationale**:
- Agent is supplementary, not essential
- Demo environment may not always have Azure AI configured
- Health checks report agent as "Degraded" (not "Unhealthy")

## Consequences

- Agent features require Azure AI Foundry configuration to function
- Without Azure AI, the chat endpoint returns 503 and the widget shows "unavailable" message
- All existing functionality works independently of agent status
- Conversation context is lost on page refresh (acceptable for demo)

## References

- [spec-agent.md](../../spec-agent.md) — Full technical specification
- [plan-agent.md](../../plan-agent.md) — Implementation plan
- [ADR-001](ADR-001-mock-jwt-auth.md) — Mock JWT authentication decision
