namespace Seems.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    IReadOnlyList<string> Roles { get; }
}
