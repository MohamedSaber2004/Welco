using FluentValidation;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Products.Commands.UpdateProductVideos
{
    public class UpdateProductVideosCommandValidator : AbstractValidator<UpdateProductVideosCommand>
    {
        public UpdateProductVideosCommandValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty().WithMessage(LocalizationKeys.Product.ProductIdRequired);
            RuleForEach(x => x.Videos).ChildRules(v =>
            {
                v.RuleFor(i => i.Url).NotEmpty().WithMessage(LocalizationKeys.Product.VideoUrlRequired);
            });
        }
    }
}
