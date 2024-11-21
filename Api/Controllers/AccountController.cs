using Api.DTOs.Account;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Text;
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
    private readonly EmailService _emailService;
    private readonly IConfiguration _config;

    public AccountController(JWTService jwtservice,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            EmailService emailService,
            IConfiguration config)
    {
      _jwtservice = jwtservice;
      _signInManager = signInManager;
      _userManager = userManager;
      _emailService = emailService;
      _config = config;
    }

    [HttpPost("login")] //end point
    public async Task<ActionResult<UserDto>> Login(LoginDto model)
    {
        var user = await _userManager.FindByNameAsync(model.UserName); // retrieve user object from database
      if (user == null) return Unauthorized("Invalid username or password.");

      if (user.EmailConfirmed == false) return Unauthorized("Please, confirm your email.");

      var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
      if (!result.Succeeded) return Unauthorized("Invalid username or password");

      return await CreateApplicationUserDto(user);

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

      return await CreateApplicationUserDto(user);
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
        Email = model.Email.ToLower()
        //EmailConfirmed = true , to be able to confirmed email throught email,  remove to automate email confirmation
      };

      var result = await _userManager.CreateAsync(userToAdd, model.Password);

      if (!result.Succeeded) return BadRequest(result.Errors);

      //add default user role new registered user , default is Player Role
      //await _userManager.AddToRoleAsync(userToAdd, SD.PlayerRole);

      try
      {
        if (await SendConfirmEmailAsync(userToAdd))
        {
          return Ok(new JsonResult(new { title = "New Account Created!", message = "Your account has been created, please confirm your email address." }));
        }
        return BadRequest("Failed to send email. Please, contact admin.");
      }
      catch (Exception)
      {
        return BadRequest("Failed to send email. Please, contact admin.");
      }

      //return Ok("Your account has been created, you can login now.");
    }

    [HttpPut("confirm-email")] // end point put means update

    public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto model)
    {
      var user = await _userManager.FindByEmailAsync(model.Email);
      if (user == null) return Unauthorized("This email address has not been registered yet.");

      if (user.EmailConfirmed == true) return BadRequest("Your email was confirmed before. Please, login to your account.");

      try
      {
        var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
        var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (result.Succeeded)
        {
          return Ok(new JsonResult(new { title = "Email confirmed", message = "Your email address is confirmed. You can login now." }));
        }

        return BadRequest("Invalid token. Please, try again.");
      }
      catch (Exception)
      {
        return BadRequest("Invalid token. Please, try again.");
      }
    }

    [HttpPost("resend-email-confirmation-link/{email}")] //end points
    public async Task<IActionResult> ResendEmailConfirmationLink(string email)
    {
      if (string.IsNullOrEmpty(email)) return BadRequest("Invalid email");
      var user = await _userManager.FindByEmailAsync(email);
      if (user == null) return Unauthorized("This email address has not been registered yet.");
      if (user.EmailConfirmed == true) return BadRequest("Your email address was confirmed before. Please, login to your account.");

      try
      {
        if (await SendConfirmEmailAsync(user))
        {
          return Ok(new JsonResult(new { title = "Confirmation link sent", message = "Please, confirm your email address." }));
        }

        return BadRequest("Failed to send email. Please, contact admin.");
      }
      catch (Exception)
      {
        return BadRequest("Failed to send email. Please, contact admin.");
      }
    }

    [HttpPost("forgot-username-or-password/{email}")] //end points
    public async Task<IActionResult> ForgotUsernameOrPassword(string email)
    {
      if (string.IsNullOrEmpty(email)) return BadRequest("Invalid email");
      var user = await _userManager.FindByEmailAsync(email);
      if (user == null) return Unauthorized("This email address has not been registered yet.");
      if (user.EmailConfirmed == false) return BadRequest("Please, confirm your email address first.");

      try
      {
        if (await SendForgotUsernameOrPassword(user))
        {
          return Ok(new JsonResult(new { title = "Forgot username or password email sent.", message = "Please, check your email." }));
        }
        return BadRequest("Failed to send email. Please, contact admin.");
      }
      catch (Exception)
      {
        return BadRequest("Failed to send email. Please, contact admin.");
      }
    }

    [HttpPut("reset-password")] //end point
    public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
    {
      var user = await _userManager.FindByEmailAsync(model.Email);
      if (user == null) return Unauthorized("This email address has not been registered yet.");
      if (user.EmailConfirmed == false) return BadRequest("Please, confirm your email address first.");

      try
      {
        var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
        var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

        if (result.Succeeded)
        {
          return Ok(new JsonResult(new { title = "Password reset success", message = "Your password has been reset." }));
        }

        return BadRequest("Invalid token. Please, try again.");
      }
      catch (Exception)
      {
        return BadRequest("Invalid token. Please, try again.");
      }
    }


    #region Private Helper Methods
    private async Task<UserDto> CreateApplicationUserDto(User user)
    {
      return new UserDto
      {
        FirstName = user.FirstName,
        LastName = user.LastName,
        JWT = await _jwtservice.CreateJWT(user)
      };
    }

    private async Task<bool> CheckEmailExistsAsync(string email)
    {
      return await _userManager.Users.AnyAsync(x => x.Email == email.ToLower());
    }

    private async Task<bool> SendConfirmEmailAsync(User user)
    {
      var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
      token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token)); //use system.text reference for Encoding
      var url = $"{_config["JWT:ClientUrl"]}/{_config["Email:ConfirmEmailPath"]}?token={token}&email={user.Email}"; //use string interpolation so put $ sign and curly brace

      var body = $"<p>Hello: {user.FirstName} {user.LastName} </p>" +
          "<p>Please, confirm your email address by clicking on the following link. </p>" +
          $"<p><a href=\"{url}\">Click here</a></p>" +
          "<p>Thank you, </p>" +
          $"<br>{_config["Email:ApplicationName"]}";

      var emailSend = new EmailSendDto(user.Email, "Confirm your email.", body);

      return await _emailService.SendEmailAsync(emailSend);
    }

    private async Task<bool> SendForgotUsernameOrPassword(User user)
    {
      var token = await _userManager.GeneratePasswordResetTokenAsync(user);
      token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
      var url = $"{_config["JWT:ClientUrl"]}/{_config["Email:ResetPasswordPath"]}?token={token}&email={user.Email}";

      var body = $"<p>Hello: {user.FirstName} {user.LastName} </p>" +
          $"<p>Username: {user.UserName}.</p>" +
          "<p>In order to reset your password, please click on the following link. </p>" +
          $"<p><a href=\"{url}\">Click here</a></p>" +
          "<p>Thank you, </p>" +
          $"<br>{_config["Email:ApplicationName"]}";

      var emailSend = new EmailSendDto(user.Email, "Forgot username or password.", body);

      return await _emailService.SendEmailAsync(emailSend);
    }
    #endregion
  }
}
