using Welco.Shared.Domain.Models;

namespace Welco.Shared.Common.Helpers
{
    /// <summary>
    /// Single source of truth for "which distributor application belongs to
    /// this user, and which one should the profile reflect?".
    ///
    /// This rule used to be copy-pasted into the user-management and auth
    /// profile handlers, where it silently drifted: both matched only on email
    /// and both took the newest row regardless of status. The shared type now
    /// keeps the two call sites in step.
    /// </summary>
    public static class DistributorApplicationResolver
    {
        /// <summary>
        /// True when the application was raised by this account.
        ///
        /// <para><see cref="BaseEntity.CreatedBy"/> is written as the creator's
        /// user id by the create command, but historical rows (and rows created
        /// while signed out) hold an account email or the literal "System", so
        /// all three shapes have to be tolerated.</para>
        /// </summary>
        public static bool BelongsToUser(
            DistributorApplication app,
            string userId,
            string userEmail)
        {
            if (app == null) return false;

            var createdBy = (app.CreatedBy ?? string.Empty).Trim();
            if (createdBy.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(userId) &&
                    string.Equals(createdBy, userId.Trim(), StringComparison.OrdinalIgnoreCase))
                    return true;

                if (!string.IsNullOrWhiteSpace(userEmail) &&
                    string.Equals(createdBy, userEmail.Trim(), StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // The business contact address is frequently a different mailbox
            // from the login, but it is the only link left for applications
            // submitted anonymously.
            var contact = (app.ContactEmail ?? string.Empty).Trim();
            return contact.Length > 0
                && !string.IsNullOrWhiteSpace(userEmail)
                && string.Equals(contact, userEmail.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Picks the application to represent the user's organization.
        /// An approved application always wins over a newer pending one —
        /// otherwise a re-submission would make an already-approved provider
        /// look pending again.
        /// </summary>
        public static DistributorApplication? ResolveForUser(
            IEnumerable<DistributorApplication> candidates,
            string userId,
            string userEmail)
        {
            if (candidates == null) return null;

            var mine = candidates
                .Where(a => !a.IsDeleted && BelongsToUser(a, userId, userEmail))
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            if (mine.Count == 0) return null;

            return mine.FirstOrDefault(a => a.Status == DistributorApplicationStatus.Approved)
                   ?? mine[0];
        }
    }
}
