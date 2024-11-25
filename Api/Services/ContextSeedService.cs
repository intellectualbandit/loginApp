using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Services
{
  public class ContextSeedService
  {
    //create constructor
    private readonly Context _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ContextSeedService(Context context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager)
    {
      _context = context;
      _userManager = userManager;
      _roleManager = roleManager;
    }

    public async Task InitializeContextAsync()
    {
      if (_context.Database.GetPendingMigrationsAsync().GetAwaiter().GetResult().Count() > 0)
      {
        // applies any pending migration into our database

        await _context.Database.MigrateAsync();
      }

      if (!_roleManager.Roles.Any())
      {
        await _roleManager.CreateAsync(new IdentityRole { Name = SD.AdminRole });
        await _roleManager.CreateAsync(new IdentityRole { Name = SD.ManagerRole });
        await _roleManager.CreateAsync(new IdentityRole { Name = SD.UserRole });
        await _roleManager.CreateAsync(new IdentityRole { Name = SD.SuperAdminRole });
        await _roleManager.CreateAsync(new IdentityRole { Name = SD.SupervisorRole });
        await _roleManager.CreateAsync(new IdentityRole { Name = SD.SuperUserRole });

        await _roleManager.CreateAsync(new IdentityRole { Name = SD.Sales });
        await _roleManager.CreateAsync(new IdentityRole { Name = SD.Procurement });
        await _roleManager.CreateAsync(new IdentityRole { Name = SD.Inventory });
        await _roleManager.CreateAsync(new IdentityRole { Name = SD.HRIS });
        await _roleManager.CreateAsync(new IdentityRole { Name = SD.Manufacturing });
        await _roleManager.CreateAsync(new IdentityRole { Name = SD.Construction });
        await _roleManager.CreateAsync(new IdentityRole { Name = SD.ProjectManagement });
        await _roleManager.CreateAsync(new IdentityRole { Name = SD.Logistics });
      }

      if (!_userManager.Users.AnyAsync().GetAwaiter().GetResult())
      {
        //asigning admin role
        var admin = new User
        {
          FirstName = "admin",
          LastName = "lemuel",
          UserName = SD.AdminUserName,
          Email = SD.AdminUserName,
          EmailConfirmed = true
        };
        await _userManager.CreateAsync(admin, "123456");
        await _userManager.AddToRolesAsync(admin, new[]
        {
                    SD.AdminRole, SD.ManagerRole, SD.UserRole,
                    SD.SuperAdminRole, SD.SuperUserRole, SD.SupervisorRole
                }); //if it's an array [] supply data using {}

        await _userManager.AddClaimsAsync(admin, new Claim[]
        {
                    //asigning admin two claims
                    new Claim(ClaimTypes.Email, admin.Email),
                    new Claim(ClaimTypes.Surname, admin.LastName)

          //adding custom claim ... format below
          //new Claim("My Type", "My Value")
        });

        //asigning role manager
        var manager = new User
        {
          FirstName = "manager",
          LastName = "guada",
          UserName = "manager@sample.com",
          Email = "manager@sample.com",
          EmailConfirmed = true
        };
        await _userManager.CreateAsync(manager, "123456");
        await _userManager.AddToRoleAsync(manager, SD.ManagerRole);

        await _userManager.AddClaimsAsync(manager, new Claim[]
        {
                    //asigning admin two claims
                    new Claim(ClaimTypes.Email, manager.Email),
                    new Claim(ClaimTypes.Surname, manager.LastName)
        });

        //asigning role player
        var player = new User
        {
          FirstName = "player",
          LastName = "JP",
          UserName = "player@sample.com",
          Email = "player@sample.com",
          EmailConfirmed = true
        };
        await _userManager.CreateAsync(player, "123456");
        await _userManager.AddToRoleAsync(player, SD.UserRole);

        await _userManager.AddClaimsAsync(player, new Claim[]
        {
                    //asigning admin two claims
                    new Claim(ClaimTypes.Email, player.Email),
                    new Claim(ClaimTypes.Surname, player.LastName)
        });

        //asigning role vipplayer
        var vipplayer = new User
        {
          FirstName = "vipplayer",
          LastName = "Pia",
          UserName = "vipplayer@sample.com",
          Email = "vipplayer@sample.com",
          EmailConfirmed = true
        };
        await _userManager.CreateAsync(vipplayer, "123456");
        await _userManager.AddToRoleAsync(vipplayer, SD.SuperUserRole);

        await _userManager.AddClaimsAsync(vipplayer, new Claim[]
        {
                    //asigning admin two claims
                    new Claim(ClaimTypes.Email, vipplayer.Email),
                    new Claim(ClaimTypes.Surname, vipplayer.LastName)
        });
      }
    }
  }
}
