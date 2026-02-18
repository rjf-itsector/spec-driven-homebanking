# Agent Chat API

## POST /api/v1/agent/chat

Send a message to the AI banking agent.

### Authentication

Requires a valid JWT Bearer token (same as all other API endpoints).

### Request Body

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `message` | string | Yes | 1-500 characters |
| `conversationHistory` | array | No | Max 20 items |
| `conversationHistory[].role` | string | Yes | `"user"` or `"assistant"` |
| `conversationHistory[].content` | string | Yes | 1-2000 characters |

#### Example Request

```json
{
  "message": "What's my checking account balance?",
  "conversationHistory": [
    { "role": "user", "content": "Hi" },
    { "role": "assistant", "content": "Hello! How can I help you with your banking today?" }
  ]
}
```

### Responses

#### 200 OK

```json
{
  "response": "Your checking account (****1234) has a balance of **$5,250.00**.",
  "toolsUsed": ["get_account_balances"],
  "tokenUsage": {
    "input": 450,
    "output": 35
  }
}
```

| Field | Type | Description |
|-------|------|-------------|
| `response` | string | Agent's natural language response (may contain markdown) |
| `toolsUsed` | string[] | Tools the agent called during this turn |
| `tokenUsage.input` | number | Input tokens consumed |
| `tokenUsage.output` | number | Output tokens generated |

#### 400 Bad Request

Returned for validation errors (empty message, message too long, invalid history).

```json
{
  "message": "Message must not exceed 500 characters"
}
```

#### 401 Unauthorized

Missing or invalid JWT token.

#### 429 Too Many Requests

Rate limit exceeded (10 requests per minute per user).

```json
{
  "message": "Rate limit exceeded. Try again in 60 seconds."
}
```

Includes `Retry-After: 60` header.

#### 503 Service Unavailable

Azure AI Foundry is unreachable or not configured.

```json
{
  "message": "Agent service is temporarily unavailable. Please try again later."
}
```
