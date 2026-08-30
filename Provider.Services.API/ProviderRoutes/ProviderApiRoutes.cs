namespace Provider.Services.API.ProviderRoutes
{
    public static class ProviderApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string Base = Root + "/" + Version + "/providers";

        public static class Providers
        {
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
        }
    }
}
