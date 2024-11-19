using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account
{
  public class RegisterDto
  {
    [Required]
    [StringLength(15, MinimumLength =3, ErrorMessage ="First name must be at least {2}, and maximum {1} characters.")]
    public string FirstName { get; set; }
    [StringLength(15, MinimumLength = 3, ErrorMessage = "Last name must be at least {2}, and maximum {1} characters.")]
    [Required]
    public string LastName { get; set; }
    [Required]
    [RegularExpression("^\\w+[\\w-\\.]*\\@\\w+((-\\w+)|(\\w*))\\.[a-z]{2,3}$", ErrorMessage ="Invalid email address.")]
    public string Email { get; set; }
    [Required]
    [StringLength(15, MinimumLength = 6, ErrorMessage = "Password must be at least {2}, and maximum {1} characters.")]
    public string Password { get; set; }
  }
}
