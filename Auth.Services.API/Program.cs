using Welco.Shared.Localization;

namespace Auth.Services.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapOpenApi();
            app.MapControllers();

            app.Run();
        }
    }
}
