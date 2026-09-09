using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Welco.Shared.Enums;

namespace Welco.Shared.Persistance.Seeding
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager, ILogger? logger = null)
        {
            try
            {
                var roleNames = Enum.GetNames<UserType>();

                foreach (var roleName in roleNames)
                {
                    if (string.IsNullOrWhiteSpace(roleName))
                        continue;

                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var identityRole = new IdentityRole<Guid>
                        {
                            Id = Guid.NewGuid(),
                            Name = roleName,
                            NormalizedName = roleName.ToUpperInvariant()
                        };

                        await roleManager.CreateAsync(identityRole);
                        logger?.LogInformation("Seeded role: {RoleName} with Id: {RoleId}", roleName, identityRole.Id);
                    }
                }

                // Remove obsolete Customer role if it exists
                var obsoleteCustomerRole = await roleManager.FindByNameAsync("Customer");
                if (obsoleteCustomerRole != null)
                {
                    await roleManager.DeleteAsync(obsoleteCustomerRole);
                    logger?.LogInformation("Removed obsolete Customer role from Identity");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error while seeding roles from UserType enum");
            }
        }
    }
}
