# ADR-001: Mock JWT Authentication for Demo Application

## Status

Accepted

## Date

2025-01-01

## Context

This application is a **demo/showcase** project. We need authentication to demonstrate protected routes, token management, and realistic user flows — but we don't need (or want) a real identity provider, OAuth integration, or persistent user store.

Key constraints:

- The API uses an **InMemory database** that resets on every restart
- There is a single demo user with hardcoded credentials
- The application must be runnable with zero external dependencies (no databases, no identity servers)
- E2E tests need deterministic, repeatable login flows

## Decision

We implement a **mock JWT authentication** system:

### Backend

- The `AuthController.Login` endpoint validates against hardcoded credentials (`demo@bank.com` / `Demo123!`)
- On success, it returns a simple token string (not a real JWT) and user info
- The `AuthenticationMiddleware` checks for a `Bearer` token in the `Authorization` header but does not validate JWT signatures
- The token is a fixed string — no expiry, no claims parsing

### Frontend

- Credentials are stored in `localStorage` (`hb_token` and `hb_user`)
- The `useAuth` hook provides `login`, `logout`, `isAuthenticated`, and `user`
- The `AuthProvider` wraps the app and reads stored auth on mount
- `ProtectedRoute` redirects unauthenticated users to `/login`
- The API client (`apiClient`) automatically attaches the `Authorization: Bearer <token>` header

### Why Not Real JWT?

- **Simplicity**: No need for `System.IdentityModel.Tokens.Jwt`, key management, or token refresh
- **Demo-friendly**: Anyone can log in without registration
- **Deterministic**: E2E tests always use the same credentials
- **Zero config**: No environment variables or secrets needed

## Consequences

### Positive

- Application starts instantly with no external dependencies
- E2E tests are fully deterministic
- Authentication flow is realistic enough to demonstrate protected routes, redirects, and token-based API calls
- Code structure mirrors production patterns (middleware, hooks, protected routes)

### Negative

- Not suitable for production — no real security
- No token expiry or refresh flow demonstrated
- Password is stored as a BCrypt hash in the seeder but the comparison is done against a hardcoded string (the hash is for demonstration only)
- No role-based access control

## Alternatives Considered

1. **Real JWT with signing keys** — Rejected: too much complexity for a demo, requires key management
2. **Cookie-based sessions** — Rejected: doesn't demonstrate Bearer token patterns common in SPAs
3. **No authentication** — Rejected: can't demonstrate protected routes or auth state management
4. **OAuth/OIDC with external provider** — Rejected: requires external infrastructure, not self-contained
