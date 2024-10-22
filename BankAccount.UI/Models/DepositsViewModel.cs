using API.Models.DTO;

namespace BankAccountSimulation.UI.Models
{
    public class DepositsViewModel
    {
        public IEnumerable<DepositsDTO> Deposits { get; set; }
        public decimal TotalDeposits { get; set; }
    }

}
