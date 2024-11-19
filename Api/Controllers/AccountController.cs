using Api.DTOs.Account;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AccountController : ControllerBase
  {
    private readonly JWTService _jwtservice;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public AccountController(JWTService jwtservice,
            SignInManager<User> signInManager,
            UserManager<User> userManager)
    {
      _jwtservice = jwtservice;
      _signInManager = signInManager;
      _userManager = userManager;
    }

    [HttpPost("login")] //end point
    public async Task<ActionResult<UserDto>> Login(LoginDto model)
    {
        var user = await _userManager.FindByNameAsync(model.UserName); // retrieve user object from database
      if (user == null) return Unauthorized("Invalid username or password.");

      if (user.EmailConfirmed == false) return Unauthorized("Please, confirm your email.");

      var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
      if (!result.Succeeded) return Unauthorized("Invalid username or password");

      return CreateApplicationUserDto(user);

    }

    [Authorize]
    [HttpGet("refresh-user-token")]
    public async Task<ActionResult<UserDto>> RefreshUserToken()
    {

      var emailClaim = User?.FindFirst(ClaimTypes.Email)?.Value;
      var user = await _userManager.FindByNameAsync(emailClaim);

      if (await _userManager.IsLockedOutAsync(user))
      {
        return Unauthorized("You have been locked out.");
      }

      return  CreateApplicationUserDto(user);
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto model)
    {
      if (await CheckEmailExistsAsync(model.Email))
      {
        return BadRequest($"An existing account is using {model.Email}, email address. Please, try with another email address.");
      }
      var userToAdd = new User
      {
        FirstName = model.FirstName.ToLower(),
        LastName = model.LastName.ToLower(),
        UserName = model.Email.ToLower(),
        Email = model.Email.ToLower(),
        EmailConfirmed = true
      };

      var result = await _userManager.CreateAsync(userToAdd, model.Password);

      if (!result.Succeeded) return BadRequest(result.Errors);

      return Ok("Your account has been created, you can login now.");
    }

    #region Private Helper Methods
    private UserDto CreateApplicationUserDto(User user)
    {
      return new UserDto
      {
        FirstName = user.FirstName,
        LastName = user.LastName,
        JWT = _jwtservice.CreateJWT(user)
      };
    }

    private async Task<bool> CheckEmailExistsAsync(string email)
    {
      return await _userManager.Users.AnyAsync(x => x.Email == email.ToLower());
    }
    #endregion
  }
}
