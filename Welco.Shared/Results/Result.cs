using System.Net;
using Welco.Shared.Localization;

namespace Welco.Shared.Results
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public T? Data { get; set; }

        public Result() { }

        public Result(bool isSuccess, int statusCode, string message, T? data = default, List<string>? errors = null)
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Errors = errors ?? new List<string>();
        }

        public static Result<T> Success(T data, string message = LocalizationKeys.ActionResults.Ok, int statusCode = (int)HttpStatusCode.OK)
        {
            return new Result<T>(true, statusCode, message, data);
        }

        public static Result<T> Created(T data, string message = LocalizationKeys.ActionResults.Created, int statusCode = (int)HttpStatusCode.Created)
        {
            return new Result<T>(true, statusCode, message, data);
        }

        public static Result<T> Failure(string error, int statusCode = (int)HttpStatusCode.BadRequest)
        {
            return new Result<T>(false, statusCode, error, default, new List<string> { error });
        }

        public static Result<T> Failure(List<string> errors, string message = LocalizationKeys.ExceptionMessages.Validation, int statusCode = (int)HttpStatusCode.BadRequest)
        {
            return new Result<T>(false, statusCode, message, default, errors);
        }

        public static Result<T> NotFound(string message = LocalizationKeys.ExceptionMessages.NotFound, List<string>? errors = null)
        {
            return new Result<T>(false, (int)HttpStatusCode.NotFound, message, default, errors ?? new List<string> { message });
        }

        public static Result<T> Unauthorized(string message = LocalizationKeys.ExceptionMessages.Unauthorized, List<string>? errors = null)
        {
            return new Result<T>(false, (int)HttpStatusCode.Unauthorized, message, default, errors ?? new List<string> { message });
        }

        public static Result<T> Forbidden(string message = LocalizationKeys.ExceptionMessages.Forbidden, List<string>? errors = null)
        {
            return new Result<T>(false, (int)HttpStatusCode.Forbidden, message, default, errors ?? new List<string> { message });
        }

        public static Result<T> Conflict(string message = LocalizationKeys.ExceptionMessages.Conflict, List<string>? errors = null)
        {
            return new Result<T>(false, (int)HttpStatusCode.Conflict, message, default, errors ?? new List<string> { message });
        }

        public static Result<T> ServerError(string message = LocalizationKeys.ExceptionMessages.InternalServerError, List<string>? errors = null)
        {
            return new Result<T>(false, (int)HttpStatusCode.InternalServerError, message, default, errors ?? new List<string> { message });
        }
    }
}
