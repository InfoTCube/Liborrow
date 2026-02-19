using API.DTOs.Auth;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AuthController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;

    public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await _userManager.Users.AnyAsync(x => x.UserName == registerDto.UserName))
            return BadRequest("Username is already taken");

        if (await _userManager.Users.AnyAsync(x => x.Email == registerDto.Email))
            return BadRequest("Email is already taken");

        var user = new AppUser
        {
            UserName = registerDto.UserName.ToLower(),
            Email = registerDto.Email.ToLower(),
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        //await _userManager.AddToRoleAsync(user, "User");

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Token = _tokenService.CreateToken(user),
            CreatedAt = user.CreatedAt
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _userManager.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == loginDto.Email.ToUpper());

        if (user == null)
            return Unauthorized("Invalid email address");

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

        if (!result.Succeeded)
            return Unauthorized("Invalid password");

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Token = _tokenService.CreateToken(user),
            CreatedAt = user.CreatedAt
        };
    }
}