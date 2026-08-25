using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Scalar.AspNetCore;
using Serilog;
using System.Reflection;
using Welco.API.Options;
using Welco.Shared.Enums;
using Welco.Shared.Localization;
using Welco.Shared.Localization.Interfaces;
using Welco.Shared.OpenApi;

namespace Welco.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var env = builder.Environment;
            var port = Environment.GetEnvironmentVariable("PORT") 
                       ?? Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS");
            if (!string.IsNullOrEmpty(port))
            {
                builder.WebHost.UseUrls($"http://*:{port}");
            }

            builder.Configuration.Sources.Clear();
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            if (env.IsDevelopment() || env.EnvironmentName == "Test")
            {
                var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                if (appAssembly != null) builder.Configuration.AddUserSecrets(appAssembly, optional: true);
            }

            builder.Configuration.AddEnvironmentVariables().AddCommandLine(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateBootstrapLogger();

            Log.Information("Welco API Gateway is starting up at {Time}", DateTime.Now);
            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());

            var ocelotDir = Path.Combine(builder.Environment.ContentRootPath, "Ocelot");
            if (Directory.Exists(ocelotDir))
            {
                builder.Configuration
                    .SetBasePath(ocelotDir)
                    .AddJsonFile("ocelot.global.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"ocelot.global.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                foreach (var file in Directory.GetFiles(ocelotDir, $"*.{env.EnvironmentName}.json"))
                {
                    if (!Path.GetFileName(file).StartsWith("ocelot.global."))
                    {
                        builder.Configuration.AddJsonFile(file, optional: false, reloadOnChange: true);
                    }
                }
            }

            builder.Services.AddControllers();
            builder.Services.AddJsonLocalization();

            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("GatewayCorsPolicy", policy =>
                {
                    if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    }
                    else
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                });
            });

            builder.Services.Configure<RateLimitingOptions>(builder.Configuration.GetSection(RateLimitingOptions.SectionName));
            var rateLimitSettings = builder.Configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>() ?? new RateLimitingOptions();

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, token) =>
                {
                    var localizer = context.HttpContext.RequestServices.GetService<ILocalizationProvider>();
                    var message = localizer?.GetLocalizedString(LocalizationKeys.Auth.TooManyAttempts);

                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        isSuccess = false,
                        statusCode = 429,
                        message = message,
                        errors = new[] { message },
                        data = (object?)null
                    }, cancellationToken: token);
                };

                options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var clientIp = httpContext.Connection.RemoteIpAddress?.ToString()
                                   ?? httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                                   ?? "anonymous";

                    return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: clientIp,
                        factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitSettings.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateLimitSettings.WindowSeconds),
                            QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                            QueueLimit = rateLimitSettings.QueueLimit
                        });
                });
            });

            builder.Services.AddOcelot(builder.Configuration);
            builder.Services.AddConfiguredOpenApi();

            var app = builder.Build();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | 
                                   Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            });

            app.UseJsonLocalization();

            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                context.Response.Headers.Remove("Server");
                context.Response.Headers.Remove("X-Powered-By");

                if (context.Request.IsHttps)
                {
                    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
                }

                var headers = context.Request.Headers;
                var langHeader = headers["Accept-Language"].FirstOrDefault()
                                 ?? headers["Language"].FirstOrDefault()
                                 ?? headers["language"].FirstOrDefault()
                                 ?? headers["X-Language"].FirstOrDefault()
                                 ?? headers["Culture"].FirstOrDefault()
                                 ?? headers["Lang"].FirstOrDefault()
                                 ?? context.Request.Query["culture"].FirstOrDefault()
                                 ?? context.Request.Query["lang"].FirstOrDefault()
                                 ?? context.Request.Query["language"].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(langHeader))
                {
                    var normalizedLang = AppLanguageExtensions.FromCode(langHeader).ToCode();
                    context.Request.Headers["Accept-Language"] = normalizedLang;
                    context.Request.Headers["Language"] = normalizedLang;
                }

                await next();
            });

            if (!app.Environment.IsEnvironment("Test") && !app.Environment.IsProduction())
            {
                app.UseHttpsRedirection();
            }
            app.UseRouting();
            app.UseCors("GatewayCorsPolicy");
            app.UseRateLimiter();

            var microserviceDocRoutes = new List<(string Name, string Route)>();
            if (Directory.Exists(ocelotDir))
            {
                foreach (var file in Directory.GetFiles(ocelotDir, $"ocelot.*.{env.EnvironmentName}.json"))
                {
                    var fileName = Path.GetFileName(file);
                    if (!fileName.StartsWith("ocelot.global."))
                    {
                        var parts = fileName.Split('.');
                        if (parts.Length >= 3)
                        {
                            var serviceName = parts[1]; 
                            var displayName = char.ToUpper(serviceName[0]) + serviceName.Substring(1) + " Microservice";
                            microserviceDocRoutes.Add((displayName, $"/api/docs/{serviceName}/openapi.json"));
                        }
                    }
                }
            }

            app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Welco.Gateway.API" }));
            app.MapGet("/", () => Results.Redirect("/scalar/v1"));
            app.MapOpenApi();
            
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("Welco Microservices Platform")
                       .WithTheme(ScalarTheme.Moon);

                if (microserviceDocRoutes.Count > 0)
                {
                    options.WithOpenApiRoutePattern(microserviceDocRoutes[0].Route);
                }
            });
            app.MapControllers();

            app.MapWhen(
                context => context.Request.Path.StartsWithSegments("/api"),
                subApp => subApp.UseOcelot().Wait()
            );

            await app.RunAsync();
        }
    }
}
