# Release Checklist

This checklist is based on `bussiness/04-acceptance-checklists.md`.

## Pre-Merge Requirements

### Build & Tests
- [ ] `dotnet build` passes (backend)
- [ ] `dotnet test` passes (backend)
- [ ] `pnpm build` passes (frontend)
- [ ] `pnpm test` passes (frontend)
- [ ] E2E tests pass

### Environment Variables
Required for production:
```bash
DATABASE_URL=postgresql://user:pass@host:5432/db
JWT_SECRET=min-32-chars-for-security
OPENAI_API_KEY=sk-...
ASPNETCORE_ENVIRONMENT=Production
```

### Migrations
- [ ] All migrations applied
- [ ] No pending migrations
- [ ] Seed data verified

### Architecture Review
- [ ] No business logic in controllers
- [ ] No business logic in UI components
- [ ] All writes go through use cases
- [ ] Provider output is normalized

### Security
- [ ] No secrets in code
- [ ] JWT properly configured
- [ ] CORS configured for production

## Release Commands

```bash
# Local pre-commit check
./scripts/check-local.ps1

# Full release gate
./scripts/release-check.ps1
```

## References
- Architecture: `bussiness/05-be-csharp-architecture-decision.md`
- Guardrails: `bussiness/03-engineering-guardrails.md`
- Checklists: `bussiness/04-acceptance-checklists.md`