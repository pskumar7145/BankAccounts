using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public TransactionType Type { get; set; }

        public Guid AccountId { get; set; }
        public Account Account { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }
    }
}
