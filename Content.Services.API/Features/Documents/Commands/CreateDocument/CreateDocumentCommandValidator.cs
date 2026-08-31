using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
    {
        public CreateDocumentCommandValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage(LocalizationKeys.Document.TitleRequired).MaximumLength(200);
            RuleFor(x => x.DocType).NotEmpty().WithMessage(LocalizationKeys.Document.DocTypeRequired).MaximumLength(50);
            RuleFor(x => x.FileUrl).NotEmpty().WithMessage(LocalizationKeys.Document.FileUrlRequired).MaximumLength(1000);
            RuleFor(x => x.FileSizeKB).GreaterThanOrEqualTo(0).WithMessage(LocalizationKeys.Document.FileSizeNotNegative);
        }
    }
}
