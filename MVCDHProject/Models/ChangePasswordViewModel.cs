using System.ComponentModel.DataAnnotations;

namespace MVCDHProject.Models
{
    public class ChangePasswordViewModel
    {
        [Required]
        [Display(Name="User Name")]
        [RegularExpression("[A-Za-z0-9._@+]{3,32}")]
        public string Name{ get; set; }
    }
}
