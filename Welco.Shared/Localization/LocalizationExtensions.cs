using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Welco.Shared.Enums;
using Welco.Shared.Localization.Interfaces;

namespace Welco.Shared.Localization
{
    public static class LocalizationExtensions
    {
        public static readonly string[] SupportedCultures = { "en", "ar", "ar-SA", "ar-EG", "ar-AE", "en-US", "en-GB" };

        public static IServiceCollection AddJsonLocalization(this IServiceCollection services, AppLanguage defaultLanguage = AppLanguage.En)
        {
            services.AddSingleton<ILocalizationProvider, JsonLocalizationProvider>();
            services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            services.AddTransient<IStringLocalizer, JsonStringLocalizer>();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultureInfos = SupportedCultures
                    .Select(c => new CultureInfo(c))
                    .ToList();

                var defaultCode = defaultLanguage.ToCode();
                options.DefaultRequestCulture = new RequestCulture(defaultCode);
                options.SupportedCultures = supportedCultureInfos;
                options.SupportedUICultures = supportedCultureInfos;
                options.FallBackToParentCultures = true;
                options.FallBackToParentUICultures = true;

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new CustomRequestCultureProvider(context =>
                    {
                        var req = context.Request;

                        var queryCulture = req.Query["culture"].FirstOrDefault() 
                                           ?? req.Query["ui-culture"].FirstOrDefault() 
                                           ?? req.Query["lang"].FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(queryCulture))
                        {
                            var lang = AppLanguageExtensions.FromCode(queryCulture).ToCode();
                            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(lang, lang));
                        }

                        var headerCulture = req.Headers["Accept-Language"].FirstOrDefault()
                                            ?? req.Headers["Culture"].FirstOrDefault()
                                            ?? req.Headers["X-Culture"].FirstOrDefault()
                                            ?? req.Headers["Lang"].FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(headerCulture))
                        {
                            var lang = AppLanguageExtensions.FromCode(headerCulture).ToCode();
                            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(lang, lang));
                        }

                        return Task.FromResult<ProviderCultureResult?>(null);
                    }),
                    new QueryStringRequestCultureProvider { QueryStringKey = "culture", UIQueryStringKey = "ui-culture" },
                    new AcceptLanguageHeaderRequestCultureProvider(),
                    new CookieRequestCultureProvider()
                };
            });

            return services;
        }

        public static IApplicationBuilder UseJsonLocalization(this IApplicationBuilder app)
        {
            return app.UseRequestLocalization();
        }
    }
}
