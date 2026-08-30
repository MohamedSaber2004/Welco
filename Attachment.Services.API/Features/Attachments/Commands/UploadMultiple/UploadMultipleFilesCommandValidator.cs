using FluentValidation;
using Microsoft.Extensions.Localization;
using Welco.Shared.Localization;

namespace Attachment.Services.API.Features.Attachments.Commands.UploadMultiple
{
    public class UploadMultipleFilesCommandValidator : AbstractValidator<UploadMultipleFilesCommand>
    {
        public UploadMultipleFilesCommandValidator(IStringLocalizer<Messages> localizer)
        {
            // Check that at least one category has files
            RuleFor(x => x)
                .Must(x => (x.Images != null && x.Images.Any()) ||
                           (x.Videos != null && x.Videos.Any()) ||
                           (x.Audios != null && x.Audios.Any()) ||
                           (x.Documents != null && x.Documents.Any()))
                .WithMessage(LocalizationKeys.AttachmentMessages.FileEmpty);

            RuleFor(x => x.ImagesPlace)
                .InclusiveBetween(0, 12).WithMessage(LocalizationKeys.AttachmentMessages.InvalidPlace);

            RuleFor(x => x.VideosPlace)
                .InclusiveBetween(0, 12).WithMessage(LocalizationKeys.AttachmentMessages.InvalidPlace);

            RuleFor(x => x.AudiosPlace)
                .InclusiveBetween(0, 12).WithMessage(LocalizationKeys.AttachmentMessages.InvalidPlace);

            RuleFor(x => x.DocumentsPlace)
                .InclusiveBetween(0, 12).WithMessage(LocalizationKeys.AttachmentMessages.InvalidPlace);
        }
    }
}
