using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.FAQs.Commands.UpdateFAQ
{
    public class UpdateFAQCommandValidator : AbstractValidator<UpdateFAQCommand>
    {
        public UpdateFAQCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Question).NotEmpty().WithMessage(LocalizationKeys.FAQ.QuestionRequired).MaximumLength(500);
            RuleFor(x => x.Answer).NotEmpty().WithMessage(LocalizationKeys.FAQ.AnswerRequired);
        }
    }
}
