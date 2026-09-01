using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Welco.Shared.Enums;
using Welco.Shared.Localization;
using Welco.Shared.Localization.Interfaces;
using Welco.Shared.Results;

namespace Welco.Shared.Controllers
{
    [ApiController]
    public abstract class AppControllerBase : ControllerBase
    {
        protected readonly IMediator _mediator;


        protected AppControllerBase(IMediator mediator)
        {
            _mediator = mediator;
        }

        [NonAction]
        public string Localize(string key, params object[] args)
        {
            var provider = HttpContext.RequestServices.GetService<ILocalizationProvider>();
            if (provider == null) return key;

            var culture = GetRequestCulture();
            return provider.GetLocalizedString(key, culture, args);
        }

        [NonAction]
        protected string GetRequestCulture()
        {
            var req = HttpContext?.Request;
            if (req != null)
            {
                var headers = req.Headers;
                var hCulture = headers["Accept-Language"].FirstOrDefault()
                               ?? headers["Language"].FirstOrDefault()
                               ?? headers["language"].FirstOrDefault()
                               ?? headers["Accept_Language"].FirstOrDefault()
                               ?? headers["X-Language"].FirstOrDefault()
                               ?? headers["x-language"].FirstOrDefault()
                               ?? headers["Culture"].FirstOrDefault()
                               ?? headers["culture"].FirstOrDefault()
                               ?? headers["X-Culture"].FirstOrDefault()
                               ?? headers["Lang"].FirstOrDefault()
                               ?? headers["lang"].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(hCulture))
                {
                    return AppLanguageExtensions.FromCode(hCulture).ToCode();
                }

                var qCulture = req.Query["culture"].FirstOrDefault() 
                               ?? req.Query["ui-culture"].FirstOrDefault() 
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

        [NonAction]
        public IActionResult CustomResult<T>(Result<T> result)
        {
            var localizedMessage = !string.IsNullOrWhiteSpace(result.Message)
                ? Localize(result.Message)
                : result.Message;

            var localizedErrors = result.Errors?
                .Select(e => !string.IsNullOrWhiteSpace(e) ? Localize(e) : e)
                .ToList();

            var resultType = result.GetType();
            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(PaginatedResult<>))
            {
                return new ObjectResult(result)
                {
                    StatusCode = result.StatusCode
                };
            }

            var localizedResult = new Result<T>(
                result.IsSuccess,
                result.StatusCode,
                localizedMessage,
                result.Data,
                localizedErrors);

            return new ObjectResult(localizedResult)
            {
                StatusCode = result.StatusCode
            };
        }

        [NonAction]
        public IActionResult ToActionResult<T>(Result<T> result)
        {
            return CustomResult(result);
        }

        [NonAction]
        public IActionResult Paginated<T>(PaginatedResult<T> result)
        {
            var localizedMessage = !string.IsNullOrWhiteSpace(result.Message)
                ? Localize(result.Message)
                : result.Message;

            var localizedErrors = result.Errors?
                .Select(e => !string.IsNullOrWhiteSpace(e) ? Localize(e) : e)
                .ToList();

            var localizedResult = new PaginatedResult<T>(
                result.IsSuccess,
                result.Data,
                result.TotalCount,
                result.PageNumber,
                result.PageSize,
                localizedMessage,
                result.StatusCode,
                localizedErrors);

            return new ObjectResult(localizedResult)
            {
                StatusCode = result.StatusCode
            };
        }

        [NonAction]
        public IActionResult ToActionResult<T>(PaginatedResult<T> result)
        {
            return Paginated(result);
        }

        [NonAction]
        public IActionResult PaginatedSuccess<T>(
            IReadOnlyList<T> data,
            int totalCount,
            int pageNumber,
            int pageSize,
            string? message = null)
        {
            var msg = message ?? Localize(LocalizationKeys.ActionResults.Ok);
            var paginated = PaginatedResult<T>.Success(data, totalCount, pageNumber, pageSize, msg);
            return Paginated(paginated);
        }

        [NonAction]
        public IActionResult Success<T>(T data, string? message = null, int statusCode = StatusCodes.Status200OK)
        {
            var msg = message ?? Localize(LocalizationKeys.ActionResults.Ok);
            return CustomResult(Result<T>.Success(data, msg, statusCode));
        }

        [NonAction]
        public IActionResult Success(string? message = null, int statusCode = StatusCodes.Status200OK)
        {
            var msg = message ?? Localize(LocalizationKeys.ActionResults.Ok);
            return CustomResult(Result<object?>.Success(null, msg, statusCode));
        }

        [NonAction]
        public IActionResult CreatedResult<T>(T data, string? message = null, int statusCode = StatusCodes.Status201Created)
        {
            var msg = message ?? Localize(LocalizationKeys.ActionResults.Created);
            return CustomResult(Result<T>.Created(data, msg, statusCode));
        }

        [NonAction]
        public IActionResult CreatedResult(string? message = null, int statusCode = StatusCodes.Status201Created)
        {
            var msg = message ?? Localize(LocalizationKeys.ActionResults.Created);
            return CustomResult(Result<object?>.Created(null, msg, statusCode));
        }

        [NonAction]
        public IActionResult Failure<T>(string error, int statusCode = StatusCodes.Status400BadRequest)
        {
            var localizedError = Localize(error);
            return CustomResult(Result<T>.Failure(localizedError, statusCode));
        }

        [NonAction]
        public IActionResult Failure<T>(List<string> errors, string? message = null, int statusCode = StatusCodes.Status400BadRequest)
        {
            var msg = message ?? Localize(LocalizationKeys.ExceptionMessages.Validation);
            var localizedErrors = errors.Select(e => Localize(e)).ToList();
            return CustomResult(Result<T>.Failure(localizedErrors, msg, statusCode));
        }

        [NonAction]
        public IActionResult Failure(string error, int statusCode = StatusCodes.Status400BadRequest)
        {
            var localizedError = Localize(error);
            return CustomResult(Result<object?>.Failure(localizedError, statusCode));
        }

        [NonAction]
        public IActionResult Failure(List<string> errors, string? message = null, int statusCode = StatusCodes.Status400BadRequest)
        {
            var msg = message ?? Localize(LocalizationKeys.ExceptionMessages.Validation);
            var localizedErrors = errors.Select(e => Localize(e)).ToList();
            return CustomResult(Result<object?>.Failure(localizedErrors, msg, statusCode));
        }

        [NonAction]
        public IActionResult NotFoundResult<T>(string? message = null, List<string>? errors = null)
        {
            var msg = message ?? Localize(LocalizationKeys.ExceptionMessages.NotFound);
            return CustomResult(Result<T>.NotFound(msg, errors));
        }

        [NonAction]
        public IActionResult NotFoundResult(string? message = null, List<string>? errors = null)
        {
            var msg = message ?? Localize(LocalizationKeys.ExceptionMessages.NotFound);
            return CustomResult(Result<object?>.NotFound(msg, errors));
        }

        [NonAction]
        public IActionResult UnauthorizedResult<T>(string? message = null, List<string>? errors = null)
        {
            var msg = message ?? Localize(LocalizationKeys.ExceptionMessages.Unauthorized);
            return CustomResult(Result<T>.Unauthorized(msg, errors));
        }

        [NonAction]
        public IActionResult UnauthorizedResult(string? message = null, List<string>? errors = null)
        {
            var msg = message ?? Localize(LocalizationKeys.ExceptionMessages.Unauthorized);
            return CustomResult(Result<object?>.Unauthorized(msg, errors));
        }

        [NonAction]
        public IActionResult ForbiddenResult<T>(string? message = null, List<string>? errors = null)
        {
            var msg = message ?? Localize(LocalizationKeys.ExceptionMessages.Forbidden);
            return CustomResult(Result<T>.Forbidden(msg, errors));
        }

        [NonAction]
        public IActionResult ForbiddenResult(string? message = null, List<string>? errors = null)
        {
            var msg = message ?? Localize(LocalizationKeys.ExceptionMessages.Forbidden);
            return CustomResult(Result<object?>.Forbidden(msg, errors));
        }

        [NonAction]
        public IActionResult ConflictResult<T>(string? message = null, List<string>? errors = null)
        {
            var msg = message ?? Localize(LocalizationKeys.ExceptionMessages.Conflict);
            return CustomResult(Result<T>.Conflict(msg, errors));
        }

        [NonAction]
        public IActionResult ConflictResult(string? message = null, List<string>? errors = null)
        {
            var msg = message ?? Localize(LocalizationKeys.ExceptionMessages.Conflict);
            return CustomResult(Result<object?>.Conflict(msg, errors));
        }

        [NonAction]
        public IActionResult ServerErrorResult<T>(string? message = null, List<string>? errors = null)
        {
            var msg = message ?? Localize(LocalizationKeys.ExceptionMessages.InternalServerError);
            return CustomResult(Result<T>.ServerError(msg, errors));
        }

        [NonAction]
        public IActionResult ServerErrorResult(string? message = null, List<string>? errors = null)
        {
            var msg = message ?? Localize(LocalizationKeys.ExceptionMessages.InternalServerError);
            return CustomResult(Result<object?>.ServerError(msg, errors));
        }
    }
}
