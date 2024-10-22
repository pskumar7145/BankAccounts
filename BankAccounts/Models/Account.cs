using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;

namespace API.Models
{
    public class Account
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string AccountNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")] 
        public decimal Balance { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
