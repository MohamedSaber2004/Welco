namespace Sales.Services.API.SalesRoutes
{
    public static class SalesApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string RFQsBase = Root + "/" + Version + "/rfqs";
        public const string QuotesBase = Root + "/" + Version + "/quotes";
        public static class RFQs
        {
            public const string Base = RFQsBase;
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string UpdateStatus = "{id}/status";
        }
        public static class Quotes
        {
            public const string Base = QuotesBase;
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Approve = "{id}/approve";
            public const string Decline = "{id}/decline";
        }
        public const string ProductInquiriesBase = Root + "/" + Version + "/product-inquiries";
        public static class ProductInquiries
        {
            public const string Base = ProductInquiriesBase;
            public const string Create = "";
        }
    }
}
