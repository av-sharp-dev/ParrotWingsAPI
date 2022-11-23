using System.ComponentModel.DataAnnotations;

namespace ParrotWingsAPI.Models.Requests
{
    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
