using Microsoft.Extensions.Localization;
using System.Globalization;
using Welco.Shared.Localization.Interfaces;

namespace Welco.Shared.Localization
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly ILocalizationProvider _provider;
        private readonly string? _culture;

        public JsonStringLocalizer(ILocalizationProvider provider, string? culture = null)
        {
            _provider = provider;
            _culture = culture;
        }

        public LocalizedString this[string name]
        {
            get
            {
                var value = _provider.GetLocalizedString(name, _culture);
                return new LocalizedString(name, value ?? name, value == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var value = _provider.GetLocalizedString(name, _culture, arguments);
                return new LocalizedString(name, value ?? name, value == null);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return Enumerable.Empty<LocalizedString>();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return new JsonStringLocalizer(_provider, culture.Name);
        }
    }
}