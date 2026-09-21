# Database Entity Analysis Report: Welco vs SNUL

**Generated:** 2026-09-21
**Purpose:** Identify duplicate domain entities between Welco and SNUL shared projects
**Scope:** `*/Domain/Models/` directories in both Welco.Shared and SNUL.Shared

## Executive Summary

| Metric | Count |
|--------|-------|
| Total Entities Analyzed | 21 |
| Exact Duplicates | 17 |
| Similar Entities | 2 |
| Unique to Welco | 0 |
| Unique to SNUL | 3 |

## Shared Entities (Exact Duplicates)

These entities are identical in both projects (differing only by namespace):

| Entity | Namespace Welco | Namespace SNUL | File |
|--------|-----------------|----------------|------|
| User | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | ApplicationUser.cs |
| UserAddress | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | UserAddress.cs |
| UserRefreshToken | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | UserRefreshToken.cs |
| Category | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | Category.cs |
| Commerce | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | Commerce.cs |
| Company | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | Company.cs |
| CompanyAddress | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | CompanyAddress.cs |
| City | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | City.cs |
| Content | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | Content.cs |
| Currency | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | Currency.cs |
| Country | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | Country.cs |
| AuditLog | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | AuditLog.cs |
| Zone | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | Zone.cs |
| ProductSpecification | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | ProductSpecification.cs |
| ProductProcedureTag | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | ProductProcedureTag.cs |
| ProductMedia | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | ProductMedia.cs |
| Sales | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | Sales.cs |
| Certification | `Welco.Shared.Domain.Models` | `SNUL.Shared.Domain.Models` | Certification.cs |

## Similar Entities

### Product

| Property | Welco | SNUL |
|----------|-------|------|
| NameEn | ✓ | ✓ |
| NameAr | ✓ | ✓ |
| Sku | ✓ | ✓ |
| Slug | ✓ | ✓ |
| Description | ✓ | ✓ |
| Price | ✓ | ✓ |
| Stock | ✓ | ✓ |
| Specifications | ✓ | ✓ |
| ImageName | ✓ | ✓ |
| Material | ✓ | ✓ |
| LengthCm | ✓ | ✓ |
| CurrencyId | ✓ | ✓ |
| CategoryId | ✓ | ✓ |
| **WelcoProductId** | ✗ | ✓ |

**Difference:** SNUL has `WelcoProductId` (Guid?) property for cross-system reference.

### Category

| Property | Welco | SNUL |
|----------|-------|------|
| NameEn | ✓ | ✓ |
| NameAr | ✓ | ✓ |
| Description | ✓ | ✓ |
| ImageName | ✓ | ✓ |
| ParentCategoryId | ✓ | ✓ |
| **WelcoCategoryId** | ✗ | ✓ |

**Difference:** SNUL has `WelcoCategoryId` (Guid?) property for cross-system reference.

## Entities Unique to SNUL (Not in Welco)

### ExchangeRate

```csharp
public class ExchangeRate : BaseEntity<Guid>
{
    public Guid BaseCurrencyId { get; set; }
    public Guid TargetCurrencyId { get; set; }
    public decimal Rate { get; set; }
    public DateOnly RateDate { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime FetchedAt { get; set; }
    public virtual Currency BaseCurrency { get; set; } = null!;
    public virtual Currency TargetCurrency { get; set; } = null!;
}
```

### ExchangeRateSyncLog

```csharp
public class ExchangeRateSyncLog : BaseEntity<Guid>
{
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ExchangeRateSyncStatus Status { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public int RatesCount { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public enum ExchangeRateSyncStatus
{
    Pending = 0,
    Success = 1,
    Failed = 2,
    Partial = 3
}
```

## Removed Integration Entities

Both projects removed these integration-related entities during ongoing cleanup:

| Entity | Removed From |
|--------|-------------|
| WelcoProviderMap | Both Welco and SNUL |
| AddCrossMarketIntegrationFields migration | Both Welco and SNUL |

## Shared Library Comparison

| Component | Welco.Shared | SNUL.Shared |
|-----------|-------------|-------------|
| DependencyInjection.cs | 153 lines | 158 lines |
| Domain/Models | 20 entities | 23 entities |
| Common/Attributes | ServiceAuthAttribute, RoleAuthorize | ServiceAuthAttribute, RoleAuthorize |
| Base classes | BaseEntity, BaseService | BaseEntity, BaseService |

## Key Findings

### Structural Differences
1. **SNUL has additional Welco*Id fields** in Product (`WelcoProductId`) and Category (`WelcoCategoryId`) for cross-system tracking
2. **SNUL has ExchangeRate entities** that Welco lacks (ExchangeRate, ExchangeRateSyncLog, ExchangeRateSyncStatus enum)
3. **SNUL Product has extra field** that may be for integration tracking
4. **SNUL Category has extra field** for cross-reference to Welco

### Namespace Difference
- Welco: `Welco.Shared.Domain.Models.*`
- SNUL: `SNUL.Shared.Domain.Models.*`

### File Differences
| File | Welco | SNUL |
|------|-------|------|
| UserProductInteraction.cs | Uses `Welco.Shared.Enums` | May differ |
| DependencyInjection.cs | 153 lines | 158 lines |

## Recommendations

### High Priority
1. **Sync ExchangeRate entities** from SNUL to Welco (if Welco will support exchange rates)
2. **Decide on WelcoProductId/WelcoCategoryId** - determine if these fields are needed in Welco
3. **Merge ExchangeRate endpoints** - coordinate which service owns rate fetching

### Medium Priority
1. **Verify UserProductInteraction** difference - check if Welco.Enums namespace exists in SNUL
2. **Align DependencyInjection** - review 5-line difference in shared service registration
3. **Document cross-system references** - decide how WelcoProductId/WelcoCategoryId are used

### Low Priority
1. **ExchangeRateSyncLog** - consider if both systems need sync logging
2. **Migration alignment** - verify both DB schemas remain compatible