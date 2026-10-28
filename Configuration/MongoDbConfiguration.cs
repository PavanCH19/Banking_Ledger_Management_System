
using BankingLedgerApi.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BankingLedgerApi.Database;

public static class MongoDbConfiguration
{
    public static IServiceCollection AddMongoDbConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Read and validate MongoDB settings.
        services
            .AddOptions<MongoDbSettings>()
            .Bind(configuration.GetSection("MongoDb"))
            .Validate(
                settings => !string.IsNullOrWhiteSpace(
                    settings.ConnectionString),
                "MongoDB connection string is required.")
            .Validate(
                settings => !string.IsNullOrWhiteSpace(
                    settings.DatabaseName),
                "MongoDB database name is required.")
            .ValidateOnStart();

        // Register the MongoDB client.
        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var settings = serviceProvider
                .GetRequiredService<IOptions<MongoDbSettings>>()
                .Value;

            return new MongoClient(settings.ConnectionString);
        });

        // Register the selected database.
        services.AddSingleton<IMongoDatabase>(serviceProvider =>
        {
            var settings = serviceProvider
                .GetRequiredService<IOptions<MongoDbSettings>>()
                .Value;

            var client = serviceProvider
                .GetRequiredService<IMongoClient>();

            return client.GetDatabase(settings.DatabaseName);
        });

        return services;
    }
}
