using API.DAL;
using API.Models.DTO;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    // GET: api/Accounts
    [HttpGet("GetAllAccounts")]
    public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
    {
        // retrieve all accounts
        return Ok(await _accountService.GetAllAccounts()); 
    }

    // GET: api/Accounts/5
    [HttpGet("GetAccountById/{id}")]
    public async Task<ActionResult<AccountDTO>> GetAccountById(Guid id)
    {
        var account = await _accountService.GetAccountById(id);

        if (account == null)
        {
            return NotFound();
        }

        return account;
    }

    // POST: api/Accounts
    [HttpPost("CreateAccount")]
    public async Task<IActionResult> CreateAccount(AccountDTO accountDto)
    {
        try
        {
            //later implemented in frontend so commented here

            //// Validate the account number
            //if (string.IsNullOrEmpty(accountDto.AccountNumber))
            //{
            //    return BadRequest("Account number cannot be empty.");
            //}

            //if (!IsValidAccountNumber(accountDto.AccountNumber))
            //{
            //    return BadRequest("Invalid account number format. It should contain only digits.");
            //}

            //if (accountDto.AccountNumber.Length != 12)
            //{
            //    return BadRequest("The account number must be exactly 12 digits.");
            //}

            // Check if the account number already exists
            bool accountExists = await _accountService.AccountExists(accountDto.AccountNumber);
            if (accountExists)
            {
                return BadRequest("Account number already exists.");
            }

            // logic for creating the account
            var accountId = await _accountService.CreateAccount(accountDto);

            // Return a successful response
            //return CreatedAtAction(nameof(GeAccountById), new { id = accountId }, null);
            return Ok(accountId);
        }
        catch (Exception ex)
        {
            // Log the exception (later consider using a logging framework)
            return StatusCode(500, "An error occurred while creating the account.");
        }
    }



    // POST: api/Accounts/{id}/deposit
    [HttpPost("Deposit/{id}")]
    public async Task<IActionResult> Deposit(Guid id, [FromBody] TransactionDTO transactionDTO)
    {
        try
        {
            await _accountService.Deposit(id, transactionDTO.Amount, transactionDTO.Description);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: api/Accounts/{id}/withdraw
    [HttpPost("Withdraw/{id}")]
    public async Task<IActionResult> Withdraw(Guid id, [FromBody] TransactionDTO transactionDTO)
    {
        try
        {
            await _accountService.Withdraw(id, transactionDTO.Amount, transactionDTO.Description);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/Accounts/{accountId}
    [HttpGet("TransactionsById/{accountId}")]
    public async Task<IActionResult> GetAccountWithTransactions(Guid accountId)
    {
        try
        {
            var accountWithTransactions = await _accountService.GetAccountWithTransactions(accountId);
            return Ok(accountWithTransactions);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            // Log the exception (later consider using a logging framework)
            return StatusCode(500, "An error occurred while retrieving the account with transactions.");
        }
    }

    // Deposits
    [HttpGet("Deposits/{accountId}")]
    public async Task<IEnumerable<DepositsDTO>> GetDeposits(Guid accountId)
    {
        return await _accountService.GetDepositsByAccountIdAsync(accountId);
    }

    // Withdrawals as Expenses
    [HttpGet("Expenses/{accountId}")]
    public async Task<IEnumerable<ExpensesDTO>> GetExpenses(Guid accountId)
    {
        return await _accountService.GetExpensesByAccountIdAsync(accountId);
    }


    // POST: api/Interests
    [HttpPost("CreateInterestRules")]
    public async Task<IActionResult> CreateInterestRules(InterestDTO interestDto)
    {
        try
        {
            //later implemented in frontend so commented here


            // Check if the Rule already exists
            //bool accountExists = await _accountService.AccountExists(accountDto.AccountNumber);
            //if (accountExists)
            //{
            //    return BadRequest("Account number already exists.");
            //}

            // Your logic for creating the interest rule
            var Id = await _accountService.CreateInterest(interestDto);
            return Ok(Id);
        }
        catch (Exception ex)
        {
            // Log the exception (later consider using a logging framework)
            return StatusCode(500, "An error occurred while creating the Interest Rule.");
        }
    }

    // GET: api/Interests
    [HttpGet("GetAllInterests")]
    public async Task<ActionResult<IEnumerable<Interest>>> GetInterests()
    {
        // retrieve all accounts
        return Ok(await _accountService.GetAllInterests());
    }

    // GET: api/Accounts/{accountId}
    [HttpGet("InrestCalculationById/{accountId}")]
    public async Task<IActionResult> GetInterestCalculationByAccountIdAsync(Guid accountId)
    {
        try
        {
            var interestCalculations = await _accountService.GetInterestCalculationByAccountIdAsync(accountId);
            return Ok(interestCalculations);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            // Log the exception (later consider using a logging framework)
            return StatusCode(500, "An error occurred while calculating interests.");
        }
    }
}