using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace API.Models.DTO
{
    public class AccountDTO
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        public string Email { get; set; }

        [ValidateNever]
        public string FullName => $"{FirstName} {LastName}";

        [Required]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Account number must be exactly 12 digits.")]
        public string AccountNumber { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Balance must be a non-negative number.")]
        public decimal Balance { get; set; }
        [ValidateNever]
        public List<TransactionDTO> Transactions { get; set; }
    }
}
