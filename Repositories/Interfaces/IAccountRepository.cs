using BankingLedgerApi.Models;

namespace BankingLedgerApi.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(string id);

    Task<Account?> GetByAccountNumberAsync(string accountNumber);

    Task<List<Account>> GetByCustomerIdAsync(string customerId);

    Task CreateAsync(Account account);

    /// <summary>
    /// Atomically applies a deposit or withdrawal to the account balance.
    /// Returns the account document as it was before the update, or
    /// <c>null</c> when no update occurred (account missing/inactive or
    /// insufficient funds for a withdrawal).
    /// </summary>
    Task<Account?> ApplyBalanceChangeAsync(
        string accountId,
        decimal amount,
        bool isWithdrawal);
}