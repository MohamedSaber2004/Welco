using FluentValidation;
using Welco.Shared.Localization;
namespace Sales.Services.API.Features.RFQs.Commands.CreateRFQ
{
    public class CreateRFQCommandValidator : AbstractValidator<CreateRFQCommand>
    {
        public CreateRFQCommandValidator()
        {
            RuleFor(x => x.CompanyId).NotEmpty().WithMessage(LocalizationKeys.Company.CompanyIdRequired);
            RuleFor(x => x.Items).NotEmpty().WithMessage(LocalizationKeys.RFQ.ItemsRequired);
            RuleForEach(x => x.Items).ChildRules(i => { i.RuleFor(v => v.ProductId).NotEmpty(); i.RuleFor(v => v.Quantity).GreaterThan(0); });
        }
    }
}
