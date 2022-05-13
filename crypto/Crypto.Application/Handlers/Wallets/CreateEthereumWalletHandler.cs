using Crypto.Application.Commands.Ethereum;
using Crypto.Application.Commands.Wallets;
using Crypto.Application.Responses.Ethereum;
using Crypto.Application.Utils;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MediatR;
using MongoDB.Bson;
using static BCrypt.Net.BCrypt;

namespace Crypto.Application.Handlers.Wallets;

public class CreateEthereumWalletHandler : IRequestHandler<CreateEthereumWalletCommand, CreatedEthereumWalletResponse>
{
    private readonly IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> _platformWalletsRepository;
    private readonly IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> _p2pWalletsRepository;
    
    public CreateEthereumWalletHandler(IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> platformWalletsRepository, 
        IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> p2PWalletsRepository)
    {
        _platformWalletsRepository = platformWalletsRepository;
        _p2pWalletsRepository = p2PWalletsRepository;
    }

    public async Task<CreatedEthereumWalletResponse> Handle(CreateEthereumWalletCommand request, CancellationToken token)
    {
        if (await _platformWalletsRepository.ExistsAsync(w => w.Email == request.Email, token))
            throw new ArgumentException($"Wallet with email {request.Email} already exists");
        var walletId = ObjectId.GenerateNewId(DateTime.Now);
        var passwordHash = HashPassword(request.Password);
        var createdWallets = await Task.WhenAll(CreatePlatformWallet(request, walletId, passwordHash, token), CreateP2PWallet(walletId, passwordHash, token));
        return createdWallets[0];
    }

    private async Task<CreatedEthereumWalletResponse> CreatePlatformWallet(CreateEthereumWalletCommand request, ObjectId walletId, string hash, CancellationToken token)
    {
        var keyStore = EthereumAccountManager.GenerateKeyStore(hash, out string privateKey);
        EthereumWallet<ObjectId> wallet = new()
            { Email = request.Email, Hash = hash, KeyStore = keyStore, Id = walletId };
        await _platformWalletsRepository.CreateAsync(wallet, token);
        return new(keyStore.Address, privateKey, walletId.ToString());
    }
    private async Task<CreatedEthereumWalletResponse> CreateP2PWallet(ObjectId walletId, string hash, CancellationToken token)
    {
        var keyStore = EthereumAccountManager.GenerateKeyStore(hash, out _);
        EthereumP2PWallet<ObjectId> wallet = new()
            { Hash = hash, KeyStore = keyStore, Id = ObjectId.GenerateNewId(), UserId = walletId };
        await _p2pWalletsRepository.CreateAsync(wallet, token);
        return null;
    }
}