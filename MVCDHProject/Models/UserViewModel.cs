using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVCDHProject.Models
{
    public class UserViewModel
    {
        [Required]
        public string? Name { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name="Confirm Password")]
        [Compare("Password",ErrorMessage ="Confirm Password should match with password.")]
        public string? ConfirmPassword { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name="Email Id")]
        public string? Email { get; set; }

        [Required]
        [RegularExpression("[6-9]\\d{9}",ErrorMessage ="Mobile No. is invalid")]
        public string? Mobile { get; set; }
    }
}
