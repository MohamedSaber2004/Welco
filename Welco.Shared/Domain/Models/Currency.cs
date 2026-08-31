using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public class Currency : BaseEntity<Guid>
    {
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Symbol { get; set; } = null!;

        public static Currency Create(
            string nameEn,
            string nameAr,
            string code,
            string symbol,
            string createdBy)
        {
            var currency = new Currency
            {
                Id = Guid.NewGuid(),
                NameEn = nameEn,
                NameAr = nameAr,
                Code = code,
                Symbol = symbol
            };
            currency.MarkAsCreated(createdBy);
            return currency;
        }

        public void Update(
            string nameEn,
            string nameAr,
            string code,
            string symbol,
            string updatedBy)
        {
            NameEn = nameEn.Trim();
            NameAr = nameAr.Trim();
            Code = code.Trim();
            Symbol = symbol.Trim();
            MarkAsUpdated(updatedBy);
        }
    }
}
