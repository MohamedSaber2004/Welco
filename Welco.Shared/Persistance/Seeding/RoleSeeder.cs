using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Welco.Shared.Persistance.Seeding
{
    public class RoleSeedModel
    {
        public string Name { get; set; } = string.Empty;
        public string? NormalizedName { get; set; }
    }

    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager, ILogger? logger = null)
        {
            try
            {
                var basePath = AppContext.BaseDirectory;
                var possiblePaths = new[]
                {
                    Path.Combine(basePath, "Persistance", "Seeding", "roles.json"),
                    Path.Combine(basePath, "roles.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Persistance", "Seeding", "roles.json")
                };

                var filePath = possiblePaths.FirstOrDefault(File.Exists);
                List<RoleSeedModel>? rolesToSeed = null;

                if (filePath != null)
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    rolesToSeed = JsonSerializer.Deserialize<List<RoleSeedModel>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                rolesToSeed ??= new List<RoleSeedModel>
                {
                    new() { Name = "Admin", NormalizedName = "ADMIN" },
                    new() { Name = "Doctor", NormalizedName = "DOCTOR" }
                };

                foreach (var roleModel in rolesToSeed)
                {
                    if (string.IsNullOrWhiteSpace(roleModel.Name))
                        continue;

                    if (!await roleManager.RoleExistsAsync(roleModel.Name))
                    {
                        var identityRole = new IdentityRole<Guid>
                        {
                            Id = Guid.NewGuid(),
                            Name = roleModel.Name,
                            NormalizedName = roleModel.NormalizedName ?? roleModel.Name.ToUpperInvariant()
                        };

                        await roleManager.CreateAsync(identityRole);
                        logger?.LogInformation("Seeded role: {RoleName} with Id: {RoleId}", roleModel.Name, identityRole.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error while seeding roles from roles.json");
            }
        }
    }
}
