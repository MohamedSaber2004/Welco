namespace Welco.Shared.Common.DTOs.Integration
{

public class ExternalCategoryDto
    {
        public Guid Id { get; set; }
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageName { get; set; }
        public Guid? ParentCategoryId { get; set; }
    }

public class InventoryCheckRequest
    {
        public List<InventoryCheckItem> Items { get; set; } = new();
    }

    public class InventoryCheckItem
    {
        public Guid WelcoProductId { get; set; }
        public int RequestedQuantity { get; set; }
    }

    public class InventoryCheckResponse
    {
        public bool AllAvailable { get; set; }
        public List<InventoryItemResult> Items { get; set; } = new();
    }

    public class InventoryItemResult
    {
        public Guid WelcoProductId { get; set; }
        public int AvailableStock { get; set; }
        public bool IsAvailable { get; set; }
    }

public class CreateExternalOrderRequest
    {
        public string? SourceMarket { get; set; } = "Egypt";
        public Guid? ExternalCustomerId { get; set; }
        public Guid? CurrencyId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<ExternalOrderItemRequest> Items { get; set; } = new();
    }

    public class ExternalOrderItemRequest
    {
        public Guid WelcoProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class ExternalOrderResponse
    {
        public Guid WelcoOrderId { get; set; }
        public string WelcoOrderNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
    }

    public class UpdateExternalStatusRequest
    {
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }
    }

public class ExternalProductDto
    {
        public Guid Id { get; set; }
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageName { get; set; }
        public Guid CategoryId { get; set; }
    }

public class ExternalProviderDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string Status { get; set; } = null!;
        public bool IsProvider { get; set; }
    }

public class ApplyDistributorRequest
    {
        public string CompanyName { get; set; } = null!;
        public string ContactPerson { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public Guid CountryId { get; set; }
        public string? SalesVolumeBand { get; set; }
        public string? CategoryInterest { get; set; }
        public string? Website { get; set; }
        public string SourceMarket { get; set; } = "Egypt";
    }

    public class DistributorApplicationDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = null!;
        public string ContactPerson { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public Guid CountryId { get; set; }
        public string? CountryName { get; set; }
        public string? SalesVolumeBand { get; set; }
        public string? CategoryInterest { get; set; }
        public string? Website { get; set; }
        public string Status { get; set; } = null!; 
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateDistributorStatusRequest
    {
        public string Status { get; set; } = null!; 
        public string? Reason { get; set; }
    }

public class CreateExternalQuoteRequest
    {
        public string? SourceMarket { get; set; } = "Egypt";
        public List<ExternalQuoteItemRequest> Items { get; set; } = new();
    }

    public class ExternalQuoteItemRequest
    {
        public Guid WelcoProductId { get; set; }
        public int Quantity { get; set; }
        public decimal? RequestedUnitPrice { get; set; }
        public string? Notes { get; set; }
    }

    public class ExternalQuoteResponse
    {
        public Guid WelcoRfqId { get; set; }
        public string WelcoRfqNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
    }

public class ExternalSupportTicketDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Subject { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Status { get; set; } = null!; 
        public string? Reply { get; set; }
        public DateTime? RepliedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ReplySupportTicketRequest
    {
        public string Reply { get; set; } = null!;
    }

public class ExternalHelpArticleDto
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string Slug { get; set; } = null!;
    }

    public class ExternalFAQDto
    {
        public Guid Id { get; set; }
        public string Question { get; set; } = null!;
        public string Answer { get; set; } = null!;
        public int SortOrder { get; set; }
    }

public class ExternalCertificationDto
    {
        public Guid Id { get; set; }
        public string CertificateNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string IssuedTo { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Description { get; set; }
        public string? CertificationImageName { get; set; }
    }
}
