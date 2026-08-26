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
                case ValidationException customValEx:
                    statusCode = customValEx.StatusCode;
                    message = Localize(customValEx.LocalizationKey, customValEx.Args);
                    if (customValEx.Errors.Any())
                    {
                        foreach (var kvp in customValEx.Errors)
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

                case FluentValidation.ValidationException fvEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = Localize(LocalizationKeys.ExceptionMessages.Validation);
                    if (fvEx.Errors.Any())
                    {
                        foreach (var err in fvEx.Errors)
                        {
                            errors.Add(Localize(err.ErrorMessage));
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

                case NotFoundException notFoundEx:
                    statusCode = notFoundEx.StatusCode;
                    message = Localize(notFoundEx.LocalizationKey, notFoundEx.Args);
                    errors.Add(message);
                    break;

                case KeyNotFoundException keyNotFoundEx:
                    statusCode = StatusCodes.Status404NotFound;
                    message = Localize(LocalizationKeys.ExceptionMessages.NotFound);
                    errors.Add(!string.IsNullOrWhiteSpace(keyNotFoundEx.Message) ? keyNotFoundEx.Message : message);
                    break;

                case UnAuthorizedException unAuthEx:
                    statusCode = unAuthEx.StatusCode;
                    message = Localize(unAuthEx.LocalizationKey, unAuthEx.Args);
                    errors.Add(message);
                    break;

                case UnauthorizedAccessException unAuthAccessEx:
                    statusCode = StatusCodes.Status401Unauthorized;
                    message = Localize(LocalizationKeys.ExceptionMessages.Unauthorized);
                    errors.Add(!string.IsNullOrWhiteSpace(unAuthAccessEx.Message) ? unAuthAccessEx.Message : message);
                    break;

                case ForbiddenException forbiddenEx:
                    statusCode = forbiddenEx.StatusCode;
                    message = Localize(forbiddenEx.LocalizationKey, forbiddenEx.Args);
                    errors.Add(message);
                    break;

                case ConflictException conflictEx:
                    statusCode = conflictEx.StatusCode;
                    message = Localize(conflictEx.LocalizationKey, conflictEx.Args);
                    errors.Add(message);
                    break;

                case ArgumentException argEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = Localize(LocalizationKeys.ExceptionMessages.BadRequest);
                    errors.Add(!string.IsNullOrWhiteSpace(argEx.Message) ? argEx.Message : message);
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
