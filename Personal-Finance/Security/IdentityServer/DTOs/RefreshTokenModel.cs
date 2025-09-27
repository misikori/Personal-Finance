namespace IdentityServer.DTOs;

public class RefreshTokenModel
{
    public required string UserName { get; set; }
    public required string RefreshToken { get; set; }
}