using API.Models.DTO;

namespace BankAccountSimulation.UI.Services
{
    public interface IAccountApiService
    {
        Task<Guid> CreateAccountAsync(AccountDTO accountDTO);
        Task<AccountDTO> GetAccountByIdAsync(Guid id);
        Task<IEnumerable<AccountDTO>> GetAllAccountsAsync();
        Task DepositAsync(Guid accountId, decimal amount, string description);
        Task WithdrawAsync(Guid accountId, decimal amount, string description);
        Task<AccountWithTransactionsDTO> GetAccountWithTransactionsAsync(Guid accountId);

        Task<IEnumerable<DepositsDTO>> GetDepositsAsync(Guid accountId);
        Task<IEnumerable<ExpensesDTO>> GetExpensesAsync(Guid accountId);
        Task<Guid> CreateInterestAsync(InterestDTO interestDTO);
        Task<IEnumerable<InterestDTO>> GetAllInterestsAsync();

    }

}
