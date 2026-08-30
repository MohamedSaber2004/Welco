using FluentValidation;
using Welco.Shared.Localization;

namespace Provider.Services.API.Features.Providers.Commands.DeleteProvider
{
    public class DeleteProviderCommandValidator : AbstractValidator<DeleteProviderCommand>
    {
        public DeleteProviderCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Provider.ProviderIdRequired);
        }
    }
}
