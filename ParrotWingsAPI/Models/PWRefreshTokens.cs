using System.ComponentModel.DataAnnotations;

namespace ParrotWingsAPI.Models
{
    public class PWRefreshTokens
    {
        [Key]
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
