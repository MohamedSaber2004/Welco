using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.DistributorApplications.Commands.RejectDistributorApplication
{
    public class RejectDistributorApplicationCommandValidator : AbstractValidator<RejectDistributorApplicationCommand>
    {
        public RejectDistributorApplicationCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(LocalizationKeys.DistributorApplication.ApplicationIdRequired);
        }
    }
}
