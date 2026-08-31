using FluentValidation;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Products.Queries.ShowProduct
{
    public class ShowProductQueryValidator : AbstractValidator<ShowProductQuery>
    {
        public ShowProductQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Product.ProductIdRequired);
        }
    }
}
