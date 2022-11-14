namespace ParrotWingsAPI.Models
{
    public class PWTransactions
    {
        public int Id { get; set; }
        public string SenderName { get; set; }
        public string RecipientName { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }// will make it private
    }
}
