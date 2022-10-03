using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class RegisterViewModel
    {

        [Required(ErrorMessage = "Username is requried.")]
        [StringLength(20)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is requried.")]
        [MinLength(8, ErrorMessage = "Password can be min 8 characters")]
        [MaxLength(16, ErrorMessage = "Password can be max 16 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Re-Password is requried.")]
        [MinLength(8, ErrorMessage = "Re-Password can be min 8 characters")]
        [MaxLength(16, ErrorMessage = "Re-Password can be max 16 characters")]
        [Compare(nameof(Password))]
        public string RePassword { get; set; }

    }

}
