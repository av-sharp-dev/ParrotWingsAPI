using System.ComponentModel.DataAnnotations;

namespace ParrotWingsAPI.Models
{
    public class PWUsers
    {
        [Key]
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public decimal Balance { get; set; }// will make it private
    }
}
