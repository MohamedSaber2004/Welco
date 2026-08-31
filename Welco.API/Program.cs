using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Scalar.AspNetCore;
using Serilog;
using System.Reflection;
using Welco.API.Options;
using Welco.Shared;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Middlewares;
using Welco.Shared.Common.Options;
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

                // FIX: Multiple AddJsonFile calls with "Routes" arrays overwrite each other (last file wins).
                // Merge all route files into one in-memory JSON so Ocelot sees every route (auth + user-management).
                var routeFiles = Directory.GetFiles(ocelotDir, $"*.{env.EnvironmentName}.json")
                    .Where(f => !Path.GetFileName(f).StartsWith("ocelot.global.", StringComparison.OrdinalIgnoreCase)
                             && !Path.GetFileName(f).StartsWith("ocelot.merged.", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                if (routeFiles.Length > 0)
                {
                    var allRoutes = new List<object?>();
                    foreach (var file in routeFiles)
                    {
                        try
                        {
                            var json = File.ReadAllText(file);
                            using var doc = System.Text.Json.JsonDocument.Parse(json);
                            if (doc.RootElement.TryGetProperty("Routes", out var routes) && routes.ValueKind == System.Text.Json.JsonValueKind.Array)
                            {
                                foreach (var route in routes.EnumerateArray())
                                {
                                    var obj = System.Text.Json.JsonSerializer.Deserialize<object>(route.GetRawText());
                                    if (obj != null) allRoutes.Add(obj);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, "Failed to merge Ocelot file {File}", file);
                        }
                    }
                    if (allRoutes.Count > 0)
                    {
                        var mergedPath = Path.Combine(ocelotDir, $"ocelot.merged.{env.EnvironmentName}.json");
                        var mergedPayload = new { Routes = allRoutes };
                        var mergedJson = System.Text.Json.JsonSerializer.Serialize(mergedPayload, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                        try { File.WriteAllText(mergedPath, mergedJson); } catch (Exception ex) { Log.Warning(ex, "Failed to write merged Ocelot file"); }
                        builder.Configuration.AddJsonFile(mergedPath, optional: false, reloadOnChange: false);
                        Log.Information("Merged {Count} Ocelot routes from {Files} into {Merged}", allRoutes.Count, string.Join(", ", routeFiles.Select(Path.GetFileName)), Path.GetFileName(mergedPath));
                    }
                }
            }

            builder.Services.AddControllers();
            builder.Services.AddJsonLocalization();
            builder.Services.AddWelcoSharedDependencies();

            // JWT — Gateway is the single entry point that validates Auth-issued tokens (Option 1)
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
            var jwtSettings = new JwtSettings();
            builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
            var gatewaySecret = !string.IsNullOrWhiteSpace(jwtSettings.Secret) && jwtSettings.Secret.Length >= 32
                ? jwtSettings.Secret
                : "V5B?*77+gzD_pk+2!%ORg<i)<D$DH+Xf.nECc?];2l;";
            var validIssuers = jwtSettings.GetAllValidIssuers().ToList();
            var validAudiences = jwtSettings.GetAllValidAudiences().ToList();

            var gatewayValidation = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(gatewaySecret)),
                ValidateIssuer = validIssuers.Count > 0,
                ValidIssuers = validIssuers.Count > 0 ? validIssuers : null,
                ValidateAudience = validAudiences.Count > 0,
                ValidAudiences = validAudiences.Count > 0 ? validAudiences : null,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
            builder.Services.AddSingleton(gatewayValidation);
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.SaveToken = true;
                    o.TokenValidationParameters = gatewayValidation;
                    o.Events = new JwtBearerEvents
                    {
                        OnChallenge = async ctx =>
                        {
                            ctx.HandleResponse();
                            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            ctx.Response.ContentType = "application/json";
                            var loc = ctx.HttpContext.RequestServices.GetService<ILocalizationProvider>();
                            var lang = ctx.Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0].Trim().ToLowerInvariant().StartsWith("ar") == true ? "ar" : "en";
                            var msg = loc?.GetLocalizedString("ExceptionMessages.Unauthorized", lang) ?? "Unauthorized";
                            await ctx.Response.WriteAsJsonAsync(new { isSuccess = false, statusCode = 401, message = msg, errors = new[] { msg }, data = (object?)null });
                        }
                    };
                });

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

            builder.Services.AddHttpClient("InsecureClient", client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(3);
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
            builder.Services.AddSingleton<Welco.API.Services.OpenApiAggregatorService>();
            builder.Services.AddHostedService<Welco.API.Services.OpenApiCacheWarmer>();
            builder.Services.Configure<OpenApiAggregatorOptions>(builder.Configuration.GetSection(OpenApiAggregatorOptions.SectionName));

            builder.Services.AddOcelot(builder.Configuration);
            builder.Services.AddConfiguredOpenApi();

            var app = builder.Build();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | 
                                   Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            });

            app.UseCustomExceptionHandler();
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
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();

            var microserviceDocRoutes = new List<(string ServiceName, string DisplayName, string DocRoute, string ScalarRoute)>();
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
                            var displayName = char.ToUpper(serviceName[0]) + serviceName.Substring(1) + " Microservice API";
                            microserviceDocRoutes.Add((serviceName, displayName, $"/api/docs/{serviceName}/openapi.json", $"/docs/{serviceName}"));
                        }
                    }
                }
            }

#pragma warning disable ASP0014
            app.UseEndpoints(endpoints =>
            {
                // Unified OpenAPI schema aggregating all microservices
                endpoints.MapGet("/openapi/all.json", async (HttpContext httpContext, Welco.API.Services.OpenApiAggregatorService aggregator, CancellationToken ct) =>
                {
                    var gatewayBaseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
                    var json = await aggregator.GetAggregatedOpenApiAsync(gatewayBaseUrl, ct);
                    return Results.Content(json, "application/json");
                });

                endpoints.MapGet("/", () => Results.Redirect("/scalar/v1"));

                // Unified Scalar UI loading ALL microservices endpoints together
                endpoints.MapScalarApiReference(options =>
                {
                    options.WithTitle("Welco Microservices Platform API")
                           .WithTheme(ScalarTheme.Moon)
                           .WithOpenApiRoutePattern("/openapi/all.json");
                });

                // Dedicated Scalar Documentation pages for individual microservices
                foreach (var doc in microserviceDocRoutes)
                {
                    var docServiceName = doc.ServiceName;
                    endpoints.MapGet(doc.DocRoute, async (HttpContext httpContext, Welco.API.Services.OpenApiAggregatorService aggregator, CancellationToken ct) =>
                    {
                        var gatewayBaseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
                        var json = await aggregator.GetServiceOpenApiAsync(docServiceName, gatewayBaseUrl, ct);
                        return Results.Content(json, "application/json");
                    });

                    endpoints.MapScalarApiReference(doc.ScalarRoute, options =>
                    {
                        options.WithTitle(doc.DisplayName)
                               .WithTheme(ScalarTheme.Moon)
                               .WithOpenApiRoutePattern(doc.DocRoute);
                    });
                }

                endpoints.MapControllers();
            });
#pragma warning restore ASP0014

            await app.UseOcelot();
            await app.RunAsync();
        }
    }
}
