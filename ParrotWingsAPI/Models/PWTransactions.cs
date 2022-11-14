using System.ComponentModel.DataAnnotations;

namespace ParrotWingsAPI.Models
{
    public class PWTransactions
    {
        public int Id { get; set; }
        public string SenderName { get; set; }
        public string RecipientName { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }

        //parameterized constructor
        public PWTransactions(string senderName, string recipientName, decimal amount, DateTime transactionDate)
        {
            this.SenderName = senderName;
            this.RecipientName = recipientName;
            this.Amount = amount;
            this.TransactionDate = transactionDate;       
        }
    }
}
