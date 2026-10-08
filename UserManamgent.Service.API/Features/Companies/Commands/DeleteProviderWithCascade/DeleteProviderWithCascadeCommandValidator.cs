using FluentValidation;
using Welco.Shared.Localization;
namespace UserManamgent.Service.API.Features.Companies.Commands.DeleteProviderWithCascade
{
    public class DeleteProviderWithCascadeCommandValidator : AbstractValidator<DeleteProviderWithCascadeCommand>
    {
        public DeleteProviderWithCascadeCommandValidator() { RuleFor(x => x.Id).NotEmpty().WithMessage(LocalizationKeys.Company.CompanyIdRequired); }
    }
}
