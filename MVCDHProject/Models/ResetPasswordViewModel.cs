using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace MVCDHProject.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Userid { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        [Display(Name="New Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name ="Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage ="Confirm password should match with Password")]
        public string ConfirmPassword { get; set; } 
    }
}
