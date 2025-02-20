using Microsoft.AspNetCore.Mvc;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using UserSerivce.Services;
using DTOs.User;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public UserController(IUserService userService, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userService = userService;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("create")]
    public IActionResult CreateUser([FromBody] Request request)
    {
        var result = _userService.AddUser(request.FirstName, request.LastName, request.UserName, request.Password);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Errors);
    }

    [Authorize]
    [HttpGet("{userName}")]
    public IActionResult GetUser(string userName)
    {
        var result = _userService.GetUserByUserName(userName);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Errors);
    }

    [Authorize]
    [HttpDelete("{userName}")]
    public IActionResult DeleteUser(string userName)
    {
        var result = _userService.RemoveUser(userName);
        if (result.IsSuccess)
        {
            return Ok("User removed successfully.");
        }
        return NotFound(result.Errors);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Register model)
    {
        var user = new IdentityUser { UserName = model.UserName, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            return Ok("User registered successfully.");
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login model)
    {
        var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

        if (result.Succeeded)
        {
            return Ok("User logged in successfully.");
        }

        return Unauthorized("Invalid login attempt.");
    }
}

