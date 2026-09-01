namespace Commerce.Services.API.CommerceRoutes
{
    public static class CommerceApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string CartsBase = Root + "/" + Version + "/carts";
        public const string OrdersBase = Root + "/" + Version + "/orders";
        public static class Carts
        {
            public const string Base = CartsBase;
            public const string GetById = "{id}";
            public const string GetByUser = "user/{userId}";
            public const string GetBySession = "session/{sessionId}";
            public const string Create = "";
            public const string AddItem = "{id}/items";
            public const string UpdateItem = "{id}/items/{itemId}";
            public const string RemoveItem = "{id}/items/{itemId}";
            public const string Clear = "{id}/clear";
        }
        public static class Orders
        {
            public const string Base = OrdersBase;
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string UpdateStatus = "{id}/status";
            public const string Track = "track/{orderNumber}";
        }
    }
}
