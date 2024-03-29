﻿using System.Linq.Expressions;
using Crypto.Data.Configuration;
using Crypto.Domain.Configuration;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
namespace Crypto.Data.Repositories;

public class EthereumP2PWalletsRepository : IEthereumP2PWalletsRepository<ObjectId>
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

    public async Task<TProjection?> FindOneAsync<TProjection>(Expression<Func<EthereumP2PWallet<ObjectId>, bool>> expr, 
        Expression<Func<EthereumP2PWallet<ObjectId>, TProjection>> projection, CancellationToken token = default)
        => await P2PWallets.AsQueryable().Where(expr).Select(projection).FirstOrDefaultAsync(token);

    public async Task<bool> ExistsAsync(Expression<Func<EthereumP2PWallet<ObjectId>, bool>> expr, CancellationToken token = default)
        => await P2PWallets.AsQueryable().AnyAsync(expr, token);

    public async Task<EthereumP2PWallet<ObjectId>> UpdateAsync(EthereumP2PWallet<ObjectId> wallet, CancellationToken token = default)
    {
        return await P2PWallets.FindOneAndReplaceAsync(Builders<EthereumP2PWallet<ObjectId>>.Filter.Eq(w => w.Id, wallet.Id), wallet,
            options: new FindOneAndReplaceOptions<EthereumP2PWallet<ObjectId>> { ReturnDocument = ReturnDocument.After },
            cancellationToken:token);
    }

    public async Task<EthereumP2PWallet<ObjectId>> UpdateAmountToBuyAsync(ObjectId walletId, decimal amount,
     string currencyType, CancellationToken token)
    {
        if (!await ExistsAsync(w => w.Id == walletId, token))
            throw new NotFoundException($"P2P wallet with id {walletId} is not found");
        UpdateDefinition<EthereumP2PWallet<ObjectId>> updateDefinition;
        switch (currencyType)
        {
            case CurrencyType.ETH:
                updateDefinition = Builders<EthereumP2PWallet<ObjectId>>.Update.Set(w => w.EthToBuy, amount);
                break;
            case CurrencyType.ERC20:
                updateDefinition = Builders<EthereumP2PWallet<ObjectId>>.Update.Set(w => w.Erc20ToBuy, amount);
                break;
            default:
                return null;
        }
        return await P2PWallets.FindOneAndUpdateAsync(Builders<EthereumP2PWallet<ObjectId>>.Filter.Eq(w => w.Id, walletId),
            updateDefinition, options: new FindOneAndUpdateOptions<EthereumP2PWallet<ObjectId>>{ReturnDocument = ReturnDocument.After}, 
            cancellationToken: token);
    }

    public async Task<EthereumP2PWallet<ObjectId>> UpdateAmountToSellAsync(ObjectId walletId, decimal amount,
     string currencyType, CancellationToken token)
    {
        UpdateDefinition<EthereumP2PWallet<ObjectId>> updateDefinition;
        switch (currencyType)
        {
            case CurrencyType.ETH:
                updateDefinition = Builders<EthereumP2PWallet<ObjectId>>.Update.Set(w => w.EthToSell, amount);
                break;
            case CurrencyType.ERC20:
                updateDefinition = Builders<EthereumP2PWallet<ObjectId>>.Update.Set(w => w.Erc20ToSell, amount);
                break;
            default:
                return null;
        }
        return await P2PWallets.FindOneAndUpdateAsync(Builders<EthereumP2PWallet<ObjectId>>.Filter.Eq(w => w.Id, walletId),
            updateDefinition, options: new FindOneAndUpdateOptions<EthereumP2PWallet<ObjectId>>{ReturnDocument = ReturnDocument.After}, 
            cancellationToken: token);
    }

    public async Task<bool> Lock(string email)
    {
        var lockDef = Builders<EthereumP2PWallet<ObjectId>>.Update.Set(w => w.IsLocked, true)
            .Set(w => w.UnlockDate, DateTime.Now + new TimeSpan(30, 0, 0, 0));
        var result = await P2PWallets.UpdateOneAsync(Builders<EthereumP2PWallet<ObjectId>>.Filter.Eq(w => w.Email, email),
            lockDef);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> Unlock(ObjectId walletId)
    {
        var lockDef = Builders<EthereumP2PWallet<ObjectId>>.Update.Set(w => w.IsLocked, false).Set(w => w.UnlockDate, null);
        var result = await P2PWallets.UpdateOneAsync(Builders<EthereumP2PWallet<ObjectId>>.Filter.Eq(w => w.Id, walletId),
            lockDef);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
}