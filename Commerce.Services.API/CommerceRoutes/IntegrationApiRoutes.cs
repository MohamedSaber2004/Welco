namespace Commerce.Services.API.CommerceRoutes
{
        public static class IntegrationApiRoutes
    {
        public const string Root = "api/integration";

        public static class Providers
        {
            public const string Base = Root + "/providers";
            public const string GetAll = "";
            public const string GetById = "{id}";
        }

        public static class Products
        {
            public const string Base = Root + "/products";
            public const string GetAll = "";
            public const string GetById = "{id}";
        }

        public static class Inventory
        {
            public const string Base = Root + "/inventory";
            public const string Check = "check";
            public const string Reserve = "reserve";
            public const string Release = "release";
        }

        public static class Orders
        {
            public const string Base = Root + "/orders";
            public const string Create = "";
            public const string GetById = "{id}";
            public const string UpdateStatus = "{id}/status";
        }

        public static class Quotes
        {
            public const string Base = Root + "/quotes";
            public const string Create = "";
            public const string GetById = "{id}";
            public const string UpdateStatus = "{id}/status";
        }

        public static class Categories
        {
            public const string Base = Root + "/categories";
            public const string GetAll = "";
            public const string GetById = "{id}";
        }
    }
}
