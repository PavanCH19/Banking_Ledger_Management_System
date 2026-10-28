
using BankingLedgerApi.DTOs;
using BankingLedgerApi.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Banking_Ledger_Management_System.Controllers;

[ApiController]
[Route("api/v1/accounts")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionService _transactionService;

    public TransactionsController(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    // POST: /api/v1/accounts/{id}/deposit
    [HttpPost("{id}/deposit")]
    public async Task<IActionResult> Deposit(
        string id,
        [FromBody] DepositRequest request)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid account ID."
            });
        }

        try
        {
            var transaction = await _transactionService.DepositAsync(id, request.Amount);

            return Ok(new
            {
                success = true,
                message = "Deposit successful.",
                transaction
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { success = false, message = ex.Message });
        }
    }

    // POST: /api/v1/accounts/{id}/withdraw
    [HttpPost("{id}/withdraw")]
    public async Task<IActionResult> Withdraw(
        string id,
        [FromBody] WithdrawRequest request)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid account ID."
            });
        }

        try
        {
            var transaction = await _transactionService.WithdrawAsync(id, request.Amount);

            return Ok(new
            {
                success = true,
                message = "Withdrawal successful.",
                transaction
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { success = false, message = ex.Message });
        }
    }

    // GET: /api/v1/accounts/{id}/transactions
    [HttpGet("{id}/transactions")]
    public async Task<IActionResult> GetHistory(string id)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid account ID."
            });
        }

        try
        {
            var transactions =
                await _transactionService.GetHistoryAsync(id);

            return Ok(new
            {
                success = true,
                count = transactions.Count,
                transactions
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }
}
