using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Common.Options;

namespace Welco.Tests;

public class ServiceAuthAttributeTests
{
    // Test-only dummy secrets (never real credentials).
    private const string ClientSecret = "test-dummy-client-secret-0123456789abcdef";
    private const string LegacySecret = "test-dummy-legacy-secret-abcdef0123456789";

    private static WelcoServiceSettings CreateSettings() => new()
    {
        ServiceSecret = LegacySecret,
        ServiceIssuer = "snul-integration",
        ServiceAudience = "welco-integration",
        Clients = new Dictionary<string, IntegrationClient>(StringComparer.OrdinalIgnoreCase)
        {
            ["snul"] = new IntegrationClient { ClientId = "snul", Secret = ClientSecret, Market = "Egypt" }
        }
    };

    private static string MintServiceToken(string secret, string? clientId, DateTime? expires = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "snul-integration"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("service", "snul"),
            new("market", "Egypt"),
        };
        if (clientId != null)
            claims.Add(new Claim("client_id", clientId));

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = "snul-integration",
            Audience = "welco-integration",
            Subject = new ClaimsIdentity(claims),
            Expires = expires ?? DateTime.UtcNow.AddMinutes(55),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        }));
    }

    private static (ActionExecutingContext context, Func<bool> wasNextCalled) CreateContext(
        WelcoServiceSettings settings, string? token)
    {
        var services = new ServiceCollection();
        services.AddSingleton(Options.Create(settings));
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
        var provider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = provider;
        if (token != null)
            httpContext.Request.Headers["Authorization"] = $"Bearer {token}";

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ActionExecutingContext(
            actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());

        var called = false;
        Task<ActionExecutedContext> Next()
        {
            called = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object()));
        }

        var attribute = new ServiceAuthAttribute();
        var task = attribute.OnActionExecutionAsync(context, Next);
        task.GetAwaiter().GetResult();
        return (context, () => called);
    }

    [Fact]
    public void PerClientToken_WithRegisteredClient_Passes()
    {
        var token = MintServiceToken(ClientSecret, "snul");

        var (context, wasNextCalled) = CreateContext(CreateSettings(), token);

        Assert.Null(context.Result);
        Assert.True(wasNextCalled());
    }

    [Fact]
    public void PerClientToken_SignedWithWrongSecret_Fails401()
    {
        var token = MintServiceToken("test-dummy-other-secret-zzz-0123456789ab", "snul");

        var (context, wasNextCalled) = CreateContext(CreateSettings(), token);

        var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.False(wasNextCalled());
    }

    [Fact]
    public void Token_WithUnknownClientId_Fails401()
    {
        var token = MintServiceToken(ClientSecret, "unknown-client");

        var (context, wasNextCalled) = CreateContext(CreateSettings(), token);

        var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.False(wasNextCalled());
    }

    [Fact]
    public void LegacyToken_WithoutClientId_StillPasses()
    {
        var token = MintServiceToken(LegacySecret, clientId: null);

        var (context, wasNextCalled) = CreateContext(CreateSettings(), token);

        Assert.Null(context.Result);
        Assert.True(wasNextCalled());
    }

    [Fact]
    public void LegacyToken_WithWrongSecret_Fails401()
    {
        var token = MintServiceToken("test-dummy-other-secret-zzz-0123456789ab", clientId: null);

        var (context, wasNextCalled) = CreateContext(CreateSettings(), token);

        var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.False(wasNextCalled());
    }

    [Fact]
    public void MissingToken_Fails401()
    {
        var (context, wasNextCalled) = CreateContext(CreateSettings(), token: null);

        var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.False(wasNextCalled());
    }
}
