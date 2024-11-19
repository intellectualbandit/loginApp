using Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Services
{
  public class JWTService
  {
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _jwtKey;
    // inject configuration
    // add constructor
    public JWTService(IConfiguration config)
    {
      _config = config;
      // jwtkey is use for encryption and decryption
      _jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"])); // JWT:Key is configured in appsettings.development.json
    }
    public string CreateJWT(User user)
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
