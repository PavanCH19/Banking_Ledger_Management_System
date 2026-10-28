using BankingLedgerApi.Models;
using BankingLedgerApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace BankingLedgerApi.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly IMongoCollection<Customer> _customers;

    public CustomerRepository(IMongoDatabase database)
    {
        _customers = database.GetCollection<Customer>("Customers");
    }

    public async Task<Customer?> GetByIdAsync(string id)
    {
        return await _customers
            .Find(customer => customer.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _customers
            .Find(customer => customer.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _customers
            .Find(_ => true)
            .ToListAsync();
    }

    public async Task CreateAsync(Customer customer)
    {
        await _customers.InsertOneAsync(customer);
    }
}