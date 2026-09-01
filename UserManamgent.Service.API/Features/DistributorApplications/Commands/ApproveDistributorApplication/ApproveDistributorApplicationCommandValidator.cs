using FluentValidation;

namespace UserManamgent.Service.API.Features.DistributorApplications.Commands.ApproveDistributorApplication
{
    public class ApproveDistributorApplicationCommandValidator : AbstractValidator<ApproveDistributorApplicationCommand>
    {
        public ApproveDistributorApplicationCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.TierLevel).InclusiveBetween(1, 5);
        }
    }
}
