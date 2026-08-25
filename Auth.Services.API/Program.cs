using Welco.Shared.Localization;

namespace Auth.Services.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var port = Environment.GetEnvironmentVariable("PORT") 
                       ?? Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS");
            if (!string.IsNullOrEmpty(port))
            {
                builder.WebHost.UseUrls($"http://*:{port}");
            }

            builder.Services.AddControllers();
            builder.Services.AddJsonLocalization();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | 
                                   Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            });

            app.UseJsonLocalization();

            if (!app.Environment.IsEnvironment("Test") && !app.Environment.IsProduction())
            {
                app.UseHttpsRedirection();
            }
            app.UseAuthorization();
            app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Auth.Services.API" }));
            app.MapOpenApi();
            app.MapControllers();

            app.Run();
        }
    }
}
