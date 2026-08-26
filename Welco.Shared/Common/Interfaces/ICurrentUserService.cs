namespace Welco.Shared.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
        string? IpAddress { get; }
    }
}
