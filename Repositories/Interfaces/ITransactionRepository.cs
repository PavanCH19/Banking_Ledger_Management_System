using BankingLedgerApi.Models;

namespace BankingLedgerApi.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task CreateAsync(LedgerTransaction transaction);

    Task<List<LedgerTransaction>> GetByAccountIdAsync(
        string accountId);

    Task UpdateResultAsync(
    string id,
    string status,
    decimal balanceBefore,
    decimal balanceAfter);
}