namespace Portfolio.Core.Services;

/// <summary>
/// Service for user information from IdentityServer
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets user ID from username by calling IdentityServer
    /// </summary>
    Task<string> GetUserIdAsync(string username);
}

