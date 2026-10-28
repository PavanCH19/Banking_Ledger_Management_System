using BankingLedgerApi.DTOs;
using BankingLedgerApi.Models;
using BankingLedgerApi.Repositories.Interfaces;

namespace BankingLedgerApi.Services;

public class CustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Customer> CreateAsync(CreateCustomerRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existingCustomer = await _customerRepository.GetByEmailAsync(normalizedEmail);

        if (existingCustomer is not null)
        {
            throw new InvalidOperationException("A customer with this email already exists.");
        }

        var customer = new Customer
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            Phone = request.Phone.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _customerRepository.CreateAsync(customer);

        return customer;
    }

    public async Task<Customer?> GetByIdAsync(string id)
    {
        return await _customerRepository.GetByIdAsync(id);
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _customerRepository.GetAllAsync();
    }
}