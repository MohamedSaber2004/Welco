using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;

namespace Welco.Shared.Persistance.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users");

            builder.Property(u => u.FullName)
                .IsRequired();

            builder.Property(u => u.CreatedBy)
                .IsRequired();

            builder.Property(u => u.Language)
                .HasConversion<string>();

            builder.Property(u => u.UserType)
                .HasConversion(new ValueConverter<UserType, string>(
                    toProvider => toProvider.ToString(),
                    fromProvider => ToUserType(fromProvider)));

            // Canonical phone key: one account per number, regardless of the
            // formatting used at signup. Filtered so users who never supplied a
            // phone (NULL) and soft-deleted rows are both excluded.
            builder.Property(u => u.NormalizedPhoneNumber)
                .HasMaxLength(20);

            builder.HasIndex(u => u.NormalizedPhoneNumber)
                .IsUnique()
                .HasDatabaseName("IX_Users_NormalizedPhoneNumber")
                .HasFilter("[NormalizedPhoneNumber] IS NOT NULL AND [NormalizedPhoneNumber] <> '' AND [IsDeleted] = 0");
        }

        private static UserType ToUserType(string? stored)
        {
            if (!string.IsNullOrWhiteSpace(stored) &&
                Enum.TryParse<UserType>(stored, ignoreCase: true, out var parsed) &&
                Enum.IsDefined(typeof(UserType), parsed))
            {
                return parsed;
            }

            return UserType.OrganizationUser;
        }
    }
}
