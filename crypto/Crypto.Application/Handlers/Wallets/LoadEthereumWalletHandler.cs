using Crypto.Application.Commands.Wallets;
using Crypto.Application.Responses.Ethereum;
using Crypto.Application.Utils;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MediatR;
using MongoDB.Bson;
using Nethereum.Web3.Accounts;
using static BCrypt.Net.BCrypt;
namespace Crypto.Application.Handlers.Wallets;

public class LoadEthereumWalletHandler : IRequestHandler<LoadEthereumWalletCommand, CreatedEthereumWalletResponse>
{
    private readonly IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> _platformWalletsRepository;
    private readonly IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> _p2pWalletsRepository;
    private readonly EthereumAccountManager _accountManager;

    public LoadEthereumWalletHandler(IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> p2pWalletsRepository,
        IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> platformWalletsRepository,
        EthereumAccountManager accountManager)
    {
        _p2pWalletsRepository = p2pWalletsRepository;
        _platformWalletsRepository = platformWalletsRepository;
        _accountManager = accountManager;
    }

    public async Task<CreatedEthereumWalletResponse> Handle(LoadEthereumWalletCommand command, CancellationToken token)
    {
        if (await _platformWalletsRepository.ExistsAsync(w => w.Email == command.Email, token))
            throw new ArgumentException($"Wallet with email {command.Email} already exists");
        
        var loadedAccount = new Account(command.PrivateKey, _accountManager.ChainId);
        if (string.IsNullOrWhiteSpace(loadedAccount.Address))
            throw new AccountNotFoundException("Specified private key is invalid: can't load account");

        var walletId = ObjectId.GenerateNewId(DateTime.Now);
        var passwordHash = HashPassword(command.Password);
        var createdWallets = await Task.WhenAll(LoadPlatformWallet(command, walletId, passwordHash, token),
            CreateP2PWallet(walletId, passwordHash, command.Email, token));
        return createdWallets[0];
    }
    
    private async Task<CreatedEthereumWalletResponse> LoadPlatformWallet(LoadEthereumWalletCommand request, ObjectId walletId, string hash, CancellationToken token)
    {
        var loadedAccount = new Account(request.PrivateKey, _accountManager.ChainId);
        if (string.IsNullOrWhiteSpace(loadedAccount.Address))
            throw new AccountNotFoundException("Specified private key is invalid: can't load account");
        
        var keyStore = EthereumAccountManager.GenerateKeyStoreFromKey(hash, loadedAccount.PrivateKey);
        EthereumWallet<ObjectId> wallet = new() { Email = request.Email, Hash = hash, KeyStore = keyStore, Id = walletId };
        await _platformWalletsRepository.CreateAsync(wallet, token);
        return new(keyStore.Address, loadedAccount.PrivateKey, walletId.ToString());
    }
    private async Task<CreatedEthereumWalletResponse> CreateP2PWallet(ObjectId walletId, string hash, string email, CancellationToken token)
    {
        var keyStore = EthereumAccountManager.GenerateKeyStore(hash, out _);
        EthereumP2PWallet<ObjectId> wallet = new()
            { Hash = hash, KeyStore = keyStore, Id = walletId, Email = email };
        await _p2pWalletsRepository.CreateAsync(wallet, token);
        return null;
    }
}