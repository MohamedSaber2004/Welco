using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Welco.Shared.Common.Exceptions;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Enums;
using Welco.Shared.Localization;
using Welco.Shared.Localization.Interfaces;
using Welco.Shared.Results;

namespace Welco.Shared.Common.Middlewares
{
    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;

        public CustomExceptionHandlerMiddleware(RequestDelegate next, ILogger<CustomExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var localizationProvider = context.RequestServices.GetService<ILocalizationProvider>();
            var culture = GetRequestCulture(context);

            string Localize(string key, params object[]? args)
            {
                if (localizationProvider == null || string.IsNullOrWhiteSpace(key))
                    return key;

                return args != null && args.Length > 0
                    ? localizationProvider.GetLocalizedString(key, culture, args)
                    : localizationProvider.GetLocalizedString(key, culture);
            }

            int statusCode;
            string message;
            List<string> errors = new();

            switch (exception)
            {
                case ValidationException validationEx:
                    statusCode = validationEx.StatusCode;
                    message = Localize(validationEx.LocalizationKey, validationEx.Args);
                    if (validationEx.Errors.Any())
                    {
                        foreach (var kvp in validationEx.Errors)
                        {
                            foreach (var err in kvp.Value)
                            {
                                errors.Add(Localize(err));
                            }
                        }
                    }
                    else
                    {
                        errors.Add(message);
                    }
                    break;

                case BadRequestException badRequestEx:
                    statusCode = badRequestEx.StatusCode;
                    message = Localize(badRequestEx.LocalizationKey, badRequestEx.Args);
                    if (badRequestEx.Errors.Any())
                    {
                        foreach (var kvp in badRequestEx.Errors)
                        {
                            foreach (var err in kvp.Value)
                            {
                                errors.Add(Localize(err));
                            }
                        }
                    }
                    else
                    {
                        errors.Add(message);
                    }
                    break;

                case ILocalizedException localizedEx:
                    statusCode = localizedEx.StatusCode;
                    message = Localize(localizedEx.LocalizationKey, localizedEx.Args);
                    errors.Add(message);
                    break;

                default:
                    _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = Localize(LocalizationKeys.ExceptionMessages.InternalServerError);
                    errors.Add(message);
                    break;
            }

            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.StatusCode = statusCode;

            var response = new Result<object?>(
                isSuccess: false,
                statusCode: statusCode,
                message: message,
                data: null,
                errors: errors);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }

        private static string GetRequestCulture(HttpContext context)
        {
            var req = context?.Request;
            if (req != null)
            {
                var headers = req.Headers;
                var hCulture = headers["Accept-Language"].FirstOrDefault()
                               ?? headers["Language"].FirstOrDefault()
                               ?? headers["language"].FirstOrDefault()
                               ?? headers["Culture"].FirstOrDefault()
                               ?? headers["Lang"].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(hCulture))
                {
                    return AppLanguageExtensions.FromCode(hCulture).ToCode();
                }

                var qCulture = req.Query["culture"].FirstOrDefault()
                               ?? req.Query["lang"].FirstOrDefault()
                               ?? req.Query["language"].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(qCulture))
                {
                    return AppLanguageExtensions.FromCode(qCulture).ToCode();
                }
            }

            var current = System.Globalization.CultureInfo.CurrentUICulture?.Name;
            return AppLanguageExtensions.FromCode(current).ToCode();
        }
    }

    public static class CustomExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CustomExceptionHandlerMiddleware>();
        }
    }
}
