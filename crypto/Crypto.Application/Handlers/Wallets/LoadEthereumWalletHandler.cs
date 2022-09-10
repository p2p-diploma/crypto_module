using Crypto.Application.Commands.Wallets;
using Crypto.Application.Handlers.Base;
using Crypto.Application.Responses.Ethereum;
using Crypto.Domain.Accounts;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Nethereum.Web3.Accounts;
using static BCrypt.Net.BCrypt;
namespace Crypto.Application.Handlers.Wallets;

public class LoadEthereumWalletHandler : EthereumWalletBaseHandler<LoadEthereumWalletCommand, CreatedEthereumWalletResponse>
{
    private readonly IEthereumP2PWalletsRepository<ObjectId> _p2pWalletsRepository;
    private readonly EthereumAccountManager _accountManager;
    private readonly ILogger<LoadEthereumWalletHandler> _logger;
 public LoadEthereumWalletHandler(IEthereumWalletsRepository<ObjectId> repository,
     EthereumAccountManager accountManager, IEthereumP2PWalletsRepository<ObjectId> p2PWalletsRepository,
     ILogger<LoadEthereumWalletHandler> logger) : base(repository)
     {
         _accountManager = accountManager;
         _p2pWalletsRepository = p2PWalletsRepository;
         _logger = logger;
     }

    public override async Task<CreatedEthereumWalletResponse> Handle(LoadEthereumWalletCommand command, CancellationToken token)
    {
        if (await _repository.ExistsAsync(w => w.Email == command.Email, token))
            throw new ArgumentException($"Wallet with email {command.Email} already exists");
        Account loadedAccount = new Account(command.PrivateKey, _accountManager.ChainId);
        _logger.LogInformation($"Loaded account for {command.Email} with private key {loadedAccount.PrivateKey}, address {loadedAccount.Address}");
        var walletId = ObjectId.GenerateNewId(DateTime.Now);
        var passwordHash = HashPassword(command.Password);
        var createdWallets = await Task.WhenAll(LoadPlatformWallet(loadedAccount, walletId, command.Email, passwordHash, token),
            CreateP2PWallet(walletId, passwordHash, command.Email, token));
        return createdWallets[0];
    }
    
    private async Task<CreatedEthereumWalletResponse> LoadPlatformWallet(Account loadedAccount, ObjectId walletId, string email, string hash, CancellationToken token)
    {
        var keyStore = _accountManager.GenerateKeyStoreFromKey(hash, loadedAccount.PrivateKey);
        EthereumWallet<ObjectId> wallet = new() { Email = email, Hash = hash, KeyStore = keyStore, Id = walletId };
        await _repository.CreateAsync(wallet, token);
        return new(keyStore.Address, loadedAccount.PrivateKey, walletId.ToString());
    }
    private async Task<CreatedEthereumWalletResponse> CreateP2PWallet(ObjectId walletId, string hash, string email, CancellationToken token)
    {
        var keyStore = _accountManager.GenerateKeyStore(hash, out _);
        EthereumP2PWallet<ObjectId> wallet = new()
            { Hash = hash, KeyStore = keyStore, Id = walletId, Email = email };
        await _p2pWalletsRepository.CreateAsync(wallet, token);
        return null;
    }
}