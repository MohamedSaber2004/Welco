using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Welco.API.Services;

namespace Welco.Tests;

public class GatewayOpenApiResilienceTests
{
    [Fact]
    public void InsecureClient_DoesNotCapPerAttemptTimeout()
    {
        var services = new ServiceCollection();
        services.AddGatewayOpenApiHttpClient();
        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();

        using var client = factory.CreateClient(GatewayHttpClientExtensions.InsecureClientName);

        Assert.Equal(Timeout.InfiniteTimeSpan, client.Timeout);
    }

    [Theory]
    [InlineData("appsettings.Test.json")]
    [InlineData("appsettings.Production.json")]
    public void EnvAppsettings_DefineOpenApiAggregatorBudget(string fileName)
    {
        var path = FindGatewayFile(fileName);

        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        Assert.True(
            doc.RootElement.TryGetProperty("OpenApiAggregator", out var section),
            $"{fileName} must define an 'OpenApiAggregator' section.");

        Assert.True(
            section.TryGetProperty("TimeoutSeconds", out var timeout) && timeout.GetInt32() >= 30,
            $"{fileName} OpenApiAggregator:TimeoutSeconds must be >= 30 (cold-start budget).");
        Assert.True(
            section.TryGetProperty("RetryCount", out var retry) && retry.GetInt32() >= 1,
            $"{fileName} OpenApiAggregator:RetryCount must be >= 1.");
    }

    private static string FindGatewayFile(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "Welco.API", fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(
            $"Could not locate Welco.API/{fileName} by walking up from {AppContext.BaseDirectory}.");
    }
}
