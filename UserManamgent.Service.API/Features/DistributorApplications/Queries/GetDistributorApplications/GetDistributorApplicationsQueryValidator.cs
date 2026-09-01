using FluentValidation;

namespace UserManamgent.Service.API.Features.DistributorApplications.Queries.GetDistributorApplications
{
    public class GetDistributorApplicationsQueryValidator : AbstractValidator<GetDistributorApplicationsQuery>
    {
        public GetDistributorApplicationsQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
        }
    }
}
