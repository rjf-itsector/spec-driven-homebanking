# Home Banking Application

A **demo** home-banking application built with a spec-driven development approach.

## Tech Stack

| Layer    | Technology                  | Path       |
|----------|-----------------------------|------------|
| Backend  | .NET 9 Web API              | `src/api/` |
| Frontend | React + Vite + TypeScript   | `src/web/` |
| E2E      | *(coming soon)*             | `e2e/`     |

## Project Structure

```
├── src/
│   ├── api/          # .NET 9 Web API
│   └── web/          # React + Vite + TypeScript SPA
├── e2e/              # End-to-end tests
├── docs/
│   ├── api/          # API documentation
│   └── architecture/ # Architecture decision records
├── scripts/
│   └── validate.sh   # Quality-gate script (single source of truth)
├── spec.md           # Application specification
└── plan.md           # Implementation plan
```

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js ≥ 18](https://nodejs.org/)

### Run Validation

```bash
./scripts/validate.sh          # all checks
./scripts/validate.sh backend  # backend only
./scripts/validate.sh frontend # frontend only
```

## Documentation

- [Specification](spec.md)
- [Implementation Plan](plan.md)
