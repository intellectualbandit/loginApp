using Api.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Services
{
  public class RoleManagementService
  {
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<User> _userManager;

    public RoleManagementService(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
    {
      _roleManager = roleManager;
      _userManager = userManager;
    }
    public async Task<bool> RoleExistsAsync(string roleName)
    {
      return await _roleManager.RoleExistsAsync(roleName);
    }

    public async Task RemoveUserFromRolesAsync(string userId, string[] roles)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user != null)
      {
        await _userManager.RemoveFromRolesAsync(user, roles);
      }
    }
    public async Task<bool> CreateRoleAsync(string roleName)
    {
      if (!await _roleManager.RoleExistsAsync(roleName))
      {
        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
        return result.Succeeded;
      }
      return false;
    }

    public async Task<bool> UpdateRoleAsync(string oldRoleName, string newRoleName)
    {
      var role = await _roleManager.FindByNameAsync(oldRoleName);
      if (role != null)
      {
        role.Name = newRoleName;
        var result = await _roleManager.UpdateAsync(role);
        return result.Succeeded;
      }
      return false;
    }

    public async Task<bool> DeleteRoleAsync(string roleName)
    {
      var role = await _roleManager.FindByNameAsync(roleName);
      if (role != null)
      {
        var result = await _roleManager.DeleteAsync(role);
        return result.Succeeded;
      }
      return false;
    }

    public IList<IdentityRole> GetAllRoles()
    {
      return _roleManager.Roles.ToList();
      //return _roleManager.Roles.Select(r => r.Name).ToList();
      // return _roleManager.Roles.Select(r => r.Name).ToList() as IList<IdentityRole>;
    }
    //public async Task<bool> AddUserToRoleAsync(string userId, string roleName)
    //{
    //  var user = await _userManager.FindByIdAsync(userId);
    //  if (user != null && await _roleManager.RoleExistsAsync(roleName))
    //  {
    //    var result = await _userManager.AddToRoleAsync(user, roleName);
    //    return result.Succeeded;
    //  }
    //  return false;
    //}

    //public async Task AddUserToRoleAsync(string userId, string roleName)
    //{
    //  var user = await _userManager.FindByIdAsync(userId);
    //  if (user != null)
    //  {
    //    await _userManager.AddToRoleAsync(user, roleName);
    //  }
    //}
    public async Task<bool> AddUserToRoleAsync(string userId, string roleName)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user != null)
      {
        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
      }
      return false;
    }

    public async Task<bool> RemoveUserFromRoleAsync(string userId, string roleName)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user != null && await _roleManager.RoleExistsAsync(roleName))
      {
        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        return result.Succeeded;
      }
      return false;
    }

    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user != null)
      {
        return await _userManager.GetRolesAsync(user);
      }
      return new List<string>();
    }

    public async Task<IList<User>> GetUsersInRoleAsync(string roleName)
    {
      return await _userManager.GetUsersInRoleAsync(roleName);
    }
  }
}
