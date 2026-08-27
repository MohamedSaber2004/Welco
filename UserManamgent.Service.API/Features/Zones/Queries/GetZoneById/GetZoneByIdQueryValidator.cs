using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Zones.Queries.GetZoneById
{
    public class GetZoneByIdQueryValidator : AbstractValidator<GetZoneByIdQuery>
    {
        public GetZoneByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Zone.ZoneIdRequired);
        }
    }
}
