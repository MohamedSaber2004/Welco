using System.Reflection;
using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Product.Services.API.Filters;
using Product.Services.API.Jobs;
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
                options.Conventions.Add(new Welco.Shared.Common.Extensions.IntegrationRouteConvention(builder.Configuration));
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

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                builder.Services.AddHangfire(configuration => configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true,
                        PrepareSchemaIfNecessary = true
                    }));

                builder.Services.AddHangfireServer(options =>
                {
                    options.WorkerCount = Math.Max(Environment.ProcessorCount, 2);
                    options.ServerName = "ProductService-ExchangeRateServer";
                });

                builder.Services.AddScoped<ExchangeRateSyncJob>();
            }

            var app = builder.Build();

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

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[] { new HangfireAuthorizationFilter() },
                    DashboardTitle = "Welco - Background Jobs"
                });
            }

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

if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    try
                    {
                        var recurringJobManager = app.Services.GetService<IRecurringJobManager>();
                        if (recurringJobManager != null)
                        {
                            var exchangeRateSettings = app.Configuration.GetSection(ExchangeRateSettings.SectionName).Get<ExchangeRateSettings>() ?? new ExchangeRateSettings();
                            var intervalHours = exchangeRateSettings.SyncIntervalHours > 0 ? exchangeRateSettings.SyncIntervalHours : 24;

                            var cronExpression = intervalHours switch
                            {
                                1 => Cron.Hourly(),
                                > 1 and < 24 => Cron.HourInterval(intervalHours),
                                _ => Cron.Daily()
                            };

                            recurringJobManager.AddOrUpdate<ExchangeRateSyncJob>(
                                "sync-latest-exchange-rates",
                                job => job.ExecuteAsync(),
                                cronExpression);

                            var logger = app.Services.GetRequiredService<ILogger<Program>>();
                            logger.LogInformation("Hangfire recurring job 'sync-latest-exchange-rates' registered (interval: {Hours}h)", intervalHours);
                        }
                    }
                    catch (Exception ex)
                    {
                        var logger = app.Services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Failed to register recurring Hangfire jobs");
                    }
                }
            }

            await app.RunAsync();
        }
    }
}
