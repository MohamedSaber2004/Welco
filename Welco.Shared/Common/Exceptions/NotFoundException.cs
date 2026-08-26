using Microsoft.AspNetCore.Http;
using Welco.Shared.Localization;

namespace Welco.Shared.Common.Exceptions
{
    public class NotFoundException : BaseCustomException
    {
        public NotFoundException()
            : base(LocalizationKeys.ExceptionMessages.NotFound, StatusCodes.Status404NotFound)
        {
        }

        public NotFoundException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status404NotFound, args)
        {
        }

        public NotFoundException(string localizationKey, Exception innerException, params object[] args)
            : base(localizationKey, innerException, StatusCodes.Status404NotFound, args)
        {
        }

        public NotFoundException(string entityName, object key)
            : base(LocalizationKeys.ExceptionMessages.NotFound, StatusCodes.Status404NotFound, entityName, key)
        {
        }
    }
}
