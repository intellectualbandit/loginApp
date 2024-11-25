using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Api.Models;
using Api.Services;
using Api.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
  //[Authorize(Roles = "Admin")]
  [Route("api/[controller]")]
  [ApiController]
  public class AdminController : ControllerBase
  {
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    //private readonly RoleManagementService _roleManagementService;

    public AdminController(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
            //RoleManagementService roleManagementService)
    {
      _userManager = userManager;
      _roleManager = roleManager;
     // _roleManagementService = roleManagementService;
    }

    [HttpGet("get-members")]
    public async Task<ActionResult<IEnumerable<MemberViewDto>>> GetMembers()
    {
      var members = await _userManager.Users
          .Where(x => x.UserName != SD.AdminUserName) // select all the users not equeal to adminuserName
          //Select is a projection
          //project the infinite members
          .Select(member => new MemberViewDto
          {
            Id = member.Id,
            UserName = member.UserName,
            FirstName = member.FirstName,
            LastName = member.LastName,
            DateCreated = member.DateCreated,
            IsLocked = _userManager.IsLockedOutAsync(member).GetAwaiter().GetResult(),
            Roles = _userManager.GetRolesAsync(member).GetAwaiter().GetResult()
          }).ToListAsync();

      return Ok(members);
    }

    [HttpGet("get-member/{id}")]
    public async Task<ActionResult<MemberAddEditDto>> GetMember(string id)
    {
      var member = await _userManager.Users
          .Where(x => x.UserName != SD.AdminUserName && x.Id == id)
          .Select(m => new MemberAddEditDto
          {
            Id = m.Id,
            UserName = m.UserName,
            FirstName = m.FirstName,
            LastName = m.LastName,
            Roles = String.Join(",", _userManager.GetRolesAsync(m).GetAwaiter().GetResult())
          }).FirstOrDefaultAsync();

      return Ok(member);
    }

    [HttpPost("add-edit-member")]
    public async Task<IActionResult> AddEditMember(MemberAddEditDto model)
    {
      User user;

      if (string.IsNullOrEmpty(model.Id))
      {
        // adding a new member
        if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 6)
        {
          ModelState.AddModelError("errors", "Password must not be null or empty and at least 6 characters.");
          return BadRequest(ModelState);
        }

        user = new User
        {
          FirstName = model.FirstName.ToLower(),
          LastName = model.LastName.ToLower(),
          UserName = model.UserName.ToLower(),
          EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);
      }
      else
      {
        // editing an existing member
        if (!string.IsNullOrEmpty(model.Password))
        {
          if (model.Password.Length < 6)
          {
            ModelState.AddModelError("errors", "Password must be at least 6 characters.");
            return BadRequest(ModelState);
          }
        }

        if (IsAdminUserId(model.Id))
        {
          return BadRequest(SD.SuperAdminChangenNotAllowed);
        }

        user = await _userManager.FindByIdAsync(model.Id);

        //fetch user
        if (user == null) return NotFound();

        user.FirstName = model.FirstName.ToLower();
        user.LastName = model.LastName.ToLower();
        user.UserName = model.UserName.ToLower();

        if (!string.IsNullOrEmpty(model.Password))
        {
          await _userManager.RemovePasswordAsync(user); // remove the current password
          await _userManager.AddPasswordAsync(user, model.Password); // update new password
        }

      }

      // handle editing and adding member
      var userRoles = await _userManager.GetRolesAsync(user);

      // removing users existing role(s)
      await _userManager.RemoveFromRolesAsync(user, userRoles);

      // Removing user's existing role(s)
      // await _roleManagementService.RemoveUserFromRoleAsync(user.Id, string.Join(",", userRoles));

      // await _roleManagementService.RemoveUserFromRolesAsync(user.Id, userRoles.ToArray());


      // add  new roles
      foreach (var role in model.Roles.Split(",").ToArray())
      {
        var roleToAdd = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == role);
        if (roleToAdd != null)
        {
          await _userManager.AddToRoleAsync(user, role);
        }
      }

      //new add new roles to cater rolesservice
      //foreach (var role in model.Roles.Split(",").ToArray())
      //{
      //  var roleExists = await _roleManagementService.RoleExistsAsync(role);
      //  if (roleExists)
      //  {
      //    await _roleManagementService.AddUserToRoleAsync(user.Id, role);
      //  }
      //}

      // check if null or empty id
      // if null or empty add new member else update member
      if (string.IsNullOrEmpty(model.Id))
      {
        return Ok(new JsonResult(new { title = "Member Created", message = $"{model.UserName} has been created." }));
      }
      else
      {
        return Ok(new JsonResult(new { title = "Member Updated", message = $"{model.UserName} has been updated." }));
      }
    }

    [HttpPut("lock-member/{id}")]
    public async Task<IActionResult> LockMembers(string id)
    {
      var user = await _userManager.FindByIdAsync(id);
      if (user == null) return NotFound();

      if (IsAdminUserId(id))
      {
        return BadRequest(SD.SuperAdminChangenNotAllowed);
      }

      //set user to lock
      await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddDays(5));
      return NoContent();
    }

    [HttpPut("unlock-member/{id}")]
    public async Task<IActionResult> UnlockMembers(string id)
    {
      var user = await _userManager.FindByIdAsync(id);
      if (user == null) return NotFound();

      if (IsAdminUserId(id))
      {
        return BadRequest(SD.SuperAdminChangenNotAllowed);
      }

      //set user to lock
      await _userManager.SetLockoutEndDateAsync(user, null);
      return NoContent();
    }

    [HttpDelete("delete-member/{id}")]
    public async Task<IActionResult> DeleteMember(string id)
    {
      var user = await _userManager.FindByIdAsync(id);
      if (user == null) return NotFound();

      if (IsAdminUserId(id))
      {
        return BadRequest(SD.SuperAdminChangenNotAllowed);
      }

      //set user to lock
      await _userManager.DeleteAsync(user);
      return NoContent();
    }

    [HttpGet("get-application-roles")]
    public async Task<ActionResult<string[]>> GetApplicationRoles()
    {
      return Ok(await _roleManager.Roles.Select(x => x.Name).ToListAsync());
    }
    private bool IsAdminUserId(string userId)
    {
      return _userManager.FindByIdAsync(userId).GetAwaiter().GetResult().UserName.Equals(SD.AdminUserName);
    }
  }
}
