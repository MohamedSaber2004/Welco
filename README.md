# 🏥 Welco — Surgical Instruments Platform

> **A hybrid B2B + B2C e-commerce platform for a global surgical instrument manufacturer.**

---

## 📖 Business Overview

**Welco** is a professional surgical instrument manufacturer that has been operating since **1994**. With over **30 years** of industry experience, the company holds **ISO 13485** (Medical Device Quality Management System) certification and **CE marking**, and exports its products to **40+ countries** worldwide.

### Who are Welco's Clients?
| Client Type | Description |
|---|---|
| 🏥 **Hospitals** | Direct institutional buyers requiring bulk surgical tools |
| 🏭 **Distributors** | Regional distributors managing resale across territories |
| 🏨 **Clinics** | Mid-scale clinical facilities with recurring orders |
| 📦 **Importers** | International trade companies purchasing wholesale |

### What Does This Platform Do?

This platform is a **full-stack digital commerce system** that serves two kinds of buyers:

- **B2C (Direct Purchase):** Any buyer can browse products, add items to a cart (even as a guest using a session), and place an order directly.
- **B2B (Sales Pipeline):** Hospitals and distributors create **Requests for Quotation (RFQ)** → the Welco sales team responds with a priced **Quote** → the buyer **approves or declines** → an approved quote can be converted into a formal **Order**.

The platform also supports:
- 📋 **Product Catalog** — categories, products, materials, specifications, pricing, multi-currency
- 📜 **Certifications** — ISO, CE, and other compliance documents publicly displayed
- 📄 **Content Management** — landing pages, documents, help articles
- 📁 **File Management** — images, videos, PDFs, and certificates stored and served via a dedicated attachment service

---

## 🏗️ Architecture

The backend is built on **.NET 10** using **Clean/Onion Architecture** and split into **independent microservices**, each owning a bounded context. All services communicate through a single **Ocelot API Gateway**.

```
┌──────────────────────────────────────────────────┐
│               Frontend / Client App               │
└────────────────────┬─────────────────────────────┘
                     │ All requests go through gateway
┌────────────────────▼─────────────────────────────┐
│            Welco.API  (Ocelot Gateway)            │
│   JWT validation · Rate limiting · OpenAPI docs   │
└──┬──────┬──────┬──────┬──────┬──────┬────────┬───┘
   │      │      │      │      │      │        │
 Auth  UserMgmt Product Commerce Sales Content Attach Cert
```

### Microservices

| Service | Bounded Context | HTTPS Port |
|---|---|:---:|
| `Auth.Services.API` | Identity & Access — register, login, OTP, JWT, profile | `7203` |
| `UserManamgent.Service.API` | Organizations — users, companies, addresses, locations | `7204` |
| `Product.Services.API` | Catalog — categories, products, currencies | `7054` |
| `Commerce.Services.API` | Direct Commerce — carts, orders | `7045` |
| `Sales.Services.API` | B2B Sales Pipeline — RFQs, quotes | `7046` |
| `Content.Services.API` | Content & Marketing — landing pages, documents, help | `7047` |
| `Certification.Services.API` | Compliance & Quality — certifications | `7101` |
| `Attachment.Services.API` | File Storage — images, videos, PDFs, audio | `7180` |
| `Welco.API` | **API Gateway** — Ocelot routing, JWT, rate limiting | `7166` |

---

## 👤 User Roles

| Role | Access Level |
|---|---|
| **Admin** | Full access — bypasses every role restriction |
| **WelcoStaff** | Internal Welco employees — manage catalog, content, quotes |
| **OrganizationUser** | B2B company account — cart, orders, RFQs |
| **Guest** | Anonymous — public catalog & content browsing only |

---

## 🔄 Business Flows

### B2C (Direct Purchase)
```
Guest/User → Browse Products → Add to Cart → Place Order
```

### B2B (Sales Pipeline)
```
OrganizationUser → Create RFQ → WelcoStaff Reviews → Creates Quote
→ Buyer Approves/Declines → (Approved) → Converts to Order
```

### Admin Operations
```
Admin → Manage Users & Companies → Manage Catalog → Manage Content
     → View All Orders & RFQs → Manage Certifications & Files
```

---

## 🌐 Environments & URLs

| Environment | Gateway URL |
|---|---|
| Development (HTTP) | `http://localhost:5293` |
| Development (HTTPS) | `https://localhost:7166` |
| Test (hosted) | `https://welco-gateway.runasp.net` |

**API Documentation (Swagger/Scalar):**
- Unified docs: `GET /` → redirects to `/scalar/v1`
- Raw OpenAPI spec: `/openapi/all.json`
- Per-service docs: `/api/docs/{service}` where `service` ∈ `auth`, `usermanagement`, `product`, `commerce`, `sales`, `content`, `certification`, `attachment`

---

## ⚙️ Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET 10 (ASP.NET Core) |
| Architecture | Clean / Onion Architecture + CQRS (MediatR) |
| Gateway | Ocelot API Gateway |
| Database | SQL Server (shared schema, EF Core Code-First) |
| Auth | ASP.NET Identity + JWT Bearer Tokens |
| Validation | FluentValidation |
| Logging | Serilog (Console + Rolling File) |
| API Docs | Scalar (OpenAPI) |
| File Storage | Local filesystem via Attachment service |
| Email | SMTP (Gmail) via EmailSettings |
| Localization | JSON-based (English + Arabic) |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or hosted)
- Visual Studio 2022 / JetBrains Rider / VS Code

### 1. Clone the Repository
```bash
git clone https://github.com/MohamedSaber2004/Welco.git
cd Welco
```

### 2. Configure Secrets (Per Service)

Each microservice has its own `appsettings.Development.json`. Fill in the following values for each service:

```json
{
  "ConnectionStrings": {
    "DatabaseConnection": "YOUR_CONNECTION_STRING_HERE"
  },
  "JwtSettings": {
    "Secret": "YOUR_JWT_SECRET_HERE"
  }
}
```

For `Auth.Services.API` only, also configure email:

```json
{
  "EmailSettings": {
    "Email": "your-email@example.com",
    "Username": "your-email@example.com",
    "Password": "YOUR_EMAIL_APP_PASSWORD_HERE"
  }
}
```

> 💡 Tip: Use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to avoid storing credentials in files:
> ```bash
> dotnet user-secrets set "ConnectionStrings:DatabaseConnection" "your-conn-string"
> ```

### 3. Apply Database Migrations

Migrations are in `Welco.Shared`. Run from any service that uses the DB:

```bash
cd Auth.Services.API
dotnet ef database update --project ../Welco.Shared
```

### 4. Run All Services

Open the solution `Welco.sln` in Visual Studio and set **multiple startup projects** (all services + gateway), or run them individually:

```bash
# Terminal 1 — Gateway
cd Welco.API && dotnet run --launch-profile https

# Terminal 2 — Auth
cd Auth.Services.API && dotnet run --launch-profile https

# Terminal 3 — Products
cd Product.Services.API && dotnet run --launch-profile https

# ... repeat for each service
```

### 5. Seed the Admin User

On first startup, the seeder reads from `Welco.Shared/Persistance/Seeding/User.json`. Update this file with your desired admin credentials before running:

```json
{
  "FullName": "Your Name",
  "Email": "admin@yourdomain.com",
  "UserName": "admin",
  "Password": "YourSecurePassword@123",
  "Language": "en",
  "Roles": ["Admin"]
}
```

---

## 📂 Project Structure

```
Welco/
├── Welco.API/                        # Ocelot API Gateway
├── Welco.Shared/                     # Shared kernel (models, DTOs, EF, migrations)
├── Auth.Services.API/                # Authentication & identity
├── UserManamgent.Service.API/        # Users, companies, addresses
├── Product.Services.API/             # Catalog: categories, products, currencies
├── Commerce.Services.API/            # Carts & orders
├── Sales.Services.API/               # RFQs & quotes (B2B pipeline)
├── Content.Services.API/             # Landing pages, documents, help articles
├── Certification.Services.API/       # ISO & compliance certifications
├── Attachment.Services.API/          # File upload/download/storage
├── API-INTEGRATION.md                # Full API reference for frontend developers
└── ATTACHMENT-INTEGRATION.md         # File upload/URL resolution guide
```

---

## 📡 Key API Endpoints (via Gateway)

| Category | Endpoint |
|---|---|
| Auth | `POST /api/v1/auth/register` · `POST /api/v1/auth/login` |
| Profile | `GET /api/v1/auth/profile` · `PUT /api/v1/auth/profile` |
| Products | `GET /api/v1/products` · `GET /api/v1/products/{id}` |
| Categories | `GET /api/v1/categories` · `GET /api/v1/categories/{id}/products` |
| Cart | `POST /api/v1/carts` · `POST /api/v1/carts/items` |
| Orders | `POST /api/v1/orders` · `GET /api/v1/orders` |
| RFQs | `POST /api/v1/rfqs` · `GET /api/v1/rfqs/{id}` |
| Quotes | `POST /api/v1/quotes` · `PUT /api/v1/quotes/{id}/approve` |
| Files | `POST /api/v1/attachments/upload` · `GET /files/{storedName}` |
| Certifications | `GET /api/v1/certifications` |

> 📖 Full endpoint reference: [`API-INTEGRATION.md`](./API-INTEGRATION.md)

---

## 🔒 Security Notes

- All protected endpoints require `Authorization: Bearer <accessToken>`
- JWT tokens expire after **60 minutes**; use `POST /api/v1/auth/refresh-token` for silent renewal (refresh token valid **30 days**)
- Role-based access is enforced at the controller level across all services
- The gateway validates JWT on every request before forwarding downstream
- **Never commit real credentials** to source control — use environment variables or .NET User Secrets

---

## 📞 Support & Contact

For integration questions, refer to [`API-INTEGRATION.md`](./API-INTEGRATION.md) and [`ATTACHMENT-INTEGRATION.md`](./ATTACHMENT-INTEGRATION.md) included in this repository.

---

*Welco © 1994–2026 — Precision. Quality. Trust.*
