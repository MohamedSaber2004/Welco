using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Welco.API.Services;

namespace Welco.Tests;

/// <summary>
/// Tests for the gateway's server-side downstream probe (the logic behind
/// GET /health/downstream). All tests use loopback only so they never depend
/// on external network access.
/// </summary>
public class GatewayDownstreamProbeTests
{
    [Fact]
    public async Task ProbeTcp_OpenPort_ReturnsTrue()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        try
        {
            var (ok, _, error) = await OpenApiAggregatorService.ProbeTcpAsync("127.0.0.1", port, 3000);

            Assert.True(ok);
            Assert.Null(error);
        }
        finally
        {
            listener.Stop();
        }
    }

    [Fact]
    public async Task ProbeTcp_ClosedPort_ReturnsFalseFast()
    {
        var sw = Stopwatch.StartNew();
        var (ok, _, error) = await OpenApiAggregatorService.ProbeTcpAsync("127.0.0.1", 1, 3000);
        sw.Stop();

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task ProbeTcp_UnresolvableHost_ReturnsFalse()
    {
        var (ok, _, error) = await OpenApiAggregatorService.ProbeTcpAsync("nonexistent.invalid", 443, 5000);

        Assert.False(ok);
        Assert.NotNull(error);
    }
}
