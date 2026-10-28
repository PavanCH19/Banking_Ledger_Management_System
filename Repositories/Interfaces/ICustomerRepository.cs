using BankingLedgerApi.Models;

namespace BankingLedgerApi.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(string id);

    Task<Customer?> GetByEmailAsync(string email);

    Task<List<Customer>> GetAllAsync();

    Task CreateAsync(Customer customer);
}