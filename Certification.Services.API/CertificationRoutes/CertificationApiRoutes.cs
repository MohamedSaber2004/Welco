namespace Certification.Services.API.CertificationRoutes
{
    public static class CertificationApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string Base = Root + "/" + Version + "/certifications";

        public static class Certifications
        {
            public const string Base = CertificationApiRoutes.Base;
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
            public const string Show = "{id}/show";
        }
    }
}
