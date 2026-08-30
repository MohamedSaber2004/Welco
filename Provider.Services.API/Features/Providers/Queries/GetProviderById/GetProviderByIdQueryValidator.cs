using FluentValidation;
using Welco.Shared.Localization;

namespace Provider.Services.API.Features.Providers.Queries.GetProviderById
{
    public class GetProviderByIdQueryValidator : AbstractValidator<GetProviderByIdQuery>
    {
        public GetProviderByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Provider.ProviderIdRequired);
        }
    }
}
