
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BankingLedgerApi.Controllers;

[ApiController]
[Route("api/health")]
public class MongoDbHealthController : ControllerBase
{
    private readonly IMongoDatabase _database;

    public MongoDbHealthController(IMongoDatabase database)
    {
        _database = database;
    }

    [HttpGet("mongodb")]
    public async Task<IActionResult> CheckMongoDb()
    {
        try
        {
            await _database.RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1));

            return Ok(new
            {
                success = true,
                message = "MongoDB connection successful.",
                database = _database.DatabaseNamespace.DatabaseName
            });
        }
        catch (MongoException)
        {
            return StatusCode(503, new
            {
                success = false,
                message = "MongoDB is currently unavailable."
            });
        }
    }

    [HttpGet("transactions-support")]
    public async Task<IActionResult> CheckTransactionSupport()
    {
        try
        {
            var result = await _database.RunCommandAsync<BsonDocument>(
                new BsonDocument("hello", 1));

            bool isReplicaSet = result.Contains("setName");
            bool isShardedCluster =
                result.TryGetValue("msg", out var msg) &&
                msg == "isdbgrid";

            return Ok(new
            {
                success = true,
                replicaSet = isReplicaSet,
                shardedCluster = isShardedCluster,
                supportsMultiDocumentTransactions =
                    isReplicaSet || isShardedCluster,
                message = isReplicaSet || isShardedCluster
                    ? "MongoDB advertises transaction-capable topology."
                    : "MongoDB appears to be standalone."
            });
        }
        catch (MongoException)
        {
            return StatusCode(503, new
            {
                success = false,
                message = "Unable to check MongoDB configuration."
            });
        }
    }
}
