using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Product.Services.API.Filters;
using Scalar.AspNetCore;
using Welco.Shared;
using Welco.Shared.Common.Behaviors;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Middlewares;
using Welco.Shared.Common.Options;
using Welco.Shared.Localization;
using Welco.Shared.OpenApi;
using Welco.Shared.Persistance;
using Welco.Shared.Persistance.Seeding;

namespace Product.Services.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(environmentName))
            {
                environmentName = "Development";
            }

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                EnvironmentName = environmentName,
                ContentRootPath = AppContext.BaseDirectory
            });

var env = builder.Environment;

            builder.Configuration.Sources.Clear();
            builder.Configuration
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            if (env.IsDevelopment() || env.EnvironmentName == "Test")
            {
                var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                if (appAssembly != null) builder.Configuration.AddUserSecrets(appAssembly, optional: true);
            }

            builder.Configuration.AddEnvironmentVariables().AddCommandLine(args);

            var port = Environment.GetEnvironmentVariable("PORT")
                       ?? Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS");
            if (!string.IsNullOrEmpty(port))
            {
                builder.WebHost.UseUrls($"http://*:{port}");
            }

            builder.Services.AddControllers(options =>
            {
            });
            builder.Services.AddJsonLocalization();
            builder.Services.AddWelcoSharedDependencies(builder.Configuration);
            builder.Services.AddWelcoIdentity(builder.Configuration);

            builder.Services.AddWelcoJwtAuthentication(builder.Configuration);

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });
            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            builder.Services.AddConfiguredOpenApi();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection")
                ?? builder.Configuration["DatabaseConnection"];

            var app = builder.Build();

            var exchangeRateConfig = app.Configuration.GetSection(ExchangeRateSettings.SectionName).Get<ExchangeRateSettings>() ?? new ExchangeRateSettings();
            var exchangeRateLogger = app.Services.GetRequiredService<ILogger<Program>>();
            exchangeRateLogger.LogInformation("ExchangeRateSettings loaded: Provider={Provider}, BaseUrl={BaseUrl}, TimeoutSeconds={TimeoutSeconds} (no cache, fetch-one per call)",
                exchangeRateConfig.Provider,
                exchangeRateConfig.BaseUrl,
                exchangeRateConfig.TimeoutSeconds);

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                                   Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            });

            app.UseCustomExceptionHandler();
            app.UseJsonLocalization();

            if (!app.Environment.IsEnvironment("Test") && !app.Environment.IsProduction())
            {
                app.UseHttpsRedirection();
            }

            app.UseCors("AllowAll");
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/", () => Results.Redirect("/scalar/v1"));
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("Product Microservice API")
                       .WithTheme(ScalarTheme.Moon);
            });
            app.MapControllers();

if (!app.Environment.IsEnvironment("Test"))
            {
                try
                {
                    using var scope = app.Services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<WelcoDbContext>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    await db.Database.MigrateAsync();
                    await CurrencySeeder.SeedAsync(db, logger);
                    await WorldLocationSeeder.SeedAsync(db, logger);

if (BogusDemoSeeder.ShouldSeedDemoData(app.Environment, app.Configuration, out var demoReason))
                    {
                        logger.LogInformation("Bogus demo seeding {Reason}", demoReason);
                        await BogusDemoSeeder.SeedDemoAsync(scope.ServiceProvider, logger);
                    }
                    else
                    {
                        logger.LogInformation("Bogus demo seeding {Reason}", demoReason);
                    }
                }
                catch (Exception ex)
                {
                    var logger = app.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Seeding / migration failed");
                }
            }

            await app.RunAsync();
        }
    }
}

