using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.DistributorApplications.Commands.ApproveDistributorApplication
{
    public class ApproveDistributorApplicationCommandValidator : AbstractValidator<ApproveDistributorApplicationCommand>
    {
        public ApproveDistributorApplicationCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(LocalizationKeys.DistributorApplication.ApplicationIdRequired);
        }
    }
}
