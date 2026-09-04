# Welco Platform — API Integration Guide

> Complete endpoint reference for frontend integration. Covers every microservice exposed through the **Welco API Gateway (Ocelot)**, including route, HTTP method, authentication, request body/parameters, response shape, and real JSON examples.

---

## 1. Business Overview & Purpose

**Welco** is a surgical-instrument manufacturer (est. 1994, ISO 13485 certified, CE marked, exporting to 40+ countries). The platform is a **hybrid B2B + B2C commerce system**: everyday buyers add-to-cart and check out directly, while hospitals and distributors request quotes (RFQ), get approvals, and manage bulk trade through a dashboard.

The codebase is a **.NET 10 Clean/Onion architecture** split into **independent microservices**, each owning a bounded context, all fronted by a single **Ocelot API Gateway**:

| Microservice | Bounded Context | Owns |
|---|---|---|
| `Auth.Services.API` | Identity & Access | Register, login, OTP verification, password reset, refresh tokens, profile |
| `UserManamgent.Service.API` | Organizations & Reference Data | Users, companies, addresses, countries, cities, zones |
| `Product.Services.API` | Catalog | Categories, products, currencies |
| `Commerce.Services.API` | Direct Commerce | Carts, orders |
| `Sales.Services.API` | B2B Sales Pipeline | RFQs, quotes |
| `Content.Services.API` | Content, Marketing & Support | Documents, landing pages |
| `Certification.Services.API` | Compliance & Quality | Certifications |
| `Attachment.Services.API` | File Storage | File upload/download (images, videos, audio, documents) |
| `Welco.API` | Gateway | Routing, JWT validation, rate limiting, unified OpenAPI docs |

**Business flows the API supports:**
- **Public storefront** — browse products/categories, view certifications, read landing pages, download documents (all public, no auth).
- **Direct purchase** — guest cart via `sessionId` → create order.
- **B2B path** — company account creates **RFQ** → sales staff converts to a priced **Quote** → buyer **approves/declines** → approved quote can become an **Order**.
- **Admin/Staff ops** — manage users, companies, catalog, currencies, statuses, content, certifications.

---

## 2. Gateway & Base URLs

The frontend **never calls microservices directly** — it calls the gateway, which forwards 1:1 identical paths (`UpstreamPathTemplate == DownstreamPathTemplate`).

| Environment | Gateway Base URL |
|---|---|
| Development (HTTP) | `http://localhost:5293` |
| Development (HTTPS) | `https://localhost:7166` |
| Test (hosted) | `https://welco-gateway.runasp.net` |

- **OpenAPI UI (unified):** `GET /` redirects to `/scalar/v1` (aggregates all services) — raw spec at `/openapi/all.json`.
- **Per-service docs:** `/api/docs/{service}` and `/api/docs/{service}/openapi.json` where `service` ∈ `auth, usermanagement, product, commerce, sales, content, certification, attachment`.

> **HTTPS:** All development Ocelot routes point downstream over HTTPS on each service's https port (auth `7203`, user-management `7204`, certification `7101`, attachment `7180`, product `7054`, commerce `7045`, sales `7046`, content `7047`). Run services with the **https** launch profile.

---

## 3. Roles / User Types

`ApplicationUser.UserType` (mirrored 1:1 by an Identity role) drives authorization.

| UserType | JSON value | Description |
|---|---|---|
| `Admin` | `1` | Global access; **bypasses every role restriction** |
| `OrganizationUser` | `2` | B2B company user (buyers, RFQ, cart/orders) |
| `WelcoStaff` | `3` | Welco employee (internal operations) |
| `Guest` | — (no account) | Anonymous; public GETs only |

**Permission matrix (summary):**

| Capability | Admin | WelcoStaff | OrganizationUser | Guest |
|---|:--:|:--:|:--:|:--:|
| Public catalog / content / certifications GETs | ✅ | ✅ | ✅ | ✅ |
| Register / login / password flows | — | — | — | ✅ |
| Profile (GET/PUT `/auth/profile`) | ✅ | ✅ | ✅ | ❌ |
| Users & companies CRUD | ✅ | ❌ | read `{id}` only | ❌ |
| Countries/cities/zones (GET) | ✅ | ✅ | ✅ | ✅ (public) |
| Countries/cities/zones (write) | ✅ | ❌ | ❌ | ❌ |
| Categories / Products (write) | ✅ | ✅ | ❌ | ❌ |
| Currencies (write) | ✅ | ❌ | ❌ | ❌ |
| Carts / Orders (own) | ✅ | read lists | ✅ | via `sessionId` |
| RFQ create / quote approve-decline | ✅ | create quotes/status | ✅ | ❌ |
| Attachments upload | ✅ | ✅ | ✅ | ❌ |
| Certifications / Documents / Landing pages (write) | ✅ | ✅ | ❌ | ❌ |

> `Admin` always passes even if not listed in the attribute's role array.

---

## 4. Global API Conventions

### 4.1 Authentication
- Send `Authorization: Bearer <accessToken>` (JWT from `login` / `verify-register-otp` / `refresh-token`).
- Unauthenticated → **401**; authenticated but wrong role → **403** (both use the standard envelope below).
- Keep the `refreshToken` for silent re-auth via `POST /auth/refresh-token`.

### 4.2 Response envelope — single item (`Result<T>`)
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "Login successful",
  "errors": [],
  "data": { }
}
```

### 4.3 Response envelope — paginated (`PaginatedResult<T>`)
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "...",
  "errors": [],
  "data": [ ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1,
  "totalCount": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### 4.4 Error envelope (400 / 401 / 403 / 404 / 409)
```json
{
  "isSuccess": false,
  "statusCode": 404,
  "message": "RFQ not found",
  "errors": ["RFQ not found"],
  "data": null
}
```
- `errors` is a **flat `string[]`** (validation messages are flattened; not `{field: [...]}`).
- HTTP status code always equals the `statusCode` field.

### 4.5 Localization
`message` and `errors[]` are localized based on `Accept-Language` / `Language` / `X-Language` header, or query `?culture=` / `?lang=` / `?language=`. Supported: `en`, `ar`. **Do not key UI logic off message text** — switch on `isSuccess` / `statusCode` / `errors`.

### 4.6 JSON conventions
- **camelCase** property names for all requests/responses.
- Request binding is case-insensitive (PascalCase also binds), output is always camelCase.
- `Guid` → lowercase UUID string (`"3fa85f64-5717-4562-b3fc-2c963f66afa6"`).
- `DateTime` → ISO-8601 (`"2026-08-31T10:00:00Z"`).
- `decimal` → JSON number.

### 4.7 Enums in JSON — two different behaviors (IMPORTANT)
- **Enum request fields serialize as integers** (System.Text.Json default, no string converter). Sending a string like `"Admin"` in an enum field **fails to bind**.
  - `UserType`: `1=Admin, 2=OrganizationUser, 3=WelcoStaff`
  - `AppLanguage`: `1=En, 2=Ar`
  - `CompanyType`: `1=Hospital, 2=Distributor, 3=Clinic, 4=Importer`
  - `CompanyStatus`: `1=Pending, 2=Approved, 3=Rejected`
  - `MediaType` (attachments): `0=Image, 1=Video, 2=Audio, 3=File`
- **Some DTO fields expose enum values as strings** (explicitly mapped): `OrderDto.status` (`Pending/Confirmed/Shipped/Delivered/Cancelled`), `RFQDto.status` / `QuoteDto.status`, `DocumentDto.docType`, `LandingPageDto.type`. These request/response fields accept **strings**.

### 4.8 Pagination query parameters
List endpoints accept `pageNumber` (default `1`, min `1`) and `pageSize` (default `10`, range `1–50`). Returns the paginated envelope (§4.3).

### 4.9 Content-Type
`application/json` for bodies; **`multipart/form-data`** for attachments (§8).

---

## 5. Auth — `api/v1/auth`

Base: `api/v1/auth`. Except `GET/PUT /profile`, all endpoints are **anonymous** (no token).

### 5.1 POST `api/v1/auth/register` — create account
Request:
```json
{
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "password": "password123",
  "confirmPassword": "password123",
  "phoneNumber": "+971501234567",
  "userType": 2,
  "language": 1
}
```
| Field | Type | Required | Rules |
|---|---|---|---|
| `fullName` | string | ✅ | |
| `email` | string | ✅ | valid; must not already exist |
| `password` | string | ✅ | min 6 |
| `confirmPassword` | string | ✅ | must equal `password` |
| `phoneNumber` | string? | ❌ | |
| `userType` | int | ❌ | default `2`; `1|2|3` |
| `language` | int | ❌ | default `1` (`1` En, `2` Ar) |

Response **200** (`data` = email string; user created **inactive** until OTP verified):
```json
{
  "isSuccess": true, "statusCode": 200,
  "message": "Registration successful. Please verify your email.",
  "errors": [], "data": "john.doe@example.com"
}
```

### 5.2 POST `api/v1/auth/login`
Request:
```json
{ "email": "john.doe@example.com", "password": "password123" }
```
Response **200**, `data` = `AuthResponseDto`:
```json
{
  "isSuccess": true, "statusCode": 200, "message": "Login successful", "errors": [],
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "John Doe",
    "email": "john.doe@example.com",
    "userName": "john.doe@example.com",
    "userType": 2,
    "companyId": null,
    "language": 1,
    "roles": ["OrganizationUser"],
    "accessToken": "<JWT>",
    "refreshToken": "<opaque string>",
    "refreshTokenExpiryTime": "2026-09-30T12:00:00Z"
  }
}
```
Failures → **401**: `InvalidCredentials` (bad credentials), `AccountDeactivated`, `EmailNotConfirmed` (must verify OTP first).

### 5.3 POST `api/v1/auth/verify-register-otp` — verify email (activation)
Request:
```json
{ "email": "john.doe@example.com", "otpCode": "483920" }
```
Response **200** with the **same `AuthResponseDto`** as login (tokens issued, account activated). Wrong/expired OTP → **400**; user missing → **404**.

### 5.4 POST `api/v1/auth/forgot-password`
Request:
```json
{ "email": "john.doe@example.com" }
```
Response **200**, `data` = localized message string. User not found → **404**.

### 5.5 POST `api/v1/auth/verify-password-otp`
Request:
```json
{ "email": "john.doe@example.com", "otpCode": "736291" }
```
Response **200** — **`data` IS the OTP string**; the frontend must pass it as `token` to reset-password:
```json
{ "isSuccess": true, "statusCode": 200, "message": "OTP verified", "errors": [], "data": "736291" }
```

### 5.6 POST `api/v1/auth/reset-password`
Request:
```json
{
  "email": "john.doe@example.com",
  "token": "736291",
  "newPassword": "newpassword123",
  "confirmNewPassword": "newpassword123"
}
```
`token` = OTP from 5.5. Response **200**, `data` = success message.

### 5.7 POST `api/v1/auth/refresh-token`
Request:
```json
{ "refreshToken": "<refresh token from login>" }
```
Revokes the old token, issues a new pair. Response **200** with `AuthResponseDto` (same as 5.2). Failures → **400** (`InvalidRefreshToken`, `RefreshTokenExpired`, `UserNotFound`).

### 5.8 POST `api/v1/auth/logout` — revoke refresh token
Body optional:
```json
{ "refreshToken": "<refresh token to revoke>" }
```
Response **200**, `data` = `""`.

### 5.9 GET `api/v1/auth/profile` — current user (🔒 any authenticated)
No body/params. Response **200**, `data` = `UserProfileDto`:
```json
{
  "isSuccess": true, "statusCode": 200, "message": "Profile fetched successfully", "errors": [],
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "John Doe",
    "email": "john.doe@example.com",
    "phoneNumber": "+971501234567",
    "profilePictureName": null,
    "userType": 2,
    "companyId": null,
    "language": 1,
    "isEmailConfirmed": true,
    "createdAt": "2026-08-01T09:15:30Z",
    "roles": ["OrganizationUser"],
    "addresses": [
      {
        "id": "6d1d2a3e-4b5c-4d6e-8f90-123456789abc",
        "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "countryId": "9f8e7d6c-5b4a-3c2d-1e0f-abcdef123456",
        "countryNameEn": "United Arab Emirates",
        "countryNameAr": "الإمارات العربية المتحدة",
        "cityId": "11111111-2222-3333-4444-555555555555",
        "cityNameEn": "Dubai",
        "cityNameAr": "دبي",
        "zoneId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
        "zoneNameEn": "Business Bay",
        "zoneNameAr": "بزنس باي",
        "street": "Sheikh Zayed Road",
        "building": "Burj Vista",
        "floor": "15",
        "apartment": "1502",
        "createdAt": "2026-08-05T10:00:00Z",
        "updatedAt": null
      }
    ]
  }
}
```

### 5.10 PUT `api/v1/auth/profile` — update current user (🔒 any authenticated)
All top-level fields optional. Returns the refreshed `UserProfileDto` (§5.9).
```json
{
  "fullName": "John Doe Jr.",
  "phoneNumber": "+971502345678",
  "profilePictureName": "1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.png",
  "language": 1,
  "addresses": [
    {
      "id": "6d1d2a3e-4b5c-4d6e-8f90-123456789abc",
      "countryId": "9f8e7d6c-5b4a-3c2d-1e0f-abcdef123456",
      "cityId": "11111111-2222-3333-4444-555555555555",
      "zoneId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
      "street": "Sheikh Zayed Road",
      "building": "Burj Vista",
      "floor": "15",
      "apartment": "1502"
    }
  ]
}
```
**`addresses` semantics (REPLACE-SET):** the array is the full desired list. Item without `id` → **create**; with existing `id` → **update**; existing addresses absent from the list → **deleted**. `addresses: []` deletes all; omitting `addresses` leaves unchanged. Each address item requires `countryId`, `cityId`, `zoneId`, `street` (geo references must form a valid country → city → zone chain, else 404).

---

## 6. User Management — `api/v1/user-management`

Base: `api/v1/user-management`. Auth rules per group (countries/cities/zones GETs are public).

### 6.1 Users

#### GET `/users` — 🔒 Admin · paginated
Query: `pageNumber`, `pageSize`, `searchTerm` (matches fullName/email/phone), `userType` (int), `isActive` (bool).
Response `PaginatedResult<UserDto>`:
```json
{
  "isSuccess": true, "statusCode": 200, "message": "users.fetched", "errors": [],
  "data": [{
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "Ahmed Ali",
    "email": "ahmed@company.com",
    "phoneNumber": "+201000000000",
    "profilePictureName": null,
    "userType": 2,
    "companyId": "6fa85f64-5717-4562-b3fc-2c963f66afa6",
    "language": 1,
    "isActive": true,
    "isEmailConfirmed": true,
    "createdAt": "2026-01-01T10:00:00Z",
    "updatedAt": null,
    "roles": ["OrganizationUser"]
  }],
  "pageNumber": 1, "pageSize": 10, "totalPages": 1, "totalCount": 1,
  "hasPreviousPage": false, "hasNextPage": false
}
```

#### GET `/users/{id}` — 🔒 any authenticated
Response `Result<UserDetailsDto>` = `UserDto` + `addresses: UserAddressDto[]`. **404** if missing.

#### POST `/users` — 🔒 Admin → **201**
```json
{
  "fullName": "Ahmed Ali",
  "email": "ahmed@company.com",
  "password": "secret123",
  "phoneNumber": "+201000000000",
  "userType": 2,
  "companyId": "6fa85f64-5717-4562-b3fc-2c963f66afa6",
  "profilePictureName": null,
  "isActive": true
}
```
`email` duplicate → **409**. Response `data` = created `UserDto`.

#### PUT `/users/{id}` — 🔒 Admin · partial update
```json
{ "fullName": "Ahmed A.", "isActive": true }
```
All fields optional (`fullName`, `phoneNumber`, `profilePictureName`, `userType`, `companyId`, `isActive`). Response `data` = updated `UserDto`.

#### DELETE `/users/{id}` — 🔒 Admin
Soft delete. Response **200** `data` = user id string. `id == current user` → **400**.

#### PUT `/users/{id}/change-password` — 🔒 any authenticated
```json
{ "newPassword": "NewSecret123" }
```
Response **200** `data` = user id string.

### 6.2 Addresses — `api/v1/user-management/addresses` · 🔒 any authenticated

| Endpoint | Body / notes | Response `data` |
|---|---|---|
| `GET /addresses/user/{userId}` | — | `UserAddressDto[]` |
| `GET /addresses/{id}` | — | `UserAddressDto` (404 if missing) |
| `POST /addresses` | `{ userId, countryId, cityId, zoneId, street (≤250), building?, floor?, apartment? }` → **201** | `UserAddressDto` |
| `PUT /addresses/{id}` | all optional: `{ countryId?, cityId?, zoneId?, street?, building?, floor?, apartment? }` | `UserAddressDto` |
| `DELETE /addresses/{id}` | — | address id string |

**`UserAddressDto`:**
```json
{
  "id": "9a85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "countryId": "7fa85f64-5717-4562-b3fc-2c963f66afa6",
  "countryNameEn": "Egypt", "countryNameAr": "مصر",
  "cityId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
  "cityNameEn": "Cairo", "cityNameAr": "القاهرة",
  "zoneId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
  "zoneNameEn": "Nasr City", "zoneNameAr": "مدينة نصر",
  "street": "25 Tahrir St",
  "building": "Building 3", "floor": "4", "apartment": "12",
  "createdAt": "2026-01-01T10:00:00Z",
  "updatedAt": null
}
```
Business checks (404): user exists; city belongs to `countryId`; zone belongs to `cityId`.

### 6.3 Countries — `api/v1/user-management/countries` · GET public / write Admin

| Endpoint | Request | Response `data` |
|---|---|---|
| `GET /countries` | — | `CountryDto[]` (sorted by nameEn) |
| `GET /countries/{id}` | — | `CountryDto` (404) |
| `POST /countries` | `{ nameEn (≤150), nameAr (≤150), code? (≤10) }` → **201** | `CountryDto` |
| `PUT /countries/{id}` | all optional: `{ nameEn?, nameAr?, code? }` | `CountryDto` |
| `DELETE /countries/{id}` | — | id string |

**`CountryDto`:** `{ "id": "7fa85f64-...", "nameEn": "Egypt", "nameAr": "مصر", "code": "EG", "isActive": true, "createdAt": "2026-01-01T10:00:00Z" }`
Duplicate nameEn/nameAr → **409**.

### 6.4 Cities — `api/v1/user-management/cities` · GET public / write Admin

| Endpoint | Request | Response `data` |
|---|---|---|
| `GET /cities` | query `countryId` (optional filter) | `CityDto[]` |
| `GET /cities/country/{countryId}` | — | `CityDto[]` |
| `GET /cities/{id}` | — | `CityDto` (404) |
| `POST /cities` | `{ countryId, nameEn, nameAr }` → **201** | `CityDto` |
| `PUT /cities/{id}` | optional: `{ countryId?, nameEn?, nameAr? }` | `CityDto` |
| `DELETE /cities/{id}` | — | id string |

**`CityDto`:** `{ "id", "countryId", "countryNameEn", "countryNameAr", "nameEn", "nameAr", "isActive", "createdAt" }`
Duplicate within country → **409**; country missing → **404**.

### 6.5 Zones — `api/v1/user-management/zones` · GET public / write Admin

| Endpoint | Request | Response `data` |
|---|---|---|
| `GET /zones` | query `cityId` (optional filter) | `ZoneDto[]` |
| `GET /zones/city/{cityId}` | — | `ZoneDto[]` |
| `GET /zones/{id}` | — | `ZoneDto` (404) |
| `POST /zones` | `{ cityId, nameEn, nameAr }` → **201** | `ZoneDto` |
| `PUT /zones/{id}` | optional: `{ cityId?, nameEn?, nameAr? }` | `ZoneDto` |
| `DELETE /zones/{id}` | — | id string |

**`ZoneDto`:** `{ "id", "cityId", "cityNameEn", "cityNameAr", "nameEn", "nameAr", "isActive", "createdAt" }`

### 6.6 Companies — `api/v1/user-management/companies` · GET authenticated / write Admin

#### GET `/companies` — 🔒 authenticated · paginated
Query: `pageNumber`, `pageSize`, `searchTerm` (matches name), `isActive`.
Response `PaginatedResult<CompanyDto>` (`countryNameEn` populated **only in this list**):
```json
{
  "isSuccess": true, "statusCode": 200, "message": "company.listFetched", "errors": [],
  "data": [{
    "id": "6fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "MedSupply Co.",
    "type": 2,
    "countryId": "7fa85f64-5717-4562-b3fc-2c963f66afa6",
    "countryNameEn": "Egypt",
    "tierLevel": 2,
    "status": 2,
    "accountManagerId": null,
    "isActive": true,
    "createdAt": "2026-01-01T10:00:00Z",
    "updatedAt": null
  }],
  "pageNumber": 1, "pageSize": 10, "totalPages": 1, "totalCount": 1,
  "hasPreviousPage": false, "hasNextPage": false
}
```
> ⚠️ `countryNameEn` is `null` on `GET /companies/{id}`, create, and update responses — only the list populates it.

#### GET `/companies/{id}` — 🔒 authenticated
`Result<CompanyDto>` (404).

#### POST `/companies` — 🔒 Admin → **201**
```json
{
  "name": "MedSupply Co.",
  "type": 2,
  "countryId": "7fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tierLevel": 2,
  "status": 1,
  "accountManagerId": null
}
```
`type` ∈ 1–4, `tierLevel` ∈ 1–5, `status` defaults `1`. Country missing → **400**.

#### PUT `/companies/{id}` — 🔒 Admin · **full update** (core fields required)
```json
{
  "name": "MedSupply Co.",
  "type": 2,
  "countryId": "7fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tierLevel": 3,
  "status": 2,
  "accountManagerId": null,
  "isActive": true
}
```

#### DELETE `/companies/{id}` — 🔒 Admin
`data` = id string.

---

## 7. Certification — `api/v1/certifications`

GETs public; writes require `Admin` or `WelcoStaff`.

| Endpoint | Request | Response `data` |
|---|---|---|
| `GET /certifications` | query `pageNumber`, `pageSize`, `searchTerm` (matches certificateNumber/title/issuedTo/issuer), `isActive` | `PaginatedResult<CertificationDto>` |
| `GET /certifications/{id}` | — | `CertificationDto` (404) |
| `GET /certifications/{id}/show` | identical to `{id}` | `CertificationDto` |
| `POST /certifications` | see below → **201** | `CertificationDto` |
| `PUT /certifications/{id}` | see below (`isActive` also accepted) | `CertificationDto` |
| `DELETE /certifications/{id}` | — | id string |

**Create body:**
```json
{
  "certificateNumber": "CERT-2026-0001",
  "title": "ISO 9001 Quality Management",
  "issuedTo": "Acme Manufacturing Co.",
  "issuer": "International Standards Organization",
  "issueDate": "2026-06-15T00:00:00Z",
  "expiryDate": "2029-06-14T00:00:00Z",
  "description": "Valid until further notice",
  "certificationImageName": "1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.jpg"
}
```
Required: `certificateNumber` (unique → **409** if taken), `title`, `issuedTo`, `issuer`, `issueDate` (≤ today). Optional: `expiryDate` (> issueDate), `description`, `certificationImageName`.

**`CertificationDto`:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "certificateNumber": "CERT-2026-0001",
  "title": "ISO 9001 Quality Management",
  "issuedTo": "Acme Manufacturing Co.",
  "issuer": "International Standards Organization",
  "issueDate": "2026-06-15T00:00:00Z",
  "expiryDate": "2029-06-14T00:00:00Z",
  "description": "Valid until further notice",
  "certificationImageName": "1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.jpg",
  "ownerUserId": "9b2b9d99-3b9b-4b9b-9b9b-9b9b9b9b9b9b",
  "isActive": true,
  "createdAt": "2026-08-30T14:22:10.123Z",
  "updatedAt": null
}
```

---

## 8. Attachment — `api/v1/attachments`

All **write** endpoints use `multipart/form-data` and require `OrganizationUser`, `WelcoStaff`, or `Admin`. Download is public.

**`place` (upload folder) values:** `0` default, `1` providers, `2` users, `3–12` generic uploads.

**Stored filename convention:** `<place>_<guid>.<ext>` (e.g. `1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.png`). **Always store this returned filename** in the entity it belongs to (e.g. `certificationImageName`, `imageName`, `profilePictureName`).

**`fileType` (MediaType) values:** `0` Image (≤5 MB, jpg/jpeg/png/gif/bmp/webp), `1` Video (≤100 MB, mp4/avi/mkv/mov/wmv), `2` Audio (≤10 MB, mp3/wav/ogg/m4a/aac), `3` File (≤10 MB, pdf/doc/docx/xls/xlsx/txt/zip/rar).

### 8.1 POST `/attachments/upload` — single file → **201**
Multipart fields:
| Form field | Type | Required |
|---|---|---|
| `file` | file | ✅ |
| `place` | int | ✅ (0–2 for upload) |
| `fileType` | int | ✅ (`0`–`3`) |

Response **201**, `data` = stored filename string:
```json
{ "isSuccess": true, "statusCode": 201, "message": "AttachmentMessages.FileUploaded", "errors": [],
  "data": "1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.png" }
```

### 8.2 POST `/attachments/upload-multiple` — multiple files → **201**
Multipart fields (repeat same field name for multiple files):
| Form field | Type |
|---|---|
| `images` / `imagesPlace` | `List<IFormFile>` / int |
| `videos` / `videosPlace` | `List<IFormFile>` / int |
| `audios` / `audiosPlace` | `List<IFormFile>` / int |
| `documents` / `documentsPlace` | `List<IFormFile>` / int |

At least one file list must be non-empty (else **400**). Response `data` = array of stored filenames:
```json
{ "isSuccess": true, "statusCode": 201, "message": "AttachmentMessages.FileUploaded", "errors": [],
  "data": ["1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.jpg", "3_d5e6f7g8a1b2c3d4e5f6a7b8c9d0e1f2.pdf"] }
```

### 8.3 PUT `/attachments/{name}` — replace file → **200**
`{name}` = existing stored filename. Multipart fields: `file`, `place` (0–12), `fileType`. Response `data` = **new** stored filename.

### 8.4 GET `/attachments/download` — file metadata (public)
Query: `place` (0–12, required), `fileName` (required).
Response **200** with `FileResponseDto` (`success:false` + `errorMessage` if the file is missing):
```json
{
  "isSuccess": true, "statusCode": 200, "message": "AttachmentMessages.FileDownloaded", "errors": [],
  "data": {
    "filePath": "Providers/1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.png",
    "fileName": "1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.png",
    "contentType": "image/png",
    "success": true,
    "errorMessage": null
  }
}
```

> **Rendering images/files:** prefer the static route **`GET /files/{relativePath}`** (e.g. `/files/Providers/1_….png`) — it streams the actual bytes.

---

## 9. Product (Catalog) — `api/v1/categories`, `api/v1/products`, `api/v1/currencies`

All GETs are **public**; writes require JWT (`Admin`+`WelcoStaff` for categories/products, **`Admin` only** for currencies).

### 9.1 Categories — `api/v1/categories`

| Endpoint | Request | Response `data` |
|---|---|---|
| `GET /categories` | query `pageNumber`, `pageSize`, `searchTerm` (nameEn/nameAr), `isActive` | `PaginatedResult<CategoryDto>` |
| `GET /categories/{id}` | — | `CategoryDto` (404) |
| `GET /categories/{id}/show` | identical to `{id}` | `CategoryDto` |
| `GET /categories/{categoryId}/products` | — | `ProductDto[]` (404 if category missing) |
| `POST /categories` | see below → **201** | `CategoryDto` |
| `PUT /categories/{id}` | same fields + `isActive` | `CategoryDto` |
| `DELETE /categories/{id}` | — | id string |

**Create/Update body:** `{ nameEn (≤200), nameAr (≤200), description? (≤1000), imageName? (≤500), parentCategoryId? }`. `parentCategoryId` must exist (else 400) and on update cannot equal `id`.

**`CategoryDto`:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nameEn": "Bedding", "nameAr": "المفروشات",
  "description": null, "imageName": null, "parentCategoryId": null,
  "isActive": true, "createdAt": "2026-08-31T10:00:00Z", "updatedAt": null
}
```

### 9.2 Products — `api/v1/products`

| Endpoint | Request | Response `data` |
|---|---|---|
| `GET /products` | query: `pageNumber`, `pageSize`, `searchTerm`, `sku`, `material`, `lengthMin`, `lengthMax`, `categoryId`, `isActive` | `PaginatedResult<ProductDto>` |
| `GET /products/{id}` | — | `ProductDto` (404) |
| `GET /products/{id}/show` | identical to `{id}` | `ProductDto` |
| `POST /products` | see below → **201** | `ProductDto` |
| `PUT /products/{id}` | same fields + `isActive` | `ProductDto` |
| `DELETE /products/{id}` | — | id string |

**Create/Update body:**
```json
{
  "nameEn": "Egyptian Cotton Sheet", "nameAr": "ملاءة قطن مصري",
  "sku": "SHEET-001", "slug": "egyptian-cotton-sheet",
  "description": "400 thread count", "price": 125.50, "stock": 40,
  "specifications": null, "imageName": "sheet.jpg", "material": "Cotton",
  "lengthCm": 200.00,
  "currencyId": "a6b7b810-9dad-11d1-80b4-00c04fd430c9",
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```
Required: `nameEn`, `nameAr`, `sku` (unique → **409**), `slug` (unique → **409**; server lowercases/trims), `price` (> 0), `categoryId` (must exist). Optional: `stock` (≥0, default 0), `description`, `specifications`, `imageName`, `material`, `lengthCm`, `currencyId` (must exist).

**`ProductDto`:**
```json
{
  "id": "6ba7b810-9dad-11d1-80b4-00c04fd430c8",
  "nameEn": "Egyptian Cotton Sheet", "nameAr": "ملاءة قطن مصري",
  "sku": "SHEET-001", "slug": "egyptian-cotton-sheet",
  "description": "400 thread count", "price": 125.50, "stock": 40,
  "specifications": null, "imageName": "sheet.jpg", "material": "Cotton",
  "lengthCm": 200.00,
  "currencyId": "a6b7b810-9dad-11d1-80b4-00c04fd430c9",
  "currencyCode": "USD", "currencySymbol": "$",
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "categoryNameEn": "Bedding", "categoryNameAr": "المفروشات",
  "isActive": true, "createdAt": "2026-08-31T10:00:00Z", "updatedAt": null
}
```

### 9.3 Currencies — `api/v1/currencies` (writes: Admin only)

| Endpoint | Request | Response `data` |
|---|---|---|
| `GET /currencies` | query `pageNumber` (default 1), `pageSize` (default **50**), `searchTerm`, `isActive` | `PaginatedResult<CurrencyDto>` |
| `GET /currencies/{id}` | — | `CurrencyDto` (404) |
| `POST /currencies` | `{ nameEn, nameAr, code, symbol }` → **201** | `CurrencyDto` |
| `PUT /currencies/{id}` | same fields + `isActive` | `CurrencyDto` |
| `DELETE /currencies/{id}` | — | id string |

`code` is uppercased server-side and unique (→ **409**).
**`CurrencyDto`:** `{ "id", "nameEn", "nameAr", "code": "USD", "symbol": "$", "isActive": true, "createdAt", "updatedAt" }`

---

## 10. Commerce — `api/v1/carts`, `api/v1/orders`

All endpoints require auth; role restrictions per row. **Admin bypasses everything.**

### 10.1 Carts

| Endpoint | Auth | Request | Response `data` |
|---|---|---|---|
| `GET /carts` | Admin | query `pageNumber`, `pageSize` | `PaginatedResult<CartDto>` (items `[]`) |
| `GET /carts/{id}` | OrgUser, Admin | — | `CartDto` (full items) |
| `GET /carts/user/{userId}` | OrgUser, Admin | — | most recent `CartDto` (full items) |
| `GET /carts/session/{sessionId}` | OrgUser, Admin | — | most recent `CartDto` (full items) |
| `POST /carts` | OrgUser, Admin | `{ userId?, sessionId?, currencyId? }` (need userId **or** sessionId) → **201** | `CartDto` |
| `POST /carts/{id}/items` | OrgUser, Admin | `{ productId, quantity (>0), unitPriceSnapshot (≥0) }` | refreshed `CartDto` |
| `PUT /carts/{id}/items/{itemId}` | OrgUser, Admin | `{ quantity (>0) }` | refreshed `CartDto` |
| `DELETE /carts/{id}/items/{itemId}` | OrgUser, Admin | — | refreshed `CartDto` |
| `POST /carts/{id}/clear` | OrgUser, Admin | — | id string |

**Add item behavior:** if the product is already in the cart, `quantity` is **added** and the price snapshot updated. Cart/product/item missing → **404**.

**`CartDto`:**
```json
{
  "id": "3f2dc1b4-1234-4abc-9def-0123456789ab",
  "userId": "a1b2c3d4-5678-4abc-9def-0123456789ab",
  "sessionId": null,
  "currencyId": null,
  "items": [
    {
      "id": "c5e8f0a1-2222-4abc-9def-0123456789ab",
      "cartId": "3f2dc1b4-1234-4abc-9def-0123456789ab",
      "productId": "9aa1bb2c-3333-4abc-9def-0123456789ab",
      "productNameEn": "Steel Pipe",
      "quantity": 5,
      "unitPriceSnapshot": 120.5
    }
  ],
  "isActive": true,
  "createdAt": "2026-08-31T10:00:00.000Z"
}
```

**Guest cart flow:** create cart with `sessionId` only (no login), then use `GET /carts/session/{sessionId}` to retrieve it. `sessionId` is client-generated (≤200 chars, e.g. UUID/cookie value).

### 10.2 Orders

| Endpoint | Auth | Request | Response `data` |
|---|---|---|---|
| `GET /orders` | Admin | query `pageNumber`, `pageSize`, `status` (string), `userId`, `companyId` | `PaginatedResult<OrderDto>` (items `[]`) |
| `GET /orders/{id}` | OrgUser, Admin | — | `OrderDto` (full items) |
| `POST /orders` | OrgUser, Admin | see below → **201** | `OrderDto` (full items) |
| `PUT /orders/{id}/status` | Admin, WelcoStaff | `{ "status": "Shipped" }` | id string |

**Create order body:**
```json
{
  "userId": "a1b2c3d4-5678-4abc-9def-0123456789ab",
  "companyId": "e5f6a7b8-...",
  "currencyId": null,
  "quoteId": null,
  "items": [
    { "productId": "9aa1bb2c-3333-4abc-9def-0123456789ab", "quantity": 5, "unitPrice": 120.5 }
  ]
}
```
`items` required & non-empty. `orderNumber` auto-generated (`ORD-yyyyMMdd-XXXXXX`), `status` starts `Pending`, `totalAmount` = Σ qty×unitPrice. Product missing → **404**.

**`OrderDto`:** `status` is a **string**:
```json
{
  "id": "77aa88bb-...",
  "orderNumber": "ORD-20260831-ABCDEF",
  "status": "Pending",
  "userId": "a1b2c3d4-...",
  "companyId": "e5f6a7b8-...",
  "currencyId": null,
  "incotermId": null,
  "totalAmount": 602.5,
  "items": [
    { "id": "b0c1d2e3-...", "orderId": "77aa88bb-...", "productId": "9aa1bb2c-...",
      "productNameEn": "Steel Pipe", "quantity": 5, "unitPrice": 120.5 }
  ],
  "isActive": true,
  "createdAt": "2026-08-31T10:00:00.000Z"
}
```
Valid `status` values: `Pending | Confirmed | Shipped | Delivered | Cancelled` (case-insensitive; invalid → 400). ⚠️ `incotermId` is never populated — always `null`. `quoteId` is accepted but not echoed back.

---

## 11. Sales — `api/v1/rfqs`, `api/v1/quotes`

All endpoints require authentication. Statuses are exposed as **strings**.

**`RFQStatus`:** `Pending` → `Quoted` → `Ordered`; `Cancelled`.
**`QuoteStatus`:** `Draft | Sent | Approved | Declined | Expired`.

### 11.1 RFQs

| Endpoint | Auth | Request | Response `data` |
|---|---|---|---|
| `GET /rfqs` | any authenticated | query `pageNumber`, `pageSize` | `PaginatedResult<RFQDto>` (**no items** in list) |
| `GET /rfqs/{id}` | any authenticated | — | `RFQDto` (full items) |
| `POST /rfqs` | OrgUser, Admin | see below → **201** | `RFQDto` (full items) |
| `PUT /rfqs/{id}/status` | WelcoStaff, Admin | `{ "status": "Quoted" }` | id string |

**Create RFQ body:**
```json
{
  "companyId": "aa11bb22-3333-4abc-9def-0123456789ab",
  "items": [
    { "productId": "12345678-aaaa-4abc-9def-0123456789ab", "quantity": 5, "notes": "expedite" },
    { "productId": "87654321-bbbb-4abc-9def-0123456789ab", "quantity": 10, "notes": null }
  ]
}
```
`companyId` required (404 if missing); `items` required & non-empty; each item `productId` required, `quantity` > 0. `rfqNumber` auto-generated (`RFQ-yyyyMMdd-XXXXXX`), status starts `Pending`.

**`RFQDto`:**
```json
{
  "id": "3f2bc1d4-...",
  "rfqNumber": "RFQ-20260831-AB12CD",
  "companyId": "aa11bb22-...",
  "status": "Pending",
  "assignedSalesRepId": null,
  "items": [
    { "id": "c0de1234-...", "rfqId": "3f2bc1d4-...",
      "productId": "12345678-...", "quantity": 5, "notes": "expedite" }
  ],
  "createdAt": "2026-08-31T10:00:00"
}
```

### 11.2 Quotes

| Endpoint | Auth | Request | Response `data` |
|---|---|---|---|
| `GET /quotes` | any authenticated | query `pageNumber`, `pageSize` | `PaginatedResult<QuoteDto>` (**no items** in list) |
| `GET /quotes/{id}` | any authenticated | — | `QuoteDto` (**items always `[]`** — not populated even by id) |
| `POST /quotes` | WelcoStaff, Admin | see below → **201** | **quote id string** |
| `POST /quotes/{id}/approve` | OrgUser, Admin | — | id string |
| `POST /quotes/{id}/decline` | OrgUser, Admin | — | id string |

**Create quote body** (no validator — all fields optional at API level; `amount` defaults 0, `validUntil` default date):
```json
{
  "rfqId": "3f2bc1d4-1111-4abc-9def-0123456789ab",
  "amount": 2500.00,
  "validUntil": "2026-12-31T00:00:00",
  "items": [
    { "productId": "12345678-aaaa-4abc-9def-0123456789ab", "quantity": 2, "unitPrice": 500.00 },
    { "productId": "87654321-bbbb-4abc-9def-0123456789ab", "quantity": 3, "unitPrice": 500.00 }
  ]
}
```
`quoteNumber` auto-generated (`QT-yyyyMMdd-XXXXXX`), status starts `Draft`, `createdBySalesRepId` = current user.

**Create quote response — `data` is the id string, not a DTO:**
```json
{ "isSuccess": true, "statusCode": 201, "message": "Quote created successfully", "errors": [], "data": "9f7a9999-..." }
```

**`QuoteDto`:**
```json
{
  "id": "9f7a8888-...",
  "quoteNumber": "QT-20260831-ZZ99AA",
  "rfqId": "3f2bc1d4-...",
  "amount": 2500.00,
  "validUntil": "2026-12-31T00:00:00",
  "status": "Draft",
  "items": [],
  "createdAt": "2026-08-31T10:00:00"
}
```

> ⚠️ `createdBySalesRepId` is **not** exposed in any quote DTO. `QuoteDto.items` is never populated.

---

## 12. Content — `api/v1/documents`, `api/v1/landing-pages`

GETs **public**; writes require `Admin` or `WelcoStaff`.

### 12.1 Documents

| Endpoint | Request | Response `data` |
|---|---|---|
| `GET /documents` | query `pageNumber`, `pageSize` | `PaginatedResult<DocumentDto>` |
| `GET /documents/{id}` | — | `DocumentDto` (404) |
| `POST /documents` | see below → **201** | `DocumentDto` |
| `DELETE /documents/{id}` | — | id string |

**Create body:** `{ title (≤200), docType (≤50), fileUrl (≤1000), fileSizeKB (≥0), productId?, publishedDate? }`. `productId` must exist (404 if provided). `publishedDate` defaults to now.

**`DocumentDto`:**
```json
{
  "id": "d0c0b0a0-...",
  "title": "2026 Product Catalog",
  "docType": "Catalog",
  "fileUrl": "https://cdn.welco.com/docs/catalog-2026.pdf",
  "fileSizeKB": 4200,
  "productId": null,
  "publishedDate": "2026-01-15T00:00:00",
  "createdAt": "2026-08-31T10:00:00"
}
```

### 12.2 Landing Pages

| Endpoint | Request | Response `data` |
|---|---|---|
| `GET /landing-pages` | query `pageNumber`, `pageSize`, `type` (string filter) | `PaginatedResult<LandingPageDto>` |
| `GET /landing-pages/slug/{slug}` | — | `LandingPageDto` (404) |
| `POST /landing-pages` | see below → **201** | `LandingPageDto` |
| *(no PUT / DELETE implemented)* | | |

**Create body:**
```json
{
  "type": "Brand",
  "slug": "welco-brand",
  "heroTitle": "Welcome to Welco",
  "heroBody": "Innovative medical solutions.",
  "contentBlock": "{\"sections\": [...]}"
}
```
`type` (≤50), `slug` (≤200, unique → **409**, lowercased server-side), `heroTitle` (≤300) required; `heroBody` (≤2000) and `contentBlock` (≤4000) optional.

**`LandingPageDto`** — note `heroBody`/`contentBlock` are **not** returned:
```json
{
  "id": "1a2b3c4d-...",
  "type": "Brand",
  "slug": "welco-brand",
  "heroTitle": "Welcome to Welco",
  "createdAt": "2026-08-31T10:00:00"
}
```

### 12.3 Support Contact Channels (`/api/v1/support/contact`)

Manage public-facing support email, telephone (Call Us), and WhatsApp contact channels.

| Endpoint | Auth | Request Body | Response `data` |
|---|---|---|---|
| `GET /api/v1/support/contact` | Public (AllowAnonymous) | — | `SupportContactDto` |
| `PUT /api/v1/support/contact` | Admin | `UpdateSupportContactCommand` | `SupportContactDto` |

**Update body:**
```json
{
  "supportEmail": "support@welco.health",
  "phoneNumber": "+971 50 000 0000",
  "whatsAppNumber": "+971500000000",
  "workingHours": "Mon - Fri: 8:00 AM - 6:00 PM (GST)"
}
```

**`SupportContactDto`:**
```json
{
  "id": "11112222-3333-4444-5555-666677778888",
  "supportEmail": "support@welco.health",
  "phoneNumber": "+971 50 000 0000",
  "whatsAppNumber": "+971500000000",
  "workingHours": "Mon - Fri: 8:00 AM - 6:00 PM (GST)",
  "updatedAt": "2026-09-04T21:16:16"
}
```

---

## 13. Frontend Integration Notes (gotchas)

1. **Single entry point:** call the gateway base URL (§2) + the documented route — do not hardcode microservice ports.
2. **Auth state:** after login/verify/refresh store `accessToken` + `refreshToken` + `refreshTokenExpiryTime`. Auto-refresh before expiry via `POST /auth/refresh-token` (revokes old token). On 401, force login.
3. **`message` is localized** (en/ar via `Accept-Language`). Never branch UI logic on the string — use `isSuccess`, `statusCode`, and `errors[]`.
4. **Enums:** numeric enum request fields (`userType`, `language`, `companyType`, `place`, `fileType`) must be sent as **numbers**; status fields on Orders/RFQs/Quotes/Documents/LandingPages are sent/received as **strings**.
5. **Malformed JSON / wrong-type bodies** return ASP.NET `ProblemDetails` (framework default 400), *not* the custom envelope — handle both shapes defensively.
6. **Paged lists omit line items** (carts, orders, RFQs, quotes return `items: []`); fetch by id for detail.
7. **`QuoteDto.items` is never populated**, `OrderDto.incotermId` is always `null`, `CompanyDto.countryNameEn` only in the list endpoint.
8. **Images/files:** upload → store returned `<place>_<guid>.<ext>` filename → render via `GET /files/{relativePath}`.
9. **Create-response bodies:** most creates return the created DTO (201); **some return only an id string** — `POST /quotes`, order/cart status updates, RFQ status update, delete endpoints. Normalize an `unwrap` helper that returns `data` regardless of shape.
10. **Common user flows:**
    - **Signup:** register → verify-register-otp → (tokens) → get profile.
    - **Password reset:** forgot-password → verify-password-otp → (capture `data` as token) → reset-password.
    - **Guest checkout:** create cart (`sessionId`) → add items → create order.
    - **B2B RFQ:** ensure company → create RFQ → (staff) create quote → (buyer) approve/decline → create order from approved quote.
