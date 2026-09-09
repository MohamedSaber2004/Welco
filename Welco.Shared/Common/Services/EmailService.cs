using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
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

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(fromName, fromEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;
                email.Body = isHtml
                    ? new TextPart(TextFormat.Html) { Text = body }
                    : new TextPart(TextFormat.Plain) { Text = body };

                using var smtp = new SmtpClient();
                smtp.Timeout = 15000;
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                var socketOptions = _emailSettings.Port switch
                {
                    465 => SecureSocketOptions.SslOnConnect,
                    587 => SecureSocketOptions.StartTls,
                    25 => SecureSocketOptions.None,
                    _ => SecureSocketOptions.Auto
                };

                try
                {
                    await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, socketOptions, cancellationToken);
                }
                catch (Exception connEx) when (_emailSettings.Port == 587)
                {
                    _logger.LogWarning(connEx, "Failed connecting to SMTP host {Host}:587 (STARTTLS). Attempting fallback to port 465 (SSL)...", _emailSettings.Host);
                    await smtp.ConnectAsync(_emailSettings.Host, 465, SecureSocketOptions.SslOnConnect, cancellationToken);
                }
                catch (Exception connEx) when (_emailSettings.Port == 465)
                {
                    _logger.LogWarning(connEx, "Failed connecting to SMTP host {Host}:465 (SSL). Attempting fallback to port 587 (STARTTLS)...", _emailSettings.Host);
                    await smtp.ConnectAsync(_emailSettings.Host, 587, SecureSocketOptions.StartTls, cancellationToken);
                }

                if (!string.IsNullOrWhiteSpace(_emailSettings.Username) && !string.IsNullOrWhiteSpace(_emailSettings.Password))
                {
                    await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password, cancellationToken);
                }

                await smtp.SendAsync(email, cancellationToken);
                await smtp.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Email successfully sent to {ToEmail} with subject '{Subject}'", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail} with subject '{Subject}'. Settings: Host={Host}, Port={Port}, Username={Username}",
                    toEmail, subject, _emailSettings.Host, _emailSettings.Port, _emailSettings.Username);
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
                <meta name=""color-scheme"" content=""light dark"">
                <meta name=""supported-color-schemes"" content=""light dark"">
                <title>{subject}</title>
                <style>
                    @media (prefers-color-scheme: dark) {{
                        .em-page {{ background-color: #1E1F22 !important; }}
                        .em-card {{ background-color: #2B2D31 !important; border: 1px solid #3A3D44 !important; }}
                        .em-title {{ color: #F2F3F5 !important; }}
                        .em-text {{ color: #B5BAC1 !important; }}
                        .em-note {{ color: #949BA4 !important; }}
                        .em-foot {{ background-color: #232428 !important; border-top: 1px solid #3A3D44 !important; }}
                        .em-foottext {{ color: #949BA4 !important; }}
                        .em-band--verify {{ background-color: #5865F2 !important; }}
                        .em-code--verify {{ color: #C7D2FE !important; background-color: #313338 !important; border-color: #5865F2 !important; }}
                    }}
                </style>
            </head>
            <body class=""em-page"" style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; margin: 0; padding: 20px;"">
                <table class=""em-card"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);"">
                    <tr>
                        <td class=""em-band--verify"" style=""background-color: #2563eb; padding: 24px; text-align: center;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 600;"">Welco</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 32px 24px;"">
                            <h2 class=""em-title"" style=""color: #1f2937; margin-top: 0; font-size: 20px;"">{subject}</h2>
                            <p class=""em-text"" style=""color: #4b5563; font-size: 16px; line-height: 1.5;"">{bodyText}</p>
                            <div style=""text-align: center; margin: 32px 0;"">
                                <span class=""em-code--verify"" style=""display: inline-block; font-size: 32px; font-weight: bold; letter-spacing: 6px; color: #2563eb; padding: 14px 28px; background-color: #eff6ff; border: 1px dashed #93c5fd; border-radius: 8px;"">
                                    {otpCode}
                                </span>
                            </div>
                            <p class=""em-note"" style=""color: #6b7280; font-size: 14px; line-height: 1.4; text-align: center;"">
                                This verification code will expire in {expiryMinutes} minutes.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class=""em-foot"" style=""background-color: #f9fafb; padding: 16px 24px; text-align: center; border-top: 1px solid #e5e7eb;"">
                            <p class=""em-foottext"" style=""color: #6b7280; font-size: 12px; margin: 0;"">
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
                <meta name=""color-scheme"" content=""light dark"">
                <meta name=""supported-color-schemes"" content=""light dark"">
                <title>{subject}</title>
                <style>
                    @media (prefers-color-scheme: dark) {{
                        .em-page {{ background-color: #1E1F22 !important; }}
                        .em-card {{ background-color: #2B2D31 !important; border: 1px solid #3A3D44 !important; }}
                        .em-title {{ color: #F2F3F5 !important; }}
                        .em-text {{ color: #B5BAC1 !important; }}
                        .em-note {{ color: #949BA4 !important; }}
                        .em-foot {{ background-color: #232428 !important; border-top: 1px solid #3A3D44 !important; }}
                        .em-foottext {{ color: #949BA4 !important; }}
                        .em-band--reset {{ background-color: #ED4245 !important; }}
                        .em-code--reset {{ color: #FCA5A5 !important; background-color: #313338 !important; border-color: #ED4245 !important; }}
                    }}
                </style>
            </head>
            <body class=""em-page"" style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; margin: 0; padding: 20px;"">
                <table class=""em-card"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);"">
                    <tr>
                        <td class=""em-band--reset"" style=""background-color: #dc2626; padding: 24px; text-align: center;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 600;"">Welco</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 32px 24px;"">
                            <h2 class=""em-title"" style=""color: #1f2937; margin-top: 0; font-size: 20px;"">Password Reset Request</h2>
                            <p class=""em-text"" style=""color: #4b5563; font-size: 16px; line-height: 1.5;"">{bodyText}</p>
                            <div style=""text-align: center; margin: 32px 0;"">
                                <span class=""em-code--reset"" style=""display: inline-block; font-size: 32px; font-weight: bold; letter-spacing: 6px; color: #dc2626; padding: 14px 28px; background-color: #fef2f2; border: 1px dashed #fca5a5; border-radius: 8px;"">
                                    {otpCode}
                                </span>
                            </div>
                            <p class=""em-note"" style=""color: #6b7280; font-size: 14px; line-height: 1.4; text-align: center;"">
                                This code will expire in {expiryMinutes} minutes. If you did not request a password reset, please ignore this email.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class=""em-foot"" style=""background-color: #f9fafb; padding: 16px 24px; text-align: center; border-top: 1px solid #e5e7eb;"">
                            <p class=""em-foottext"" style=""color: #6b7280; font-size: 12px; margin: 0;"">
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
