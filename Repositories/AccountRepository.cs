using BankingLedgerApi.Models;
using BankingLedgerApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace BankingLedgerApi.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly IMongoCollection<Account> _accounts;

    public AccountRepository(IMongoDatabase database)
    {
        _accounts = database.GetCollection<Account>("Accounts");
    }

    public async Task<Account?> GetByIdAsync(string id)
    {
        return await _accounts
            .Find(account => account.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<Account?> GetByAccountNumberAsync(
        string accountNumber)
    {
        return await _accounts
            .Find(account => account.AccountNumber == accountNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Account>> GetByCustomerIdAsync(
        string customerId)
    {
        return await _accounts
            .Find(account => account.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task CreateAsync(Account account)
    {
        await _accounts.InsertOneAsync(account);
    }

    public async Task<Account?> ApplyBalanceChangeAsync(
        string accountId,
        decimal amount,
        bool isWithdrawal)
    {
        var filter = Builders<Account>.Filter.And(
            Builders<Account>.Filter.Eq(a => a.Id, accountId),
            Builders<Account>.Filter.Eq(a => a.Status, "Active"));

        if (isWithdrawal)
        {
            // Never allow the balance to drop below zero.
            filter = Builders<Account>.Filter.And(
                filter,
                Builders<Account>.Filter.Gte(a => a.Balance, amount));
        }

        var update = isWithdrawal
            ? Builders<Account>.Update.Inc(a => a.Balance, -amount)
            : Builders<Account>.Update.Inc(a => a.Balance, amount);

        var options = new FindOneAndUpdateOptions<Account>
        {
            ReturnDocument = ReturnDocument.Before
        };

        // Returns null when the filter did not match, i.e. the balance
        // change was rejected and must not be recorded as completed.
        return await _accounts.FindOneAndUpdateAsync(filter, update, options);
    }
}