using AutoMapper;
using IdentityServer.Controllers.Base;
using IdentityServer.DTOs;
using IdentityServer.Entities;
using IdentityServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class AuthenticationController : RegistrationControllerBase
{
    
    private readonly IAuthenticationService _authService;


    public AuthenticationController(ILogger<AuthenticationController> logger, IMapper mapper,
        UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IAuthenticationService authService) : base(logger, mapper, userManager, roleManager)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterUser([FromBody] NewUserDto newUser)
    {
        return await RegisterNewUserWithRoles(newUser, new string[] { Roles.User });
    }
    
    
    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAdministrator([FromBody] NewUserDto newUser)
    {
        return await RegisterNewUserWithRoles(newUser, new string[] { Roles.Admin });
    }

    [HttpPost("[action]")]
    [ProducesResponseType(typeof(AuthenticationModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] UserCredentialsDto userCredentials)
    {
        var user = await _authService.ValidateUser(userCredentials);
        if (user is null)
        {
            _logger.LogWarning("{Login}: Authentication failed. Wrong username or password.", nameof(Login));
            return Unauthorized();
        }

        return Ok(await _authService.CreateAuthenticationModel(user));
    }
    [HttpPost("[action]")]
    [ProducesResponseType(typeof(AuthenticationModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthenticationModel>> Refresh([FromBody] RefreshTokenModel refreshTokenCredentials)
    {
        var user = await _userManager.FindByNameAsync(refreshTokenCredentials.UserName);
        if (user is null)
        {
            _logger.LogWarning("{Refresh}: Refreshing token failed. Unknown username {UserName}.", nameof(Refresh), refreshTokenCredentials.UserName);
            return Forbid();
        }

        var refreshToken = user.RefreshTokens.FirstOrDefault(r => r.Token == refreshTokenCredentials.RefreshToken);
        if (refreshToken is null)
        {
            _logger.LogWarning("{Refresh}: Refreshing token failed. The refresh token is not found.", nameof(Refresh));
            return Unauthorized();
        }

        if (refreshToken.ExpiryTime < DateTime.Now)
        {
            _logger.LogWarning("{Refresh}: Refreshing token failed. The refresh token is not valid.", nameof(Refresh));
            return Unauthorized();
        }

        return Ok(await _authService.CreateAuthenticationModel(user));
    }
    
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenModel refreshTokenCredentials)
    {
        var user = await _userManager.FindByNameAsync(refreshTokenCredentials.UserName);
        if (user is null)
        {
            _logger.LogWarning("{Logout}: Logout failed. Unknown username {UserName}.", nameof(Logout), refreshTokenCredentials.UserName);
            return Forbid();
        }

        await _authService.RemoveRefreshToken(user, refreshTokenCredentials.RefreshToken);

        return Accepted();
    }
}