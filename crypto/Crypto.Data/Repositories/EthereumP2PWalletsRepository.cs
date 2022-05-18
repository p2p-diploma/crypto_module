using System.Linq.Expressions;
using Crypto.Data.Configuration;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
namespace Crypto.Data.Repositories;

public class EthereumP2PWalletsRepository : IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId>
{
    private readonly IMongoDatabase _database;
    private readonly string _collectionName;
    private IMongoCollection<EthereumP2PWallet<ObjectId>> P2PWallets => _database.GetCollection<EthereumP2PWallet<ObjectId>>(_collectionName);
    public EthereumP2PWalletsRepository(DatabaseSettings settings, IMongoClient client)
    {
        _database = client.GetDatabase(settings.DatabaseName);
        _collectionName = settings.EthereumP2PWalletsCollection;
        if (_database.GetCollection<EthereumP2PWallet<ObjectId>>(_collectionName) == null) _database.CreateCollection(_collectionName);
    }

    public async Task<EthereumP2PWallet<ObjectId>> CreateAsync(EthereumP2PWallet<ObjectId> wallet,
        CancellationToken token = default)
    {
        await P2PWallets.InsertOneAsync(wallet, new InsertOneOptions { BypassDocumentValidation = false }, token);
        return wallet;
    }
    public async Task<EthereumP2PWallet<ObjectId>> FindOneAsync(Expression<Func<EthereumP2PWallet<ObjectId>, bool>> expr, 
        CancellationToken token = default) =>
        await P2PWallets.AsQueryable().FirstOrDefaultAsync(expr, token);

    public async Task<bool> ExistsAsync(Expression<Func<EthereumP2PWallet<ObjectId>, bool>> expr, CancellationToken token = default)
        => await P2PWallets.AsQueryable().AnyAsync(expr, token);
}