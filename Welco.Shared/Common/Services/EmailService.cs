using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;
using Welco.Shared.Localization;
using Welco.Shared.Localization.Interfaces;

namespace Welco.Shared.Common.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILocalizationProvider _localizer;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILocalizationProvider localizer,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
        {
            try
            {
                var fromEmail = !string.IsNullOrWhiteSpace(_emailSettings.Email)
                    ? _emailSettings.Email
                    : (!string.IsNullOrWhiteSpace(_emailSettings.Username) ? _emailSettings.Username : null);

                if (string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(_emailSettings.Host))
                {
                    _logger.LogWarning("Email sending skipped: EmailSettings is not configured (Email/Username or Host is empty). Target: {ToEmail}", toEmail);
                    return;
                }

                var fromName = string.IsNullOrWhiteSpace(_emailSettings.Name) ? "Welco" : _emailSettings.Name;

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                var builder = new BodyBuilder();
                if (isHtml)
                {
                    builder.HtmlBody = body;
                }
                else
                {
                    builder.TextBody = body;
                }
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                client.Timeout = 15000;
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                var secureSocketOptions = _emailSettings.Port switch
                {
                    587 => SecureSocketOptions.StartTls,
                    465 => SecureSocketOptions.SslOnConnect,
                    25 => SecureSocketOptions.None,
                    _ => SecureSocketOptions.Auto
                };

                await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, secureSocketOptions, cancellationToken);

                if (!string.IsNullOrWhiteSpace(_emailSettings.Username) && !string.IsNullOrWhiteSpace(_emailSettings.Password))
                {
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password, cancellationToken);
                }

                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Email successfully sent to {ToEmail} with subject '{Subject}'", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail} with subject '{Subject}'", toEmail, subject);
            }
        }

        public async Task SendVerificationEmailAsync(string toEmail, string otpCode, string? culture = null, CancellationToken cancellationToken = default)
        {
            var subject = _localizer.GetLocalizedString(LocalizationKeys.Auth.OtpEmailSubject, culture);
            var bodyText = _localizer.GetLocalizedString(LocalizationKeys.Auth.OtpEmailBody, culture, otpCode);
            var expiryMinutes = _emailSettings.VerificationCodeExpiryMinutes > 0 ? _emailSettings.VerificationCodeExpiryMinutes : 10;

            var htmlBody = $@"<!DOCTYPE html>
            <html>
            <head>
                <meta charset=""utf-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>{subject}</title>
            </head>
            <body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; margin: 0; padding: 20px;"">
                <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);"">
                    <tr>
                        <td style=""background-color: #2563eb; padding: 24px; text-align: center;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 600;"">Welco</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 32px 24px;"">
                            <h2 style=""color: #1f2937; margin-top: 0; font-size: 20px;"">{subject}</h2>
                            <p style=""color: #4b5563; font-size: 16px; line-height: 1.5;"">{bodyText}</p>
                            <div style=""text-align: center; margin: 32px 0;"">
                                <span style=""display: inline-block; font-size: 32px; font-weight: bold; letter-spacing: 6px; color: #2563eb; padding: 14px 28px; background-color: #eff6ff; border: 1px dashed #93c5fd; border-radius: 8px;"">
                                    {otpCode}
                                </span>
                            </div>
                            <p style=""color: #6b7280; font-size: 14px; line-height: 1.4; text-align: center;"">
                                This verification code will expire in {expiryMinutes} minutes.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f9fafb; padding: 16px 24px; text-align: center; border-top: 1px solid #e5e7eb;"">
                            <p style=""color: #9ca3af; font-size: 12px; margin: 0;"">
                                &copy; {DateTime.UtcNow.Year} Welco. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";

            await SendEmailAsync(toEmail, subject, htmlBody, isHtml: true, cancellationToken);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string otpCode, string? culture = null, CancellationToken cancellationToken = default)
        {
            var subject = _localizer.GetLocalizedString(LocalizationKeys.Auth.OtpEmailSubject, culture);
            var bodyText = _localizer.GetLocalizedString(LocalizationKeys.Auth.OtpEmailBody, culture, otpCode);
            var expiryMinutes = _emailSettings.VerificationCodeExpiryMinutes > 0 ? _emailSettings.VerificationCodeExpiryMinutes : 10;

            var htmlBody = $@"<!DOCTYPE html>
            <html>
            <head>
                <meta charset=""utf-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>{subject}</title>
            </head>
            <body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; margin: 0; padding: 20px;"">
                <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);"">
                    <tr>
                        <td style=""background-color: #dc2626; padding: 24px; text-align: center;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 600;"">Welco</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 32px 24px;"">
                            <h2 style=""color: #1f2937; margin-top: 0; font-size: 20px;"">Password Reset Request</h2>
                            <p style=""color: #4b5563; font-size: 16px; line-height: 1.5;"">{bodyText}</p>
                            <div style=""text-align: center; margin: 32px 0;"">
                                <span style=""display: inline-block; font-size: 32px; font-weight: bold; letter-spacing: 6px; color: #dc2626; padding: 14px 28px; background-color: #fef2f2; border: 1px dashed #fca5a5; border-radius: 8px;"">
                                    {otpCode}
                                </span>
                            </div>
                            <p style=""color: #6b7280; font-size: 14px; line-height: 1.4; text-align: center;"">
                                This code will expire in {expiryMinutes} minutes. If you did not request a password reset, please ignore this email.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f9fafb; padding: 16px 24px; text-align: center; border-top: 1px solid #e5e7eb;"">
                            <p style=""color: #9ca3af; font-size: 12px; margin: 0;"">
                                &copy; {DateTime.UtcNow.Year} Welco. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";

            await SendEmailAsync(toEmail, subject, htmlBody, isHtml: true, cancellationToken);
        }
    }
}
