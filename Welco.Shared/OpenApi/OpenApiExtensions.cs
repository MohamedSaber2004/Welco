using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;

namespace Welco.Shared.OpenApi
{
    public static class OpenApiExtensions
    {
        public static IServiceCollection AddConfiguredOpenApi(this IServiceCollection services)
        {
            services.AddOpenApi(options =>
            {
                options.AddOperationTransformer<AcceptLanguageHeaderTransformer>();
            });

            return services;
        }
    }
}
