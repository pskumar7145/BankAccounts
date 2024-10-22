using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace API.Models.DTO
{
    public class InterestDTO
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        [Required] 
        public string RuleId { get; set; }
        [Required]
        [Range(0.01, 99.99, ErrorMessage = "Interest Rate must be greater than 0 and less than 100")]
        public decimal Rate { get; set; }
    }
}
