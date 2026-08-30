namespace Welco.Shared.Common.Options
{
    public class UploadPathsOptions
    {
        public string? RootPath { get; set; }
        public string DefaultPath { get; set; } = null!;
        public string Providers { get; set; } = null!;
        public string Users { get; set; } = null!;
    }
}
