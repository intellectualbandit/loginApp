using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class RCPracticeController : ControllerBase
  {
    private readonly UserManager<User> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RCPracticeController(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
    {
      _userManager = userManager;
      _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("public")]
    public IActionResult Public()
    {
      return Ok("BAHO KAG LUBOTS");
    }

    #region Roles

    [HttpGet("admin-role")]
    [Authorize(Roles = "Admin")]

    public IActionResult AdminRole()
    {
      return Ok("admin-role");
    }

    [HttpGet("manager-role")]
    [Authorize(Roles = "Manager")]

    public IActionResult ManagerRole()
    {
      return Ok("manager-role");
    }

    [HttpGet("player-role")]
    [Authorize(Roles = "User")]

    public IActionResult PlayerRole()
    {
      return Ok("player-role");
    }

    [HttpGet("admin-or-manager-role")]
    [Authorize(Roles = "Admin, Manager")]
    public IActionResult AdminOrManagerRole()
    {
      return Ok("admin or manager role");
    }

    [HttpGet("admin-or-player-role")]
    [Authorize(Roles = "Admin, User")]

    public IActionResult AdminOrPlayerRole()
    {
      return Ok("admin or player role");
    }

    #endregion

    #region Policy

    [HttpGet("admin-policy")]
    [Authorize(policy: "AdminPolicy")]
    public IActionResult AdminPolicy()
    {
      return Ok("admin policy");
    }

    [HttpGet("manager-policy")]
    [Authorize(policy: "ManagerPolicy")]
    public IActionResult ManagerPolicy()
    {
      return Ok("manager policy");
    }

    [HttpGet("player-policy")]
    [Authorize(policy: "PlayerPolicy")]
    public IActionResult PlayerPolicy()
    {
      return Ok("player policy");
    }

    [HttpGet("admin-or-manager-policy")]
    [Authorize(policy: "AdminOrManagerPolicy")]
    public IActionResult AdminOrManagerPolicy()
    {
      return Ok("admin or manager policy");
    }

    [HttpGet("admin-and-manager-policy")]
    [Authorize(policy: "AdminAndManagerPolicy")]
    public IActionResult AdminAndManagerPolicy()
    {
      return Ok("admin and manager policy");
    }

    [HttpGet("all-role-policy")]
    [Authorize(policy: "AllRolePolicy")]
    public IActionResult AllRolePolicy()
    {
      return Ok("all role policy");
    }

    #endregion

    #region Claim Policy

    [HttpGet("admin-email-policy")]
    [Authorize(policy: "AdminEmailPolicy")]
    public IActionResult AdminEmailPolicy()
    {
      return Ok("admin email policy");
    }

    [HttpGet("guada-surname-policy")]
    [Authorize(policy: "GuadaSurnamePolicy")]
    public IActionResult GuadaSurnamePolicy()
    {
      return Ok("guada surname policy");
    }

    [HttpGet("manager-email-and-guada-policy")]
    [Authorize(policy: "ManagerEmailAndGuadaSurnamePolicy")]
    public IActionResult ManagerEmailAndGuadaSurnamePolicy()
    {
      return Ok("manager email and guada surname policy");
    }

    [HttpGet("vip-policy")]
    [Authorize(policy: "VIPPolicy")]
    public IActionResult VIPPolicy()
    {
      return Ok("vip policy");
    }
  }
}
#endregion
