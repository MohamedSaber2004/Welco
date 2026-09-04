using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.DistributorApplications.Commands.CreateDistributorApplication
{
    public class CreateDistributorApplicationCommandValidator : AbstractValidator<CreateDistributorApplicationCommand>
    {
        public CreateDistributorApplicationCommandValidator()
        {
            RuleFor(x => x.CompanyName).NotEmpty().WithMessage(LocalizationKeys.DistributorApplication.CompanyNameRequired).MaximumLength(200);
            RuleFor(x => x.CountryId).NotEmpty().WithMessage(LocalizationKeys.Country.CountryIdRequired);
            RuleFor(x => x.SalesVolumeBand).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.SalesVolumeBand));
            RuleFor(x => x.ContactPerson).NotEmpty().WithMessage(LocalizationKeys.DistributorApplication.ContactPersonRequired).MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().WithMessage(LocalizationKeys.DistributorApplication.EmailRequired).EmailAddress().WithMessage(LocalizationKeys.DistributorApplication.EmailInvalid).MaximumLength(200);
            RuleFor(x => x.Website).MaximumLength(300).When(x => !string.IsNullOrWhiteSpace(x.Website));
            RuleFor(x => x.Phone).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Phone));
            RuleFor(x => x.CategoryInterest).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.CategoryInterest));
        }
    }
}
