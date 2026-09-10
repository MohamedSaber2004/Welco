using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Welco.Shared.Common.Options;

namespace Welco.Shared.Common.Extensions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IntegrationRouteNameAttribute : Attribute
    {
        public string RouteKey { get; }
        public IntegrationRouteNameAttribute(string routeKey)
        {
            RouteKey = routeKey;
        }
    }

        public class IntegrationRouteConvention : IApplicationModelConvention
    {
        private readonly IntegrationRoutesOptions _routes;

        public IntegrationRouteConvention(IConfiguration configuration)
        {
            _routes = configuration.GetSection(IntegrationRoutesOptions.SectionName).Get<IntegrationRoutesOptions>()
                      ?? new IntegrationRoutesOptions();
        }

        public IntegrationRouteConvention(IntegrationRoutesOptions routes)
        {
            _routes = routes;
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                var attr = controller.Attributes.OfType<IntegrationRouteNameAttribute>().FirstOrDefault();
                if (attr != null)
                {
                    var baseRoute = attr.RouteKey switch
                    {
                        "Orders" => _routes.OrdersBase,
                        "Inventory" => _routes.InventoryBase,
                        "Products" => _routes.ProductsBase,
                        "Providers" => _routes.ProvidersBase,
                        "Quotes" => _routes.QuotesBase,
                        "Categories" => _routes.CategoriesBase,
                        "Distributors" => _routes.DistributorsBase,
                        "Support" => _routes.SupportBase,
                        "Help" => _routes.HelpBase,
                        "Certifications" => _routes.CertificationsBase,
                        _ => null
                    };

                    if (!string.IsNullOrWhiteSpace(baseRoute))
                    {
                        controller.Selectors.Clear();
                        controller.Selectors.Add(new SelectorModel
                        {
                            AttributeRouteModel = new AttributeRouteModel
                            {
                                Template = baseRoute
                            }
                        });
                    }
                }
            }
        }
    }
}
