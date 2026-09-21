using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Welco.API.Options;

namespace Welco.API.Services
{
    public class OpenApiAggregatorService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<OpenApiAggregatorService> _logger;
        private readonly OpenApiAggregatorOptions _options;

        public OpenApiAggregatorService(
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment env,
            ILogger<OpenApiAggregatorService> logger,
            IOptions<OpenApiAggregatorOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _env = env;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<string> GetAggregatedOpenApiAsync(string gatewayBaseUrl, CancellationToken cancellationToken = default)
        {
            var ocelotDir = Path.Combine(_env.ContentRootPath, "Ocelot");
            var cacheDir = Path.Combine(ocelotDir, "Cache");
            if (!Directory.Exists(cacheDir))
            {
                try { Directory.CreateDirectory(cacheDir); } catch { }
            }

            var downstreamEndpoints = await GetDownstreamOpenApiEndpointsAsync(cancellationToken);

            var mergedDoc = new JsonObject
            {
                ["openapi"] = "3.1.0",
                ["info"] = new JsonObject
                {
                    ["title"] = "Welco Microservices Platform API",
                    ["version"] = "v1",
                    ["description"] = "Aggregated OpenAPI documentation for all Welco microservices"
                },
                ["servers"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["url"] = gatewayBaseUrl,
                        ["description"] = "API Gateway"
                    }
                },
                ["paths"] = new JsonObject(),
                ["components"] = new JsonObject
                {
                    ["schemas"] = new JsonObject(),
                    ["securitySchemes"] = new JsonObject
                    {
                        ["Bearer"] = new JsonObject
                        {
                            ["type"] = "http",
                            ["scheme"] = "bearer",
                            ["bearerFormat"] = "JWT",
                            ["description"] = "JWT Authorization header using the Bearer scheme."
                        }
                    }
                },
                ["tags"] = new JsonArray()
            };

            var mergedPaths = mergedDoc["paths"]!.AsObject();
            var mergedSchemas = mergedDoc["components"]!["schemas"]!.AsObject();
            var mergedTags = mergedDoc["tags"]!.AsArray();
            var existingTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var fetchTasks = downstreamEndpoints.Select(endpoint =>
            {
                var (serviceName, url) = endpoint;
                var cacheFile = Path.Combine(cacheDir, $"openapi.{serviceName}.json");
                return FetchOpenApiWithCacheAsync(serviceName, url, cacheFile, cancellationToken);
            });

            var fetchedDocs = await Task.WhenAll(fetchTasks);

            foreach (var serviceObj in fetchedDocs)
            {
                if (serviceObj == null) continue;

                if (serviceObj.TryGetPropertyValue("paths", out var pathsNode) && pathsNode is JsonObject pathsObj)
                {
                    foreach (var (pathKey, pathValue) in pathsObj)
                    {
                        if (pathKey == "/") continue;
                        if (pathValue != null)
                        {
                            mergedPaths[pathKey] = pathValue.DeepClone();
                        }
                    }
                }

                if (serviceObj.TryGetPropertyValue("components", out var componentsNode) && componentsNode is JsonObject componentsObj)
                {
                    if (componentsObj.TryGetPropertyValue("schemas", out var schemasNode) && schemasNode is JsonObject schemasObj)
                    {
                        foreach (var (schemaKey, schemaValue) in schemasObj)
                        {
                            if (schemaValue != null && !mergedSchemas.ContainsKey(schemaKey))
                            {
                                mergedSchemas[schemaKey] = schemaValue.DeepClone();
                            }
                        }
                    }
                }

                if (serviceObj.TryGetPropertyValue("tags", out var tagsNode) && tagsNode is JsonArray tagsArray)
                {
                    foreach (var tag in tagsArray)
                    {
                        if (tag is JsonObject tagObj && tagObj.TryGetPropertyValue("name", out var tagName))
                        {
                            var nameStr = tagName?.ToString();
                            if (!string.IsNullOrEmpty(nameStr) && existingTags.Add(nameStr))
                            {
                                mergedTags.Add(tag.DeepClone());
                            }
                        }
                    }
                }
            }

            return mergedDoc.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }

        public async Task<string> GetServiceOpenApiAsync(string targetServiceName, string gatewayBaseUrl, CancellationToken cancellationToken = default)
        {
            var ocelotDir = Path.Combine(_env.ContentRootPath, "Ocelot");
            var cacheDir = Path.Combine(ocelotDir, "Cache");
            var cacheFile = Path.Combine(cacheDir, $"openapi.{targetServiceName}.json");

            var endpoints = await GetDownstreamOpenApiEndpointsAsync(cancellationToken);
            var endpointUrl = endpoints.FirstOrDefault(e => string.Equals(e.ServiceName, targetServiceName, StringComparison.OrdinalIgnoreCase)).Url;

            if (!string.IsNullOrEmpty(endpointUrl))
            {
                try
                {
                    if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);
                    var serviceObj = await FetchOpenApiWithCacheAsync(targetServiceName, endpointUrl, cacheFile, cancellationToken);
                    if (serviceObj != null)
                    {
                        return AdjustServiceOpenApi(serviceObj.ToJsonString(), gatewayBaseUrl);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not reach downstream service at {Url}", endpointUrl);
                }
            }

            if (File.Exists(cacheFile))
            {
                try
                {
                    var cachedContent = await File.ReadAllTextAsync(cacheFile, cancellationToken);
                    return AdjustServiceOpenApi(cachedContent, gatewayBaseUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load cached OpenAPI for {ServiceName}", targetServiceName);
                }
            }

            return "{}";
        }

        public async Task WarmUpAsync(CancellationToken cancellationToken = default)
        {
            var ocelotDir = Path.Combine(_env.ContentRootPath, "Ocelot");
            var cacheDir = Path.Combine(ocelotDir, "Cache");
            if (!Directory.Exists(cacheDir))
            {
                try { Directory.CreateDirectory(cacheDir); } catch { }
            }

            var endpoints = await GetDownstreamOpenApiEndpointsAsync(cancellationToken);
            // Fetch in parallel (same pattern as GetAggregatedOpenApiAsync): sequential awaits
            // multiply one slow cold-starting downstream across every other service's warm-up.
            var warmUpTasks = endpoints.Select(endpoint =>
            {
                var (serviceName, url) = endpoint;
                var cacheFile = Path.Combine(cacheDir, $"openapi.{serviceName}.json");
                _logger.LogInformation("Pre-warming OpenAPI schema for '{ServiceName}' from {Url}", serviceName, url);
                return FetchOpenApiWithCacheAsync(serviceName, url, cacheFile, cancellationToken);
            }).ToList();
            await Task.WhenAll(warmUpTasks);
        }

        public sealed record DownstreamProbeResult(
            string ServiceName,
            string Url,
            string? IpAddress,
            bool TcpOk,
            double LatencyMs,
            bool? TcpPort80Ok,
            int? HttpStatus,
            string? Error);

        /// <summary>
        /// Server-side connectivity probe: tests every downstream from THIS gateway box
        /// (DNS + TCP connect + HTTP GET), in parallel. Surfaced via GET /health/downstream
        /// so server-to-server reachability can be checked without console access to the host.
        /// </summary>
        public async Task<IReadOnlyList<DownstreamProbeResult>> ProbeDownstreamConnectivityAsync(CancellationToken cancellationToken = default)
        {
            var endpoints = await GetDownstreamOpenApiEndpointsAsync(cancellationToken);
            var tasks = endpoints.Select(e => ProbeOneAsync(e.ServiceName, e.Url, cancellationToken)).ToList();
            return await Task.WhenAll(tasks);
        }

        public static async Task<(bool Ok, double LatencyMs, string? Error)> ProbeTcpAsync(
            string host, int port, int timeoutMs = 5000, CancellationToken cancellationToken = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                using var tcp = new TcpClient();
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(timeoutMs);
                await tcp.ConnectAsync(host, port, cts.Token);
                sw.Stop();
                return (true, sw.Elapsed.TotalMilliseconds, null);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                sw.Stop();
                return (false, sw.Elapsed.TotalMilliseconds, $"TCP connect to {host}:{port} timed out after {timeoutMs}ms");
            }
            catch (Exception ex)
            {
                sw.Stop();
                return (false, sw.Elapsed.TotalMilliseconds, $"{ex.GetType().Name}: {ex.Message}");
            }
        }

        private async Task<DownstreamProbeResult> ProbeOneAsync(string serviceName, string url, CancellationToken cancellationToken)
        {
            string? ip = null;
            try
            {
                var uri = new Uri(url);
                var addresses = await Dns.GetHostAddressesAsync(uri.Host, cancellationToken);
                ip = addresses.FirstOrDefault()?.ToString();

                var (tcpOk, latencyMs, tcpError) = await ProbeTcpAsync(uri.Host, uri.Port, 5000, cancellationToken);
                var (tcp80Ok, _, _) = await ProbeTcpAsync(uri.Host, 80, 5000, cancellationToken);

                int? httpStatus = null;
                var error = tcpError;
                if (tcpOk)
                {
                    try
                    {
                        var httpClient = _httpClientFactory.CreateClient(GatewayHttpClientExtensions.InsecureClientName);
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                        cts.CancelAfter(TimeSpan.FromSeconds(15));
                        using var response = await httpClient.GetAsync(url, cts.Token);
                        httpStatus = (int)response.StatusCode;
                    }
                    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                    {
                        error = "HTTP GET timed out after 15s (TCP was OK; downstream app may be cold)";
                    }
                    catch (Exception ex)
                    {
                        error = $"HTTP GET failed: {ex.GetType().Name}: {ex.Message}";
                    }
                }

                return new DownstreamProbeResult(serviceName, url, ip, tcpOk, Math.Round(latencyMs, 1), tcp80Ok, httpStatus, error);
            }
            catch (Exception ex)
            {
                return new DownstreamProbeResult(serviceName, url, ip, false, 0, null, null, $"{ex.GetType().Name}: {ex.Message}");
            }
        }

        public async Task<IReadOnlyList<(string ServiceName, string Url)>> GetDownstreamOpenApiEndpointsAsync(CancellationToken cancellationToken = default)
        {
            var ocelotDir = Path.Combine(_env.ContentRootPath, "Ocelot");
            var downstreamEndpoints = new List<(string ServiceName, string Url)>();

            if (!Directory.Exists(ocelotDir)) return downstreamEndpoints;

            var files = Directory.GetFiles(ocelotDir, $"ocelot.*.{_env.EnvironmentName}.json");
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                if (fileName.StartsWith("ocelot.global.", StringComparison.OrdinalIgnoreCase)
                    || fileName.StartsWith("ocelot.merged.", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var parts = fileName.Split('.');
                if (parts.Length < 3) continue;

                var serviceName = parts[1];
                try
                {
                    var jsonContent = await File.ReadAllTextAsync(file, cancellationToken);
                    using var doc = JsonDocument.Parse(jsonContent);
                    if (!doc.RootElement.TryGetProperty("Routes", out var routes)) continue;

                    foreach (var route in routes.EnumerateArray())
                    {
                        if (route.TryGetProperty("DownstreamPathTemplate", out var downstreamPath) &&
                            downstreamPath.GetString() == "/openapi/v1.json" &&
                            route.TryGetProperty("DownstreamScheme", out var scheme) &&
                            route.TryGetProperty("DownstreamHostAndPorts", out var hostAndPorts))
                        {
                            var firstHost = hostAndPorts[0];
                            var host = firstHost.GetProperty("Host").GetString();
                            var port = firstHost.GetProperty("Port").GetInt32();
                            var url = $"{scheme.GetString()}://{host}:{port}/openapi/v1.json";
                            downstreamEndpoints.Add((serviceName, url));
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse Ocelot file {File}", file);
                }
            }

            return downstreamEndpoints;
        }

        private async Task<JsonObject?> FetchOpenApiWithCacheAsync(string serviceName, string url, string cacheFile, CancellationToken cancellationToken)
        {
            // Cache-first: if a cached schema exists, return it immediately and
            // refresh from the live downstream in the background. This ensures the
            // UI (Swagger aggregation) loads instantly regardless of downstream health.
            if (File.Exists(cacheFile))
            {
                try
                {
                    var cachedContent = await File.ReadAllTextAsync(cacheFile, cancellationToken);
                    var cachedNode = JsonNode.Parse(cachedContent);
                    if (cachedNode is JsonObject cachedObj)
                    {
                        _logger.LogInformation("Serving cached OpenAPI specification for '{ServiceName}'; refreshing in background.", serviceName);
                        // Fire-and-forget background refresh — does not block the caller.
                        _ = RefreshCacheInBackgroundAsync(serviceName, url, cacheFile);
                        return cachedObj;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load cached OpenAPI specification for '{ServiceName}'; will attempt live fetch.", serviceName);
                }
            }

            // No cache yet — do a single live fetch with a short timeout.
            return await FetchLiveAsync(serviceName, url, cacheFile, cancellationToken);
        }

        private async Task<JsonObject?> FetchLiveAsync(string serviceName, string url, string cacheFile, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient(GatewayHttpClientExtensions.InsecureClientName);
            try
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                linkedCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds)));

                var response = await httpClient.GetAsync(url, linkedCts.Token);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(linkedCts.Token);
                    try
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(cacheFile)))
                            Directory.CreateDirectory(Path.GetDirectoryName(cacheFile)!);
                        await File.WriteAllTextAsync(cacheFile, content, cancellationToken);
                    }
                    catch { }

                    var serviceNode = JsonNode.Parse(content);
                    if (serviceNode is JsonObject serviceObj)
                        return serviceObj;
                }
                else
                {
                    _logger.LogWarning("Failed to fetch OpenAPI schema from {Url}. Status: {StatusCode}", url, response.StatusCode);
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Timed out fetching OpenAPI schema from {Url} (downstream service may not be running).", url);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not reach downstream service at {Url}", url);
            }

            return null;
        }

        private async Task RefreshCacheInBackgroundAsync(string serviceName, string url, string cacheFile)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds)));
                var result = await FetchLiveAsync(serviceName, url, cacheFile, cts.Token);
                if (result != null)
                    _logger.LogInformation("Background refresh of OpenAPI schema for '{ServiceName}' succeeded.", serviceName);
                else
                    _logger.LogInformation("Background refresh of OpenAPI schema for '{ServiceName}' found service unreachable; cache retained.", serviceName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Background refresh of OpenAPI schema for '{ServiceName}' failed.", serviceName);
            }
        }

        private static string AdjustServiceOpenApi(string openApiJson, string gatewayBaseUrl)
        {
            try
            {
                var node = JsonNode.Parse(openApiJson);
                if (node is JsonObject obj)
                {
                    obj["openapi"] = "3.1.0";
                    obj["servers"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["url"] = gatewayBaseUrl,
                            ["description"] = "API Gateway"
                        }
                    };

                    if (obj.TryGetPropertyValue("paths", out var pathsNode) && pathsNode is JsonObject pathsObj)
                    {
                        pathsObj.Remove("/");
                    }

                    return obj.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
                }
            }
            catch
            {

            }

            return openApiJson;
        }
    }
}
