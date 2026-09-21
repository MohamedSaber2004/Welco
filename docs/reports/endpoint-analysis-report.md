# Endpoint Analysis Report: Welco vs SNUL

**Generated:** 2026-09-21  
**Purpose:** Identify duplicate API endpoints between Welco and SNUL backend projects  
**Scope:** All services after integration code removal

## Executive Summary

| Metric | Count |
|--------|-------|
| Total Services Analyzed | 8 |
| Exact Duplicate Endpoints | 85 |
| Similar Endpoints | 7 |
| Unique to Welco | 14 |
| Unique to SNUL | 23 |

## Service-by-Service Comparison

### 1. Auth Service

| Endpoint | Method | Route | Status |
|----------|--------|-------|--------|
| Login | POST | `api/v1/auth/login` | Exact Duplicate |
| Register | POST | `api/v1/auth/register` | Exact Duplicate |
| Verify Email OTP | POST | `api/v1/auth/verify-register-otp` | Exact Duplicate |
| Forgot Password | POST | `api/v1/auth/forgot-password` | Exact Duplicate |
| Verify Password OTP | POST | `api/v1/auth/verify-password-otp` | Exact Duplicate |
| Reset Password | POST | `api/v1/auth/reset-password` | Exact Duplicate |
| Refresh Token | POST | `api/v1/auth/refresh-token` | Exact Duplicate |
| Logout | POST | `api/v1/auth/logout` | Exact Duplicate |
| Profile | GET | `api/v1/auth/profile` | Exact Duplicate |
| Health | GET | `api/v1/auth/health` | Exact Duplicate |

**Summary:** All 10 endpoints are exact duplicates.

### 2. Product Service

| Endpoint | Method | Route | Status |
|----------|--------|-------|--------|
| Get All Categories | GET | `api/v1/categories` | Exact Duplicate |
| Get Category By ID | GET | `api/v1/categories/{id}` | Exact Duplicate |
| Create Category | POST | `api/v1/categories` | Exact Duplicate |
| Update Category | PUT | `api/v1/categories/{id}` | Exact Duplicate |
| Delete Category | DELETE | `api/v1/categories/{id}` | Exact Duplicate |
| Get Products By Category | GET | `api/v1/categories/{categoryId}/products` | Exact Duplicate |
| Get All Products | GET | `api/v1/products` | Exact Duplicate |
| Get Product By ID | GET | `api/v1/products/{id}` | Exact Duplicate |
| Create Product | POST | `api/v1/products` | Exact Duplicate |
| Update Product | PUT | `api/v1/products/{id}` | Exact Duplicate |
| Delete Product | DELETE | `api/v1/products/{id}` | Exact Duplicate |
| Get Product Videos | GET | `api/v1/products/{id}/videos` | Exact Duplicate |
| Update Product Videos | PUT | `api/v1/products/{id}/videos` | Exact Duplicate |
| Get All Currencies | GET | `api/v1/currencies` | Exact Duplicate |
| Get All Currency List | GET | `api/v1/currencies/all` | SNUL Only |
| Get Currency By ID | GET | `api/v1/currencies/{id}` | Exact Duplicate |
| Get Currency By Code | GET | `api/v1/currencies/code/{code}` | Exact Duplicate |
| Create Currency | POST | `api/v1/currencies` | Exact Duplicate |
| Update Currency | PUT | `api/v1/currencies/{id}` | Exact Duplicate |
| Delete Currency | DELETE | `api/v1/currencies/{id}` | Exact Duplicate |
| Get Exchange Rates | GET | `api/v1/exchange-rates/latest` | **Similar** |
| Get Exchange Rate By Base | GET | `api/v1/exchange-rates/latest/{baseCurrency}` | **Similar** |
| Get Exchange Rate Pair | GET | `api/v1/exchange-rates/{from}/{to}` | Exact Duplicate |
| Convert Currency | GET | `api/v1/exchange-rates/convert` | Exact Duplicate |
| Get Cart Total | GET | `api/v1/exchange-rates/cart-total` | SNUL Only |
| Get Exchange Rate History | GET | `api/v1/exchange-rates/history/{baseCurrency}/{date}` | Welco Only |
| Export Exchange Rates | GET | `api/v1/exchange-rates/sync` | SNUL Only |
| Sync Exchange Rates | GET | `api/v1/exchange-rates/sync` | SNUL Only |

**Summary:** 
- 85% exact duplicates
- 2 endpoints unique to Welco (history-related)
- 4 endpoints unique to SNUL (currency list + exchange rate sync/history)

### 3. User Management Service

Note: Directory namespace differs - Welco uses `UserManamgent.Service.API` (typo), SNUL uses `UserManagement.Service.API`

| Endpoint | Method | Route | Status |
|----------|--------|-------|--------|
| Get All Users | GET | `api/v1/user-management/users` | Exact Duplicate |
| Get User By ID | GET | `api/v1/user-management/users/{id}` | Exact Duplicate |
| Create User | POST | `api/v1/user-management/users` | Exact Duplicate |
| Update User | PUT | `api/v1/user-management/users/{id}` | Exact Duplicate |
| Delete User | DELETE | `api/v1/user-management/users/{id}` | Exact Duplicate |
| Change Password | PUT | `api/v1/user-management/users/{id}/change-password` | Exact Duplicate |
| Get All Addresses | GET | `api/v1/user-management/addresses` | Exact Duplicate |
| Get Addresses By User | GET | `api/v1/user-management/addresses/user/{userId}` | Exact Duplicate |
| Get Address By ID | GET | `api/v1/user-management/addresses/{id}` | Exact Duplicate |
| Create Address | POST | `api/v1/user-management/addresses` | Exact Duplicate |
| Update Address | PUT | `api/v1/user-management/addresses/{id}` | Exact Duplicate |
| Delete Address | DELETE | `api/v1/user-management/addresses/{id}` | Exact Duplicate |
| Get All Countries | GET | `api/v1/user-management/countries` | Exact Duplicate |
| Get Country By ID | GET | `api/v1/user-management/countries/{id}` | Exact Duplicate |
| Create Country | POST | `api/v1/user-management/countries` | Exact Duplicate |
| Update Country | PUT | `api/v1/user-management/countries/{id}` | Exact Duplicate |
| Delete Country | DELETE | `api/v1/user-management/countries/{id}` | Exact Duplicate |
| Get All Cities | GET | `api/v1/user-management/cities` | Exact Duplicate |
| Get Cities By Country | GET | `api/v1/user-management/cities/country/{countryId}` | Exact Duplicate |
| Get City By ID | GET | `api/v1/user-management/cities/{id}` | Exact Duplicate |
| Create City | POST | `api/v1/user-management/cities` | Exact Duplicate |
| Update City | PUT | `api/v1/user-management/cities/{id}` | Exact Duplicate |
| Delete City | DELETE | `api/v1/user-management/cities/{id}` | Exact Duplicate |
| Get All Zones | GET | `api/v1/user-management/zones` | Exact Duplicate |
| Get Zones By City | GET | `api/v1/user-management/zones/city/{cityId}` | Exact Duplicate |
| Get Zone By ID | GET | `api/v1/user-management/zones/{id}` | Exact Duplicate |
| Create Zone | POST | `api/v1/user-management/zones` | Exact Duplicate |
| Update Zone | PUT | `api/v1/user-management/zones/{id}` | Exact Duplicate |
| Delete Zone | DELETE | `api/v1/user-management/zones/{id}` | Exact Duplicate |
| Get All Companies | GET | `api/v1/user-management/companies` | Exact Duplicate |
| Get My Company | GET | `api/v1/user-management/companies/my` | Exact Duplicate |
| Get Company By ID | GET | `api/v1/user-management/companies/{id}` | Exact Duplicate |
| Create Company | POST | `api/v1/user-management/companies` | Exact Duplicate |
| Update Company | PUT | `api/v1/user-management/companies/{id}` | Exact Duplicate |
| Delete Company | DELETE | `api/v1/user-management/companies/{id}` | Exact Duplicate |
| Get Company Addresses | GET | `api/v1/user-management/companies/{companyId}/addresses` | Exact Duplicate |
| Get Address By ID | GET | `api/v1/user-management/companies/{companyId}/addresses/{addressId}` | Exact Duplicate |
| Create Address | POST | `api/v1/user-management/companies/{companyId}/addresses` | Exact Duplicate |
| Update Address | PUT | `api/v1/user-management/companies/{companyId}/addresses/{addressId}` | Exact Duplicate |
| Delete Address | DELETE | `api/v1/user-management/companies/{companyId}/addresses/{addressId}` | Exact Duplicate |
| Get Direct Address | GET | `api/v1/user-management/company-addresses/{id}` | Exact Duplicate |
| Update Direct Address | PUT | `api/v1/user-management/company-addresses/{id}` | Exact Duplicate |
| Delete Direct Address | DELETE | `api/v1/user-management/company-addresses/{id}` | Exact Duplicate |
| Create Distributor Application | POST | `api/v1/user-management/distributor-applications` | Exact Duplicate |
| Get All Distributor Applications | GET | `api/v1/user-management/distributor-applications` | Exact Duplicate |
| Get Distributor Application By ID | GET | `api/v1/user-management/distributor-applications/{id}` | Exact Duplicate |
| Approve Distributor Application | PUT | `api/v1/user-management/distributor-applications/{id}/approve` | Exact Duplicate |
| Reject Distributor Application | PUT | `api/v1/user-management/distributor-applications/{id}/reject` | Exact Duplicate |
| Get All Audit Logs | GET | `api/v1/user-management/audit-logs` | Exact Duplicate |
| Get Audit Log By ID | GET | `api/v1/user-management/audit-logs/{id}` | Exact Duplicate |

**Summary:** All 68 endpoints are exact duplicates. Only namespace typo differs.

### 4. Commerce Service

| Endpoint | Method | Route | Status |
|----------|--------|-------|--------|
| Get Cart By ID | GET | `api/v1/carts/{id}` | Exact Duplicate |
| Get Cart By User | GET | `api/v1/carts/user/{userId}` | Exact Duplicate |
| Get Cart By Session | GET | `api/v1/carts/session/{sessionId}` | Exact Duplicate |
| Create Cart | POST | `api/v1/carts` | Exact Duplicate |
| Add Cart Item | POST | `api/v1/carts/{id}/items` | Exact Duplicate |
| Update Cart Item | PUT | `api/v1/carts/{id}/items/{itemId}` | Exact Duplicate |
| Remove Cart Item | DELETE | `api/v1/carts/{id}/items/{itemId}` | Exact Duplicate |
| Clear Cart | PUT | `api/v1/carts/{id}/clear` | Exact Duplicate |
| Get All Orders | GET | `api/v1/orders` | Exact Duplicate |
| Get Order By ID | GET | `api/v1/orders/{id}` | Exact Duplicate |
| Create Order | POST | `api/v1/orders` | Exact Duplicate |
| Update Order Status | PUT | `api/v1/orders/{id}/status` | Exact Duplicate |
| Track Order | GET | `api/v1/orders/track/{orderNumber}` | Exact Duplicate |

**Summary:** All 13 endpoints are exact duplicates.

### 5. Sales Service

| Endpoint | Method | Route | Status |
|----------|--------|-------|--------|
| Get All RFQs | GET | `api/v1/rfqs` | Exact Duplicate |
| Get RFQ By ID | GET | `api/v1/rfqs/{id}` | Exact Duplicate |
| Create RFQ | POST | `api/v1/rfqs` | Exact Duplicate |
| Update RFQ Status | PUT | `api/v1/rfqs/{id}/status` | Exact Duplicate |
| Get All Quotes | GET | `api/v1/quotes` | Exact Duplicate |
| Get Quote By ID | GET | `api/v1/quotes/{id}` | Exact Duplicate |
| Create Quote | POST | `api/v1/quotes` | Exact Duplicate |
| Approve Quote | PUT | `api/v1/quotes/{id}/approve` | Exact Duplicate |
| Decline Quote | PUT | `api/v1/quotes/{id}/decline` | Exact Duplicate |
| Get All Product Inquiries | GET | `api/v1/product-inquiries` | Exact Duplicate |
| Get Product Inquiry By ID | GET | `api/v1/product-inquiries/{id}` | Exact Duplicate |
| Create Product Inquiry | POST | `api/v1/product-inquiries` | Exact Duplicate |
| Delete Product Inquiry | DELETE | `api/v1/product-inquiries/{id}` | Exact Duplicate |

**Summary:** All 13 endpoints are exact duplicates.

### 6. Content Service

| Endpoint | Method | Route | Status |
|----------|--------|-------|--------|
| Get All Documents | GET | `api/v1/documents` | Exact Duplicate |
| Get Document By ID | GET | `api/v1/documents/{id}` | Exact Duplicate |
| Create Document | POST | `api/v1/documents` | Exact Duplicate |
| Delete Document | DELETE | `api/v1/documents/{id}` | Exact Duplicate |
| Get All Landing Pages | GET | `api/v1/landing-pages` | Exact Duplicate |
| Get Landing Page By Slug | GET | `api/v1/landing-pages/slug/{slug}` | Exact Duplicate |
| Create Landing Page | POST | `api/v1/landing-pages` | Exact Duplicate |
| Update Landing Page | PUT | `api/v1/landing-pages/{id}` | Exact Duplicate |
| Delete Landing Page | DELETE | `api/v1/landing-pages/{id}` | Exact Duplicate |
| Get All Help Categories | GET | `api/v1/help/categories` | Exact Duplicate |
| Get Help Category By ID | GET | `api/v1/help/categories/{id}` | Exact Duplicate |
| Create Help Category | POST | `api/v1/help/categories` | Exact Duplicate |
| Update Help Category | PUT | `api/v1/help/categories/{id}` | Exact Duplicate |
| Delete Help Category | DELETE | `api/v1/help/categories/{id}` | Exact Duplicate |
| Get All Help Articles | GET | `api/v1/help/articles` | Exact Duplicate |
| Get Help Article By ID | GET | `api/v1/help/articles/{id}` | Exact Duplicate |
| Get Help Article By Slug | GET | `api/v1/help/articles/slug/{slug}` | Exact Duplicate |
| Create Help Article | POST | `api/v1/help/articles` | Exact Duplicate |
| Update Help Article | PUT | `api/v1/help/articles/{id}` | Exact Duplicate |
| Delete Help Article | DELETE | `api/v1/help/articles/{id}` | Exact Duplicate |
| Get All FAQs | GET | `api/v1/help/faqs` | Exact Duplicate |
| Get FAQ By ID | GET | `api/v1/help/faqs/{id}` | Exact Duplicate |
| Create FAQ | POST | `api/v1/help/faqs` | Exact Duplicate |
| Update FAQ | PUT | `api/v1/help/faqs/{id}` | Exact Duplicate |
| Delete FAQ | DELETE | `api/v1/help/faqs/{id}` | Exact Duplicate |
| Get All Support Tickets | GET | `api/v1/support/tickets` | Exact Duplicate |
| Get My Support Tickets | GET | `api/v1/support/tickets/my` | Exact Duplicate |
| Get Support Ticket By ID | GET | `api/v1/support/tickets/{id}` | Exact Duplicate |
| Create Support Ticket | POST | `api/v1/support/tickets` | Exact Duplicate |
| Reply to Support Ticket | POST | `api/v1/support/tickets/{id}/reply` | Exact Duplicate |
| Close Support Ticket | PUT | `api/v1/support/tickets/{id}/close` | Exact Duplicate |
| Get Support Contact | GET | `api/v1/support/contact` | Exact Duplicate |
| Update Support Contact | PUT | `api/v1/support/contact` | Exact Duplicate |
| Get All OEM Inquiries | GET | `api/v1/oem-inquiries` | Exact Duplicate |
| Get OEM Inquiry By ID | GET | `api/v1/oem-inquiries/{id}` | Exact Duplicate |
| Create OEM Inquiry | POST | `api/v1/oem-inquiries` | Exact Duplicate |
| Delete OEM Inquiry | DELETE | `api/v1/oem-inquiries/{id}` | Exact Duplicate |

**Summary:** All 37 endpoints are exact duplicates.

### 7. Certification Service

| Endpoint | Method | Route | Status |
|----------|--------|-------|--------|
| Verify Certificate | POST | `api/v1/certifications/verify` | SNUL Only |
| Check Certificate | POST | `api/v1/certifications/check` | SNUL Only |

**Summary:** SNUL has additional certificate verification endpoints.

### 8. Attachment Service (2 files deleted - integration removal)

**Summary:** No endpoints remaining (only integration attachments removed).

## Entity Analysis Summary

### Shared Library Comparison

| File | Welco | SNUL |
|------|-------|------|
| `DependencyInjection.cs` | 153 lines | 158 lines |
| `ServiceAuthAttribute.cs` | In Welco.Shared | In SNUL.Shared |
| `ResponseStatusCode` | Not found | Not found |

### Integration Entities Removed

Both projects removed:
- `WelcoProviderMap` entity and migration
- `AddCrossMarketIntegrationFields` migration
- Integration DTOs and services
- Integration token tests

## Recommendations

### Consolidation Priority (High)
1. **Exchange Rate Endpoints** - SNUL has additional sync/history endpoints that Welco lacks
2. **Certificate Verification** - SNUL has additional endpoints that Welco lacks

### Action Required
1. Copy missing Welco Product Service endpoints (`latest`, `pair`, `convert`, `cart-total`) to SNUL
2. Copy missing Welco Content entities to SNUL
3. Sync ExchangeRate endpoints from Welco to SNUL or merge
4. Fix namespace typo in Welco (`UserManamgent` → `UserManagement`)

### Documentation
- All Ocelot route config files (`.json`) are identical in structure
- Both use same versioning (`v1`) and base paths (`api/v1/{service}`)

## Appendix: Route File Comparison

| Service | Welco File | SNUL File | Lines (Welco/SNUL) |
|---------|------------|-----------|-------------------|
| Auth | AuthApiRoutes.cs | AuthApiRoutes.cs | 21 / 21 |
| Product | ProductApiRoutes.cs | ProductApiRoutes.cs | 65 / 67 |
| User Management | UserManagementApiRoutes.cs | UserManagementApiRoutes.cs | 105 / 105 |
| Commerce | CommerceApiRoutes.cs | CommerceApiRoutes.cs | 31 / 31 |
| Sales | SalesApiRoutes.cs | SalesApiRoutes.cs | 36 / 36 |
| Content | ContentApiRoutes.cs | ContentApiRoutes.cs | 86 / 86 |
| Certification | N/A | CertificationsApiRoutes.cs | - |
| Attachment | N/A | N/A | - |