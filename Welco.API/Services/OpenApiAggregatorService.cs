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
            var downstreamEndpoints = new List<(string ServiceName, string Url)>();

            if (Directory.Exists(ocelotDir))
            {
                var files = Directory.GetFiles(ocelotDir, $"ocelot.*.{_env.EnvironmentName}.json");
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    if (!fileName.StartsWith("ocelot.global."))
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
                ["openapi"] = "3.0.1",
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

            foreach (var (serviceName, url) in downstreamEndpoints)
            {
                try
                {
                    var response = await httpClient.GetAsync(url, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync(cancellationToken);
                        var serviceNode = JsonNode.Parse(content);
                        if (serviceNode is JsonObject serviceObj)
                        {
                            // Merge Paths
                            if (serviceObj.TryGetPropertyValue("paths", out var pathsNode) && pathsNode is JsonObject pathsObj)
                            {
                                foreach (var (pathKey, pathValue) in pathsObj)
                                {
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
                    }
                    else
                    {
                        _logger.LogWarning("Failed to fetch OpenAPI schema from {Url}. Status: {StatusCode}", url, response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not reach downstream service at {Url}", url);
                }
            }

            return mergedDoc.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
