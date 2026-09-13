namespace Auth.Services.API.Features.Integration.Token
{
    public class IntegrationTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }
}
