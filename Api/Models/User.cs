using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
  public class User : IdentityUser
  {
    //property
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public long UserId { get; set; }
    public DateTime DateCreated { get; set; }
  }
}
