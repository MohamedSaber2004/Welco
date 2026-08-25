using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Welco.Shared.OpenApi
{
    public class AcceptLanguageHeaderTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            operation.Parameters ??= new List<IOpenApiParameter>();

            if (!operation.Parameters.Any(p => string.Equals(p.Name, "Accept-Language", StringComparison.OrdinalIgnoreCase)))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Accept-Language",
                    In = ParameterLocation.Header,
                    Required = false,
                    Description = "Preferred language for localized response messages ('en' or 'ar')"
                });
            }

            return Task.CompletedTask;
        }
    }
}
