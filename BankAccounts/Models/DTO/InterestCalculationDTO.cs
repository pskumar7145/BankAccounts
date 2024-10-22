using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.DTO
{
    public class InterestCalculationDTO
    {
        public DateTime Date { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal Rate { get; set; }
    }
}
