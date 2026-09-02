namespace Content.Services.API.ContentRoutes
{
    public static class ContentApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string DocumentsBase = Root + "/" + Version + "/documents";
        public const string LandingPagesBase = Root + "/" + Version + "/landing-pages";
        public const string HelpCategoriesBase = Root + "/" + Version + "/help/categories";
        public const string HelpArticlesBase = Root + "/" + Version + "/help/articles";
        public const string FaqsBase = Root + "/" + Version + "/help/faqs";
        public const string SupportTicketsBase = Root + "/" + Version + "/support/tickets";
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
        public static class HelpCategories
        {
            public const string Base = HelpCategoriesBase;
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
        }
        public static class HelpArticles
        {
            public const string Base = HelpArticlesBase;
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string GetBySlug = "slug/{slug}";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
        }
        public static class Faqs
        {
            public const string Base = FaqsBase;
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
        }
        public static class SupportTickets
        {
            public const string Base = SupportTicketsBase;
            public const string GetAll = "";
            public const string GetMy = "my";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Reply = "{id}/reply";
            public const string Close = "{id}/close";
        }
        public const string TradeShowsBase = Root + "/" + Version + "/trade-shows";
    }
}
