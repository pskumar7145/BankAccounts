using API.Models.DTO;

namespace BankAccountSimulation.UI.Models
{
    public class ExpensesViewModel
    {
        public IEnumerable<ExpensesDTO> Expenses { get; set; }
        public decimal TotalExpenses { get; set; }
    }

}
