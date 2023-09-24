using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Smartway.FileLoaderApi.Contracts;
using Smartway.FileLoaderApi.Dtos;
using Smartway.FileLoaderApi.Entities;

namespace Smartway.FileLoaderApi.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ITokenService tokenService,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserToReturnDto>> Register(RegisterDto registerDto)
    {
        if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
        {
            _logger.LogError("--> Почта уже используется");
            return BadRequest("Почта уже используется");
        }

        var user = new AppUser
        {
            Email = registerDto.Email,
            UserName = registerDto.Email
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            _logger.LogError(String.Join(", ", result.Errors.Select(x => x.Description)));
            return BadRequest("Ошибка создания пользователя");
        }

        var userDto = new UserToReturnDto
        {
            Email = user.Email,
            Token = _tokenService.CreateJwtToken(user)
        };

        return userDto;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserToReturnDto>> Login(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);

        if (user == null) return Unauthorized("Ошибка входа в систему.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

        if (!result.Succeeded) return Unauthorized("Ошибка входа в систему.");

        return new UserToReturnDto
        {
            Email = user.Email,
            Token = _tokenService.CreateJwtToken(user),
        };
    }
}
