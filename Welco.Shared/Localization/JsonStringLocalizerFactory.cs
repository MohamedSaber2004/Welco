using Microsoft.Extensions.Localization;
using Welco.Shared.Localization.Interfaces;

namespace Welco.Shared.Localization
{
    public class JsonStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ILocalizationProvider _provider;

        public JsonStringLocalizerFactory(ILocalizationProvider provider)
        {
            _provider = provider;
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            return new JsonStringLocalizer(_provider);
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return new JsonStringLocalizer(_provider);
        }
    }
}