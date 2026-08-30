using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Welco.Shared.Localization
{
    public class JsonStringLocalizer<T> : IStringLocalizer<T>
    {
        private readonly IStringLocalizer _localizer;

        public JsonStringLocalizer(IStringLocalizerFactory factory)
        {
            _localizer = factory.Create(typeof(T));
        }

        public LocalizedString this[string name] => _localizer[name];

        public LocalizedString this[string name, params object[] arguments] => _localizer[name, arguments];

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return _localizer.GetAllStrings(includeParentCultures);
        }
    }
}
