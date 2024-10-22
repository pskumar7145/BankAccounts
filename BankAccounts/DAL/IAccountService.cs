using API.Models;
using API.Models.DTO;

namespace API.DAL
{
    public interface IAccountService
    {
        Task<bool> AccountExists(string accountNumber);
        Task<Guid> CreateAccount(AccountDTO accountDTO);
        Task<AccountDTO> GetAccountById(Guid id);
        Task<IEnumerable<AccountDTO>> GetAllAccounts();
        Task Deposit(Guid accountId, decimal amount, string description);
        Task Withdraw(Guid accountId, decimal amount, string description);
        Task<AccountWithTransactionsDTO> GetAccountWithTransactions(Guid accountId);

        Task<IEnumerable<DepositsDTO>> GetDepositsByAccountIdAsync(Guid accountId);
        Task<IEnumerable<ExpensesDTO>> GetExpensesByAccountIdAsync(Guid accountId);
        Task<Guid> CreateInterest(InterestDTO interestDTO);
        Task<IEnumerable<InterestDTO>> GetAllInterests();
        Task<string> GetInterestCalculationByAccountIdAsync(Guid accountId);
    }
}
