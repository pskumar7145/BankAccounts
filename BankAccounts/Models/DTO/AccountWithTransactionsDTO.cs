namespace API.Models.DTO
{
    public class AccountWithTransactionsDTO
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public List<TransactionDTO> Transactions { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
