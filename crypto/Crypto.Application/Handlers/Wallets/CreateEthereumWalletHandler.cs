using Crypto.Application.Commands.Wallets;
using Crypto.Application.Handlers.Base;
using Crypto.Application.Responses.Ethereum;
using Crypto.Domain.Accounts;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using static BCrypt.Net.BCrypt;

namespace Crypto.Application.Handlers.Wallets;

public class CreateEthereumWalletHandler : EthereumWalletBaseHandler<CreateEthereumWalletCommand, CreatedEthereumWalletResponse>
{
    private readonly IEthereumP2PWalletsRepository<ObjectId> _p2pWalletsRepository;
    private readonly EthereumAccountManager _accountManager;

    public CreateEthereumWalletHandler(IEthereumWalletsRepository<ObjectId> repository, 
        IEthereumP2PWalletsRepository<ObjectId> p2PWalletsRepository, EthereumAccountManager accountManager) : base(repository)
    {
        _p2pWalletsRepository = p2PWalletsRepository;
        _accountManager = accountManager;
    }
    public override async Task<CreatedEthereumWalletResponse> Handle(CreateEthereumWalletCommand command, CancellationToken token)
    {
        if (await _repository.ExistsAsync(w => w.Email == command.Email, token))
            throw new ArgumentException($"Wallet with email {command.Email} already exists");
        var walletId = ObjectId.GenerateNewId(DateTime.Now);
        var passwordHash = HashPassword(command.Password);
        var createdWallets = await Task.WhenAll(CreatePlatformWallet(command, walletId, passwordHash, token), CreateP2PWallet(walletId, passwordHash, command.Email, token));
        return createdWallets[0];
    }

    private async Task<CreatedEthereumWalletResponse> CreatePlatformWallet(CreateEthereumWalletCommand request, ObjectId walletId, string hash, CancellationToken token)
    {
        var keyStore = _accountManager.GenerateKeyStore(hash, out string privateKey);
        EthereumWallet<ObjectId> wallet = new() { Email = request.Email, Hash = hash, KeyStore = keyStore, Id = walletId };
        await _repository.CreateAsync(wallet, token);
        return new(keyStore.Address, privateKey, walletId.ToString());
    }
    private async Task<CreatedEthereumWalletResponse> CreateP2PWallet(ObjectId walletId, string hash, string email, CancellationToken token)
    {
        var keyStore = _accountManager.GenerateKeyStore(hash, out _);
        EthereumP2PWallet<ObjectId> wallet = new() { Hash = hash, KeyStore = keyStore, Id = walletId, Email = email };
        await _p2pWalletsRepository.CreateAsync(wallet, token);
        return null;
    }
}