namespace Content.Services.API.ContentRoutes
{
    public static class ContentApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string DocumentsBase = Root + "/" + Version + "/documents";
        public const string LandingPagesBase = Root + "/" + Version + "/landing-pages";
        public static class Documents
        {
            public const string Base = DocumentsBase;
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Delete = "{id}";
        }
        public static class LandingPages
        {
            public const string Base = LandingPagesBase;
            public const string GetAll = "";
            public const string GetBySlug = "slug/{slug}";
            public const string Create = "";
        }
    }
}
