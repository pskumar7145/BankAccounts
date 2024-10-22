using API.Models;
using API.Models.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace API.DAL
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;

        public AccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AccountExists(string accountNumber)
        {
            return await _context.Accounts.AnyAsync(a => a.AccountNumber == accountNumber);
        }

        public async Task<Guid> CreateAccount(AccountDTO accountDTO)
        {
            var newAccount = new Account
            {
                FirstName = accountDTO.FirstName,
                LastName = accountDTO.LastName,
                Email = accountDTO.Email,
                AccountNumber = accountDTO.AccountNumber,
                Balance = accountDTO.Balance,
                Transactions = new List<Transaction>()
            };

            //Add to the context
            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            //Insert into Transaction
            var initialTransaction = new Transaction
            {
                AccountId = newAccount.Id,
                Amount = accountDTO.Balance,
                TransactionDate = DateTime.UtcNow,
                Description = "Initial Deposit",
                Type = TransactionType.Deposit,
                Balance = accountDTO.Balance
            };

            _context.Transactions.Add(initialTransaction);
            await _context.SaveChangesAsync();

            return newAccount.Id;
        }

        public async Task<AccountDTO> GetAccountById(Guid id)
        {
            var account = await _context.Accounts
                                        .Include(a => a.Transactions)
                                        .FirstOrDefaultAsync(a => a.Id == id);

            if (account == null)
            {
                throw new KeyNotFoundException($"Account with ID {id} not found");
            }

            var accountDTO = new AccountDTO
            {
                Id = account.Id,
                FirstName = account.FirstName,
                LastName = account.LastName,
                Email = account.Email,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                Transactions = account.Transactions.Select(t => new TransactionDTO
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate,
                    Description = t.Description,
                    Type = t.Type
                }).ToList()
            };

            return accountDTO;
        }


        public async Task<IEnumerable<AccountDTO>> GetAllAccounts()
        {
            var accounts = await _context.Accounts
                .Include(a => a.Transactions)
                .ToListAsync();

            return accounts.Select(a => new AccountDTO
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Email = a.Email,
                AccountNumber = a.AccountNumber,
                Balance = a.Balance,
                Transactions = a.Transactions.Select(t => new TransactionDTO
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate,
                    Description = t.Description,
                    Type = t.Type
                }).ToList()
            });
        }

        public async Task Deposit(Guid accountId, decimal amount, string description)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            account.Balance += amount;
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = amount,
                TransactionDate = DateTime.UtcNow,
                Description = description,
                Type = TransactionType.Deposit,
                Balance = account.Balance
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task Withdraw(Guid accountId, decimal amount, string description)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            if (account.Balance < amount)
            {
                throw new Exception("Insufficient balance");
            }

            account.Balance -= amount;
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = amount,
                TransactionDate = DateTime.UtcNow,
                Description = description,
                Type = TransactionType.Withdrawal,
                Balance = account.Balance
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<AccountWithTransactionsDTO> GetAccountWithTransactions(Guid accountId)
        {
            var account = await _context.Accounts
                                        .Include(a => a.Transactions)
                                        .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null)
            {
                throw new KeyNotFoundException($"Account with ID {accountId} not found");
            }

            var accountWithTransactionsDTO = new AccountWithTransactionsDTO
            {
                Id = account.Id,
                FirstName = account.FirstName,
                LastName = account.LastName,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                Transactions = account.Transactions.Select(t => new TransactionDTO
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate,
                    Description = t.Description,
                    Type = t.Type
                }).ToList().OrderByDescending(t => t.TransactionDate).ToList(),
            };

            return accountWithTransactionsDTO;
        }

        public async Task<IEnumerable<DepositsDTO>> GetDepositsByAccountIdAsync(Guid accountId)
        {
            return await _context.Transactions
                .Where(d => d.AccountId == accountId && d.Type == 0)
                .Select(d => new DepositsDTO
                {
                    Id = d.Id,
                    Date = d.TransactionDate,
                    Amount = d.Amount,
                    Description = d.Description
                }).OrderByDescending(o => o.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExpensesDTO>> GetExpensesByAccountIdAsync(Guid accountId)
        {
            return await _context.Transactions
                .Where(e => e.AccountId == accountId && e.Type == TransactionType.Withdrawal)
                .Select(e => new ExpensesDTO
                {
                    Id = e.Id,
                    Date = e.TransactionDate,
                    Amount = e.Amount,
                    Description = e.Description
                }).OrderByDescending(o => o.Date)
                .ToListAsync();
        }

        public async Task<Guid> CreateInterest(InterestDTO interestDTO)
        {
            var newInterest = new Interest
            {
                Date = DateTime.UtcNow,
                RuleId = interestDTO.RuleId,
                Rate = interestDTO.Rate
            };

            //Add to the context
            _context.Interests.Add(newInterest);
            await _context.SaveChangesAsync();

            return newInterest.Id;
        }


        public async Task<IEnumerable<InterestDTO>> GetAllInterests()
        {
            var interests = await _context.Interests
                                .ToListAsync();

            return interests.Select(a => new InterestDTO
            {
                Id = a.Id,
                Date = a.Date,
                RuleId = a.RuleId,
                Rate = a.Rate,
            });
        }

        public async Task<string> GetInterestCalculationByAccountIdAsync(Guid accountId)
        {
            var account = await _context.Accounts
                                        .Include(a => a.Transactions)
                                        .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null)
            {
                throw new KeyNotFoundException($"Account with ID {accountId} not found");
            }

            DateTime now = DateTime.Now;
            DateTime firstDayCurrentMonth = new DateTime(now.Year, now.Month, 1);
            var firstDayOfLastMonth = firstDayCurrentMonth.AddMonths(-1).Date;
            var lastDayOfLastMonth = now.Date.AddDays(-now.Day);
            //==================Remove below later -Just for Testing only
            firstDayOfLastMonth = new DateTime(2023, 6, 1); ;
            lastDayOfLastMonth = new DateTime(2023, 6, 30); ;
            //==================Remove below later -Just for Testing only
            var accountWithTransactionsDTO = new AccountWithTransactionsDTO
            {
                Id = account.Id,
                FirstName = account.FirstName,
                LastName = account.LastName,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                Transactions = account.Transactions.Select(t => new TransactionDTO
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate,
                    Description = t.Description,
                    Type = t.Type,
                    Balance = t.Balance
                }).ToList().Where(a => (a.TransactionDate >= firstDayOfLastMonth) & (a.TransactionDate <= lastDayOfLastMonth)).OrderByDescending(t => t.TransactionDate).ToList(),
            };

            List<InterestTransactionDTO> InterestTransactions = new List<InterestTransactionDTO>();

            foreach (var Trans in accountWithTransactionsDTO.Transactions)
            {
                if (Trans.Type == TransactionType.Withdrawal)
                {
                    InterestTransactions.Add(new InterestTransactionDTO
                    {
                        Date = Trans.TransactionDate,
                        Amount = - Trans.Amount,
                        Balance = Trans.Balance,
                    });
                }
                else 
                {
                    InterestTransactions.Add(new InterestTransactionDTO
                    {
                        Date = Trans.TransactionDate,
                        Amount = Trans.Amount,
                        Balance = Trans.Balance,
                    });
                }
            }
            var InterestTransactionsSorted = InterestTransactions.OrderBy(x => x.Date).ToList();
            var interests = await _context.Interests.ToListAsync();

            List<InterestCalculationDTO> InterestCalculations = new List<InterestCalculationDTO>();

            foreach (var interestCalc in interests)
            {
                InterestCalculations.Add(new InterestCalculationDTO
                {
                    Date = interestCalc.Date,
                    Rate = interestCalc.Rate,
                });            
            }
            var InterestCalculationsSorted = InterestCalculations.OrderBy(x => x.Date).ToList();
            Dictionary<DateTime,decimal> eodBalances = CalculateEODBalance(InterestTransactionsSorted);

            decimal TotalInterest = 0.00M;
            List<string> TestLst = new List<string>();
            foreach (var eodBalance in eodBalances)
            {
                DateTime date = eodBalance.Key;
                decimal balance = eodBalance.Value;
                decimal interestRate = GetApplicableInterestRate(InterestCalculationsSorted, date);
                decimal interest = CalculateInterest(balance, interestRate, 1); 
                TotalInterest = interest + TotalInterest;               
            }

            string interestDescription = "Interetst for the Month of " + DateTime.Now.Month + "-" + DateTime.Now.Year;
            var account1 = await _context.Accounts.FindAsync(accountId);
            if (account1 == null)
            {
                throw new Exception("Account not found");
            }

            account1.Balance += TotalInterest;
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = TotalInterest,
                TransactionDate = DateTime.UtcNow,
                Description = interestDescription,
                Type = TransactionType.Interest,
                Balance = account1.Balance
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return "Interest of $ " + Math.Round(TotalInterest, 2).ToString() + " credited successfully into the account " + accountId; 
        }
        static Dictionary<DateTime, decimal> CalculateEODBalance(List<InterestTransactionDTO> InterestTransactionsSorted)//transactions
        {
            DateTime now = DateTime.Now;
            DateTime firstDayCurrentMonth = new DateTime(now.Year, now.Month, 1);
            var firstDayOfLastMonth = firstDayCurrentMonth.AddMonths(-1).Date;
            var lastDayOfLastMonth = now.Date.AddDays(-now.Day);
            //==================Remove below later -Just for Testing only
            firstDayOfLastMonth = new DateTime(2023, 6, 1); ;
            lastDayOfLastMonth = new DateTime(2023, 6, 30); ;
            //==================Remove below later -Just for Testing only            
            Dictionary<DateTime, decimal> dailyBalanceForMonth = new Dictionary<DateTime, decimal>();
            Dictionary<DateTime, string> transactionDetails = InterestTransactionsSorted.ToDictionary(g => g.Date, g => g.Amount +"#"+ g.Balance);
            var sortedTransactionDetails = transactionDetails.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
            var transactionDetailBalance = Convert.ToDecimal(sortedTransactionDetails.ElementAt(0).Value.Split('#')[1]) - Convert.ToDecimal(sortedTransactionDetails.ElementAt(0).Value.Split('#')[0]);
            decimal currentTransactionDetailBalance = 0.00M;
            for (int i = firstDayOfLastMonth.Day-1;i < lastDayOfLastMonth.Day; i++)
            {

                foreach (var sortedTransactionDetail in sortedTransactionDetails.Keys)
                {
                    if (sortedTransactionDetail.Date == firstDayOfLastMonth.AddDays(i).Date)
                    {
                            currentTransactionDetailBalance = Convert.ToDecimal(sortedTransactionDetails[sortedTransactionDetail].Split('#')[0]);
                            transactionDetailBalance = transactionDetailBalance + currentTransactionDetailBalance;
                    }
                }
                dailyBalanceForMonth.Add(firstDayOfLastMonth.AddDays(i), transactionDetailBalance);
            }
            return dailyBalanceForMonth;
        }

        static decimal GetApplicableInterestRate(List<InterestCalculationDTO> interestRates, DateTime date)
        {
            return interestRates
                .Where(rate => rate.Date <= date)
                .OrderByDescending(rate => rate.Date)
                .First()
                .Rate;
        }
        static decimal CalculateInterest(decimal balance, decimal annualInterestRate, int days)
        {
            return balance * ((annualInterestRate / 100) / 365) * days;
        }

    }
}
