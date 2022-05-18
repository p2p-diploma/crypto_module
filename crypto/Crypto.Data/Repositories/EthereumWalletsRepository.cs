using System.Linq.Expressions;
using Crypto.Data.Configuration;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
namespace Crypto.Data.Repositories;

public class EthereumWalletsRepository : IWalletsRepository<EthereumWallet<ObjectId>, ObjectId>
{
    private readonly IMongoDatabase _database;
    private readonly string _collectionName;
    private IMongoCollection<EthereumWallet<ObjectId>> Wallets => _database.GetCollection<EthereumWallet<ObjectId>>(_collectionName);
    public EthereumWalletsRepository(DatabaseSettings settings, IMongoClient client)
    {
        _database = client.GetDatabase(settings.DatabaseName);
        _collectionName = settings.EthereumWalletsCollection;
        if (_database.GetCollection<EthereumWallet<ObjectId>>(_collectionName) == null) _database.CreateCollection(_collectionName);
    }

    public async Task<EthereumWallet<ObjectId>> CreateAsync(EthereumWallet<ObjectId> ethereumWallet, CancellationToken token = default)
    {
        await Wallets.InsertOneAsync(ethereumWallet, new InsertOneOptions { BypassDocumentValidation = false }, token);
        return ethereumWallet;
    }

    public async Task<EthereumWallet<ObjectId>> FindOneAsync(Expression<Func<EthereumWallet<ObjectId>, bool>> expr, 
        CancellationToken token = default) =>
        await Wallets.AsQueryable().FirstOrDefaultAsync(expr, token);

    public async Task<bool> ExistsAsync(Expression<Func<EthereumWallet<ObjectId>, bool>> expr, CancellationToken token = default)
        => await Wallets.AsQueryable().AnyAsync(expr, token);
    
}