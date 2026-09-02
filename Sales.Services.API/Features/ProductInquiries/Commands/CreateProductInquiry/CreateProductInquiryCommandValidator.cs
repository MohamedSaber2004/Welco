using FluentValidation;

namespace Sales.Services.API.Features.ProductInquiries.Commands.CreateProductInquiry
{
    public class CreateProductInquiryCommandValidator : AbstractValidator<CreateProductInquiryCommand>
    {
        public CreateProductInquiryCommandValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Organization).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }
}
