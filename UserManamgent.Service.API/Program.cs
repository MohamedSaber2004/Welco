using System.Reflection;
using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Welco.Shared;
using Welco.Shared.Common.Behaviors;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Middlewares;
using Welco.Shared.Common.Options;
using Welco.Shared.Localization;
using Welco.Shared.Localization.Interfaces;
using Welco.Shared.OpenApi;
using Welco.Shared.Persistance.Seeding;

namespace UserManamgent.Service.API
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

            builder.Services.AddControllers();
            builder.Services.AddJsonLocalization();
            builder.Services.AddWelcoSharedDependencies(builder.Configuration);
            builder.Services.AddWelcoIdentity(builder.Configuration);

            // JWT via Welco.Shared (not Auth reference) — suitable microservice validation, Auth remains sole issuer
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
            var jwtSettingsTmp = new JwtSettings();
            builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettingsTmp);
            var umSecret = !string.IsNullOrWhiteSpace(jwtSettingsTmp.Secret) && jwtSettingsTmp.Secret.Length >= 32 ? jwtSettingsTmp.Secret : "V5B?*77+gzD_pk+2!%ORg<i)<D$DH+Xf.nECc?];2l;";
            var validIssuers = jwtSettingsTmp.GetAllValidIssuers().ToList();
            var validAudiences = jwtSettingsTmp.GetAllValidAudiences().ToList();

            var umValidation = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(umSecret)),
                ValidateIssuer = validIssuers.Count > 0,
                ValidIssuers = validIssuers.Count > 0 ? validIssuers : null,
                ValidateAudience = validAudiences.Count > 0,
                ValidAudiences = validAudiences.Count > 0 ? validAudiences : null,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
            builder.Services.AddSingleton(umValidation);
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
            {
                o.SaveToken = true;
                o.TokenValidationParameters = umValidation;
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

            // MediatR + FluentValidation
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

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    await RoleSeeder.SeedRolesAsync(roleManager, logger);
                }
                catch (Exception)
                {
                }
            }

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
                options.WithTitle("User Management Microservice API")
                       .WithTheme(ScalarTheme.Moon);
            });
            app.MapControllers();

            await app.RunAsync();
        }
    }
}
