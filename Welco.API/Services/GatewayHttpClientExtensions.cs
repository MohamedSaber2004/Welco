namespace Welco.API.Services
{
    public static class GatewayHttpClientExtensions
    {
        public const string InsecureClientName = "InsecureClient";

        public static IServiceCollection AddGatewayOpenApiHttpClient(this IServiceCollection services)
        {
            services.AddHttpClient(InsecureClientName, client =>
                {
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
            return services;
        }
    }
}
