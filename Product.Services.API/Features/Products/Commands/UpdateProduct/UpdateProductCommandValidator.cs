using FluentValidation;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Product.ProductIdRequired);

            RuleFor(x => x.NameEn)
                .NotEmpty().WithMessage(LocalizationKeys.Product.NameEnRequired)
                .MaximumLength(200);

            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage(LocalizationKeys.Product.NameArRequired)
                .MaximumLength(200);

            RuleFor(x => x.Price)
                .NotEmpty().WithMessage(LocalizationKeys.Product.PriceRequired)
                .GreaterThan(0).WithMessage(LocalizationKeys.Product.PricePositive);

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage(LocalizationKeys.Product.StockNotNegative);

            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage(LocalizationKeys.Product.SkuRequired)
                .MaximumLength(50);

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage(LocalizationKeys.Product.SlugRequired)
                .MaximumLength(200);

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage(LocalizationKeys.Product.CategoryRequired);

            RuleFor(x => x.Material)
                .MaximumLength(100)
                .When(x => x.Material != null);

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .When(x => x.Description != null);

            RuleFor(x => x.Specifications)
                .MaximumLength(2000)
                .When(x => x.Specifications != null);

            RuleFor(x => x.ImageName)
                .MaximumLength(500)
                .When(x => x.ImageName != null);
        }
    }
}
