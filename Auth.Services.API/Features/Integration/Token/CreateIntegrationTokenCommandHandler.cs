using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Welco.Shared.Common.Options;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Integration.Token
{
    public class CreateIntegrationTokenCommandHandler : IRequestHandler<CreateIntegrationTokenCommand, Result<IntegrationTokenResponse>>
    {
        public const string InvalidClientCredentialsMessage = "Invalid client credentials.";
        public const int TokenLifetimeMinutes = 55;
        public const int ExpiresInSeconds = 3300;

        private readonly WelcoServiceSettings _settings;
        private readonly ILogger<CreateIntegrationTokenCommandHandler> _logger;

        public CreateIntegrationTokenCommandHandler(
            IOptions<WelcoServiceSettings> options,
            ILogger<CreateIntegrationTokenCommandHandler> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public Task<Result<IntegrationTokenResponse>> Handle(CreateIntegrationTokenCommand request, CancellationToken cancellationToken)
        {
            var clientId = (request.ClientId ?? string.Empty).Trim();

            var match = _settings.Clients?
                .FirstOrDefault(kvp => string.Equals(kvp.Key, clientId, StringComparison.OrdinalIgnoreCase));

            if (match?.Value is null)
            {
                _logger.LogWarning("[IntegrationToken] Unknown client login attempt.");
                return Task.FromResult(Unauthorized());
            }

            var client = match.Value.Value;

            if (string.IsNullOrWhiteSpace(client.Secret) || client.Secret.Length < 32)
            {
                _logger.LogWarning("[IntegrationToken] Misconfigured secret for a known client (min 32 chars).");
                return Task.FromResult(Unauthorized());
            }

            var providedSecret = request.ClientSecret ?? string.Empty;
            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(client.Secret),
                    Encoding.UTF8.GetBytes(providedSecret)))
            {
                _logger.LogWarning("[IntegrationToken] Invalid secret for a known client.");
                return Task.FromResult(Unauthorized());
            }

            var now = DateTime.UtcNow;
            var market = string.IsNullOrWhiteSpace(client.Market) ? "Egypt" : client.Market;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(client.Secret));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.WriteToken(handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = string.IsNullOrWhiteSpace(_settings.ServiceIssuer) ? "snul-integration" : _settings.ServiceIssuer,
                Audience = string.IsNullOrWhiteSpace(_settings.ServiceAudience) ? "welco-integration" : _settings.ServiceAudience,
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, "snul-integration"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("client_id", match.Value.Key),
                    new Claim("service", "snul"),
                    new Claim("market", market),
                }),
                NotBefore = now,
                Expires = now.AddMinutes(TokenLifetimeMinutes),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            }));

            return Task.FromResult(Result<IntegrationTokenResponse>.Success(
                new IntegrationTokenResponse
                {
                    AccessToken = jwt,
                    ExpiresIn = ExpiresInSeconds,
                    TokenType = "Bearer"
                }));
        }

        private static Result<IntegrationTokenResponse> Unauthorized()
        {
            return Result<IntegrationTokenResponse>.Unauthorized(
                InvalidClientCredentialsMessage,
                new List<string> { InvalidClientCredentialsMessage });
        }
    }
}
