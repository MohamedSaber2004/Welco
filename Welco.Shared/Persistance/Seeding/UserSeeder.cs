using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;

namespace Welco.Shared.Persistance.Seeding
{
    public static class UserSeeder
    {
        private const string JsonResourceName = "Welco.Shared.Persistance.Seeding.User.json";
        private const string JsonFileName = "User.json";
        private static readonly string JsonFolderName = string.Concat("Persistance", System.IO.Path.DirectorySeparatorChar, "Seeding");

        public static async Task SeedUsersAsync(
            UserManager<ApplicationUser> userManager,
            ILogger? logger = null,
            string? jsonFilePath = null)
        {
            try
            {
                var json = ReadJson(jsonFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    logger?.LogWarning("User seeding skipped: no User.json content found.");
                    return;
                }

                var seedUsers = DeserializeSeedUsers(json);
                if (seedUsers == null || seedUsers.Count == 0)
                {
                    logger?.LogWarning("User seeding skipped: no users found in User.json.");
                    return;
                }

                foreach (var seedUser in seedUsers)
                {
                    await SeedUserAsync(userManager, seedUser, logger);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error while seeding users from User.json");
            }
        }

        private static async Task SeedUserAsync(UserManager<ApplicationUser> userManager, UserSeedModel seedUser, ILogger? logger)
        {
            if (string.IsNullOrWhiteSpace(seedUser.Email) || string.IsNullOrWhiteSpace(seedUser.Password))
            {
                logger?.LogWarning("Skipping seed user with missing email or password.");
                return;
            }

            var existingUser = await userManager.FindByEmailAsync(seedUser.Email);
            if (existingUser != null)
            {
                logger?.LogInformation("Seed user already exists, skipping: {Email}", seedUser.Email);
                return;
            }

            var user = new ApplicationUser
            {
                FullName = seedUser.FullName,
                Email = seedUser.Email,
                UserName = string.IsNullOrWhiteSpace(seedUser.UserName) ? seedUser.Email : seedUser.UserName,
                Language = AppLanguageExtensions.FromCode(seedUser.Language),
                UserType = ResolveUserType(seedUser.Roles),
                EmailConfirmed = true,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(user, seedUser.Password);
            if (!createResult.Succeeded)
            {
                logger?.LogError("Failed to create seed user {Email}: {Errors}",
                    seedUser.Email,
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return;
            }

            if (seedUser.Roles != null && seedUser.Roles.Count > 0)
            {
                var addRolesResult = await userManager.AddToRolesAsync(user, seedUser.Roles);
                if (!addRolesResult.Succeeded)
                {
                    logger?.LogError("Failed to assign roles to seed user {Email}: {Errors}",
                        seedUser.Email,
                        string.Join(", ", addRolesResult.Errors.Select(e => e.Description)));
                }
            }

            logger?.LogInformation("Seeded user: {Email} with Id: {UserId}", user.Email, user.Id);
        }

        private static UserType ResolveUserType(IReadOnlyList<string>? roles)
        {
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    if (Enum.TryParse<UserType>(role, ignoreCase: true, out var userType))
                        return userType;
                }
            }

            return UserType.OrganizationUser;
        }

        private static string? ReadJson(string? jsonFilePath)
        {
            if (!string.IsNullOrWhiteSpace(jsonFilePath) && File.Exists(jsonFilePath))
            {
                return File.ReadAllText(jsonFilePath);
            }

            var assembly = typeof(UserSeeder).Assembly;

            using var stream = assembly.GetManifestResourceStream(JsonResourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }

            var possiblePaths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, JsonFolderName, JsonFileName),
                Path.Combine(Directory.GetCurrentDirectory(), JsonFolderName, JsonFileName)
            };

            var filePath = possiblePaths.FirstOrDefault(File.Exists);
            return filePath != null ? File.ReadAllText(filePath) : null;
        }

        private static List<UserSeedModel>? DeserializeSeedUsers(string json)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                return JsonSerializer.Deserialize<List<UserSeedModel>>(json, options);
            }

            var single = JsonSerializer.Deserialize<UserSeedModel>(json, options);
            return single != null ? new List<UserSeedModel> { single } : null;
        }
    }
}
