namespace Welco.Shared.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
        Task SendVerificationEmailAsync(string toEmail, string otpCode, string? culture = null, CancellationToken cancellationToken = default);
        Task SendPasswordResetEmailAsync(string toEmail, string otpCode, string? culture = null, CancellationToken cancellationToken = default);
    }
}
