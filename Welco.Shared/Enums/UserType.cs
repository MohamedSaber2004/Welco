namespace Welco.Shared.Enums
{
    public enum UserType
    {
        Admin = 1,
        OrganizationUser = 2,  // Provider / Distributor / Supplier — supplies goods to the platform
        WelcoStaff = 3,
        Client = 4             // End buyer who purchases products from the site
    }
}
