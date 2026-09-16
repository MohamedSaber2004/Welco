namespace Welco.API.Services
{
    public static class GatewayHttpClientExtensions
    {
        public const string InsecureClientName = "InsecureClient";

        public static IServiceCollection AddGatewayOpenApiHttpClient(this IServiceCollection services)
        {
            services.AddHttpClient(InsecureClientName, client =>
                {
                    // Keep infinite at the HttpClient level; per-request timeouts are enforced
                    // via a linked CancellationTokenSource in FetchOpenApiWithCacheAsync.
                    // Setting a finite Timeout here as a safety net ensures that even if the
                    // OS-level TCP socket error (10060, ~20 s on Windows) fires before the
                    // CancelAfter() window, the HttpClient framework can still abort the call.
                    client.Timeout = TimeSpan.FromSeconds(22);
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
            return services;
        }
    }
}
