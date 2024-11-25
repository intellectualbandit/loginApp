using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Api
{
  public static class SD
  {
    public const string Facebook = "facebook";
    public const string Google = "google";

    //Roles
    public const string SuperAdminRole = "SuperAdmin";
    public const string AdminRole = "Admin";
    public const string ManagerRole = "Manager";
    public const string SupervisorRole = "Supervisor";
    public const string SuperUserRole = "SuperUser";
    public const string UserRole = "User";

    //Modular Roles
    public const string Sales = "Sales";
    public const string Procurement = "Procurement";
    public const string Inventory = "Inventory";
    public const string HRIS = "HRIS";
    public const string Manufacturing = "Manufacturing";
    public const string Construction = "Construction";
    public const string ProjectManagement = "ProjectManagement";
    public const string Logistics = "Logistics";

    public const string AdminUserName = "admin@sample.com";
    public const string SuperAdminChangenNotAllowed = "Super Admin change is not allowed!";
    public const int MaximumLoginAttempts = 3;
    public static bool VIPPolicy(AuthorizationHandlerContext context)
    {
      if (context.User.IsInRole(UserRole) &&
          context.User.HasClaim(c => c.Type == ClaimTypes.Email && c.Value.Contains("vip")))
      {
        return true;
      }

      return false;
    }
  }
}
