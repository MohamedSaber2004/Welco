using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.DistributorApplications.Queries.GetDistributorApplications
{
    public class GetDistributorApplicationsQueryValidator : AbstractValidator<GetDistributorApplicationsQuery>
    {
        public GetDistributorApplicationsQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage(LocalizationKeys.AuditLog.PageNumberPositive);
            RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).WithMessage(LocalizationKeys.AuditLog.PageSizeRange);
        }
    }
}
