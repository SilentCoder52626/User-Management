using System.ComponentModel.DataAnnotations;

namespace User_Management.Areas.User.Models
{
    public class RegisterViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and Confirm Password didnot match.")]
        public string ConfirmPassword { get; set; }
    }
}
