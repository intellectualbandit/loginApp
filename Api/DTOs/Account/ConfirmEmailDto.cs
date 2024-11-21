using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account
{
  public class ConfirmEmailDto
  {
    [Required]
    public string Token { get; set; }

    [Required]
    [RegularExpression("^\\w+[\\w-\\.]*\\@\\w+((-\\w+)|(\\w*))\\.[a-z]{2,3}$", ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }
  }
}
