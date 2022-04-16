using EthereumTransferGrpc.Configuration;
using EthereumTransferGrpc.Documents;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EthereumTransferGrpc.Data;

public class EthereumWalletsRepository
{
    private readonly IMongoDatabase _database;
    private const string _collectionName = "ether_accounts";
    private IMongoCollection<EthereumWallet> Accounts => _database.GetCollection<EthereumWallet>(_collectionName);
    public EthereumWalletsRepository(IOptions<DatabaseSettings> options, MongoClient client)
    {
        _database = client.GetDatabase(options.Value.DatabaseName);
        if (_database.GetCollection<EthereumWallet>(_collectionName) == null) _database.CreateCollection(_collectionName);
    }


    public async Task CreateAsync(EthereumWallet ethereumWallet, CancellationToken token = default) => 
        await Accounts.InsertOneAsync(ethereumWallet, new InsertOneOptions
        {
            BypassDocumentValidation = false
        }, token);

    public async Task<EthereumWallet> FindByIdAsync(ObjectId id, CancellationToken token = default) =>
        await Accounts.Find(Builders<EthereumWallet>.Filter.Eq(w => w.Id, id)).FirstOrDefaultAsync(token);

    public async Task<bool> WalletExistsAsync(string email, CancellationToken token = default)
        => await Accounts.AsQueryable().AnyAsync(a => a.Email == email, token);
}