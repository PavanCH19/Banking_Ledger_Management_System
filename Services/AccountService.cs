using BankingLedgerApi.DTOs;
using BankingLedgerApi.Models;
using BankingLedgerApi.Repositories.Interfaces;
using System.Security.Cryptography;

namespace BankingLedgerApi.Services;

public class AccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITransactionRepository _transactionRepository;

    public AccountService(
        IAccountRepository accountRepository,
        ICustomerRepository customerRepository,
        ITransactionRepository transactionRepository)
    {
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Account> CreateAsync(CreateAccountRequest request)
    {
        // 1. Verify that the customer exists.
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId);

        if (customer is null)
        {
            throw new KeyNotFoundException("Customer not found.");
        }

        // 2. Validate the opening amount.
        if (request.InitialDeposit < 0 || request.InitialDeposit != decimal.Round(request.InitialDeposit, 2))
        {
            throw new ArgumentException(
                "Initial deposit must not be negative and must have at most two decimal places.");
        }

        // 3. Generate an account number that is not currently in use.
        string accountNumber;

        do
        {
            accountNumber = GenerateAccountNumber();
        }
        while (await _accountRepository.GetByAccountNumberAsync(accountNumber) is not null);

        // 4. Create the account, opening it with the requested initial deposit
        //    instead of dropping the amount.
        var account = new Account
        {
            CustomerId = request.CustomerId,
            AccountNumber = accountNumber,
            AccountType = request.AccountType,
            Balance = request.InitialDeposit,
            Currency = "INR",
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        await _accountRepository.CreateAsync(account);

        // 5. Record the opening deposit in the ledger so the transaction
        //    history balances with the account.
        if (request.InitialDeposit > 0m)
        {
            await _transactionRepository.CreateAsync(new LedgerTransaction
            {
                AccountId = account.Id!,
                Type = "Deposit",
                Amount = request.InitialDeposit,
                BalanceBefore = 0m,
                BalanceAfter = request.InitialDeposit,
                Status = "Completed",
                CreatedAt = account.CreatedAt
            });
        }

        return account;
    }

    public async Task<Account?> GetByIdAsync(string id)
    {
        return await _accountRepository.GetByIdAsync(id);
    }

    public async Task<List<Account>> GetByCustomerIdAsync(string customerId)
    {
        return await _accountRepository.GetByCustomerIdAsync(customerId);
    }

    private static string GenerateAccountNumber()
    {
        var number = RandomNumberGenerator.GetInt32(1_000_000_000, int.MaxValue);
        return $"10{number}";
    }
}