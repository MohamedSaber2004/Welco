using FluentValidation;
using Microsoft.Extensions.Localization;
using Welco.Shared.Localization;

namespace Attachment.Services.API.Features.Attachments.Commands.DownloadFile
{
    public class DownloadFileCommandValidator : AbstractValidator<DownloadFileCommand>
    {
        public DownloadFileCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage(LocalizationKeys.AttachmentMessages.FileNotFound)
                .NotNull().WithMessage(LocalizationKeys.AttachmentMessages.FileNotFound);

            RuleFor(x => x.Place)
                .InclusiveBetween(0, 12).WithMessage(LocalizationKeys.AttachmentMessages.InvalidPlace);
        }
    }
}
