using Auth.Services.API.Features.Integration.Token;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Welco.Shared.Common.Options;

namespace Welco.Tests;

public class IntegrationTokenTests
{
    // Test-only dummy secret (never a real credential).
    private const string ClientSecret = "test-dummy-client-secret-0123456789abcdef";

    private static CreateIntegrationTokenCommandHandler CreateHandler(string? market = "Egypt")
    {
        var settings = new WelcoServiceSettings
        {
            ServiceIssuer = "snul-integration",
            ServiceAudience = "welco-integration",
            Clients = new Dictionary<string, IntegrationClient>(StringComparer.OrdinalIgnoreCase)
            {
                ["snul"] = new IntegrationClient { ClientId = "snul", Secret = ClientSecret, Market = market! }
            }
        };
        return new CreateIntegrationTokenCommandHandler(
            Options.Create(settings),
            NullLogger<CreateIntegrationTokenCommandHandler>.Instance);
    }

    private static JwtSecurityToken ValidateWithClientSecret(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ClientSecret)),
            ValidateIssuer = true,
            ValidIssuer = "snul-integration",
            ValidateAudience = true,
            ValidAudience = "welco-integration",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out var validated);
        return (JwtSecurityToken)validated;
    }

    [Fact]
    public async Task ValidCredentials_ReturnsTokenWithExpectedClaims()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(
            new CreateIntegrationTokenCommand { ClientId = "snul", ClientSecret = ClientSecret },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(3300, result.Data.ExpiresIn);
        Assert.Equal("Bearer", result.Data.TokenType);
        Assert.False(string.IsNullOrWhiteSpace(result.Data.AccessToken));

        var jwt = ValidateWithClientSecret(result.Data.AccessToken);
        Assert.Equal("snul-integration", jwt.Issuer);
        Assert.Equal("snul", jwt.Claims.First(c => c.Type == "client_id").Value);
        Assert.Equal("snul-integration", jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.False(string.IsNullOrWhiteSpace(jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value));
        Assert.Equal("snul", jwt.Claims.First(c => c.Type == "service").Value);
        Assert.Equal("Egypt", jwt.Claims.First(c => c.Type == "market").Value);
        Assert.DoesNotContain(jwt.Claims, c => c.Type == "azp");
        Assert.InRange((jwt.ValidTo - jwt.ValidFrom).TotalMinutes, 54.9, 55.1);
    }

    [Fact]
    public async Task WrongSecret_And_UnknownClient_ReturnIdentical401()
    {
        var handler = CreateHandler();

        var wrongSecret = await handler.Handle(
            new CreateIntegrationTokenCommand { ClientId = "snul", ClientSecret = "test-dummy-wrong-secret-zzz-0123456789" },
            CancellationToken.None);
        var unknownClient = await handler.Handle(
            new CreateIntegrationTokenCommand { ClientId = "nope", ClientSecret = ClientSecret },
            CancellationToken.None);

        Assert.False(wrongSecret.IsSuccess);
        Assert.False(unknownClient.IsSuccess);
        Assert.Equal(401, wrongSecret.StatusCode);
        Assert.Equal(401, unknownClient.StatusCode);
        Assert.Equal("Invalid client credentials.", wrongSecret.Message);
        Assert.Equal(wrongSecret.Message, unknownClient.Message);
        Assert.Equal(wrongSecret.Errors, unknownClient.Errors);
    }

    [Fact]
    public async Task ClientIdLookup_IsCaseInsensitive()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(
            new CreateIntegrationTokenCommand { ClientId = "SNUL", ClientSecret = ClientSecret },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        var jwt = ValidateWithClientSecret(result.Data.AccessToken);
        Assert.Equal("snul", jwt.Claims.First(c => c.Type == "client_id").Value);
    }

    [Fact]
    public void Validator_RejectsEmptyCredentials()
    {
        var validator = new CreateIntegrationTokenCommandValidator();

        var result = validator.Validate(new CreateIntegrationTokenCommand());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateIntegrationTokenCommand.ClientId));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateIntegrationTokenCommand.ClientSecret));
    }
}
