#!/usr/bin/env bash
###############################################################################
# validate.sh — Single source of truth for quality gates
#
# Usage:  ./scripts/validate.sh          (runs all checks)
#         ./scripts/validate.sh backend  (backend only)
#         ./scripts/validate.sh frontend (frontend only)
###############################################################################
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

pass() { echo -e "${GREEN}✔ $1${NC}"; }
fail() { echo -e "${RED}✖ $1${NC}"; exit 1; }
info() { echo -e "${YELLOW}▸ $1${NC}"; }

###############################################################################
# Backend — .NET 9 (src/api)
###############################################################################
run_backend() {
  info "Backend: building…"
  dotnet build "$ROOT_DIR/src/api" || fail "dotnet build"
  pass "Backend: build succeeded"

  info "Backend: running tests…"
  dotnet test "$ROOT_DIR/src/api" || fail "dotnet test"
  pass "Backend: tests passed"
}

###############################################################################
# Frontend — React + Vite + TypeScript (src/web)
###############################################################################
run_frontend() {
  info "Frontend: installing dependencies…"
  npm ci --prefix "$ROOT_DIR/src/web" || fail "npm ci"
  pass "Frontend: install succeeded"

  info "Frontend: linting…"
  npm run lint --prefix "$ROOT_DIR/src/web" || fail "npm run lint"
  pass "Frontend: lint passed"

  info "Frontend: running tests…"
  npm run test --prefix "$ROOT_DIR/src/web" || fail "npm run test"
  pass "Frontend: tests passed"

  info "Frontend: building…"
  npm run build --prefix "$ROOT_DIR/src/web" || fail "npm run build"
  pass "Frontend: build succeeded"
}

###############################################################################
# Main
###############################################################################
TARGET="${1:-all}"

echo "============================================"
echo " Home Banking — Validation"
echo "============================================"
echo ""

case "$TARGET" in
  backend)  run_backend  ;;
  frontend) run_frontend ;;
  all)
    run_backend
    echo ""
    run_frontend
    ;;
  *)
    echo "Usage: $0 {all|backend|frontend}"
    exit 1
    ;;
esac

echo ""
echo -e "${GREEN}All validations passed!${NC}"
