using FluentValidation;

namespace UserManamgent.Service.API.Features.DistributorApplications.Commands.RejectDistributorApplication
{
    public class RejectDistributorApplicationCommandValidator : AbstractValidator<RejectDistributorApplicationCommand>
    {
        public RejectDistributorApplicationCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
