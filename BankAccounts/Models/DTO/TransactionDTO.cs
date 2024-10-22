namespace API.Models.DTO
{
    public class TransactionDTO
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public TransactionType Type { get; set; }
        public decimal Balance { get; set; }
    }

}
