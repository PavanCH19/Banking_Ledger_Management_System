using BankingLedgerApi.Models;
using BankingLedgerApi.Repositories.Interfaces;

namespace BankingLedgerApi.Services;

public class TransactionService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
    }

    public Task<LedgerTransaction> DepositAsync(
        string accountId, decimal amount)
    {
        return ProcessAsync(accountId, amount, false);
    }

    public Task<LedgerTransaction> WithdrawAsync(string accountId, decimal amount)
    {
        return ProcessAsync(accountId, amount, true);
    }

    public async Task<List<LedgerTransaction>> GetHistoryAsync(
        string accountId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);

        if (account is null)
            throw new KeyNotFoundException("Account not found.");

        return await _transactionRepository.GetByAccountIdAsync(accountId);
    }

    private async Task<LedgerTransaction> ProcessAsync(string accountId, decimal amount, bool isWithdrawal)
    {
        // 1. Validate the amount.
        if (amount <= 0 || amount != decimal.Round(amount, 2))
        {
            throw new ArgumentException("Amount must be positive and have at most two decimal places.");
        }

        // 2. Confirm the account exists and is active.
        var account = await _accountRepository.GetByIdAsync(accountId);

        if (account is null) throw new KeyNotFoundException("Account not found.");

        if (account.Status != "Active") throw new InvalidOperationException("Account is not active.");

        // 3. Create an initial ledger entry (Pending until the balance
        //    change is actually applied).
        var transaction = new LedgerTransaction
        {
            AccountId = accountId,
            Type = isWithdrawal ? "Withdrawal" : "Deposit",
            Amount = amount,
            BalanceBefore = account.Balance,
            BalanceAfter = account.Balance,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _transactionRepository.CreateAsync(transaction);

        // 4. Atomically update the account balance. The returned document is
        //    the account state *before* the change, so it is the authoritative
        //    opening balance for this transaction (unlike the earlier read,
        //    which can be stale when requests run concurrently).
        Account? accountBeforeChange;

        try
        {
            accountBeforeChange = await _accountRepository.ApplyBalanceChangeAsync(accountId, amount, isWithdrawal);
        }
        catch
        {
            await MarkFailedAsync(transaction.Id!, account.Balance);
            throw;
        }

        // 5. No match means the balance was not changed (insufficient funds or
        //    the account stopped being active). Never record this as completed,
        //    otherwise the ledger would report money that was never debited.
        if (accountBeforeChange is null)
        {
            await MarkFailedAsync(transaction.Id!, account.Balance);

            throw new InvalidOperationException(
                isWithdrawal
                    ? "Insufficient funds for this withdrawal."
                    : "Account is not active.");
        }

        // 6. Record the successful balance change using the pre-update snapshot.
        var balanceBefore = accountBeforeChange.Balance;
        var balanceAfter = isWithdrawal
            ? balanceBefore - amount
            : balanceBefore + amount;

        await _transactionRepository.UpdateResultAsync(
            transaction.Id!,
            "Completed",
            balanceBefore,
            balanceAfter);

        transaction.BalanceBefore = balanceBefore;
        transaction.BalanceAfter = balanceAfter;
        transaction.Status = "Completed";

        return transaction;
    }

    private Task MarkFailedAsync(string transactionId, decimal balance)
    {
        return _transactionRepository.UpdateResultAsync(
            transactionId,
            "Failed",
            balance,
            balance);
    }
}