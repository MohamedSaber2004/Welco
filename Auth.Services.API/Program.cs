using System.Text;
using Auth.Services.API.Infrastructure;
using Auth.Services.API.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
            builder.Services.AddWelcoSharedDependencies(builder.Configuration);
            builder.Services.AddWelcoIdentity(builder.Configuration);

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
            var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
            builder.Services.AddSingleton(jwtSettings);

            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            var secretKey = !string.IsNullOrWhiteSpace(jwtSettings.Secret) && jwtSettings.Secret.Length >= 32
                ? jwtSettings.Secret
                : "V5B?*77+gzD_pk+2!%ORg<i)<D$DH+Xf.nECc?];2l;";

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = !string.IsNullOrWhiteSpace(jwtSettings.Issuer),
                ValidIssuer = !string.IsNullOrWhiteSpace(jwtSettings.Issuer) ? jwtSettings.Issuer : null,
                ValidateAudience = !string.IsNullOrWhiteSpace(jwtSettings.Audience),
                ValidAudience = !string.IsNullOrWhiteSpace(jwtSettings.Audience) ? jwtSettings.Audience : null,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };
            builder.Services.AddSingleton(tokenValidationParameters);

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = tokenValidationParameters;
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var localizer = context.HttpContext.RequestServices.GetService<ILocalizationProvider>();
                        var localizedMessage = localizer?.GetLocalizedString(LocalizationKeys.ExceptionMessages.Unauthorized)
                                               ?? "You are not authorized to perform this action.";

                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            isSuccess = false,
                            statusCode = StatusCodes.Status401Unauthorized,
                            message = localizedMessage,
                            errors = new[] { localizedMessage },
                            data = (object?)null
                        });

                        return context.Response.WriteAsync(result);
                    }
                };
            });

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

            //app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Auth.Services.API" }));
            app.MapGet("/", () => Results.Redirect("/scalar/v1"));
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("Welco Auth Microservice API")
                       .WithTheme(ScalarTheme.Moon);
            });
            app.MapControllers();

            app.Run();
        }
    }
}
