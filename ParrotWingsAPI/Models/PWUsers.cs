using System.ComponentModel.DataAnnotations;

namespace ParrotWingsAPI.Models
{
    public class PWUsers
    {
        [Key]
        public string Email { get; set; }
        public string Name { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public decimal Balance { get; set; } = 0;
    }
}
