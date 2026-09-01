using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.FAQs.Commands.CreateFAQ
{
    public class CreateFAQCommandValidator : AbstractValidator<CreateFAQCommand>
    {
        public CreateFAQCommandValidator()
        {
            RuleFor(x => x.Question).NotEmpty().WithMessage(LocalizationKeys.FAQ.QuestionRequired).MaximumLength(500);
            RuleFor(x => x.Answer).NotEmpty().WithMessage(LocalizationKeys.FAQ.AnswerRequired);
        }
    }
}
