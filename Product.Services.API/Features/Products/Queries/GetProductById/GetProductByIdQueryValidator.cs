using FluentValidation;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
    {
        public GetProductByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Product.ProductIdRequired);
        }
    }
}
