namespace UserManamgent.Service.API.UserManagementRoutes
{
    public static class UserManagementApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string Base = Root + "/" + Version + "/user-management";

        public static class Users
        {
            public const string Base = UserManagementApiRoutes.Base + "/users";
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
            public const string ChangePassword = "{id}/change-password";
        }

        public static class Addresses
        {
            public const string Base = UserManagementApiRoutes.Base + "/addresses";
            public const string GetAllByUser = "user/{userId}";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
        }

        public static class Countries
        {
            public const string Base = UserManagementApiRoutes.Base + "/countries";
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
        }

        public static class Cities
        {
            public const string Base = UserManagementApiRoutes.Base + "/cities";
            public const string GetAll = "";
            public const string GetByCountry = "country/{countryId}";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
        }

        public static class Zones
        {
            public const string Base = UserManagementApiRoutes.Base + "/zones";
            public const string GetAll = "";
            public const string GetByCity = "city/{cityId}";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
        }

        public static class Companies
        {
            public const string Base = UserManagementApiRoutes.Base + "/companies";
            public const string GetAll = "";
            public const string GetMyCompany = "my";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
        }

        public static class CompanyAddresses
        {
            public const string Base = UserManagementApiRoutes.Base + "/companies/{companyId}/addresses";
            public const string GetAll = "";
            public const string GetById = "{addressId}";
            public const string Create = "";
            public const string Update = "{addressId}";
            public const string Delete = "{addressId}";
            // fallback direct route for delete/update without companyId
            public const string DirectBase = UserManagementApiRoutes.Base + "/company-addresses";
            public const string DirectGetById = "{id}";
            public const string DirectUpdate = "{id}";
            public const string DirectDelete = "{id}";
        }

        public static class DistributorApplications
        {
            public const string Base = UserManagementApiRoutes.Base + "/distributor-applications";
            public const string Create = "";
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Approve = "{id}/approve";
            public const string Reject = "{id}/reject";
        }

        public static class AuditLogs
        {
            public const string Base = UserManagementApiRoutes.Base + "/audit-logs";
            public const string GetAll = "";
            public const string GetById = "{id}";
        }
    }
}
