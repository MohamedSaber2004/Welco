using FluentValidation;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
    {
        public DeleteProductCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Product.ProductIdRequired);
        }
    }
}
