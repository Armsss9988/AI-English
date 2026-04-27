# Personal Mode — Local Identity

## Overview

This app runs in **personal-local mode**: a single learner uses it locally without any login or authentication system. This is intentional for the current phase.

## How Identity Works

### Learner Identity

All `/me/*` endpoints resolve the current user via the `X-User-Id` HTTP header.

- **If the header is present**: the provided value is used as the user ID.
- **If the header is missing**: a stable default user ID (`00000000-0000-0000-0000-000000000001`) is used automatically.

This means the frontend does **not** need to send any auth tokens or login credentials. All requests "just work" for the local learner.

### Admin Identity

Admin content routes (`/admin/*`) require the `X-User-Role: Admin` header. This is a simple header check, **not** a real authorization system.

- The check is **case-insensitive** (`Admin`, `admin`, `ADMIN` all work).
- Without the header, admin routes return `403 Forbidden`.
- The local frontend sends this header automatically when accessing admin pages.

## What This Is NOT

- ❌ This is **not** production-grade security.
- ❌ There is no JWT, OAuth, session store, or password flow.
- ❌ There is no multi-user isolation.

## When Real Auth Will Be Added

Real authentication (likely JWT with an external provider) will be added in a future phase when the app needs:

- Multi-user support
- Cloud deployment
- Data isolation between users

Until then, personal-local mode is the documented and tested default.

## API Endpoint

`GET /me/identity` returns the current identity metadata:

```json
{
  "userId": "00000000-0000-0000-0000-000000000001",
  "mode": "personal-local",
  "isAdmin": false,
  "note": "Production auth is intentionally deferred. See docs/PERSONAL-MODE.md."
}
```

## Headers Reference

| Header | Purpose | Required |
|---|---|---|
| `X-User-Id` | Override the default learner ID | Optional |
| `X-User-Role` | Set to `Admin` for admin routes | Required for `/admin/*` only |
