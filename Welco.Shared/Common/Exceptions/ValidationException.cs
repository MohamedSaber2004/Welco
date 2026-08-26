using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Welco.Shared.Localization;

namespace Welco.Shared.Common.Exceptions
{
    public class ValidationException : BaseCustomException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException()
            : base(LocalizationKeys.ExceptionMessages.Validation, StatusCodes.Status400BadRequest)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status400BadRequest, args)
        {
            Errors = new Dictionary<string, string[]> { { string.Empty, new[] { localizationKey } } };
        }

        public ValidationException(IEnumerable<ValidationFailure> failures, string localizationKey = LocalizationKeys.ExceptionMessages.Validation)
            : base(localizationKey, StatusCodes.Status400BadRequest)
        {
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
        }

        public ValidationException(IDictionary<string, string[]> errors, string localizationKey = LocalizationKeys.ExceptionMessages.Validation)
            : base(localizationKey, StatusCodes.Status400BadRequest)
        {
            Errors = errors;
        }
    }
}
