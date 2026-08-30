namespace Attachment.Services.API.AttachmentRoutes
{
    public static class AttachmentApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string Base = Root + "/" + Version + "/attachments";
        public static class Attachments
        {
            public const string Upload = "upload";
            public const string UploadMultiple = "upload-multiple";
            public const string Update = "{name}";
            public const string Download = "download";
        }
    }
}
