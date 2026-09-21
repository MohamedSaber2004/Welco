# Plan: Duplicate Database Entity Analysis Between Welco and SNUL

## Goal
Create a comprehensive analysis plan to identify duplicate database entities/models between Welco and SNUL, comparing entity structures, relationships, and migration schemas across separate databases.

## Background
Both Welco and SNUL maintain separate databases but share similar domain models (Users, Products, Orders, Categories, Distributors, etc.). After removing integration-related entities (WelcoProviderMap, AddCrossMarketIntegrationFields), we need to verify which core entities are duplicated.

## Steps

### 1. Extract All Domain Entities from Both Projects
1. [ ] Parse all `*Domain\Models\*` folders in both projects
2. [ ] Extract entity class names and namespaces
3. [ ] List all `[Key]`, `[ForeignKey]`, and relationship attributes
4. [ ] Build entity inventory: `{Service} -> {EntityName}`

### 2. Compare Entity Structures
1. [ ] For each matching entity name, compare:
   - Property names and types
   - Navigation properties
   - Data annotations (MaxLength, Required, etc.)
   - Fluent API configurations (if any)
2. [ ] Categorize matches:
   - **Exact duplicates**: Same entity name, same properties
   - **Similar entities**: Same name, different properties
   - **Unique to Welco**: Entities only in Welco
   - **Unique to SNUL**: Entities only in SNUL

### 3. Analyze Migrations
1. [ ] Compare migration files in both `Migrations` folders
2. [ ] Identify migration operations that create identical tables
3. [ ] Check for shared `AddCrossMarketIntegrationFields` migration removal
4. [ ] Verify database schema differences

### 4. Check DbContext Configurations
1. [ ] Compare `DbContext` classes and `OnModelCreating` configurations
2. [ ] Identify entity configurations that mirror each other
3. [ ] Check for conflicting configurations if databases were to be unified

### 5. Document Findings
1. [ ] Generate comparison report with entity tables
2. [ ] Highlight entities needing alignment or separation
3. [ ] Recommend which system should own each entity definition

## Risks / Assumptions
- Both projects use EF Core with separate databases
- Entity names may differ despite representing same domain concept
- Migration snapshots may not reflect latest model changes
- Shared project references (e.g., SNUL.Shared vs Welco.Shared) may contain overlapping entities

## Success Criteria
- Complete entity inventory for both projects
- Clear categorization of all duplicate/similar/unique entities
- Actionable recommendations for entity consolidation or separation