using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentByIdQueryValidator : AbstractValidator<GetDocumentByIdQuery>
    {
        public GetDocumentByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(LocalizationKeys.Document.DocumentIdRequired);
        }
    }
}
