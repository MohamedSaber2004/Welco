namespace Welco.Shared.Persistance.Seeding
{
    public class UserSeedModel
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Language { get; set; } = "en";
        public List<string> Roles { get; set; } = new();
    }
}
