using API.Models;
using API.Models.DTO;
using BankAccountSimulation.UI.Constants;
using Microsoft.Identity.Client;
using System.Text.Json;

namespace BankAccountSimulation.UI.Services
{
    public class AccountApiService : IAccountApiService
    {
        private readonly HttpClient _httpClient;

        public AccountApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Guid> CreateAccountAsync(AccountDTO accountDTO)
        {
            var url = $"{ApiConstants.BaseApiUrl}/Accounts/CreateAccount";
            var response = await _httpClient.PostAsJsonAsync(url, accountDTO);

            if (!response.IsSuccessStatusCode)
            {
                // Log response content for debugging
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error creating account: {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<Guid>();
        }

        public async Task<AccountDTO> GetAccountByIdAsync(Guid id)
        {
            var url = $"{ApiConstants.BaseApiUrl}/Accounts/{id}";
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<AccountDTO>();
        }

        public async Task<IEnumerable<AccountDTO>> GetAllAccountsAsync()
        {
            var url = $"{ApiConstants.BaseApiUrl}/Accounts/GetAllAccounts";
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<AccountDTO>>();
        }

        public async Task DepositAsync(Guid accountId, decimal amount, string description)
        {
            var url = $"{ApiConstants.BaseApiUrl}/Accounts/Deposit/{accountId}";
            var content = new
            {
                AccountId = accountId,
                Amount = amount,
                Description = description,
                Type = TransactionType.Deposit
            };

            var response = await _httpClient.PostAsJsonAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task WithdrawAsync(Guid accountId, decimal amount, string description)
        {
            var url = $"{ApiConstants.BaseApiUrl}/Accounts/Withdraw/{accountId}";
            var content = new
            {
                AccountId = accountId,
                Amount = amount,
                Description = description,
                Type = TransactionType.Withdrawal
            };

            var response = await _httpClient.PostAsJsonAsync(url, content);

            response.EnsureSuccessStatusCode();
        }

        public async Task<AccountWithTransactionsDTO> GetAccountWithTransactionsAsync(Guid accountId)
        {
            var url = $"{ApiConstants.BaseApiUrl}/Accounts/TransactionsById/{accountId}";
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<AccountWithTransactionsDTO>();
        }

        public async Task<IEnumerable<DepositsDTO>> GetDepositsAsync(Guid accountId)
        {
            // Implement the API call to fetch deposits
            var url = $"{ApiConstants.BaseApiUrl}/Accounts/deposits/{accountId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<DepositsDTO>>();
        }

        public async Task<IEnumerable<ExpensesDTO>> GetExpensesAsync(Guid accountId)
        {         
            var url = $"{ApiConstants.BaseApiUrl}/Accounts/expenses/{accountId}";

            // Implement the API call to fetch expenses
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<ExpensesDTO>>();
        }


        public async Task<Guid> CreateInterestAsync(InterestDTO interestDTO)
        {
            var url = $"{ApiConstants.BaseApiUrl}/Accounts/CreateInterestRules";
            var response = await _httpClient.PostAsJsonAsync(url, interestDTO);

            if (!response.IsSuccessStatusCode)
            {
                // Log response content for debugging
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error creating account: {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<Guid>();
        }

        public async Task<IEnumerable<InterestDTO>> GetAllInterestsAsync()
        {
            var url = $"{ApiConstants.BaseApiUrl}/Accounts/GetAllInterests";
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<InterestDTO>>();
        }
    }
}
