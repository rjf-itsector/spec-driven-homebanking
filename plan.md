# Home Banking Application - Implementation Plan

**Version**: 1.1.0  
**Date**: February 17, 2026  
**Project**: Spec-Driven Home Banking Demo

---

## Global Definition of Done (DoD)

> **Every task MUST satisfy ALL criteria below before it can be marked completed.**  
> This DoD is the universal quality gate — no exceptions, no shortcuts.

### 1. Build & Lint: Zero Tolerance

| Check | Local Command | CI Step |
|-------|--------------|---------|
| Backend build (no warnings) | `dotnet build --configuration Release --warnaserror` | Same |
| Backend lint / format | `dotnet format --verify-no-changes` | Same |
| Frontend build (no warnings) | `cd frontend && npm run build` | Same |
| Frontend lint (zero errors) | `cd frontend && npm run lint` | Same |
| Frontend type check | `cd frontend && npx tsc --noEmit` | Same |

- **Zero lint errors** from both `dotnet format` and `eslint`.
- **Zero build warnings** — `--warnaserror` is enforced on .NET; the Vite build must produce no warnings.

### 2. Testing: Complete & Green

| Check | Local Command | CI Step |
|-------|--------------|---------|
| Backend unit/integration tests | `dotnet test --configuration Release --no-build --verbosity normal` | Same |
| Frontend unit tests | `cd frontend && npm run test:coverage` | Same |
| E2E tests (when applicable) | `cd e2e && npx playwright test` | Same |

- **New functionality MUST have new tests** — unit tests at minimum; integration/E2E where appropriate.
- **ALL tests pass** — both new and existing. A single failure blocks completion.
- Test coverage must not decrease. New code should aim for ≥80% line coverage.

### 3. Documentation & Comments

- XML doc comments on all public C# types and members.
- JSDoc/TSDoc on exported TypeScript functions and components.
- Update relevant docs (README, API docs, architecture docs) when behaviour changes.
- Inline comments for non-obvious logic only (prefer self-documenting code).

### 4. Plan Maintenance

After **every** task completion:

1. **Update task status** in this plan (`pending` → `done`; checkboxes ticked).
2. **Fill the "Plan changes" field** on the completed task — even if "none".
3. **Review ALL upcoming tasks** and evaluate whether the completed work caused changes (new dependencies, scope adjustments, obsoleted tasks, new tasks).
4. **Document every plan change** in detail:
   - Which task triggered the change.
   - What changed and why.
   - Any new tasks added or existing tasks modified.
5. **Commit** with message format: `T<NNN>: <short description>`.

### 5. Local ↔ CI Parity (Critical)

> **The local validation gate and the CI pipeline MUST be identical.**  
> Same commands. Same flags. Same thresholds. If it passes locally it passes on CI, and vice versa.

All validation commands are centralized in a single gate script (`scripts/validate.sh`) that is invoked both:
- **Locally** by the developer/AI agent before marking a task done.
- **In CI** by every GitHub Actions workflow.

```bash
#!/usr/bin/env bash
# scripts/validate.sh — Single source of truth for quality gates
set -euo pipefail

echo "=== Backend: Restore ==="
dotnet restore backend/

echo "=== Backend: Build (warnings-as-errors) ==="
dotnet build backend/ --configuration Release --warnaserror --no-restore

echo "=== Backend: Format Check ==="
dotnet format backend/ --verify-no-changes

echo "=== Backend: Tests ==="
dotnet test backend/ --configuration Release --no-build --verbosity normal

echo "=== Frontend: Install ==="
(cd frontend && npm ci)

echo "=== Frontend: Lint ==="
(cd frontend && npm run lint)

echo "=== Frontend: Type Check ==="
(cd frontend && npx tsc --noEmit)

echo "=== Frontend: Tests ==="
(cd frontend && npm run test:coverage)

echo "=== Frontend: Build ==="
(cd frontend && npm run build)

echo "✅ All gates passed."
```

CI workflows call this same script — they do NOT duplicate commands inline. This eliminates local/CI drift.

### 6. CI Must Be Green

- A task is **NOT done** until the pushed commit has **all GitHub Actions workflows passing green**.
- If CI fails on push, the task reopens — fix, re-validate locally, push again.
- The DoD checklist on each task implicitly includes "CI green" even if not listed per-task.

### 7. AI-Assisted Development Guardrails

These additional practices apply when tasks are executed by AI agents:

- **Spec fidelity**: Generated code must conform to `spec.md`. Any deviation must be flagged and approved.
- **Deterministic validation**: AI agents run `scripts/validate.sh` (the same gate script) before declaring a task done. No self-assessment — only automated tooling counts.
- **Incremental commits**: Each task = one atomic commit. No multi-task mega-commits.
- **Diff review**: After code generation, the agent must review its own diff for unintended changes (e.g., deleted code, leftover debug statements, hallucinated imports).
- **Regression vigilance**: If a previously-passing test breaks, the root cause must be identified and fixed before proceeding — never delete or skip a test to make CI green.
- **Context window discipline**: Large tasks should be broken into sub-steps. If a task's scope exceeds what can be reliably tracked in a single context, split it.

### DoD Checklist (copy into each task)

```markdown
- [ ] Code compiles with zero warnings (`--warnaserror`)
- [ ] Zero lint errors (dotnet format + eslint)
- [ ] New tests written for new functionality
- [ ] All tests pass (new + existing)
- [ ] Documentation/comments updated where relevant
- [ ] `scripts/validate.sh` passes locally
- [ ] Changes committed with `T<NNN>: <description>` message
- [ ] Pushed to remote; CI workflows green
- [ ] plan.md updated (task status, plan changes, upcoming task review)
```

### Plan Change Log

| Date | Source Task | Change Description |
|------|-----------|-------------------|
| 2026-02-17 | — | Added Global Definition of Done section (v1.1.0) |
| 2026-02-17 | T000 | Added T000 (Environment Bootstrap); marked done. Scaffolded src/api (.NET 9 webapi) and src/web (React+Vite+TS). Added root .gitignore and nuget.config. |
| 2026-02-17 | T001 | Adapted monorepo to existing src/api + src/web structure (instead of backend/ + frontend/). Created HomeBanking.sln at src/api/ so `dotnet build src/api` works. All subsequent tasks will use src/api and src/web paths. Added placeholder test script to src/web/package.json. |
| 2026-02-17 | T002 | Added inter-project references. Contracts kept dependency-free (no Core ref). Cleaned up WeatherForecast template, added /health endpoint. |
| 2026-02-17 | T003 | Configured Tailwind v4, shadcn/ui, Vitest, Prettier. All frontend tooling in place. |
| 2026-02-17 | T004 | E2E Playwright project initialized in e2e/ with correct paths to src/api and src/web. |
| 2026-02-17 | T005 | Created .github/workflows/{backend,frontend,e2e}.yml. Corrected paths to src/api and src/web. All workflows trigger on push+PR with path filters. E2E uploads Playwright report on failure. |
| 2026-02-17 | T006 | Created User, Account, Transaction entities with enums (AccountType, TransactionType, TransactionCategory) in HomeBanking.Core/Entities. Full XML docs. |
| 2026-02-17 | T007 | Created HomeBankingDbContext with fluent API config, DataSeeder with 79 transactions across 4 accounts. InMemory DB configured. Program.cs seeds on startup. |

---

## Phase 0: Environment Bootstrap

### Task T000: Project Scaffold and Build Verification
- **Status**: done
- **Dependencies**: none
- **Estimate**: S
- **DoD**:
  - [x] Folder structure created (src/api, src/web)
  - [x] .NET Web API project created in src/api (HomeBanking.API, .NET 9.0)
  - [x] React+Vite+TypeScript project created in src/web
  - [x] Root .gitignore covers .NET, Node, editor, and OS artifacts
  - [x] Root nuget.config restricts package sources to nuget.org
  - [x] `dotnet build` succeeds with zero errors
  - [x] `npm run build` succeeds with zero errors
  - [x] No application code written
  - [x] Committed with message "T000: project scaffold and build verification"
- **Plan changes**: Added this task (T000) to plan.md as Phase 0.

---

## Overview

This plan breaks down the implementation into **30 atomic tasks**, each independently deployable and testable. Tasks are organized into 7 phases: Foundation, Backend Core, Backend Quality, Frontend Core, Frontend Quality, E2E & CI, and Documentation.

**Key Principles**:
- Each task has clear Definition of Done (DoD)
- Dependencies are explicitly tracked
- Estimates use S/M/L sizing (S=2-4h, M=4-8h, L=8-16h)
- Post-completion, track actual changes in "Plan changes" field

---

## Phase 1: Foundation

### Task T001: Initialize Monorepo Structure
- **Status**: done
- **Dependencies**: none
- **Estimate**: S
- **DoD**:
  - [x] Root folder structure created (src/api, src/web existing; e2e/, docs/ created)
  - [x] Git repository initialized with .gitignore
  - [x] .gitignore includes node_modules/, bin/, obj/, .env, coverage/
  - [x] Root README.md created with project overview
  - [x] HomeBanking.sln created at src/api/ for solution-level build/test
  - [x] scripts/validate.sh created as single source of truth for quality gates
  - [x] All validation commands pass (dotnet build/test src/api, npm lint/test src/web)
  - [x] Committed to Git with message "T001: Initialize monorepo structure"
- **Plan changes**: Adapted from backend/frontend to src/api/src/web structure. Created .sln early (originally T002 scope) so `dotnet build src/api` works. Added placeholder test script to package.json.

**Implementation Notes**:
```bash
# Create folder structure
mkdir -p backend/src backend/tests frontend/src frontend/tests e2e/tests docs/api docs/architecture

# Initialize Git
git init
echo "# Home Banking Demo\n\nSee spec.md and plan.md for details." > README.md

# Create comprehensive .gitignore
cat > .gitignore << 'EOF'
# .NET
bin/
obj/
*.user
*.suo
*.cache
*.dll
*.exe
.vs/
.vscode/

# Node
node_modules/
dist/
build/
*.log
npm-debug.log*
.env
.env.local

# Testing
coverage/
TestResults/
playwright-report/
test-results/

# OS
.DS_Store
Thumbs.db
EOF

git add .
git commit -m "T001: Initialize monorepo structure"
```

---

### Task T002: Backend .NET Solution Setup
- **Status**: done
- **Dependencies**: T001
- **Estimate**: M
- **DoD**:
  - [x] Solution file created (HomeBanking.sln)
  - [x] Four projects created: API, Core, Infrastructure, Contracts
  - [x] Project references configured correctly
  - [x] All projects target .NET 9.0
  - [x] Solution builds successfully (`dotnet build`)
  - [x] Committed with message "T002: Backend .NET solution setup"
- **Plan changes**: Projects and solution already existed from T000/T001. T002 focused on adding inter-project references (API→Core/Infrastructure/Contracts, Infrastructure→Core, API.Tests→API, Core.Tests→Core). Contracts deliberately kept dependency-free (DTOs don't need Core). Removed WeatherForecast template from Program.cs, replaced with minimal health endpoint.

**Implementation Notes**:
```bash
cd backend

# Create solution
dotnet new sln -n HomeBanking

# Create projects
dotnet new webapi -n HomeBanking.API -o src/HomeBanking.API
dotnet new classlib -n HomeBanking.Core -o src/HomeBanking.Core
dotnet new classlib -n HomeBanking.Infrastructure -o src/HomeBanking.Infrastructure
dotnet new classlib -n HomeBanking.Contracts -o src/HomeBanking.Contracts

# Add to solution
dotnet sln add src/HomeBanking.API
dotnet sln add src/HomeBanking.Core
dotnet sln add src/HomeBanking.Infrastructure
dotnet sln add src/HomeBanking.Contracts

# Configure references
dotnet add src/HomeBanking.API reference src/HomeBanking.Core
dotnet add src/HomeBanking.API reference src/HomeBanking.Infrastructure
dotnet add src/HomeBanking.API reference src/HomeBanking.Contracts
dotnet add src/HomeBanking.Infrastructure reference src/HomeBanking.Core
dotnet add src/HomeBanking.Contracts reference src/HomeBanking.Core

# Create test projects
dotnet new xunit -n HomeBanking.API.Tests -o tests/HomeBanking.API.Tests
dotnet new xunit -n HomeBanking.Core.Tests -o tests/HomeBanking.Core.Tests
dotnet sln add tests/HomeBanking.API.Tests
dotnet sln add tests/HomeBanking.Core.Tests

# Build to verify
dotnet build
```

**Project Structure**:
- **API**: Controllers, Program.cs, middleware
- **Core**: Entities, interfaces, business services
- **Infrastructure**: EF Core, repositories, data seeding
- **Contracts**: DTOs for API requests/responses

---

### Task T003: Frontend React+Vite+TypeScript Setup
- **Status**: done
- **Dependencies**: T001
- **Estimate**: M
- **DoD**:
  - [x] Vite project created with React+TypeScript template
  - [x] Tailwind CSS installed and configured
  - [x] shadcn/ui initialized (components.json created)
  - [x] Basic folder structure created (components/ui, features/, lib/, pages/)
  - [x] TypeScript strict mode enabled
  - [x] ESLint and Prettier configured
  - [x] Dev server runs successfully (`npm run dev`)
  - [x] Builds successfully (`npm run build`)
  - [x] Committed with message "T003: Frontend React+Vite+TypeScript setup"
- **Plan changes**: Vite project already existed from T000/T001 (React 19 + Vite 7). Installed Tailwind CSS v4 (not v3 as spec suggests) with @tailwindcss/vite plugin. Set up shadcn/ui with new-york style and Tailwind v4 CSS variables. Added Vitest with jsdom environment and @testing-library. Integrated Prettier into ESLint flat config. Created folder structure under components/ (features, layouts, ui) and lib/ (api, hooks). Replaced stock counter App.tsx with minimal placeholder.

**Implementation Notes**:
```bash
cd frontend

# Create Vite project
npm create vite@latest . -- --template react-swc-ts

# Install dependencies
npm install

# Install Tailwind CSS
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p

# Install shadcn/ui dependencies
npm install class-variance-authority clsx tailwind-merge
npm install @radix-ui/react-slot
npm install lucide-react

# Install other core dependencies
npm install react-router-dom
npm install @tanstack/react-query
npm install axios
npm install react-hook-form @hookform/resolvers zod
npm install date-fns

# Install dev dependencies
npm install -D @types/node
npm install -D eslint @typescript-eslint/eslint-plugin @typescript-eslint/parser
npm install -D prettier eslint-config-prettier
npm install -D vitest @testing-library/react @testing-library/jest-dom @testing-library/user-event
npm install -D @vitejs/plugin-react-swc

# Initialize shadcn/ui
npx shadcn@latest init

# Create folder structure
mkdir -p src/components/ui src/components/features src/components/layouts
mkdir -p src/lib/api src/lib/hooks src/lib/utils
mkdir -p src/pages tests/unit
```

**Configure tsconfig.json**:
```json
{
  "compilerOptions": {
    "target": "ES2020",
    "useDefineForClassFields": true,
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "skipLibCheck": true,
    "moduleResolution": "bundler",
    "allowImportingTsExtensions": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx",
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noFallthroughCasesInSwitch": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"]
    }
  },
  "include": ["src"],
  "references": [{ "path": "./tsconfig.node.json" }]
}
```

---

### Task T004: E2E Playwright Project Setup
- **Status**: done
- **Dependencies**: T001
- **Estimate**: S
- **DoD**:
  - [x] Playwright installed and initialized
  - [x] playwright.config.ts configured for Chromium, Firefox, WebKit
  - [x] Test folder structure created (tests/, fixtures/)
  - [x] Base URL configured (http://localhost:5173)
  - [x] Example test runs successfully
  - [x] @axe-core/playwright installed for accessibility testing
  - [x] Committed with message "T004: E2E Playwright project setup"
- **Plan changes**: WebServer config adapted to use src/api and src/web paths (not backend/frontend from original notes). Added tsconfig.json for type-checking e2e code. Added @types/node for process.env types.

**Implementation Notes**:
```bash
cd e2e

# Initialize npm project
npm init -y

# Install Playwright
npm install -D @playwright/test
npm install -D @axe-core/playwright

# Initialize Playwright (creates config and example tests)
npx playwright install --with-deps

# Create folder structure
mkdir -p tests fixtures
```

**playwright.config.ts**:
```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
    { name: 'webkit', use: { ...devices['Desktop Safari'] } },
  ],
  webServer: [
    {
      command: 'cd ../backend && dotnet run --project src/HomeBanking.API',
      url: 'http://localhost:5000/health',
      reuseExistingServer: !process.env.CI,
    },
    {
      command: 'cd ../frontend && npm run dev',
      url: 'http://localhost:5173',
      reuseExistingServer: !process.env.CI,
    },
  ],
});
```

---

### Task T005: GitHub Actions CI Workflow Scaffolding
- **Status**: done
- **Dependencies**: T002, T003, T004
- **Estimate**: M
- **DoD**:
  - [x] Three workflow files created: backend.yml, frontend.yml, e2e.yml
  - [x] Backend workflow builds and tests .NET solution
  - [x] Frontend workflow builds, lints, type-checks, and tests
  - [x] E2E workflow runs Playwright tests
  - [x] All workflows trigger on push and pull_request
  - [x] Workflows run successfully (create empty tests if needed)
  - [x] Committed with message "T005: GitHub Actions CI workflow scaffolding"
- **Plan changes**: Corrected paths from backend/frontend to src/api/src/web. Removed branch filter to support feature branches. Added path filter for own workflow file. E2E uploads Playwright report artifact on failure.

**Implementation Notes**:

Create `.github/workflows/backend.yml`:
```yaml
name: Backend CI

on:
  push:
    branches: [ main ]
    paths:
      - 'backend/**'
      - '.github/workflows/backend.yml'
  pull_request:
    branches: [ main ]
    paths:
      - 'backend/**'

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
      working-directory: backend
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
      working-directory: backend
    
    - name: Run tests
      run: dotnet test --no-build --verbosity normal --configuration Release --collect:"XPlat Code Coverage"
      working-directory: backend
    
    - name: Upload coverage
      uses: codecov/codecov-action@v4
      if: always()
      with:
        files: backend/**/coverage.cobertura.xml
        flags: backend
```

Create `.github/workflows/frontend.yml`:
```yaml
name: Frontend CI

on:
  push:
    branches: [ main ]
    paths:
      - 'frontend/**'
      - '.github/workflows/frontend.yml'
  pull_request:
    branches: [ main ]
    paths:
      - 'frontend/**'

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '20'
        cache: 'npm'
        cache-dependency-path: frontend/package-lock.json
    
    - name: Install dependencies
      run: npm ci
      working-directory: frontend
    
    - name: Lint
      run: npm run lint
      working-directory: frontend
    
    - name: Type check
      run: npx tsc --noEmit
      working-directory: frontend
    
    - name: Run tests
      run: npm run test:coverage
      working-directory: frontend
    
    - name: Build
      run: npm run build
      working-directory: frontend
    
    - name: Upload coverage
      uses: codecov/codecov-action@v4
      if: always()
      with:
        files: frontend/coverage/lcov.info
        flags: frontend
```

Create `.github/workflows/e2e.yml`:
```yaml
name: E2E Tests

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  e2e:
    runs-on: ubuntu-latest
    timeout-minutes: 10
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '20'
    
    - name: Install backend dependencies
      run: dotnet restore
      working-directory: backend
    
    - name: Install frontend dependencies
      run: npm ci
      working-directory: frontend
    
    - name: Install E2E dependencies
      run: npm ci
      working-directory: e2e
    
    - name: Install Playwright browsers
      run: npx playwright install --with-deps
      working-directory: e2e
    
    - name: Run E2E tests
      run: npm test
      working-directory: e2e
    
    - name: Upload Playwright report
      uses: actions/upload-artifact@v4
      if: failure()
      with:
        name: playwright-report
        path: e2e/playwright-report/
        retention-days: 7
```

---

## Phase 2: Backend Core

### Task T006: Domain Models (Entities & Enums)
- **Status**: done
- **Dependencies**: T002
- **Estimate**: M
- **DoD**:
  - [x] User entity created with properties (Id, Email, PasswordHash, etc.)
  - [x] Account entity created with AccountType enum
  - [x] Transaction entity created with TransactionType and TransactionCategory enums
  - [x] All entities use Guid for IDs, decimal for money amounts
  - [x] Navigation properties configured
  - [x] XML documentation comments added
  - [x] Builds successfully with no warnings
  - [x] Committed with message "T006: Domain models (entities & enums)"
- **Plan changes**: Entities placed at src/api/HomeBanking.Core/Entities/ (not backend/src/).

**Implementation Notes**:

Create `backend/src/HomeBanking.Core/Entities/User.cs`:
```csharp
namespace HomeBanking.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string FirstName { get; set; } = "Demo";
    public string LastName { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
```

Create `backend/src/HomeBanking.Core/Entities/Account.cs`:
```csharp
namespace HomeBanking.Core.Entities;

public class Account
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required string AccountNumber { get; set; }
    public AccountType Type { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
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

Create `backend/src/HomeBanking.Core/Entities/Transaction.cs`:
```csharp
namespace HomeBanking.Core.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public required Guid AccountId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public required string Description { get; set; }
    public TransactionCategory Category { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public required string ReferenceNumber { get; set; }
    public Guid? RelatedTransactionId { get; set; }
    
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

---

### Task T007: EF Core DbContext & Demo Data Seeder
- **Status**: done
- **Dependencies**: T006
- **Estimate**: L
- **DoD**:
  - [x] HomeBankingDbContext created with DbSet properties
  - [x] Entity configurations created (fluent API)
  - [x] InMemory database provider configured
  - [x] DataSeeder class created with realistic demo data
  - [x] Seed data includes: 1 user, 4 accounts, 60+ transactions (79 total)
  - [x] Balance calculations verified (sum of transactions = balance via adjustment transactions)
  - [x] Program.cs seeds data on startup
  - [x] Database accessible from controllers
  - [x] Committed with message "T007: EF Core DbContext & demo data seeder"
- **Plan changes**: EF packages added to Infrastructure. InMemory also added to API .csproj. DataSeeder uses BuildTransactions pattern with adjustment transactions for exact balance reconciliation.

**Implementation Notes**:

Install NuGet packages:
```bash
cd backend/src/HomeBanking.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

Create `backend/src/HomeBanking.Infrastructure/Data/HomeBankingDbContext.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using HomeBanking.Core.Entities;

namespace HomeBanking.Infrastructure.Data;

public class HomeBankingDbContext : DbContext
{
    public HomeBankingDbContext(DbContextOptions<HomeBankingDbContext> options)
        : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(256);
        });
        
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.HasOne(a => a.User)
                  .WithMany(u => u.Accounts)
                  .HasForeignKey(a => a.UserId);
        });
        
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.BalanceAfter).HasPrecision(18, 2);
            entity.HasOne(t => t.Account)
                  .WithMany(a => a.Transactions)
                  .HasForeignKey(t => t.AccountId);
        });
    }
}
```

Create `backend/src/HomeBanking.Infrastructure/Data/DataSeeder.cs` with demo data:
- 1 User: demo@bank.com
- 4 Accounts: Checking ($5,250), Savings ($12,800), Credit Card (-$1,243.50), Investment ($28,500)
- 60-80 transactions per account with realistic descriptions, dates, categories

---

### Task T008: JWT Authentication Infrastructure
- **Status**: done
- **Dependencies**: T007
- **Estimate**: M
- **DoD**:
  - [x] JWT configuration added to appsettings.json
  - [x] JwtService created with GenerateToken method
  - [x] JWT authentication middleware configured in Program.cs
  - [x] [Authorize] attribute tested on dummy endpoint
  - [x] Token expiration set to 2 hours
  - [x] Claims include userId and email
  - [x] Builds successfully
  - [x] Committed with message "T008: JWT authentication infrastructure"
- **Plan changes**: 

**Implementation Notes**:

Install NuGet packages:
```bash
cd backend/src/HomeBanking.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
```

Create `backend/src/HomeBanking.Core/Interfaces/IJwtService.cs`:
```csharp
namespace HomeBanking.Core.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId, string email);
    Guid? ValidateToken(string token);
}
```

Create `backend/src/HomeBanking.Infrastructure/Services/JwtService.cs`:
```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using HomeBanking.Core.Interfaces;

namespace HomeBanking.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    
    public JwtService(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("JWT secret key not configured");
        _issuer = configuration["Jwt:Issuer"] ?? "HomeBankingAPI";
    }
    
    public string GenerateToken(Guid userId, string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = _issuer,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    public Guid? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            
            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            
            return Guid.Parse(userIdClaim);
        }
        catch
        {
            return null;
        }
    }
}
```

Configure in `Program.cs`:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

app.UseAuthentication();
app.UseAuthorization();
```

---

### Task T009: Auth Endpoints (Login)
- **Status**: done
- **Dependencies**: T008
- **Estimate**: M
- **DoD**:
  - [x] AuthController created with Login endpoint
  - [x] LoginRequest and LoginResponse DTOs created
  - [x] Endpoint validates email/password against demo user
  - [x] Returns JWT token on success (200)
  - [x] Returns 401 for invalid credentials
  - [x] FluentValidation configured for LoginRequest
  - [x] Endpoint tested manually (Postman/curl)
  - [x] Committed with message "T009: Auth endpoints (login)"
- **Plan changes**: 

**Implementation Notes**:

Create DTOs in `backend/src/HomeBanking.Contracts/`:
```csharp
namespace HomeBanking.Contracts.Requests;

public record LoginRequest(
    string Email,
    string Password
);
```

```csharp
namespace HomeBanking.Contracts.Responses;

public record LoginResponse(
    string Token,
    int ExpiresIn,
    UserDto User
);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName
);
```

Create `backend/src/HomeBanking.API/Controllers/AuthController.cs`:
```csharp
using Microsoft.AspNetCore.Mvc;
using HomeBanking.Contracts.Requests;
using HomeBanking.Contracts.Responses;
using HomeBanking.Core.Interfaces;
using HomeBanking.Infrastructure.Data;

namespace HomeBanking.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly HomeBankingDbContext _context;
    private readonly IJwtService _jwtService;
    
    public AuthController(HomeBankingDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }
    
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Demo: Hardcoded credentials
        if (request.Email != "demo@bank.com" || request.Password != "Demo123!")
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }
        
        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }
        
        var token = _jwtService.GenerateToken(user.Id, user.Email);
        
        return Ok(new LoginResponse(
            Token: token,
            ExpiresIn: 7200, // 2 hours in seconds
            User: new UserDto(user.Id, user.Email, user.FirstName, user.LastName)
        ));
    }
}
```

---

### Task T010: Account Endpoints
- **Status**: done
- **Dependencies**: T009
- **Estimate**: M
- **DoD**:
  - [x] AccountsController created
  - [x] GET /api/v1/accounts endpoint returns user's accounts
  - [x] GET /api/v1/accounts/{id} endpoint returns account details
  - [x] Endpoints require [Authorize] attribute
  - [x] DTOs created (AccountDto, AccountDetailDto)
  - [x] Returns 403 if user tries to access another user's account
  - [x] Returns 404 for non-existent account
  - [x] Manually tested with JWT token
  - [x] Committed with message "T010: Account endpoints"
- **Plan changes**: 

**Implementation Notes**:

Create `backend/src/HomeBanking.API/Controllers/AccountsController.cs`:
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeBanking.Contracts.Responses;
using HomeBanking.Infrastructure.Data;
using System.Security.Claims;

namespace HomeBanking.API.Controllers;

[ApiController]
[Route("api/v1/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly HomeBankingDbContext _context;
    
    public AccountsController(HomeBankingDbContext context)
    {
        _context = context;
    }
    
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAccounts()
    {
        var userId = GetCurrentUserId();
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId && a.IsActive)
            .Select(a => new AccountDto(
                a.Id,
                a.AccountNumber,
                a.Type.ToString(),
                a.Balance,
                a.Currency,
                a.IsActive,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .ToListAsync();
        
        return Ok(new { accounts });
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAccount(Guid id)
    {
        var userId = GetCurrentUserId();
        var account = await _context.Accounts
            .Where(a => a.Id == id && a.UserId == userId)
            .Select(a => new AccountDetailDto(
                a.Id,
                a.AccountNumber,
                a.Type.ToString(),
                a.Balance,
                a.Currency,
                a.IsActive,
                a.Transactions.Count,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .FirstOrDefaultAsync();
        
        if (account == null)
        {
            return NotFound(new { message = "Account not found" });
        }
        
        return Ok(account);
    }
}
```

---

### Task T011: Transaction Endpoints (Cursor Pagination)
- **Status**: done
- **Dependencies**: T010
- **Estimate**: L
- **DoD**:
  - [x] POST /api/v1/accounts/{accountId}/transactions with cursor pagination
  - [x] Query params: cursor (optional), limit (default 20, max 100)
  - [x] Returns transactions in reverse chronological order
  - [x] Response includes pagination metadata (hasMore, nextCursor)
  - [x] GET /api/v1/transactions/{id} returns single transaction
  - [x] Authorization checks (user owns account)
  - [x] TransactionDto created with all required fields
  - [x] Manually tested with multiple pages
  - [x] Committed with message "T011: Transaction endpoints (cursor pagination)"
- **Plan changes**: 

**Implementation Notes**:

Create `backend/src/HomeBanking.API/Controllers/TransactionsController.cs`:
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeBanking.Contracts.Responses;
using HomeBanking.Infrastructure.Data;
using System.Security.Claims;

namespace HomeBanking.API.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly HomeBankingDbContext _context;
    
    public TransactionsController(HomeBankingDbContext context)
    {
        _context = context;
    }
    
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
    
    [HttpGet("accounts/{accountId:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(
        Guid accountId,
        [FromQuery] Guid? cursor = null,
        [FromQuery] int limit = 20)
    {
        var userId = GetCurrentUserId();
        
        // Verify account ownership
        var accountExists = await _context.Accounts
            .AnyAsync(a => a.Id == accountId && a.UserId == userId);
        
        if (!accountExists)
        {
            return NotFound(new { message = "Account not found" });
        }
        
        // Validate limit
        limit = Math.Min(limit, 100);
        
        // Build query with cursor
        var query = _context.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.Timestamp)
            .ThenByDescending(t => t.Id);
        
        if (cursor.HasValue)
        {
            var cursorTransaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == cursor.Value);
            
            if (cursorTransaction != null)
            {
                query = query.Where(t => 
                    t.Timestamp < cursorTransaction.Timestamp ||
                    (t.Timestamp == cursorTransaction.Timestamp && t.Id.CompareTo(cursor.Value) < 0))
                    .OrderByDescending(t => t.Timestamp)
                    .ThenByDescending(t => t.Id);
            }
        }
        
        // Fetch limit + 1 to check if more exist
        var transactions = await query
            .Take(limit + 1)
            .Select(t => new TransactionDto(
                t.Id,
                t.Type.ToString(),
                t.Amount,
                t.BalanceAfter,
                t.Description,
                t.Category.ToString(),
                t.Timestamp,
                t.ReferenceNumber,
                t.RelatedTransactionId
            ))
            .ToListAsync();
        
        var hasMore = transactions.Count > limit;
        var items = hasMore ? transactions.Take(limit).ToList() : transactions;
        var nextCursor = hasMore ? items.Last().Id : (Guid?)null;
        
        return Ok(new
        {
            transactions = items,
            pagination = new
            {
                hasMore,
                nextCursor
            }
        });
    }
    
    [HttpGet("transactions/{id:guid}")]
    public async Task<IActionResult> GetTransaction(Guid id)
    {
        var userId = GetCurrentUserId();
        
        var transaction = await _context.Transactions
            .Where(t => t.Id == id && t.Account.UserId == userId)
            .Select(t => new TransactionDto(
                t.Id,
                t.Type.ToString(),
                t.Amount,
                t.BalanceAfter,
                t.Description,
                t.Category.ToString(),
                t.Timestamp,
                t.ReferenceNumber,
                t.RelatedTransactionId
            ))
            .FirstOrDefaultAsync();
        
        if (transaction == null)
        {
            return NotFound(new { message = "Transaction not found" });
        }
        
        return Ok(transaction);
    }
}
```

---

### Task T012: Transfer Endpoint (Create Transfer)
- **Status**: done
- **Dependencies**: T011
- **Estimate**: L
- **DoD**:
  - [x] POST /api/v1/transfers endpoint created
  - [x] TransferRequest DTO with validation (amount > 0, accounts differ)
  - [x] Business logic: check sufficient balance, create 2 transactions atomically
  - [x] Update both account balances
  - [x] Link transactions via RelatedTransactionId
  - [x] Generate unique reference numbers
  - [x] Returns 201 with TransferResponse
  - [x] Returns 400 for validation errors (insufficient funds, same account, etc.)
  - [x] Transaction rolled back on error
  - [x] Manually tested with successful and failing transfers
  - [x] Committed with message "T012: Transfer endpoint (create transfer)"
- **Plan changes**: 

**Implementation Notes**:

Create `backend/src/HomeBanking.Contracts/Requests/TransferRequest.cs`:
```csharp
namespace HomeBanking.Contracts.Requests;

public record TransferRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Description
);
```

Create `backend/src/HomeBanking.API/Controllers/TransfersController.cs`:
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeBanking.Contracts.Requests;
using HomeBanking.Contracts.Responses;
using HomeBanking.Core.Entities;
using HomeBanking.Infrastructure.Data;
using System.Security.Claims;

namespace HomeBanking.API.Controllers;

[ApiController]
[Route("api/v1/transfers")]
[Authorize]
public class TransfersController : ControllerBase
{
    private readonly HomeBankingDbContext _context;
    
    public TransfersController(HomeBankingDbContext context)
    {
        _context = context;
    }
    
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTransfer([FromBody] TransferRequest request)
    {
        var userId = GetCurrentUserId();
        
        // Validation
        if (request.Amount <= 0)
        {
            return BadRequest(new { message = "Amount must be greater than zero" });
        }
        
        if (request.FromAccountId == request.ToAccountId)
        {
            return BadRequest(new { message = "Cannot transfer to the same account" });
        }
        
        if (request.Amount > 1_000_000)
        {
            return BadRequest(new { message = "Amount exceeds maximum transfer limit" });
        }
        
        // Fetch accounts with ownership check
        var fromAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.FromAccountId && a.UserId == userId);
        
        var toAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.ToAccountId && a.UserId == userId);
        
        if (fromAccount == null || toAccount == null)
        {
            return NotFound(new { message = "One or both accounts not found" });
        }
        
        // Check sufficient balance
        if (fromAccount.Balance < request.Amount)
        {
            return BadRequest(new
            {
                message = "Insufficient funds",
                detail = $"Balance insufficient for transfer. Available: ${fromAccount.Balance:F2}"
            });
        }
        
        // Create transfer (atomic transaction)
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var referenceNumber = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
            var debitId = Guid.NewGuid();
            var creditId = Guid.NewGuid();
            
            // Debit transaction (from account)
            var debitTransaction = new Transaction
            {
                Id = debitId,
                AccountId = fromAccount.Id,
                Type = TransactionType.Transfer,
                Amount = -request.Amount,
                BalanceAfter = fromAccount.Balance - request.Amount,
                Description = request.Description ?? $"Transfer to {toAccount.AccountNumber}",
                Category = TransactionCategory.Other,
                Timestamp = DateTime.UtcNow,
                ReferenceNumber = referenceNumber,
                RelatedTransactionId = creditId
            };
            
            // Credit transaction (to account)
            var creditTransaction = new Transaction
            {
                Id = creditId,
                AccountId = toAccount.Id,
                Type = TransactionType.Transfer,
                Amount = request.Amount,
                BalanceAfter = toAccount.Balance + request.Amount,
                Description = request.Description ?? $"Transfer from {fromAccount.AccountNumber}",
                Category = TransactionCategory.Other,
                Timestamp = DateTime.UtcNow,
                ReferenceNumber = referenceNumber,
                RelatedTransactionId = debitId
            };
            
            // Update balances
            fromAccount.Balance -= request.Amount;
            fromAccount.UpdatedAt = DateTime.UtcNow;
            
            toAccount.Balance += request.Amount;
            toAccount.UpdatedAt = DateTime.UtcNow;
            
            _context.Transactions.AddRange(debitTransaction, creditTransaction);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return CreatedAtAction(
                nameof(GetTransfer),
                new { id = debitId },
                new TransferResponse(
                    debitId,
                    debitId,
                    creditId,
                    fromAccount.Balance,
                    toAccount.Balance,
                    DateTime.UtcNow
                ));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Transfer failed", detail = ex.Message });
        }
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTransfer(Guid id)
    {
        var userId = GetCurrentUserId();
        var transaction = await _context.Transactions
            .Where(t => t.Id == id && t.Account.UserId == userId)
            .FirstOrDefaultAsync();
        
        if (transaction == null)
        {
            return NotFound();
        }
        
        return Ok(transaction);
    }
}
```

---

## Phase 3: Backend Quality

### Task T013: Scalar OpenAPI & Health Checks
- **Status**: done
- **Dependencies**: T012
- **Estimate**: M
- **DoD**:
  - [x] Scalar.AspNetCore package installed
  - [x] OpenAPI specification generated from controllers
  - [x] Scalar UI accessible at /scalar/v1
  - [x] Health check endpoints configured (/health, /health/ready)
  - [x] DbContext health check added
  - [x] XML documentation comments on all endpoints
  - [x] All endpoints documented with examples
  - [x] OpenAPI spec includes authentication bearer scheme
  - [x] Committed with message "T013: Scalar OpenAPI & health checks"
- **Plan changes**: 

**Implementation Notes**:

Install packages:
```bash
cd backend/src/HomeBanking.API
dotnet add package Scalar.AspNetCore
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
```

Configure in `Program.cs`:
```csharp
// Enable XML documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<HomeBankingDbContext>();

// After app.Build()
app.MapOpenApi();
app.MapScalarApiReference(); // Available at /scalar/v1

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

Enable XML documentation in `.csproj`:
```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

Add XML comments to all controllers.

---

### Task T014: Backend Unit Tests (Domain Logic)
- **Status**: done
- **Dependencies**: T012
- **Estimate**: M
- **DoD**:
  - [x] Test project dependencies added (xUnit, FluentAssertions, Moq)
  - [x] Unit tests for Transfer validation rules
  - [x] Unit tests for balance calculations
  - [x] Unit tests for JWT token generation/validation
  - [x] Unit tests for transaction categorization (if applicable)
  - [x] All tests pass (`dotnet test`)
  - [x] Code coverage report generated
  - [x] Minimum 70% coverage achieved
  - [x] Committed with message "T014: Backend unit tests (domain logic)"
- **Plan changes**: 

**Implementation Notes**:

Add packages to test project:
```bash
cd backend/tests/HomeBanking.Core.Tests
dotnet add package FluentAssertions
dotnet add package Moq
dotnet add package coverlet.collector
```

Example test file `backend/tests/HomeBanking.Core.Tests/Services/TransferValidationTests.cs`:
```csharp
using FluentAssertions;
using HomeBanking.Core.Entities;
using Xunit;

namespace HomeBanking.Core.Tests.Services;

public class TransferValidationTests
{
    [Fact]
    public void Transfer_WithSufficientBalance_Succeeds()
    {
        // Arrange
        var fromAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AccountNumber = "****1234",
            Type = AccountType.Checking,
            Balance = 1000m
        };
        
        decimal transferAmount = 500m;
        
        // Act
        var hasEnough = fromAccount.Balance >= transferAmount;
        
        // Assert
        hasEnough.Should().BeTrue();
    }
    
    [Fact]
    public void Transfer_WithInsufficientBalance_Fails()
    {
        // Arrange
        var fromAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AccountNumber = "****1234",
            Type = AccountType.Checking,
            Balance = 100m
        };
        
        decimal transferAmount = 500m;
        
        // Act
        var hasEnough = fromAccount.Balance >= transferAmount;
        
        // Assert
        hasEnough.Should().BeFalse();
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Transfer_WithNonPositiveAmount_IsInvalid(decimal amount)
    {
        // Act
        var isValid = amount > 0;
        
        // Assert
        isValid.Should().BeFalse();
    }
}
```

Run with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

### Task T015: Backend Integration Tests (API Endpoints)
- **Status**: done
- **Dependencies**: T014
- **Estimate**: L
- **DoD**:
  - [x] WebApplicationFactory configured for integration tests
  - [x] Test database seeded with known data
  - [x] Tests for AuthController (login success/failure)
  - [x] Tests for AccountsController (get accounts, authorization)
  - [x] Tests for TransactionsController (pagination, cursor logic)
  - [x] Tests for TransfersController (successful transfer, validation errors)
  - [x] All HTTP status codes verified (200, 201, 400, 401, 404)
  - [x] All tests pass
  - [x] Committed with message "T015: Backend integration tests (API endpoints)"
- **Plan changes**: 

**Implementation Notes**:

Add package:
```bash
cd backend/tests/HomeBanking.API.Tests
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

Create `backend/tests/HomeBanking.API.Tests/TestWebApplicationFactory.cs`:
```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HomeBanking.Infrastructure.Data;

namespace HomeBanking.API.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<HomeBankingDbContext>));
            
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            
            // Add test database
            services.AddDbContext<HomeBankingDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });
            
            // Seed test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HomeBankingDbContext>();
            context.Database.EnsureCreated();
            DataSeeder.SeedData(context);
        });
    }
}
```

Example test:
```csharp
public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    
    public AuthControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var request = new LoginRequest("demo@bank.com", "Demo123!");
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeEmpty();
    }
}
```

---

## Phase 4: Frontend Core

### Task T016: API Client with Axios & JWT Interceptor
- **Status**: done
- **Dependencies**: T003
- **Estimate**: M
- **DoD**:
  - [x] Axios client configured in src/lib/api/client.ts
  - [x] Base URL set to http://localhost:5000/api/v1
  - [x] Request interceptor adds JWT from localStorage
  - [x] Response interceptor handles 401 (redirect to login)
  - [x] TypeScript types defined for all API responses
  - [x] API service files created (auth.ts, accounts.ts, transactions.ts, transfers.ts)
  - [x] Error handling wrapper function created
  - [x] Builds successfully with no TypeScript errors
  - [x] Committed with message "T016: API client with axios & JWT interceptor"
- **Plan changes**: 

**Implementation Notes**:

Create `frontend/src/lib/api/client.ts`:
```typescript
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor: Add JWT token
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor: Handle 401
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('authToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

Create `frontend/src/lib/api/types.ts`:
```typescript
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresIn: number;
  user: User;
}

export interface Account {
  id: string;
  accountNumber: string;
  type: string;
  balance: number;
  currency: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Transaction {
  id: string;
  type: string;
  amount: number;
  balanceAfter: number;
  description: string;
  category: string;
  timestamp: string;
  referenceNumber: string;
  relatedTransactionId?: string;
}

export interface TransferRequest {
  fromAccountId: string;
  toAccountId: string;
  amount: number;
  description: string;
}

export interface TransferResponse {
  transferId: string;
  debitTransactionId: string;
  creditTransactionId: string;
  fromAccountBalance: number;
  toAccountBalance: number;
  timestamp: string;
}
```

Create API service files with typed functions.

---

### Task T017: Auth Context & Login Page
- **Status**: done
- **Dependencies**: T016
- **Estimate**: M
- **DoD**:
  - [x] AuthContext created with login/logout functions
  - [x] useAuth hook exported
  - [x] LoginPage component created with form
  - [x] Email and password inputs with validation
  - [x] Login button submits credentials
  - [x] Success: Store token, navigate to dashboard
  - [x] Error: Display error message
  - [x] Loading state during API call
  - [x] ProtectedRoute component created
  - [x] Tested: Login flow works end-to-end
  - [x] Committed with message "T017: Auth context & login page"
- **Plan changes**: 

**Implementation Notes**:

Create `frontend/src/lib/hooks/useAuth.tsx`:
```typescript
import React, { createContext, useContext, useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { login as apiLogin } from '@/lib/api/auth';
import type { User, LoginRequest } from '@/lib/api/types';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();
  
  useEffect(() => {
    // Check for existing token on mount
    const token = localStorage.getItem('authToken');
    const savedUser = localStorage.getItem('user');
    
    if (token && savedUser) {
      setUser(JSON.parse(savedUser));
    }
    setIsLoading(false);
  }, []);
  
  const login = async (credentials: LoginRequest) => {
    setIsLoading(true);
    try {
      const response = await apiLogin(credentials);
      localStorage.setItem('authToken', response.token);
      localStorage.setItem('user', JSON.stringify(response.user));
      setUser(response.user);
      navigate('/dashboard');
    } finally {
      setIsLoading(false);
    }
  };
  
  const logout = () => {
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');
    setUser(null);
    navigate('/login');
  };
  
  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, login, logout, isLoading }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};
```

Create `frontend/src/pages/LoginPage.tsx` with form using React Hook Form + Zod.

Create `frontend/src/components/features/auth/ProtectedRoute.tsx` that redirects to login if not authenticated.

---

### Task T018: Dashboard Layout & Routing
- **Status**: done
- **Dependencies**: T017
- **Estimate**: M
- **DoD**:
  - [x] React Router configured with routes
  - [x] DashboardLayout component created (header, nav, main content)
  - [x] Header shows user name and logout button
  - [x] Navigation links (Dashboard, Transfer)
  - [x] Dark theme applied from shadcn/ui
  - [x] Responsive layout (mobile-first)
  - [x] Protected routes wrap dashboard pages
  - [x] Routing tested (navigation works)
  - [x] Committed with message "T018: Dashboard layout & routing"
- **Plan changes**: 

**Implementation Notes**:

Configure routes in `frontend/src/App.tsx`:
```typescript
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from '@/lib/hooks/useAuth';
import { LoginPage } from '@/pages/LoginPage';
import { DashboardPage } from '@/pages/DashboardPage';
import { TransferPage } from '@/pages/TransferPage';
import { ProtectedRoute } from '@/components/features/auth/ProtectedRoute';
import { DashboardLayout } from '@/components/layouts/DashboardLayout';

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route
              path="/"
              element={
                <ProtectedRoute>
                  <DashboardLayout />
                </ProtectedRoute>
              }
            >
              <Route index element={<Navigate to="/dashboard" replace />} />
              <Route path="dashboard" element={<DashboardPage />} />
              <Route path="transfer" element={<TransferPage />} />
            </Route>
          </Routes>
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
```

Create `frontend/src/components/layouts/DashboardLayout.tsx` with responsive design.

---

### Task T019: Account Balance Cards
- **Status**: done
- **Dependencies**: T018
- **Estimate**: M
- **DoD**:
  - [x] AccountCard component created using shadcn Card
  - [x] AccountList component fetches accounts via React Query
  - [x] Balance formatted as currency with Intl.NumberFormat
  - [x] Account type displayed with icon
  - [x] Last 4 digits of account number shown (****1234)
  - [x] Negative balances styled red (credit cards)
  - [x] Loading state shows skeleton cards
  - [x] Error state displays error message
  - [x] Mobile-responsive grid layout
  - [x] Tested with demo data
  - [x] Committed with message "T019: Account balance cards"
- **Plan changes**: 

**Implementation Notes**:

Create `frontend/src/components/features/accounts/AccountCard.tsx`:
```typescript
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { formatCurrency } from '@/lib/utils/format';
import type { Account } from '@/lib/api/types';

interface AccountCardProps {
  account: Account;
}

export const AccountCard: React.FC<AccountCardProps> = ({ account }) => {
  const isNegative = account.balance < 0;
  
  return (
    <Card className="hover:shadow-lg transition-shadow">
      <CardHeader>
        <CardTitle className="text-sm font-medium text-muted-foreground">
          {account.type}
        </CardTitle>
        <div className="text-xs text-muted-foreground">{account.accountNumber}</div>
      </CardHeader>
      <CardContent>
        <div className={`text-2xl font-bold ${isNegative ? 'text-red-600' : 'text-foreground'}`}>
          {formatCurrency(account.balance)}
        </div>
      </CardContent>
    </Card>
  );
};
```

Create `frontend/src/lib/utils/format.ts`:
```typescript
export const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2,
  }).format(amount);
};

export const formatDate = (date: string): string => {
  return new Intl.DateTimeFormat('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(date));
};
```

Use React Query in AccountList for data fetching with loading/error states.

---

### Task T020: Transaction List with Infinite Scroll
- **Status**: done
- **Dependencies**: T019
- **Estimate**: L
- **DoD**:
  - [x] TransactionList component created
  - [x] useInfiniteQuery hook configured for cursor pagination
  - [x] TransactionRow component displays transaction details
  - [x] Intersection Observer detects scroll to bottom
  - [x] Automatically loads next batch when scrolling
  - [x] Loading indicator shown while fetching more
  - [x] "No more transactions" message at list end
  - [x] Empty state for no transactions
  - [x] Mobile-responsive list layout
  - [x] Tested with scrolling through 50+ transactions
  - [x] Committed with message "T020: Transaction list with infinite scroll"
- **Plan changes**: 

**Implementation Notes**:

Create `frontend/src/components/features/transactions/useInfiniteTransactions.ts`:
```typescript
import { useInfiniteQuery } from '@tanstack/react-query';
import { getTransactions } from '@/lib/api/transactions';

export const useInfiniteTransactions = (accountId: string) => {
  return useInfiniteQuery({
    queryKey: ['transactions', accountId],
    queryFn: ({ pageParam }) => getTransactions(accountId, pageParam),
    getNextPageParam: (lastPage) => 
      lastPage.pagination.hasMore ? lastPage.pagination.nextCursor : undefined,
    initialPageParam: undefined as string | undefined,
  });
};
```

Create `frontend/src/components/features/transactions/TransactionList.tsx`:
```typescript
import { useEffect, useRef } from 'react';
import { useInfiniteTransactions } from './useInfiniteTransactions';
import { TransactionRow } from './TransactionRow';
import { Loader2 } from 'lucide-react';

interface TransactionListProps {
  accountId: string;
}

export const TransactionList: React.FC<TransactionListProps> = ({ accountId }) => {
  const { data, fetchNextPage, hasNextPage, isFetchingNextPage, isLoading } = 
    useInfiniteTransactions(accountId);
  
  const loadMoreRef = useRef<HTMLDivElement>(null);
  
  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
          fetchNextPage();
        }
      },
      { threshold: 0.1 }
    );
    
    if (loadMoreRef.current) {
      observer.observe(loadMoreRef.current);
    }
    
    return () => observer.disconnect();
  }, [fetchNextPage, hasNextPage, isFetchingNextPage]);
  
  if (isLoading) {
    return <div className="flex justify-center p-8"><Loader2 className="animate-spin" /></div>;
  }
  
  const transactions = data?.pages.flatMap(page => page.transactions) ?? [];
  
  return (
    <div className="space-y-2">
      {transactions.map((txn) => (
        <TransactionRow key={txn.id} transaction={txn} />
      ))}
      
      <div ref={loadMoreRef} className="flex justify-center p-4">
        {isFetchingNextPage && <Loader2 className="animate-spin" />}
        {!hasNextPage && transactions.length > 0 && (
          <p className="text-sm text-muted-foreground">No more transactions</p>
        )}
      </div>
    </div>
  );
};
```

---

### Task T021: Transaction Category Badges
- **Status**: done
- **Dependencies**: T020
- **Estimate**: S
- **DoD**:
  - [x] CategoryBadge component created
  - [x] Badge color and icon mapped to each category
  - [x] Icons from lucide-react (ShoppingCart, Utensils, Car, etc.)
  - [x] Badge uses shadcn Badge component
  - [x] Accessible (aria-label with category name)
  - [x] Responsive sizing
  - [x] Integrated into TransactionRow
  - [x] All 8 categories styled
  - [x] Committed with message "T021: Transaction category badges"
- **Plan changes**: 

**Implementation Notes**:

Create `frontend/src/components/features/transactions/CategoryBadge.tsx`:
```typescript
import { Badge } from '@/components/ui/badge';
import { 
  ShoppingCart, 
  Utensils, 
  Car, 
  ShoppingBag, 
  Zap, 
  Heart, 
  Film,
  MoreHorizontal 
} from 'lucide-react';

const categoryConfig = {
  Groceries: { icon: ShoppingCart, color: 'bg-green-100 text-green-800', label: 'Groceries' },
  Dining: { icon: Utensils, color: 'bg-orange-100 text-orange-800', label: 'Dining' },
  Transportation: { icon: Car, color: 'bg-blue-100 text-blue-800', label: 'Transportation' },
  Shopping: { icon: ShoppingBag, color: 'bg-purple-100 text-purple-800', label: 'Shopping' },
  Bills: { icon: Zap, color: 'bg-red-100 text-red-800', label: 'Bills' },
  Healthcare: { icon: Heart, color: 'bg-pink-100 text-pink-800', label: 'Healthcare' },
  Entertainment: { icon: Film, color: 'bg-yellow-100 text-yellow-800', label: 'Entertainment' },
  Other: { icon: MoreHorizontal, color: 'bg-gray-100 text-gray-800', label: 'Other' },
};

interface CategoryBadgeProps {
  category: string;
}

export const CategoryBadge: React.FC<CategoryBadgeProps> = ({ category }) => {
  const config = categoryConfig[category as keyof typeof categoryConfig] || categoryConfig.Other;
  const Icon = config.icon;
  
  return (
    <Badge className={`${config.color} gap-1`} aria-label={config.label}>
      <Icon className="h-3 w-3" />
      <span className="text-xs">{config.label}</span>
    </Badge>
  );
};
```

---

### Task T022: Transfer Form with Validation
- **Status**: done
- **Dependencies**: T019, T021
- **Estimate**: L
- **DoD**:
  - [x] TransferForm component created
  - [x] React Hook Form integrated with Zod schema
  - [x] From/To account dropdowns populated from user accounts
  - [x] Amount input with currency formatting
  - [x] Description textarea (max 200 chars)
  - [x] Validation: amount > 0, accounts differ, sufficient balance
  - [x] Real-time validation feedback
  - [x] Submit button disabled during submission
  - [x] Success: Show toast, navigate to dashboard
  - [x] Error: Display error message
  - [x] Form resets after successful transfer
  - [x] Tested with valid and invalid inputs
  - [x] Committed with message "T022: Transfer form with validation"
- **Plan changes**: 

**Implementation Notes**:

Create Zod schema in `frontend/src/lib/utils/validation.ts`:
```typescript
import { z } from 'zod';

export const transferSchema = z.object({
  fromAccountId: z.string().uuid('Invalid account'),
  toAccountId: z.string().uuid('Invalid account'),
  amount: z.number().positive('Amount must be greater than zero').max(1000000, 'Amount exceeds limit'),
  description: z.string().max(200, 'Description too long').optional(),
}).refine(data => data.fromAccountId !== data.toAccountId, {
  message: 'Cannot transfer to the same account',
  path: ['toAccountId'],
});

export type TransferFormData = z.infer<typeof transferSchema>;
```

Create `frontend/src/components/features/transfers/TransferForm.tsx` using React Hook Form, shadcn Select, Input components, and validation.

Use React Query mutation for transfer submission with optimistic updates.

---

## Phase 5: Frontend Quality

### Task T023: Frontend Unit Tests (Components & Utils)
- **Status**: done
- **Dependencies**: T022
- **Estimate**: M
- **Commit**: 420a9bb
- **DoD**:
  - [x] Vitest configured with Testing Library
  - [x] Tests for formatCurrency utility
  - [x] Tests for formatDate utility
  - [x] Tests for AccountCard component (balance display, negative styling)
  - [x] Tests for CategoryBadge (correct icon/color per category)
  - [x] Tests for TransferForm validation
  - [x] All tests pass (`npm run test`)
  - [x] Coverage report generated
  - [x] Minimum 60% coverage achieved
  - [x] Committed with message "T023: Frontend unit tests (components & utils)"
- **Plan changes**: Added @vitest/coverage-v8 dependency. Created 8 test files (format, validation, AccountCard, CategoryBadge, TransactionRow, errors, useAuth, LoginPage) with 83 total tests. Coverage: 61.79% statements / 63.42% lines.

**Implementation Notes**:

Configure `frontend/vite.config.ts`:
```typescript
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react-swc';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './tests/setup.ts',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html', 'lcov'],
      exclude: ['node_modules/', 'tests/'],
    },
  },
});
```

Create `frontend/tests/setup.ts`:
```typescript
import '@testing-library/jest-dom';
```

Example test `frontend/tests/unit/utils/format.test.ts`:
```typescript
import { describe, it, expect } from 'vitest';
import { formatCurrency } from '@/lib/utils/format';

describe('formatCurrency', () => {
  it('formats positive amounts correctly', () => {
    expect(formatCurrency(1234.56)).toBe('$1,234.56');
  });
  
  it('formats negative amounts correctly', () => {
    expect(formatCurrency(-500.00)).toBe('-$500.00');
  });
  
  it('handles zero', () => {
    expect(formatCurrency(0)).toBe('$0.00');
  });
});
```

Write component tests using Testing Library.

---

### Task T024: Loading States & Error Boundaries
- **Status**: done
- **Dependencies**: T023
- **Estimate**: M
- **Commit**: b057704
- **DoD**:
  - [x] Skeleton loaders created for AccountCard
  - [x] Spinner component for transaction list initial load
  - [x] ErrorBoundary component catches React errors
  - [x] Error fallback UI displays friendly message
  - [x] Network error toast notifications
  - [x] Loading button states during form submission
  - [x] Empty states for no accounts/transactions
  - [x] All async operations show loading feedback
  - [x] Tested by simulating slow network
  - [x] Committed with message "T024: Loading states & error boundaries"
- **Plan changes**: Skeleton loaders, spinners, empty states, and loading buttons were already implemented in prior tasks (T019-T022). Created ErrorBoundary class component wrapping App routes. Added global mutation error toast via QueryClient defaultOptions. Added 5 ErrorBoundary tests (88 total tests).

**Implementation Notes**:

Create `frontend/src/components/ui/skeleton.tsx` using shadcn skeleton component.

Create `frontend/src/components/layouts/ErrorBoundary.tsx`:
```typescript
import React, { Component, ReactNode } from 'react';
import { AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }
  
  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }
  
  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('ErrorBoundary caught:', error, errorInfo);
  }
  
  render() {
    if (this.state.hasError) {
      return (
        <div className="flex flex-col items-center justify-center min-h-screen p-4">
          <AlertCircle className="h-16 w-16 text-destructive mb-4" />
          <h1 className="text-2xl font-bold mb-2">Something went wrong</h1>
          <p className="text-muted-foreground mb-4">
            We're sorry, but something unexpected happened.
          </p>
          <Button onClick={() => window.location.href = '/'}>
            Return to Dashboard
          </Button>
        </div>
      );
    }
    
    return this.props.children;
  }
}
```

Add loading skeletons to AccountList and TransactionList components.

---

### Task T025: Optimistic UI Updates for Transfers
- **Status**: done
- **Dependencies**: T024
- **Estimate**: M
- **Commit**: 8767a31
- **DoD**:
  - [x] Transfer mutation configured with optimistic update
  - [x] Account balances update instantly on transfer submit
  - [x] Transaction appears immediately in list
  - [x] Rollback on API error
  - [x] Success toast displays immediately
  - [x] Accounts list refetched after successful transfer
  - [x] Smooth transition without jarring UI changes
  - [x] Tested: Submit transfer, observe instant feedback
  - [x] Committed with message "T025: Optimistic UI updates for transfers"
- **Plan changes**: Refactored TransferForm mutation to use onMutate (optimistic balance update + immediate success toast), onError (rollback + error toast), onSuccess (reset form + navigate), onSettled (invalidate accounts & transactions queries).

**Implementation Notes**:

Configure mutation in `frontend/src/lib/hooks/useTransfers.ts`:
```typescript
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createTransfer } from '@/lib/api/transfers';
import type { TransferRequest, Account } from '@/lib/api/types';

export const useTransferMutation = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: createTransfer,
    onMutate: async (newTransfer: TransferRequest) => {
      // Cancel outgoing refetches
      await queryClient.cancelQueries({ queryKey: ['accounts'] });
      
      // Snapshot previous value
      const previousAccounts = queryClient.getQueryData<{ accounts: Account[] }>(['accounts']);
      
      // Optimistically update balances
      queryClient.setQueryData<{ accounts: Account[] }>(['accounts'], (old) => {
        if (!old) return old;
        
        return {
          accounts: old.accounts.map(account => {
            if (account.id === newTransfer.fromAccountId) {
              return { ...account, balance: account.balance - newTransfer.amount };
            }
            if (account.id === newTransfer.toAccountId) {
              return { ...account, balance: account.balance + newTransfer.amount };
            }
            return account;
          })
        };
      });
      
      return { previousAccounts };
    },
    onError: (err, newTransfer, context) => {
      // Rollback on error
      if (context?.previousAccounts) {
        queryClient.setQueryData(['accounts'], context.previousAccounts);
      }
    },
    onSettled: () => {
      // Refetch to ensure consistency
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
    },
  });
};
```

---

## Phase 6: E2E & CI

### Task T026: E2E Test - Login & Dashboard Load
- **Status**: done
- **Dependencies**: T018
- **Estimate**: S
- **DoD**:
  - [x] Playwright test file created: auth.spec.ts
  - [x] Test: Navigate to login page
  - [x] Test: Fill credentials and submit
  - [x] Test: Verify redirect to dashboard
  - [x] Test: Verify account cards visible
  - [x] Test: Invalid credentials show error
  - [x] Test: Unauthenticated redirect to login
  - [x] All tests pass (`npx playwright test`)
  - [ ] Committed with message "T026: E2E test - login & dashboard load"
- **Plan changes**: 

**Implementation Notes**:

Create `e2e/tests/auth.spec.ts`:
```typescript
import { test, expect } from '@playwright/test';

test.describe('Authentication', () => {
  test('user can log in with valid credentials', async ({ page }) => {
    await page.goto('/login');
    
    await page.fill('[data-testid="email"]', 'demo@bank.com');
    await page.fill('[data-testid="password"]', 'Demo123!');
    await page.click('[data-testid="login-button"]');
    
    // Should redirect to dashboard
    await expect(page).toHaveURL('/dashboard');
    await expect(page.locator('[data-testid="dashboard"]')).toBeVisible();
  });
  
  test('shows error with invalid credentials', async ({ page }) => {
    await page.goto('/login');
    
    await page.fill('[data-testid="email"]', 'wrong@example.com');
    await page.fill('[data-testid="password"]', 'WrongPassword');
    await page.click('[data-testid="login-button"]');
    
    await expect(page.locator('text=Invalid email or password')).toBeVisible();
  });
  
  test('redirects unauthenticated user to login', async ({ page }) => {
    await page.goto('/dashboard');
    
    await expect(page).toHaveURL('/login');
  });
});
```

Add `data-testid` attributes to components for stable selectors.

---

### Task T027: E2E Test - Transaction List Infinite Scroll
- **Status**: done
- **Dependencies**: T020, T026
- **Estimate**: M
- **DoD**:
  - [x] Playwright test file created: transactions.spec.ts
  - [x] Test: Login and navigate to account transactions
  - [x] Test: Initial 20 transactions load
  - [x] Test: Scroll triggers next batch load
  - [x] Test: "No more transactions" appears at end
  - [x] Test: Transactions sorted by timestamp (newest first)
  - [x] Test: Category badges visible
  - [x] All tests pass
  - [x] Committed with message "T027: E2E test - transaction list infinite scroll"
- **Plan changes**: 

**Implementation Notes**:

Create `e2e/tests/transactions.spec.ts`:
```typescript
import { test, expect } from '@playwright/test';

test.describe('Transaction List', () => {
  test.beforeEach(async ({ page }) => {
    // Login
    await page.goto('/login');
    await page.fill('[data-testid="email"]', 'demo@bank.com');
    await page.fill('[data-testid="password"]', 'Demo123!');
    await page.click('[data-testid="login-button"]');
    await expect(page).toHaveURL('/dashboard');
  });
  
  test('loads and displays transactions with infinite scroll', async ({ page }) => {
    // Click first account to view transactions
    await page.click('[data-testid="account-card"]:first-child');
    
    // Wait for initial transactions to load
    await expect(page.locator('[data-testid="transaction-row"]').first()).toBeVisible();
    
    // Count initial transactions
    const initialCount = await page.locator('[data-testid="transaction-row"]').count();
    expect(initialCount).toBeGreaterThanOrEqual(10);
    
    // Scroll to bottom
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    
    // Wait for more transactions to load
    await page.waitForTimeout(1000);
    
    const afterScrollCount = await page.locator('[data-testid="transaction-row"]').count();
    expect(afterScrollCount).toBeGreaterThan(initialCount);
  });
  
  test('displays category badges', async ({ page }) => {
    await page.click('[data-testid="account-card"]:first-child');
    
    await expect(page.locator('[data-testid="category-badge"]').first()).toBeVisible();
  });
});
```

---

### Task T028: E2E Test - Complete Transfer Flow
- **Status**: pending
- **Dependencies**: T022, T026
- **Estimate**: M
- **DoD**:
  - [x] Playwright test file created: transfer.spec.ts
  - [x] Test: Login, navigate to transfer page
  - [x] Test: Fill form with valid data
  - [x] Test: Submit transfer
  - [x] Test: Verify success message
  - [x] Test: Verify account balances updated
  - [x] Test: Verify transaction appears in list
  - [x] Test: Form validation errors display
  - [x] Test: Cannot transfer to same account
  - [x] All tests pass
  - [x] Committed with message "T028: E2E test - complete transfer flow"
- **Plan changes**: 

**Implementation Notes**:

Create `e2e/tests/transfer.spec.ts`:
```typescript
import { test, expect } from '@playwright/test';

test.describe('Transfer Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.fill('[data-testid="email"]', 'demo@bank.com');
    await page.fill('[data-testid="password"]', 'Demo123!');
    await page.click('[data-testid="login-button"]');
    await expect(page).toHaveURL('/dashboard');
  });
  
  test('completes transfer successfully', async ({ page }) => {
    // Navigate to transfer page
    await page.click('[data-testid="transfer-link"]');
    await expect(page).toHaveURL('/transfer');
    
    // Get initial balance
    await page.goto('/dashboard');
    const initialBalance = await page.locator('[data-testid="account-balance"]:first-child').textContent();
    
    // Go back to transfer
    await page.click('[data-testid="transfer-link"]');
    
    // Fill form
    await page.selectOption('[data-testid="from-account"]', { index: 0 });
    await page.selectOption('[data-testid="to-account"]', { index: 1 });
    await page.fill('[data-testid="amount"]', '100');
    await page.fill('[data-testid="description"]', 'Test transfer from E2E');
    
    // Submit
    await page.click('[data-testid="submit-transfer"]');
    
    // Verify success
    await expect(page.locator('[data-testid="success-message"]')).toContainText('Transfer successful');
    
    // Verify balance updated
    await page.goto('/dashboard');
    const newBalance = await page.locator('[data-testid="account-balance"]:first-child').textContent();
    expect(newBalance).not.toBe(initialBalance);
  });
  
  test('prevents transfer to same account', async ({ page }) => {
    await page.click('[data-testid="transfer-link"]');
    
    await page.selectOption('[data-testid="from-account"]', { index: 0 });
    await page.selectOption('[data-testid="to-account"]', { index: 0 });
    await page.fill('[data-testid="amount"]', '100');
    
    await page.click('[data-testid="submit-transfer"]');
    
    await expect(page.locator('text=Cannot transfer to the same account')).toBeVisible();
  });
});
```

---

### Task T029: CI Workflows - Final Integration
- **Status**: done
- **Dependencies**: T015, T023, T028
- **Estimate**: M
- **DoD**:
  - [x] All three CI workflows tested on GitHub
  - [x] Backend workflow passes (builds, tests, coverage uploaded)
  - [x] Frontend workflow passes (builds, lints, tests, coverage uploaded)
  - [x] E2E workflow passes (all Playwright tests green)
  - [x] Pull request checks configured (require all pass)
  - [x] Branch protection rules enabled
  - [x] Codecov integration working (optional)
  - [x] README badge added showing CI status
  - [x] Committed with message "T029: CI workflows - final integration"
- **Plan changes**: 

**Implementation Notes**:

Push to GitHub and verify all workflows run successfully:
```bash
git push origin main
```

Create a test PR to verify checks:
```bash
git checkout -b test-ci
git push origin test-ci
# Create PR on GitHub
```

Verify:
- All three workflows (backend, frontend, e2e) run
- All tests pass
- Coverage reports uploaded

Configure branch protection on `main`:
- Settings → Branches → Add rule
- Require status checks to pass before merging
- Require branches to be up to date before merging

Add CI badge to README:
```markdown
![Backend CI](https://github.com/username/repo/actions/workflows/backend.yml/badge.svg)
![Frontend CI](https://github.com/username/repo/actions/workflows/frontend.yml/badge.svg)
![E2E Tests](https://github.com/username/repo/actions/workflows/e2e.yml/badge.svg)
```

---

## Phase 7: Documentation

### Task T030: Comprehensive README & Documentation
- **Status**: done
- **Dependencies**: T029
- **Estimate**: M
- **DoD**:
  - [x] README.md updated with full project overview
  - [x] Prerequisites listed (.NET 9, Node 20+)
  - [x] Setup instructions (clone, install, run)
  - [x] Demo credentials documented
  - [x] Architecture diagram included
  - [x] Tech stack section with versions
  - [x] Testing instructions (unit, integration, E2E)
  - [x] CI/CD badges added
  - [x] Known limitations documented (InMemory DB, mock auth)
  - [x] Future enhancements section
  - [x] Contributing guidelines (if applicable)
  - [x] License added (if applicable)
  - [x] Screenshots or GIF of app (optional but recommended)
  - [x] docs/architecture/ folder with ADRs
  - [x] Committed with message "T030: Comprehensive README & documentation"
- **Plan changes**: 

**Implementation Notes**:

Update `README.md` with sections:

```markdown
# Home Banking Demo Application

A modern, full-stack home banking application demonstrating production-ready patterns with .NET 9, React 18, and TypeScript.

## 🚀 Features

- **Secure Authentication**: Mock JWT-based auth with protected routes
- **Account Management**: View multiple account types (Checking, Savings, Credit Card, Investment)
- **Transaction History**: Infinite scroll pagination with category badges
- **Money Transfers**: Validated transfers between accounts with optimistic UI updates
- **Responsive Design**: Mobile-first with Tailwind CSS and shadcn/ui
- **Comprehensive Testing**: Unit, integration, and E2E tests
- **CI/CD Pipeline**: Automated testing with GitHub Actions

## 📋 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- Git

## 🛠️ Setup Instructions

### Clone Repository
\`\`\`bash
git clone https://github.com/username/home-banking.git
cd home-banking
\`\`\`

### Backend Setup
\`\`\`bash
cd backend
dotnet restore
dotnet build
dotnet run --project src/HomeBanking.API
\`\`\`

Backend runs at: http://localhost:5000
API Documentation: http://localhost:5000/scalar/v1

### Frontend Setup
\`\`\`bash
cd frontend
npm install
npm run dev
\`\`\`

Frontend runs at: http://localhost:5173

## 🔐 Demo Credentials

- **Email**: demo@bank.com
- **Password**: Demo123!

## 🏗️ Architecture

[Include architecture diagram here]

## 🧪 Testing

### Backend Tests
\`\`\`bash
cd backend
dotnet test --collect:"XPlat Code Coverage"
\`\`\`

### Frontend Tests
\`\`\`bash
cd frontend
npm run test
npm run test:coverage
\`\`\`

### E2E Tests
\`\`\`bash
# Start backend and frontend first
cd e2e
npm test
\`\`\`

## 📦 Tech Stack

See [spec.md](spec.md) for detailed version information.

## ⚠️ Limitations

- **InMemory Database**: Data resets on API restart (by design for demo)
- **Mock Authentication**: Hardcoded credentials, no password hashing
- **No Persistence**: Not suitable for production use

## 📝 License

[Add license if applicable]
```

Create `docs/architecture/ADR-001-mock-jwt-auth.md` documenting key decisions.

---

## Summary

**Total Tasks**: 30  
**Estimated Time**: ~120-160 hours (3-4 weeks for single developer)

**Phase Breakdown**:
- Phase 1 (Foundation): 5 tasks, ~15 hours
- Phase 2 (Backend Core): 7 tasks, ~30 hours
- Phase 3 (Backend Quality): 3 tasks, ~18 hours
- Phase 4 (Frontend Core): 7 tasks, ~32 hours
- Phase 5 (Frontend Quality): 3 tasks, ~15 hours
- Phase 6 (E2E & CI): 4 tasks, ~16 hours
- Phase 7 (Documentation): 1 task, ~5 hours

**Status Tracking**:
Update task status as: `pending` → `in-progress` → `completed`

**Review Points**:
- After T012: Backend API complete, manually test all endpoints
- After T022: Frontend UI complete, manually test all flows
- After T028: E2E tests complete, full system verification
- After T030: Final review, deployment readiness check

---

**End of Plan**
