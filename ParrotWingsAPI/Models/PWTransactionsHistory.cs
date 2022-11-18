using System.ComponentModel.DataAnnotations;

namespace ParrotWingsAPI.Models
{
    public class PWTransactionsHistory
    {
        [Key]
        public int Id { get; set; }
        public string SenderEmail { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientName { get; set; }
        public decimal ResultingBalance { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
