# Integration Docs Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make `GET /api/docs/integration/openapi.json` (Scalar `/docs/integration`) show all `/api/integration/*` endpoints instead of `{}`.

**Architecture:** Reuse existing aggregated doc (`GetAggregatedOpenApiAsync`), filter to `paths` starting with `/api/integration/`, special-case the `integration` service name in gateway `Program.cs`. Fix Production Ocelot hosts.

**Tech Stack:** ASP.NET Core (.NET 8/9), Ocelot, Microsoft.AspNetCore.OpenApi, Scalar, System.Text.Json.Nodes

---

## File Structure Map

- Modify: `Welco.API/Services/OpenApiAggregatorService.cs` — add `GetIntegrationOpenApiAsync()`, owns integration-only filtering.
- Modify: `Welco.API/Program.cs:325-341` — special-case `integration` DocRoute to call new method.
- Modify: `Welco.API/Ocelot/ocelot.integration.Production.json` — replace `localhost` hosts with `welco-*.runasp.net:443`.
- Test: `Welco.Tests/` — add aggregator filter unit test (new file if missing, check existing).
- Verify live: `https://welco-gateway.runasp.net/api/docs/integration/openapi.json` + `/openapi/all.json` (128 paths today).

## Task Outlines

### Task 1: Fix Production Ocelot hosts

**Files:**
- Modify: `Welco.API/Ocelot/ocelot.integration.Production.json`
- Test: manual `git diff` + compare hosts to `ocelot.integration.Test.json`

- [ ] **Step 1: Verify current broken hosts**

Run: `Select-String -Pattern "localhost" Welco.API/Ocelot/ocelot.integration.Production.json`
Expected: 8+ hits on ports 7045/7046/7047/7054/7101/7204

- [ ] **Step 2: Replace hosts per-route (copy from Test file)**

Mapping: orders/inventory→`welco-commerce.runasp.net`, products/providers/categories→`welco-product.runasp.net`, quotes→`welco-sales.runasp.net`, support/help→`welco-content.runasp.net`, distributors→`welco-user.runasp.net`, certifications→`welco-certification.runasp.net`, all Port 443, `DangerousAcceptAnyServerCertificateValidator: true` (match Test).

- [ ] **Step 3: Diff + commit**

Run: `git diff Welco.API/Ocelot/ocelot.integration.Production.json`
Expected: only Host/Port lines changed, no route renames.
```bash
git add Welco.API/Ocelot/ocelot.integration.Production.json
git commit -m "fix(gateway): point production integration routes to runasp hosts"
```

### Task 2: Add GetIntegrationOpenApiAsync

**Files:**
- Modify: `Welco.API/Services/OpenApiAggregatorService.cs:176` (insert after `GetServiceOpenApiAsync`, before `WarmUpAsync`)
- Test: `Welco.Tests/OpenApiAggregatorIntegrationFilterTests.cs` (create if missing — check folder first)

- [ ] **Step 1: Insert method skeleton (no logic)**

```csharp
public async Task<string> GetIntegrationOpenApiAsync(string gatewayBaseUrl, CancellationToken cancellationToken = default)
{
    throw new NotImplementedException();
}
```

- [ ] **Step 2: Implement filter (full body)**

```csharp
public async Task<string> GetIntegrationOpenApiAsync(string gatewayBaseUrl, CancellationToken cancellationToken = default)
{
    var allJson = await GetAggregatedOpenApiAsync(gatewayBaseUrl, cancellationToken);
    var node = JsonNode.Parse(allJson);
    if (node is not JsonObject obj) return allJson;
    if (obj["info"] is JsonObject info)
    {
        info["title"] = "Welco Integration API";
        info["description"] = "External integration endpoints (ServiceAuth service-secret). Internal /api/v1/* excluded.";
    }
    if (obj["paths"] is JsonObject paths)
    {
        var toRemove = paths.Where(kv => !kv.Key.StartsWith("/api/integration/", StringComparison.OrdinalIgnoreCase)).Select(kv => kv.Key).ToList();
        foreach (var k in toRemove) paths.Remove(k);
    }
    return obj.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
}
```
`JsonNode/JsonObject/JsonSerializerOptions` usings already exist in file (lines 1-2).

- [ ] **Step 3: Add unit test** — parse result, assert every path starts with `/api/integration/` and `/api/v1/rfqs` absent, `info.title` equals `Welco Integration API`.

- [ ] **Step 4: Commit**

```bash
git add Welco.API/Services/OpenApiAggregatorService.cs Welco.Tests/
git commit -m "feat(gateway): add integration-only OpenAPI aggregation"
```

### Task 3: Wire Program.cs special-case

**Files:**
- Modify: `Welco.API/Program.cs:325-341` (inside `foreach (var doc in microserviceDocRoutes)`, the `MapGet(doc.DocRoute, ...)` handler)

- [ ] **Step 1: Branch on integration**

```csharp
var json = docServiceName.Equals("integration", StringComparison.OrdinalIgnoreCase)
    ? await aggregator.GetIntegrationOpenApiAsync(gatewayBaseUrl, ct)
    : await aggregator.GetServiceOpenApiAsync(docServiceName, gatewayBaseUrl, ct);
```
Replaces the single line `var json = await aggregator.GetServiceOpenApiAsync(...)`.

- [ ] **Step 2: Build gateway**

Run: `dotnet build Welco.API/Welco.Gateway.API.csproj -c Release`
Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Step 3: Commit**

```bash
git add Welco.API/Program.cs
git commit -m "feat(gateway): serve filtered integration docs page"
```

### Task 4: Build + live verify (Test only)

**Files:** none (verification only)

- [ ] **Step 1: Local gateway smoke** — run gateway + sales + content in Development, open `/api/docs/integration/openapi.json`, assert contains `/api/integration/quotes` and `/api/integration/support/tickets`, assert NOT contains `/api/v1/rfqs`.

- [ ] **Step 2: Test-env live checks (no deploy needed until merged)**

```powershell
(Invoke-WebRequest https://welco-gateway.runasp.net/api/docs/integration/openapi.json -UseBasicParsing | ConvertFrom-Json).paths | Get-Member -MemberType NoteProperty | Select -Expand Name
```

Expected after deploy: ~25 `/api/integration/*` paths, 0 `/api/v1/*` paths. Routing already verified Test: `GET .../support/tickets`→401, `GET .../quotes`→405 prove Ocelot reaches downstream.

- [ ] **Step 3: Self-review** — no TBD/TODO, method names match (`GetIntegrationOpenApiAsync`), Production hosts fixed, Test file untouched (already correct).
