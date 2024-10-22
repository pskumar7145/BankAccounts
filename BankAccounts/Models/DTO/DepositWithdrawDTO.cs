using System.ComponentModel.DataAnnotations;

namespace API.Models.DTO
{
    public class DepositWithdrawDTO
    {
        [Required]
        public Guid AccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        public string Description { get; set; }
    }

}
