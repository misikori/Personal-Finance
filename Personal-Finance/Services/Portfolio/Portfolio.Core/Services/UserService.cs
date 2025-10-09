using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Portfolio.Core.Services;

/// <summary>
/// Service to get user information from IdentityServer
/// </summary>
public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserService> _logger;
    private readonly Dictionary<string, string> _userIdCache = new();
    private readonly object _cacheLock = new();

    public UserService(HttpClient httpClient, ILogger<UserService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Gets user ID from username by calling IdentityServer
    /// Caches results to minimize IdentityServer calls
    /// </summary>
    public async Task<string> GetUserIdAsync(string username)
    {
        // Check cache first
        lock (_cacheLock)
        {
            if (_userIdCache.TryGetValue(username, out var cachedUserId))
            {
                _logger.LogDebug("User ID cache hit for {Username}: {UserId}", username, cachedUserId);
                return cachedUserId;
            }
        }

        try
        {
            _logger.LogInformation("Fetching user ID for {Username} from IdentityServer", username);
            
            var response = await _httpClient.GetAsync($"/api/v1/user/{username}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get user from IdentityServer. Status: {Status}", response.StatusCode);
                throw new InvalidOperationException($"User '{username}' not found in IdentityServer");
            }
            
            var userDetails = await response.Content.ReadFromJsonAsync<UserDetailsDto>();
            
            if (userDetails == null || string.IsNullOrEmpty(userDetails.Id))
            {
                throw new InvalidOperationException($"User '{username}' not found");
            }

            // Cache the result
            lock (_cacheLock)
            {
                _userIdCache[username] = userDetails.Id;
            }

            _logger.LogDebug("Resolved {Username} → UserId: {UserId}", username, userDetails.Id);
            
            return userDetails.Id;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling IdentityServer for user {Username}", username);
            throw new InvalidOperationException($"IdentityServer unavailable: {ex.Message}", ex);
        }
    }

    private class UserDetailsDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}

