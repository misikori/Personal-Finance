namespace IdentityServer.DTOs;

public class AuthenticationModel
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}