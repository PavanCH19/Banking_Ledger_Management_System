using BankingLedgerApi.DTOs;
using BankingLedgerApi.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace BankingLedgerApi.Controllers;

[ApiController]
[Route("api/v1/customers")]
public class CustomersController : ControllerBase
{
    private readonly CustomerService _customerService;
    private readonly AccountService _accountService;

    public CustomersController(CustomerService customerService, AccountService accountService)
    {
        _customerService = customerService;
        _accountService = accountService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        try
        {
            var customer = await _customerService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetCustomerById),
                new { id = customer.Id },
                customer);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerById(string id)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid customer ID."
            });
        }

        var customer = await _customerService.GetByIdAsync(id);

        if (customer is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Customer not found."
            });
        }

        return Ok(customer);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCustomers()
    {
        var customers = await _customerService.GetAllAsync();

        return Ok(customers);
    }

    [HttpGet("{id}/accounts")]
    public async Task<IActionResult> GetCustomerAccounts(string id)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid customer ID."
            });
        }

        var customer = await _customerService.GetByIdAsync(id);

        if (customer is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Customer not found."
            });
        }

        var accounts = await _accountService.GetByCustomerIdAsync(id);

        return Ok(accounts);
    }
}