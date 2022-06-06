using System.Linq.Expressions;
using Crypto.Data.Configuration;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using Crypto.Domain.Models.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
namespace Crypto.Data.Repositories;

public class EthereumWalletsRepository : IEthereumWalletsRepository<ObjectId>
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

    public async Task<TProjection?> FindOneAndProjectAsync<TProjection>(Expression<Func<EthereumWallet<ObjectId>, bool>> expr,
        Expression<Func<EthereumWallet<ObjectId>, TProjection>> projection, CancellationToken token = default) =>
        await Wallets.AsQueryable().Where(expr).Select(projection).FirstOrDefaultAsync(token);

    public async Task<bool> ExistsAsync(Expression<Func<EthereumWallet<ObjectId>, bool>> expr, CancellationToken token = default)
        => await Wallets.AsQueryable().AnyAsync(expr, token);

    public async Task<EthereumWallet<ObjectId>> UpdateAsync(EthereumWallet<ObjectId> wallet, CancellationToken token = default)
    {
        return await Wallets.FindOneAndReplaceAsync(Builders<EthereumWallet<ObjectId>>.Filter.Eq(w => w.Id, wallet.Id), wallet,
            options: new FindOneAndReplaceOptions<EthereumWallet<ObjectId>> { ReturnDocument = ReturnDocument.After },
            cancellationToken:token);
    }

    public async Task<bool> Freeze(string email)
    {
        var freezeDef = Builders<EthereumWallet<ObjectId>>.Update.Set(w => w.IsFrozen, true)
            .Set(w => w.DateOfUnfreeze, DateTime.Now + new TimeSpan(30, 0, 0, 0));
        var result = await Wallets.UpdateOneAsync(Builders<EthereumWallet<ObjectId>>.Filter.Eq(w => w.Email, email),
            freezeDef);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
    
    public async Task<bool> Unfreeze(ObjectId walletId)
    {
        var freezeDef = Builders<EthereumWallet<ObjectId>>.Update.Set(w => w.IsFrozen, false).Set(w => w.DateOfUnfreeze, null);
        var result = await Wallets.UpdateOneAsync(Builders<EthereumWallet<ObjectId>>.Filter.Eq(w => w.Id, walletId),
            freezeDef);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
}