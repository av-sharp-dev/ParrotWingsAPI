using System.ComponentModel.DataAnnotations;

namespace ParrotWingsAPI.Models.Requests
{
    public class PWUsersLogin
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Incorrect email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(30, ErrorMessage = "No more than 30 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
