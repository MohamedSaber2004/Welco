using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.DistributorApplications.Queries.GetDistributorApplicationById
{
    public class GetDistributorApplicationByIdQueryValidator : AbstractValidator<GetDistributorApplicationByIdQuery>
    {
        public GetDistributorApplicationByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(LocalizationKeys.DistributorApplication.ApplicationIdRequired);
        }
    }
}
