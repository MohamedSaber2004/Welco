using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;
using Welco.Shared.Common.Repositories.Implementation.Base;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Common.Services;
using Welco.Shared.Domain.Models;
using Welco.Shared.Persistance;

namespace Welco.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWelcoSharedDependencies(
            this IServiceCollection services,
            IConfiguration? configuration = null,
            string connectionStringName = "DatabaseConnection")
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            if (configuration != null)
            {
                services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
                services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
            }
            else
            {
                services.AddOptions<JwtSettings>()
                    .Configure<IConfiguration>((options, config) =>
                    {
                        config.GetSection(JwtSettings.SectionName).Bind(options);
                    });

                services.AddOptions<EmailSettings>()
                    .Configure<IConfiguration>((options, config) =>
                    {
                        config.GetSection(EmailSettings.SectionName).Bind(options);
                    });
            }

            services.AddDbContext<WelcoDbContext>((serviceProvider, options) =>
            {
                var config = configuration ?? serviceProvider.GetRequiredService<IConfiguration>();
                var connectionString = config.GetConnectionString(connectionStringName)
                    ?? config.GetConnectionString("DefaultConnection")
                    ?? config.GetConnectionString("DatabaseConnection");

                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(WelcoDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
                }
            });

            services.AddScoped<IWelcoDbContext>(provider => provider.GetRequiredService<WelcoDbContext>());

            return services;
        }

        public static IServiceCollection AddWelcoIdentity(
            this IServiceCollection services,
            IConfiguration? configuration = null,
            Action<IdentityOptions>? configureCustomOptions = null)
        {
            services.AddOptions<CustomIdentityOptions>()
                .Configure<IConfiguration>((options, config) =>
                {
                    var sourceConfig = configuration ?? config;
                    sourceConfig.GetSection(CustomIdentityOptions.SectionName).Bind(options);
                });

            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<WelcoDbContext>()
                .AddDefaultTokenProviders();

            services.AddOptions<IdentityOptions>()
                .Configure<IServiceProvider>((identityOptions, serviceProvider) =>
                {
                    var config = configuration ?? serviceProvider.GetService<IConfiguration>();
                    var customIdentityOptions = config?.GetSection(CustomIdentityOptions.SectionName).Get<CustomIdentityOptions>()
                                                ?? serviceProvider.GetService<IOptions<CustomIdentityOptions>>()?.Value
                                                ?? new CustomIdentityOptions();

                    identityOptions.Password.RequireDigit = customIdentityOptions.RequiredDigit;
                    identityOptions.Password.RequiredLength = customIdentityOptions.RequiredLength;
                    identityOptions.Password.RequireLowercase = customIdentityOptions.RequireLowercase;
                    identityOptions.Password.RequiredUniqueChars = customIdentityOptions.RequiredUniqueChars;
                    identityOptions.Password.RequireUppercase = customIdentityOptions.RequireUppercase;
                    identityOptions.Password.RequireNonAlphanumeric = customIdentityOptions.RequireNonAlphanumeric;
                    identityOptions.Lockout.MaxFailedAccessAttempts = customIdentityOptions.MaxFailedAttempts;
                    identityOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(customIdentityOptions.LockoutTimeSpanInDays);
                    identityOptions.Lockout.AllowedForNewUsers = true;
                    identityOptions.User.RequireUniqueEmail = customIdentityOptions.RequireUniqueEmail;
                    identityOptions.User.AllowedUserNameCharacters = customIdentityOptions.AllowedUserNameCharacters;
                    identityOptions.SignIn.RequireConfirmedEmail = customIdentityOptions.RequireConfirmedEmail;

                    configureCustomOptions?.Invoke(identityOptions);
                });

            return services;
        }
    }
}
