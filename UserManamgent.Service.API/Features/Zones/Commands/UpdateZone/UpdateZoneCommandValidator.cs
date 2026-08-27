using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Zones.Commands.UpdateZone
{
    public class UpdateZoneCommandValidator : AbstractValidator<UpdateZoneCommand>
    {
        public UpdateZoneCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Zone.ZoneIdRequired);

            RuleFor(x => x.NameEn)
                .MaximumLength(150)
                .When(x => !string.IsNullOrEmpty(x.NameEn));

            RuleFor(x => x.NameAr)
                .MaximumLength(150)
                .When(x => !string.IsNullOrEmpty(x.NameAr));
        }
    }
}
