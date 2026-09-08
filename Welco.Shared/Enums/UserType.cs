namespace Welco.Shared.Enums
{
    public enum UserType
    {
        Admin = 1,
        OrganizationUser = 2,
        WelcoStaff = 3,
        /// <summary>
        /// Direct buyer — registers WITHOUT company fields and needs NO admin
        /// approval. OrganizationUser WITH a linked company is the
        /// Provider/Distributor side (B2B, approval-gated).
        /// </summary>
        Customer = 4
    }
}
