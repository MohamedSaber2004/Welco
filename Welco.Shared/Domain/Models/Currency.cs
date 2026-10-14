using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public class Currency : BaseEntity<Guid>
    {
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Symbol { get; set; } = null!;
        public string SymbolNative { get; set; } = null!;
        public int DecimalDigits { get; set; } = 2;
        public string? CountryCode { get; set; }

        public static Currency Create(
            string nameEn,
            string nameAr,
            string code,
            string symbol,
            string createdBy,
            string? symbolNative = null,
            int decimalDigits = 2,
            string? countryCode = null)
        {
            var currency = new Currency
            {
                Id = Guid.NewGuid(),
                NameEn = nameEn.Trim(),
                NameAr = nameAr.Trim(),
                Code = code.Trim().ToUpperInvariant(),
                Symbol = symbol.Trim(),
                SymbolNative = (symbolNative ?? symbol).Trim(),
                DecimalDigits = decimalDigits,
                CountryCode = countryCode?.Trim().ToUpperInvariant()
            };
            currency.MarkAsCreated(createdBy);
            return currency;
        }

        public void Update(
            string nameEn,
            string nameAr,
            string code,
            string symbol,
            string updatedBy,
            string? symbolNative = null,
            int? decimalDigits = null,
            string? countryCode = null)
        {
            NameEn = nameEn.Trim();
            NameAr = nameAr.Trim();
            Code = code.Trim().ToUpperInvariant();
            Symbol = symbol.Trim();
            if (symbolNative != null) SymbolNative = symbolNative.Trim();
            if (decimalDigits.HasValue) DecimalDigits = decimalDigits.Value;
            if (countryCode != null) CountryCode = countryCode.Trim().ToUpperInvariant();
            MarkAsUpdated(updatedBy);
        }
    }
}
