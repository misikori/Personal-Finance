using AutoMapper;
using IdentityServer.DTOs;
using IdentityServer.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Controllers;

[Authorize]
[Route("api/v1/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;

    public UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [Authorize(Roles = Roles.Admin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDetailsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDetailsDto>>> GetAllUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        return Ok(_mapper.Map<IEnumerable<UserDetailsDto>>(users));
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.User, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] 
    [HttpGet("{username}")]
    [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDetailsDto>> GetUser(string username)
    {
        var user = await _userManager.FindByNameAsync(username); //FirstOrDefaultAsync(user => user.UserName == username);
        return Ok(_mapper.Map<UserDetailsDto>(user));
    }
}