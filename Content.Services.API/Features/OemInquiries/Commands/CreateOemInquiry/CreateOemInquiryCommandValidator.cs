using FluentValidation;
using Welco.Shared.Localization;
namespace Content.Services.API.Features.OemInquiries.Commands.CreateOemInquiry
{
    public class CreateOemInquiryCommandValidator : AbstractValidator<CreateOemInquiryCommand>
    {
        public CreateOemInquiryCommandValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().WithMessage(LocalizationKeys.OemInquiry.FullNameRequired).MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().WithMessage(LocalizationKeys.OemInquiry.EmailRequired).EmailAddress().WithMessage(LocalizationKeys.OemInquiry.EmailInvalid).MaximumLength(256);
            RuleFor(x => x.CompanyName).NotEmpty().WithMessage(LocalizationKeys.OemInquiry.CompanyNameRequired).MaximumLength(200);
            RuleFor(x => x.ServiceType).NotEmpty().WithMessage(LocalizationKeys.OemInquiry.ServiceTypeRequired).MaximumLength(200);
            RuleFor(x => x.Message).NotEmpty().WithMessage(LocalizationKeys.OemInquiry.MessageRequired).MaximumLength(2000);
        }
    }
}
