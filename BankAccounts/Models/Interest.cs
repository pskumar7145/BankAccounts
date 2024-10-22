using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class Interest
    {
         public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string RuleId { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal Rate { get; set; }
    }
}
