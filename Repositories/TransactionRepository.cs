using BankingLedgerApi.Models;
using BankingLedgerApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace BankingLedgerApi.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly IMongoCollection<LedgerTransaction> _transactions;

    public TransactionRepository(IMongoDatabase database)
    {
        _transactions = database.GetCollection<LedgerTransaction>("Transactions");
    }

    public async Task CreateAsync(LedgerTransaction transaction)
    {
        await _transactions.InsertOneAsync(transaction);
    }

    public async Task<List<LedgerTransaction>> GetByAccountIdAsync(
        string accountId)
    {
        return await _transactions
            .Find(transaction => transaction.AccountId == accountId)
            .SortByDescending(transaction => transaction.CreatedAt)
            .ToListAsync();
    }

    public async Task UpdateResultAsync(
    string id,
    string status,
    decimal balanceBefore,
    decimal balanceAfter)
    {
        var filter = Builders<LedgerTransaction>.Filter.Eq(
            t => t.Id, id);

        var update = Builders<LedgerTransaction>.Update
            .Set(t => t.Status, status)
            .Set(t => t.BalanceBefore, balanceBefore)
            .Set(t => t.BalanceAfter, balanceAfter);

        var result = await _transactions.UpdateOneAsync(filter, update);

        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException(
                "Ledger transaction record was not found.");
        }
    }
}