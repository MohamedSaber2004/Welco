using Microsoft.AspNetCore.Http;
using Welco.Shared.Common.Interfaces;

namespace Welco.Shared.Common.Exceptions
{
    public abstract class BaseCustomException : Exception, ILocalizedException
    {
        public string LocalizationKey { get; }
        public object[]? Args { get; }
        public int StatusCode { get; }

        protected BaseCustomException(string localizationKey, int statusCode = StatusCodes.Status400BadRequest, params object[] args)
            : base(localizationKey)
        {
            LocalizationKey = localizationKey;
            StatusCode = statusCode;
            Args = args.Length > 0 ? args : null;
        }

        protected BaseCustomException(string localizationKey, Exception innerException, int statusCode = StatusCodes.Status400BadRequest, params object[] args)
            : base(localizationKey, innerException)
        {
            LocalizationKey = localizationKey;
            StatusCode = statusCode;
            Args = args.Length > 0 ? args : null;
        }
    }
}
