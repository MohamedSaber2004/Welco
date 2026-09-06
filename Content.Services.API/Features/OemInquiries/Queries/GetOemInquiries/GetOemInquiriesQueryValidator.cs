using FluentValidation;
namespace Content.Services.API.Features.OemInquiries.Queries.GetOemInquiries
{
    public class GetOemInquiriesQueryValidator : AbstractValidator<GetOemInquiriesQuery>
    {
        public GetOemInquiriesQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
