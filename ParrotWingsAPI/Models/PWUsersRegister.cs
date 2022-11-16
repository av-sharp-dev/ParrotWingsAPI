using System.ComponentModel.DataAnnotations;

namespace ParrotWingsAPI.Models
{
    public class PWUsersRegister
    {
        [Key]
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Incorrect email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(40, ErrorMessage = "No more than 40 characters")]
        [RegularExpression("[a-zA-Z\\s]+$", ErrorMessage = "Incorrect name format. Enter first and last name")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(30, ErrorMessage = "No more than 30 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Password confirmation is required")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string PasswordConfirmation { get; set; }
    }
}
