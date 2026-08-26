using Microsoft.AspNetCore.Http;
using Welco.Shared.Localization;

namespace Welco.Shared.Common.Exceptions
{
    public class BadRequestException : BaseCustomException
    {
        public IDictionary<string, string[]> Errors { get; }

        public BadRequestException()
            : base(LocalizationKeys.ExceptionMessages.BadRequest, StatusCodes.Status400BadRequest)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public BadRequestException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status400BadRequest, args)
        {
            Errors = new Dictionary<string, string[]> { { string.Empty, new[] { localizationKey } } };
        }

        public BadRequestException(string[] errors, string localizationKey = LocalizationKeys.ExceptionMessages.BadRequest)
            : base(localizationKey, StatusCodes.Status400BadRequest)
        {
            Errors = new Dictionary<string, string[]> { { string.Empty, errors } };
        }

        public BadRequestException(IDictionary<string, string[]> errors, string localizationKey = LocalizationKeys.ExceptionMessages.BadRequest)
            : base(localizationKey, StatusCodes.Status400BadRequest)
        {
            Errors = errors;
        }

        public BadRequestException(string localizationKey, Exception innerException, params object[] args)
            : base(localizationKey, innerException, StatusCodes.Status400BadRequest, args)
        {
            Errors = new Dictionary<string, string[]> { { string.Empty, new[] { localizationKey } } };
        }
    }
}
