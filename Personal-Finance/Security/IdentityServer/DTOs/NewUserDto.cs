using System.ComponentModel.DataAnnotations;

namespace IdentityServer.DTOs;

public class NewUserDto
{
    [Required(ErrorMessage = "FirstName is required")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "Username is required")]
    public required string UserName { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Email is required")]
    public required string Email { get; set; }

    public required string PhoneNumber { get; set; }
}