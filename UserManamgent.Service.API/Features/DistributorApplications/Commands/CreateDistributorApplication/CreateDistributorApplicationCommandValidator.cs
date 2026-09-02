using FluentValidation;

namespace UserManamgent.Service.API.Features.DistributorApplications.Commands.CreateDistributorApplication
{
    public class CreateDistributorApplicationCommandValidator : AbstractValidator<CreateDistributorApplicationCommand>
    {
        public CreateDistributorApplicationCommandValidator()
        {
            RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.CountryId).NotEmpty().WithMessage("CountryId is required");
            RuleFor(x => x.SalesVolumeBand).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.SalesVolumeBand));
            RuleFor(x => x.ContactPerson).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
            RuleFor(x => x.Website).MaximumLength(300).When(x => !string.IsNullOrWhiteSpace(x.Website));
            RuleFor(x => x.Phone).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Phone));
            RuleFor(x => x.CategoryInterest).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.CategoryInterest));
        }
    }
}
