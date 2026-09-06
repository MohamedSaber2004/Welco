using FluentValidation;
namespace Sales.Services.API.Features.ProductInquiries.Queries.GetProductInquiries
{
    public class GetProductInquiriesQueryValidator : AbstractValidator<GetProductInquiriesQuery>
    {
        public GetProductInquiriesQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
