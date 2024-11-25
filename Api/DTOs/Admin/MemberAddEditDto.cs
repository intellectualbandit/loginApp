using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Admin
{
  public class MemberAddEditDto
  {
    public string Id { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    public string Password { get; set; }
    //[Required]
    //[RegularExpression("^\\w+[\\w-\\.]*\\@\\w+((-\\w+)|(\\w*))\\.[a-z]{2,3}$", ErrorMessage = "Invalid email address.")]
    //public string Email { get; set; }
    [Required]
    // ex: "Admin, Player, Manager" for roles
    public string Roles { get; set; }
  }
}
