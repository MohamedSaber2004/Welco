# Plan: Duplicate Endpoint Analysis Between Welco and SNUL

## Goal
Create a comprehensive analysis plan to identify duplicate API endpoints between Welco and SNUL backend projects, comparing route patterns, HTTP methods, and controller structures.

## Background
Both Welco and SNUL share similar microservice architectures with identical service names (Auth, Product, Commerce, Sales, Content, Certification, UserManagement). After removing integration controllers, we need to verify which standard endpoints exist in both systems.

## Steps

### 1. Extract All API Routes from Both Projects
1. [ ] Parse all `*ApiRoutes.cs` files from both projects to catalog route constants
2. [ ] Extract controller classes and their `[Route]` attributes
3. [ ] Extract action methods with HTTP verb attributes (`[HttpGet]`, `[HttpPost]`, etc.)
4. [ ] Build complete endpoint inventory: `{Service} -> {Controller} -> {Method} {Route}`

### 2. Normalize Routes for Comparison
1. [ ] Replace route parameters with placeholders (e.g., `{id}` -> `{id}`)
2. [ ] Standardize base paths (e.g., `api/v1/products` vs `api/v1/product`)
3. [ ] Remove version prefixes for core comparison
4. [ ] Create canonical route signatures: `{HTTP_METHOD} /api/v1/{service}/{resource}/{action}`

### 3. Compare Endpoint Sets
1. [ ] Match endpoints by canonical signature between Welco and SNUL
2. [ ] Categorize matches:
   - **Exact duplicates**: Same route, method, service
   - **Similar endpoints**: Same resource, different routes/methods
   - **Unique to Welco**: Endpoints only in Welco
   - **Unique to SNUL**: Endpoints only in SNUL

### 4. Analyze DTO/Request/Response Contracts
1. [ ] For each duplicate endpoint, compare request/response DTOs
2. [ ] Check field names, types, validation attributes
3. [ ] Identify semantic differences despite structural similarity

### 5. Document Findings
1. [ ] Generate comparison report with tables
2. [ ] Highlight endpoints needing consolidation
3. [ ] Recommend which system should own each endpoint

## Risks / Assumptions
- Assumes both projects use consistent route naming conventions
- obj/bin folders excluded from analysis
- Integration endpoints already removed (verified)
- May need manual review for attribute-based routing variations

## Success Criteria
- Complete endpoint inventory for both projects
- Clear categorization of all duplicate/similar/unique endpoints
- Actionable recommendations for consolidation or separation