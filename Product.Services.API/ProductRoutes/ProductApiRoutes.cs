namespace Product.Services.API.ProductRoutes
{
    public static class ProductApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string Base = Root + "/" + Version + "/products";
        public const string CategoriesBase = Root + "/" + Version + "/categories";
        public const string CurrenciesBase = Root + "/" + Version + "/currencies";

        public static class Categories
        {
            public const string Base = CategoriesBase;
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Show = "{id}/show";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
            public const string GetProductsByCategory = "{categoryId}/products";
        }

        public static class Products
        {
            public const string Base = ProductApiRoutes.Base;
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Show = "{id}/show";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
            public const string GetVideos = "{id}/videos";
            public const string UpdateVideos = "{id}/videos";
        }

        public static class Currencies
        {
            public const string Base = CurrenciesBase;
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string GetByCode = "code/{code}";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
        }

        public static class ExchangeRates
        {
            public const string Base = Root + "/" + Version + "/exchange-rates";
            public const string Latest = "latest";
            public const string LatestByBase = "latest/{baseCurrency}";
            public const string History = "history/{baseCurrency}/{date}";
            public const string Pair = "{from}/{to}";
            public const string Convert = "convert";
            public const string Sync = "sync";
            public const string SyncHistory = "sync/history/{date}";
            public const string SyncLogs = "sync/logs";
        }

        public static class Wishlist
        {
            public const string Base = Root + "/" + Version + "/wishlist";
            public const string GetAll = "";
            public const string Add = "{productId}";
            public const string Remove = "{productId}";
            public const string Check = "{productId}/check";
        }
    }
}
