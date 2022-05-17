using Crypto.Application.Commands.Ethereum;
using Crypto.Application.Handlers.Base;
using Crypto.Application.Utils;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.Ethereum;

public class TransferEtherToP2PWalletHandler : EthereumTransferHandlerBase<TransferEtherToP2PWalletCommand, bool>
{
    private readonly IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> _p2pWalletsRepository;
    private readonly IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> _platformWalletsRepository;
    public TransferEtherToP2PWalletHandler(EthereumAccountManager accountManager, 
        IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> platformWalletsRepository, 
        IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> p2PWalletsRepository) 
        : base(accountManager)
    {
        _platformWalletsRepository = platformWalletsRepository;
        _p2pWalletsRepository = p2PWalletsRepository;
    }

    public override async Task<bool> Handle(TransferEtherToP2PWalletCommand request, CancellationToken cancellationToken)
    {
        var userId = ObjectId.Parse(request.WalletId);
        var userWallet = await _platformWalletsRepository.FindOneAsync(w => w.Id == userId, cancellationToken);
        if (userWallet == null || userWallet.Id == ObjectId.Empty)
            throw new ArgumentException($"Wallet with id {request.WalletId} does not exist");
        
        var p2pWallet = await _p2pWalletsRepository.FindOneAsync(w => w.UserWalletId == userId, cancellationToken);
        var scryptService = new KeyStoreScryptService();
        var account = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(userWallet.KeyStore), userWallet.Hash);
        return await Transfer(p2pWallet.KeyStore.Address, request.Amount, account, cancellationToken);
    }
}