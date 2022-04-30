using Crypto.Configuration;
using Crypto.Interfaces;
using Crypto.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Crypto.Data.Repositories;

public class EthereumWalletsRepository : IWalletsRepository<EthereumWallet, ObjectId>
{
    private readonly IMongoDatabase _database;
    private const string COLLECTION_NAME = "ether_accounts";
    private IMongoCollection<EthereumWallet> Accounts => _database.GetCollection<EthereumWallet>(COLLECTION_NAME);
    public EthereumWalletsRepository(IOptions<DatabaseSettings> options, MongoClient client)
    {
        _database = client.GetDatabase(options.Value.DatabaseName);
        if (_database.GetCollection<EthereumWallet>(COLLECTION_NAME) == null) _database.CreateCollection(COLLECTION_NAME);
    }


    public async Task CreateAsync(EthereumWallet ethereumWallet, CancellationToken token = default) => 
        await Accounts.InsertOneAsync(ethereumWallet, new InsertOneOptions { BypassDocumentValidation = false }, token);

    public async Task<EthereumWallet> FindByIdAsync(ObjectId id, CancellationToken token = default) =>
        await Accounts.Find(Builders<EthereumWallet>.Filter.Eq(w => w.Id, id)).FirstOrDefaultAsync(token);

    public async Task<bool> ExistsAsync(string email, CancellationToken token = default)
        => await Accounts.AsQueryable().AnyAsync(a => a.Email == email, token);
}