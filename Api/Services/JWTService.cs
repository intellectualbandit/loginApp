using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace Api.Services
{
  public class JWTService
  {
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _jwtKey;
    private readonly UserManager<User> _userManager;

    // inject configuration
    // add constructor
    public JWTService(IConfiguration config, UserManager<User> userManager)
    {
      _userManager = userManager;
      _config = config;
      // jwtkey is use for encryption and decryption
      _jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"])); // JWT:Key is configured in appsettings.development.json
      
    }
    public async Task<string> CreateJWT(User user)
    {
      var userClaims = new List<Claim>
      {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.GivenName, user.FirstName),
        new Claim(ClaimTypes.Surname, user.LastName)
        // you can create your own claims , sample below
        // new Claim("my own claim name", "this is the value")
      };

      var roles = await _userManager.GetRolesAsync(user);

      userClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

      var credentials = new SigningCredentials(_jwtKey, SecurityAlgorithms.HmacSha512Signature);
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(userClaims),
        Expires = DateTime.UtcNow.AddDays(int.Parse(_config["JWT:ExpiresInDays"])),
        SigningCredentials = credentials,
        Issuer = _config["JWT:Issuer"]
      };

      var tokenHandler = new JwtSecurityTokenHandler();
      var jwt = tokenHandler.CreateToken(tokenDescriptor);

      return tokenHandler.WriteToken(jwt);
    }
  }
}
