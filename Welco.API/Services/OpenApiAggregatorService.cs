using System.Text.Json;
using System.Text.Json.Nodes;

namespace Welco.API.Services
{
    public class OpenApiAggregatorService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<OpenApiAggregatorService> _logger;

        public OpenApiAggregatorService(
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment env,
            ILogger<OpenApiAggregatorService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _env = env;
            _logger = logger;
        }

        public async Task<string> GetAggregatedOpenApiAsync(string gatewayBaseUrl, CancellationToken cancellationToken = default)
        {
            var ocelotDir = Path.Combine(_env.ContentRootPath, "Ocelot");
            var cacheDir = Path.Combine(ocelotDir, "Cache");
            if (!Directory.Exists(cacheDir))
            {
                try { Directory.CreateDirectory(cacheDir); } catch { /* ignore */ }
            }

            var downstreamEndpoints = new List<(string ServiceName, string Url)>();

            if (Directory.Exists(ocelotDir))
            {
                var files = Directory.GetFiles(ocelotDir, $"ocelot.*.{_env.EnvironmentName}.json");
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    if (!fileName.StartsWith("ocelot.global.", StringComparison.OrdinalIgnoreCase)
                        && !fileName.StartsWith("ocelot.merged.", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = fileName.Split('.');
                        if (parts.Length >= 3)
                        {
                            var serviceName = parts[1];
                            try
                            {
                                var jsonContent = await File.ReadAllTextAsync(file, cancellationToken);
                                using var doc = JsonDocument.Parse(jsonContent);
                                if (doc.RootElement.TryGetProperty("Routes", out var routes))
                                {
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
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to parse Ocelot file {File}", file);
                            }
                        }
                    }
                }
            }

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

            var httpClient = _httpClientFactory.CreateClient("InsecureClient");

            var fetchTasks = downstreamEndpoints.Select(async endpoint =>
            {
                var (serviceName, url) = endpoint;
                var cacheFile = Path.Combine(cacheDir, $"openapi.{serviceName}.json");

                try
                {
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    linkedCts.CancelAfter(TimeSpan.FromSeconds(2));

                    var response = await httpClient.GetAsync(url, linkedCts.Token);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync(linkedCts.Token);
                        try
                        {
                            await File.WriteAllTextAsync(cacheFile, content, cancellationToken);
                        }
                        catch
                        {
                            // ignore file caching errors
                        }

                        var serviceNode = JsonNode.Parse(content);
                        if (serviceNode is JsonObject serviceObj)
                        {
                            return serviceObj;
                        }
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

                // Fallback to cached schema on disk if available
                if (File.Exists(cacheFile))
                {
                    try
                    {
                        var cachedContent = await File.ReadAllTextAsync(cacheFile, cancellationToken);
                        var cachedNode = JsonNode.Parse(cachedContent);
                        if (cachedNode is JsonObject cachedObj)
                        {
                            _logger.LogInformation("Using cached OpenAPI specification for '{ServiceName}'.", serviceName);
                            return cachedObj;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load cached OpenAPI specification for '{ServiceName}'.", serviceName);
                    }
                }

                return null;
            });

            var fetchedDocs = await Task.WhenAll(fetchTasks);

            foreach (var serviceObj in fetchedDocs)
            {
                if (serviceObj == null) continue;

                // Merge Paths
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

                // Merge Components -> Schemas
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

                // Merge Tags
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

            string? endpointUrl = null;
            if (Directory.Exists(ocelotDir))
            {
                var files = Directory.GetFiles(ocelotDir, $"ocelot.{targetServiceName}.{_env.EnvironmentName}.json");
                if (files.Length > 0)
                {
                    try
                    {
                        var jsonContent = await File.ReadAllTextAsync(files[0], cancellationToken);
                        using var doc = JsonDocument.Parse(jsonContent);
                        if (doc.RootElement.TryGetProperty("Routes", out var routes))
                        {
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
                                    endpointUrl = $"{scheme.GetString()}://{host}:{port}/openapi/v1.json";
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse Ocelot file for {ServiceName}", targetServiceName);
                    }
                }
            }

            if (!string.IsNullOrEmpty(endpointUrl))
            {
                try
                {
                    var httpClient = _httpClientFactory.CreateClient("InsecureClient");
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    linkedCts.CancelAfter(TimeSpan.FromSeconds(2));

                    var response = await httpClient.GetAsync(endpointUrl, linkedCts.Token);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync(linkedCts.Token);
                        try
                        {
                            if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);
                            await File.WriteAllTextAsync(cacheFile, content, cancellationToken);
                        }
                        catch { /* ignore */ }

                        return AdjustServiceOpenApi(content, gatewayBaseUrl);
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
                // return raw if parsing fails
            }

            return openApiJson;
        }
    }
}
