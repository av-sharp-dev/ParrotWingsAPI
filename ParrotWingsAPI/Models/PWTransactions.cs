using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParrotWingsAPI.Models
{
    public class PWTransactions
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string SenderEmail { get; set; }
        public string RecipientEmail { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }

        //parameterized constructor
        public PWTransactions(string senderEmail, string recipientEmail, decimal amount, DateTime transactionDate)
        {
            this.SenderEmail = senderEmail;
            this.RecipientEmail = recipientEmail;
            this.Amount = amount;
            this.TransactionDate = transactionDate;       
        }
    }
}
