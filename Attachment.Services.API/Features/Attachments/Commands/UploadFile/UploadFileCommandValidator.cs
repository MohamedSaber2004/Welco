using FluentValidation;
using Welco.Shared.Localization;

namespace Attachment.Services.API.Features.Attachments.Commands.UploadFile
{
    public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
    {
        public UploadFileCommandValidator()
        {
            RuleFor(x => x.File)
                .NotNull().WithMessage(LocalizationKeys.AttachmentMessages.FileEmpty)
                .NotEmpty().WithMessage(LocalizationKeys.AttachmentMessages.FileEmpty);

            RuleFor(x => x.Place)
                .InclusiveBetween(0, 2).WithMessage(LocalizationKeys.AttachmentMessages.InvalidPlace);

            RuleFor(x => x.FileType)
                .IsInEnum().WithMessage(LocalizationKeys.AttachmentMessages.InvalidFileType);
        }
    }
}
