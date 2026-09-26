namespace Welco.Shared.Common.DTOs.Products
{
    public class ExchangeRateDto
    {
        public Guid Id { get; set; }
        public string BaseCurrency { get; set; } = string.Empty;
        public string TargetCurrency { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateOnly RateDate { get; set; }
        public string Source { get; set; } = string.Empty;
        public DateTime FetchedAt { get; set; }
    }

    public class ConversionResultDto
    {
        public decimal Amount { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal ConvertedAmount { get; set; }
        public DateOnly RateDate { get; set; }
        public string Source { get; set; } = string.Empty;
        public decimal SafetyMarginPercent { get; set; }
    }

    public class CartTotalLineRequest
    {
        public string Key { get; set; } = string.Empty;
        public decimal UnitAmount { get; set; }
        public int Quantity { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
    }

    public class ConvertCartTotalRequest
    {
        public string ToCurrency { get; set; } = string.Empty;
        public List<CartTotalLineRequest> Lines { get; set; } = new();
    }

public class CartTotalLineResultDto
{
    public string Key { get; set; } = string.Empty;
    public string FromCurrency { get; set; } = string.Empty;
    public decimal UnitAmount { get; set; }
    public decimal CeiledUnitAmount { get; set; }
    public int Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal ConvertedUnitAmount { get; set; }
    public decimal LineTotal { get; set; }
}

    public class CartTotalResultDto
    {
        public string ToCurrency { get; set; } = string.Empty;
        public List<CartTotalLineResultDto> Lines { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public DateOnly RateDate { get; set; }
        public string Source { get; set; } = string.Empty;
        public decimal SafetyMarginPercent { get; set; }
    }


}
