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
        }

        public static class Currencies
        {
            public const string Base = CurrenciesBase;
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
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
