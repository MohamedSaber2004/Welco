using FluentValidation;
using Microsoft.Extensions.Localization;
using Welco.Shared.Localization;

namespace Attachment.Services.API.Features.Attachments.Commands.UpdateFile
{
    public class UpdateFileCommandValidator : AbstractValidator<UpdateFileCommand>
    {
        public UpdateFileCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.File)
                .NotEmpty().WithMessage(LocalizationKeys.AttachmentMessages.FileEmpty)
                .NotNull().WithMessage(LocalizationKeys.AttachmentMessages.FileEmpty);

            RuleFor(x => x.Place)
                .InclusiveBetween(0, 12).WithMessage(LocalizationKeys.AttachmentMessages.InvalidPlace);

            RuleFor(x => x.FileType)
                .IsInEnum().WithMessage(LocalizationKeys.AttachmentMessages.InvalidFileType);
        }
    }
}
