using System.ComponentModel.DataAnnotations;

namespace IdentityServer.DTOs;

public class UserCredentialsDto
{
    [Required(ErrorMessage = "Username is required")]
    public required string UserName { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}