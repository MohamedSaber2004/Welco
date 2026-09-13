# Client-Credentials Integration Auth Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Welco issues integration JWTs from a secured `POST /api/integration/token` endpoint (client-credentials flow); SNUL fetches + caches the token with its `client_id`/`client_secret` instead of minting locally. Same credential values, symmetric in both systems.

**Architecture:** New `IntegrationTokenController` in Welco `Auth.Services.API` (MediatR, `[AllowAnonymous]`, fixed-time secret compare, HMAC-SHA256 mint, 55-min expiry) proxied via gateway Ocelot; `ServiceAuthAttribute` validates per-`client_id` secrets with legacy fallback; SNUL `WelcoIntegrationService` fetches/caches the token (SemaphoreSlim, refresh-before-expiry) with local mint kept as transition fallback. Secrets via env vars, never committed.

**Tech Stack:** .NET 8/9, JWT (HMAC-SHA256), Welco.Shared / SNUL.Shared, Ocelot gateways (docs only), xUnit

---

## Goal (planning)
Welco becomes the sole issuer of integration JWTs via `POST /api/integration/token`; SNUL fetches + caches tokens with symmetric `client_id`/`client_secret`, without breaking current Test traffic.

## Steps (planning summary)
1. [x] Lock the token-endpoint contract (route, shapes, claims, symmetric creds, security) — DONE in Task 1 below
2. [x] SNUL: `ClientId`/`ClientSecret` config (flat + per-system) — DONE, reviewed, 6/6 tests green
3. [ ] Welco: token endpoint in Auth service + per-`client_id` validation in `[ServiceAuth]` + Ocelot route
4. [ ] SNUL: fetch + cache token from Welco endpoint (local mint stays as transition fallback)
5. [ ] Verify Test live (token 200/401 + integration calls + docs), then Production env wiring

## File Structure Map

- SNUL done: `SNUL.Shared` options/resolver/service + 12 `appsettings.*.json` (`ClientId` + empty `ClientSecret`) + `SNUL.Shared.Tests` (6 green)
- Welco create: `Auth.Services.API/Features/Integration/Token/...` (MediatR command/handler), `Auth.Services.API/Controllers/IntegrationTokenController.cs`
- Welco modify: `Welco.Shared/Common/Options/WelcoServiceSettings.cs` (`Clients` map), `Welco.Shared/Common/Attributes/ServiceAuthAttribute.cs`
- Welco modify: `Auth.Services.API/appsettings.*.json` (`WelcoServiceSettings:Clients`), `Welco.API/Ocelot/ocelot.integration.{Development,Test,Production}.json` (token route → auth host) + env vars
- Test: `Auth.Services.API` tests for endpoint, `Welco.Tests/*ServiceAuth*`, SNUL fetch/cache tests with mocked HTTP

## Current State (verified in repo)

- Welco validates in `Welco.Shared/Common/Attributes/ServiceAuthAttribute.cs:62-102`: single `WelcoServiceSettings.ServiceSecret` (min 32 chars), `ValidIssuer=snul-integration`, `ValidAudience=welco-integration`, 2-min skew. Applied via `[ServiceAuth]` on every `Integration*Controller` (sales quotes, content support/help, commerce orders/inventory, product products/providers/categories, etc.).
- SNUL generates in `SNUL.Shared/Common/Services/WelcoIntegrationService.cs:302-327` (`GenerateServiceJwt`): same secret/issuer/audience, claims `sub=snul-integration`, `jti`, `service=snul`, `market`. Per-system routing via `WelcoSystemTarget`/`WelcoSystemResolver`, but secret is still effectively one value copied across appsettings (`A;Jl^7f...` in plain text).
- Gap: no `client_id` identity, no per-client secret, secret in committed JSON.

### Task 1: Token-endpoint contract (LOCKED — implement against this, do not renegotiate)

- [x] **Endpoint:** `POST /api/integration/token` → Welco `Auth.Services.API`, proxied by gateway Ocelot (same as other integration routes). Public (no JWT) like Login; secured by client_secret + HTTPS + gateway rate limiting.
- [x] **Request (JSON):** `{ "clientId": "snul", "clientSecret": "<min-32-chars>" }`
- [x] **Response (standard envelope):** `data: { "accessToken": "<jwt>", "expiresIn": 3300, "tokenType": "Bearer" }`; unknown client or bad secret → 401 `Invalid client credentials.` (same message both cases — no oracle).
- [x] **Minted claims:** `iss=snul-integration`, `aud=welco-integration` (UNCHANGED — keeps ServiceAuth issuer config identical), `sub=snul-integration`, `jti`, `client_id`, `service`, `market` (from client config, default `Egypt`); expiry 55 min.
- [x] **Symmetric values:** identical `client_id`+`client_secret` strings in Welco `WelcoServiceSettings:Clients:{id}` and SNUL `WelcoIntegration` (`:Systems:{sys}` or flat); real secrets ONLY via env vars (`WelcoServiceSettings__Clients__snul__Secret`, `WelcoIntegration__ClientSecret`); JSON carries `ClientId` + empty `ClientSecret`.
- [x] **Secret compare:** fixed-time (`CryptographicOperations.FixedTimeEquals`) on the token endpoint.
- [x] **Compat:** `ServiceAuth` dual-accepts one release — token WITH `client_id` → per-client secret path; WITHOUT → legacy shared-secret path + warn log. SNUL local mint stays as fallback until Test green, then removed.

### Task 2: SNUL — fetch + cache token from Welco (replaces local mint as primary)

**Files:**
- Modify: `E:\SNUL Site\backend\SNUL\SNUL.Shared\Common\Services\WelcoIntegrationService.cs` — add `GetServiceTokenAsync(target, ct)` (SemaphoreSlim-cached, refresh 5 min early; POST `{BaseUrl}/api/integration/token` with target ClientId/ClientSecret-or-ServiceSecret; 401 → clear cache + throw `UnauthorizedAccessException("...invalid client credentials...")`, no retry loop); `AttachHeaders` becomes async-caller of it.
- Keep: `GenerateServiceJwt` as transition fallback ONLY when the token endpoint is unreachable (network/timeout — NOT on 401), with warn log. Removal in follow-up.
- Test: extend `SNUL.Shared.Tests` with mocked `HttpMessageHandler`: endpoint success → cached reuse (one HTTP call for two AttachHeaders); 401 → throws + cache cleared; endpoint down → falls back to local mint.

- [ ] **Step 1: Failing tests** for cache-reuse, 401-no-retry, and down-fallback.

Run: `dotnet test SNUL.Shared.Tests/ -k "TokenFetch"`
Expected: FAIL (no fetch path today)

- [ ] **Step 2: Implement fetch + cache only** (no claim/contract changes — Task 1 locked).

- [ ] **Step 3: Full suite green + commit**

```bash
git add SNUL.Shared/ SNUL.Shared.Tests/
git commit -m "feat(snul): fetch integration token from welco endpoint"
```

> Status: config half (flat + per-system creds, 12 appsettings, 6/6 tests) DONE + reviewed. This task adds the fetch path.

### Task 3: Welco — token endpoint + per-client validation

**Files:**
- Modify: `E:\Welco Site\Welco\Welco.Shared\Common\Options\WelcoServiceSettings.cs` — add `Dictionary<string, IntegrationClient> Clients` (`Secret` required min-32, `Market` default `Egypt`); keep flat `ServiceSecret`/`ServiceIssuer`/`ServiceAudience` as legacy fallback.
- Create: `E:\Welco Site\Welco\Auth.Services.API\Features\Integration\Token\CreateIntegrationTokenCommand.cs` (+ handler: lookup client case-insensitive, fixed-time secret compare via `CryptographicOperations.FixedTimeEquals`, mint HMAC-SHA256 JWT `iss=snul-integration`/`aud=welco-integration`/`sub=snul-integration`/`jti`/`client_id`/`service`/`market`, 55-min expiry; unknown/bad → `Result.Unauthorized("Invalid client credentials.")` — identical message both cases).
- Create: `E:\Welco Site\Welco\Auth.Services.API\Controllers\IntegrationTokenController.cs` — `[AllowAnonymous][ApiController][Route("api/integration/token")] : AppControllerBase`, single `[HttpPost] Issue(...)` following `AuthController` MediatR/`ToActionResult` pattern.
- Modify: `E:\Welco Site\Welco\Welco.Shared\Common\Attributes\ServiceAuthAttribute.cs:62-102` — token WITH `client_id` → resolve secret from `Clients` (unknown → 401); WITHOUT → legacy shared-secret path + warn log (one release).
- Modify: `E:\Welco Site\Welco\Auth.Services.API\appsettings.Test.json` (+ Development) — `WelcoServiceSettings:Clients:snul` with `ClientId: snul`, EMPTY `Secret` (env var `WelcoServiceSettings__Clients__snul__Secret` at runtime).
- Modify: `E:\Welco Site\Welco\Welco.API\Ocelot\ocelot.integration.{Development,Test,Production}.json` — add `POST /api/integration/token` route → auth host (`localhost:7203` Dev / `welco-auth.runasp.net:443` Test+Prod, same `DangerousAcceptAnyServerCertificateValidator` as sibling routes).

- [ ] **Step 1: Failing tests** — (a) endpoint: valid creds → 200 + JWT with `client_id`, wrong secret AND unknown client → identical 401; (b) ServiceAuth: per-client-signed token passes, wrong-secret/unknown-client fails, legacy token still passes with warn.

Run: `dotnet test Welco.Tests/ Auth.Tests/ -k "IntegrationToken or ServiceAuth"`
Expected: FAIL (no endpoint, single-secret only)

- [ ] **Step 2: Implement settings + endpoint + validator** (follow `AuthController`/`LoginCommand` patterns exactly; no changes to existing auth flows).

- [ ] **Step 3: Full suites green + commit**

```bash
git add Welco.Shared/ Auth.Services.API/ Welco.API/Ocelot/ Welco.Tests/
git commit -m "feat(welco): issue integration JWT from token endpoint"
```

### Task 4: Rollout + verify (Test first, per your target)

- [ ] **Step 1: Generate + wire Test credentials** — on your machine run `.\scripts\New-IntegrationClient.ps1 -ClientId snul` (CSPRNG, 48-char secret, shown once); set `WelcoServiceSettings__Clients__snul__Secret` on Welco Auth (Test) and `WelcoIntegration__ClientSecret` on SNUL services (Test) to the SAME value; restart Auth, gateway, one SNUL caller.
- [ ] **Step 2: Live Test checks**
  - `POST https://welco-gateway.runasp.net/api/integration/token` with creds → 200 + `accessToken`; wrong secret → 401 (identical message).
  - SNUL→Welco call (e.g. products) works end-to-end on fetched token; `GET .../api/integration/support/tickets` (no token) → 401.
  - `GET .../api/docs/integration/openapi.json` → lists `POST /api/integration/token` + 25 existing paths.
  - Logs: zero `Unknown client` / unexpected `Token validation failed` for legit traffic.
- [ ] **Step 3: Remove legacy** — after Test green one release: delete SNUL local-mint fallback + Welco legacy shared-secret path + old flat secrets; rotate to distinct per-client secrets. Never commit secrets.

### Task 5: SNUL TEST-ONLY token endpoint (SUPERSEDES public-issuer variant — no credentials in request)

- No public client-credentials endpoint in SNUL. Instead ONE test-only endpoint in `Commerce.Services.API` (home of `IntegrationController`): `GET api/v1/integration/test/token`, `[AllowAnonymous]`, HARD-GATED to non-Production (`IWebHostEnvironment` Development/Test only → 404 otherwise). No gateway Ocelot change (direct service access in Test).
- Reads `ClientId` + `ClientSecret`-else-`ServiceSecret` from `IOptions<WelcoIntegrationOptions>` (option pattern, env-backed values); misconfigured/short → 500 misconfiguration (no leak). No request body, no query credentials.
- Mints via shared `WelcoIntegrationCredentials` resolver semantics so SNUL's own `[ServiceAuth]` accepts the token (claims identical to Welco-issued: iss/aud, `client_id`, 55-min, no `azp`).
- Automatic Authorization on integrated calls is EXISTING (`AttachHeadersAsync` fetch+cache+Bearer on all Send paths) — verify, don't rebuild.
- Tests: minted token passes SNUL filter; Production gate returns 404; misconfigured secret → 500; full suites green; no commits; no secrets.

## Risks / Assumptions

- Token endpoint is intentionally public (like Login) — its security IS the client_secret + HTTPS + gateway rate limiting; weak secrets (<32 chars rejected) or HTTP would void it.
- Same value copy-pasted per client defeats the purpose — distinct 32+ char secrets per system after rollout.
- Clock skew: 2-min both sides; 55-min token expiry both sides (Welco mints 55, SNUL refreshes 5 min early).
- `iss`/`aud` deliberately unchanged (`snul-integration`/`welco-integration`) so existing ServiceAuth issuer config and old tokens keep working during dual-accept.
- Assumption: HMAC-SHA256 stays; no per-client scopes/roles in token yet (all clients get same integration surface).

## Success Criteria

- `POST /api/integration/token` issues JWTs with `client_id` (no `azp`); bad creds → identical 401.
- SNUL Test fetches + caches (one token call per ~50 min per instance); integration calls for quotes/support/help succeed on fetched tokens.
- ServiceAuth accepts per-client tokens, still accepts legacy during window, rejects unknown/wrong with 401.
- No secret in git; Production untouched until Test is green.

## Execution Status (2026-09-13, all uncommitted unless noted)

- [x] Welco token endpoint + per-client ServiceAuth + Ocelot routes + Auth Clients blocks (29/29 tests; user committed as `a127498`)
- [x] SNUL ClientId/ClientSecret config + fetch/cache + overhaul (ServiceAuth-only controller, mint deleted, secrets scrubbed; 21/21 tests)
- [x] `scripts/New-IntegrationClient.ps1` CSPRNG generator (verified, secret never in repo)
- [x] Secret scrub: real ClientSecret value found in all 12 SNUL appsettings removed (0 hits, JSON valid, 21/21)
- [ ] USER: set env vars on Test hosts (same pair both sides), deploy Welco Auth + gateway + SNUL Commerce, run Task 4 live checks
- [ ] Follow-up: delete SNUL local-mint fallback + Welco legacy path + old secrets; rotate to distinct per-client secrets; decide on `prompt.txt` live Admin JWT (delete + rotate) and SNUL gateway JwtSettings secret (move to env)
