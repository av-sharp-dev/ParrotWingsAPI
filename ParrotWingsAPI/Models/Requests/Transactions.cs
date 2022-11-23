using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParrotWingsAPI.Models.Requests
{
    public class Transactions
    {
        [Required(ErrorMessage = "Recipient name is required")]
        public string RecipientName { get; set; }
        [Required(ErrorMessage = "PW amount is required")]
        [RegularExpression("(?<=^| )\\d+(\\.\\d+)?(?=$| )", ErrorMessage = "Incorrect amount format")]
        public decimal Amount { get; set; }
    }
}
