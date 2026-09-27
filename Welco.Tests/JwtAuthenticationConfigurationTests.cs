using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.Extensions;

namespace Welco.Tests;

/// <summary>
/// The token-issuer and the services that validate its tokens must agree on the
/// signing key. Previously a service deployed without JwtSettings:Secret fell
/// back to a hard-coded constant that lives in public source, so a mismatch
/// either validated with a forgeable key or rejected every token with an opaque
/// 401. Startup must now fail loudly instead.
/// </summary>
public sealed class JwtAuthenticationConfigurationTests
{
    private static IConfiguration Config(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
    [Fact]
    public void Production_Throws_WhenSecretMissing()
    {
        var config = Config(new Dictionary<string, string?>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            // deliberately no JwtSettings:Secret
        });

        var ex = Assert.Throws<System.InvalidOperationException>(
            () => new Microsoft.Extensions.DependencyInjection.ServiceCollection()
                .AddWelcoJwtAuthentication(config));

        Assert.Contains("JwtSettings", ex.Message);
    }

    [Fact]
    public void Production_Throws_WhenSecretTooShort()
    {
        var config = Config(new Dictionary<string, string?>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["JwtSettings:Secret"] = "too-short",
        });

        Assert.Throws<System.InvalidOperationException>(
            () => new Microsoft.Extensions.DependencyInjection.ServiceCollection()
                .AddWelcoJwtAuthentication(config));
    }

    [Fact]
    public void Production_Succeeds_WhenSecretConfigured()
    {
        var config = Config(new Dictionary<string, string?>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["JwtSettings:Secret"] = new string('k', 43),
            ["JwtSettings:Issuer"] = "https://issuer.test/",
        });

        // Must not throw.
        new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddWelcoJwtAuthentication(config);
    }

    [Fact]
    public void Development_FallsBackSoLocalRunNeedsNoSecret()
    {
        var config = Config(new Dictionary<string, string?>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development",
        });

        // Must not throw.
        new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddWelcoJwtAuthentication(config);
    }

    /// <summary>
    /// AddIdentity&lt;,&gt;() — called by AddWelcoIdentity before the JWT extension in
    /// every microservice — sets DefaultAuthenticateScheme/DefaultChallengeScheme to
    /// "Identity.Application". AddAuthentication("Bearer") alone only sets
    /// DefaultScheme, and ASP.NET Core consults the authenticate scheme first, so
    /// UseAuthentication() silently ran cookie auth. HttpContext.User stayed
    /// anonymous and every handler reading ICurrentUserService saw Guid.Empty and
    /// returned 401; only RoleAuthorizeAttribute escaped it because it re-runs
    /// AuthenticateAsync explicitly.
    /// </summary>
    [Fact]
    public void JwtScheme_OverridesTheCookieDefaultSetByAddIdentity()
    {
        var config = Config(new Dictionary<string, string?>
        {
            ["JwtSettings:Secret"] = new string('k', 43),
        });

        var services = new ServiceCollection();

        // Exactly the scheme side effect of AddIdentity<ApplicationUser, IdentityRole<Guid>>().
        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
            o.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
        });

        services.AddWelcoJwtAuthentication(config);

        var options = services.BuildServiceProvider()
            .GetRequiredService<IOptions<AuthenticationOptions>>().Value;

        Assert.Equal(JwtBearerDefaults.AuthenticationScheme, options.DefaultScheme);
        Assert.Equal(JwtBearerDefaults.AuthenticationScheme, options.DefaultAuthenticateScheme);
        Assert.Equal(JwtBearerDefaults.AuthenticationScheme, options.DefaultChallengeScheme);
    }
}
