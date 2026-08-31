using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
    {
        public DeleteDocumentCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(LocalizationKeys.Document.DocumentIdRequired);
        }
    }
}
