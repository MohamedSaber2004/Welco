using FluentValidation;
using Welco.Shared.Localization;

namespace Sales.Services.API.Features.ProductInquiries.Commands.CreateProductInquiry
{
    public class CreateProductInquiryCommandValidator : AbstractValidator<CreateProductInquiryCommand>
    {
        public CreateProductInquiryCommandValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty().WithMessage(LocalizationKeys.ProductInquiry.ProductIdRequired);
            RuleFor(x => x.Name).NotEmpty().WithMessage(LocalizationKeys.ProductInquiry.NameRequired).MaximumLength(200);
            RuleFor(x => x.Organization).NotEmpty().WithMessage(LocalizationKeys.ProductInquiry.OrganizationRequired).MaximumLength(200);
            RuleFor(x => x.Message).NotEmpty().WithMessage(LocalizationKeys.ProductInquiry.MessageRequired).MaximumLength(2000);
            RuleFor(x => x.Email).EmailAddress().WithMessage(LocalizationKeys.ProductInquiry.EmailInvalid).When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }
}
