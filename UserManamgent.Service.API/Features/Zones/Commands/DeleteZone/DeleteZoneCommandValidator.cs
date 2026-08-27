using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Zones.Commands.DeleteZone
{
    public class DeleteZoneCommandValidator : AbstractValidator<DeleteZoneCommand>
    {
        public DeleteZoneCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Zone.ZoneIdRequired);
        }
    }
}
