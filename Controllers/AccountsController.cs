using BankingLedgerApi.DTOs;
using BankingLedgerApi.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace BankingLedgerApi.Controllers;

[ApiController]
[Route("api/v1/accounts")]
public class AccountsController : ControllerBase
{
    private readonly AccountService _accountService;

    public AccountsController(AccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        try
        {
            var account = await _accountService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetAccountById),
                new { id = account.Id },
                account);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById(string id)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid account ID."
            });
        }

        var account = await _accountService.GetByIdAsync(id);

        if (account is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Account not found."
            });
        }

        return Ok(account);
    }

    [HttpGet("{id}/balance")]
    public async Task<IActionResult> GetBalance(string id)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid account ID."
            });
        }

        var account = await _accountService.GetByIdAsync(id);

        if (account is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Account not found."
            });
        }

        return Ok(new
        {
            accountId = account.Id,
            accountNumber = account.AccountNumber,
            balance = account.Balance,
            currency = account.Currency
        });
    }

}