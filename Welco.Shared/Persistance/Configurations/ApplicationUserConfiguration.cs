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

            // Legacy rows may store UserType values removed from the enum
            // (e.g. 'Customer'). A plain string conversion throws on read and
            // 500s every query touching the row (reported via the admin Users
            // page). Fall back to OrganizationUser - the old buyer role - so a
            // single stale row can never take down a whole listing again.
            builder.Property(u => u.UserType)
                .HasConversion(new ValueConverter<UserType, string>(
                    toProvider => toProvider.ToString(),
                    fromProvider => ToUserType(fromProvider)));
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
