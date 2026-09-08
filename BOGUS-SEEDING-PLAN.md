# Bogus Test-Data Seeding — Implementation Plan

> Set up the [Bogus](https://github.com/bchavez/Bogus) fake-data package and
> use it to seed realistic development data into the database.
> Plan only — no implementation yet.

## 1. Analysis findings (audited against the repo)

- **No Bogus reference exists** anywhere in the solution (verified via
  `*.csproj` search). Target framework is **net10.0**, EF Core **10.0.11**,
  SQL Server.
- Seeders live in **`Welco.Shared/Persistance/Seeding/`** as static classes
  with `Seed*Async(WelcoDbContext db, ILogger logger)` (or
  `UserManager`/`RoleManager` variants), all **idempotent**
  (`AnyAsync`/`Exists` guards, `!IsDeleted` filters). Invoked per service:
  - `Auth.Services.API/Program.cs` → `RoleSeeder`, `UserSeeder`
  - `UserManamgent.Service.API/Program.cs` → `RoleSeeder`
  - `Content.Services.API/Program.cs` → `LandingPageSeeder.SeedAboutUsAsync`
  - `Product.Services.API/Program.cs` → `CurrencySeeder`, `WorldLocationSeeder`
- Domain models expose **factory methods** (`Product.Create`,
  `Company.Create`, …) plus `MarkAsCreated("…")` audit on
  `BaseEntity<Guid>` (soft-delete via `IsDeleted`). Bogus seeders must use
  the factories, never bare initializers, so invariants hold.
- Identity users must go through **`UserManager`** (password hashing, roles);
  plain entities go through `WelcoDbContext` + `SaveChangesAsync`.
- Enums available: `UserType` (Admin 1, OrganizationUser 2, WelcoStaff 3,
  Customer 4), `CompanyType`, `CompanyStatus`, `AppLanguage`, `MediaType`.
- Uniqueness constraints to respect: `Product.Sku`/`Slug`, user/ company
  e-mails, `LandingPage.Slug`.

## 2. Phase 1 — Package setup (~15 min)

1. Add to `Welco.Shared/Welco.Shared.csproj` (seeders already live there —
   minimal churn, follows repo convention):
   ```xml
   <PackageReference Include="Bogus" Version="35.*" />
   ```
   via `dotnet add Welco.Shared/Welco.Shared.csproj package Bogus`.
   The DLL ships but never executes outside seeding paths (see Phase 2).
2. `dotnet restore && dotnet build Welco.sln` green.
3. (Alternative, if the team prefers zero prod-binaries churn: new
   `Welco.Seeding` project referenced only by API hosts. Not recommended —
   doubles wiring for no runtime benefit since execution is env-gated.)

## 3. Phase 2 — Seeder harness (safety first)

1. New static class `BogusDemoSeeder` in `Persistance/Seeding/` with
   `SeedDemoAsync(WelcoDbContext db, UserManager<ApplicationUser> users, ILogger log)`.
2. **Env gate — demo data only in Development**: run only when
   `IHostEnvironment.IsDevelopment()` **and** explicit opt-in
   (`SEED_DEMO_DATA=true` env var / appsettings flag, default off).
   Reference-data seeders (roles, currencies, about-us) keep running
   unconditionally as today.
3. **Idempotency**: skip entirely if any non-deleted `Product` with
   `CreatedBy == "BogusSeeder"` exists; all faker entities set
   `CreatedBy = "BogusSeeder"` so demo rows are identifiable and bulk-
   deletable. Deterministic output via `Randomizer.Seed = new Random(1234)`.
4. Wire into `Product.Services.API/Program.cs` (catalog sets) and
   `Auth`/`UserManamgent` programs only for the user/company parts they own
   — or a single call site where `WelcoDbContext` + `UserManager` are both
   available. Order matters (FKs): Countries/Cities/Zones (already seeded)
   → Currencies (seeded) → Categories → Products (+specs/media/tags) →
   Companies (+addresses) → Users (via `UserManager`, roles assigned) →
   RFQs → Quotes → Orders (+items/invoices) → Help/FAQs/Tickets/TradeShows.

## 4. Phase 3 — Faker rules per entity (suggested volumes)

| Entity | Volume | Key rules |
|---|---|---|
| `Category` | ~6 roots + ~18 children | Fixed EN/AR names (Dental, Orthopedic, Spine, Plastic Surgery, CMF, ENT); slug from name; tree via `ParentCategoryId` |
| `Product` | ~120 | `Sku` = `"WL-" + 6 digits` (unique), `Slug` unique, price 15–2500, stock 0–300, `Material` from pool (German Stainless Steel…), `LengthCm` 10–30, `CategoryId` from seeded categories, `CurrencyId` = USD id |
| `ProductSpecification/Media/ProcedureTag` | 2–4 / 1–3 / 1–2 per product | Spec pairs (Sterilization/Autoclave 134°C, DIN 1.4021…); `ImageName` placeholder file names only (no blobs) |
| `Company` | ~12 | Types Hospital/Distributor/Clinic/Importer, `Status = Approved`, `TierLevel` 1–3, country from seeded list; half flagged `IsProvider` |
| `ApplicationUser` | ~20 | Via `UserManager.CreateAsync` + `AddToRoleAsync`; `Customer` (no company), `OrganizationUser` linked to companies, `EmailConfirmed = true`, `IsActive = true`, password from dev secret (never committed — env var) |
| `RFQ → Quote → Order` | ~15 chains | `RFQItem` qty 5–200; `Quote` priced ±10% with `ValidUntil` +30d; `Order` with `OrderNumber` sequence, `Incoterm` EXW/FOB/CIF/DDP, items snapshot prices |
| `HelpCategory/HelpArticle/FAQItem` | 5 / ~15 / ~10 | EN + AR bodies (curated Arabic strings, not machine filler) |
| `TradeShowEvent` | ~6 | Past + upcoming dates, real expo names/cities |
| Skip | — | `Document` file blobs (seed metadata only if needed), `AuditLog` (system-generated), `Notification` |

**Arabic data**: do not rely on a Bogus `ar` locale dataset; use curated
in-code EN/AR pools (instrument names, specialties, cities) with
`faker.Random.ArrayElement(...)` / `faker.PickRandom(...)`.

## 5. Phase 4 — Verification & acceptance

1. `dotnet build Welco.sln` — 0 errors.
2. Run Product + Auth + Content services with `ASPNETCORE_ENVIRONMENT=Development`
   and `SEED_DEMO_DATA=true`; confirm log lines `Seeded … (Bogus)`.
3. Row counts: products by category, users by type/role, one full
   RFQ→Quote→Order chain per company; public storefront + admin lists render.
4. Re-run → **zero new rows** (idempotency); `SEED_DEMO_DATA` unset → zero
   demo rows; Production config → seeder method never invoked (assert via
   startup log review).
5. `dotnet test` green.
6. Acceptance: (1) fresh dev DB is demo-ready with one env flag;
   (2) re-runs are no-ops; (3) nothing demo-related can execute in
   Production; (4) all rows traceable via `CreatedBy = "BogusSeeder"`.

## 6. Out of scope / notes

- Real file uploads for `ProductMedia`/`Document` blobs (names only).
- Production/anonymized-prod cloning — Development synthetic data only.
- Follow-up (optional): reuse the faker rules as test-data builders in
  `Welco.Tests`.
- Risks: dev password handling (env var, never committed); unique-index
  collisions on re-seed after manual edits (guarded by the marker check —
  wipe `CreatedBy = 'BogusSeeder'` rows to regenerate).
