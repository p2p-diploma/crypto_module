using Crypto.Application.Commands.Ethereum;
using Crypto.Application.Handlers.Base;
using Crypto.Application.Utils;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.Ethereum;

public class TransferEtherFromP2PWalletHandler : EthereumTransferHandlerBase<TransferEtherFromP2PWalletCommand, bool>
{
    private readonly IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> _repository;
    public TransferEtherFromP2PWalletHandler(EthereumAccountManager accountManager,
        IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> repository) : base(accountManager)
    {
        _repository = repository;
    }

    public override async Task<bool> Handle(TransferEtherFromP2PWalletCommand request, CancellationToken cancellationToken)
    {
        var id = ObjectId.Parse(request.WalletId);
        var p2pWallet = await _repository.FindOneAsync(w => w.UserWalletId == id, cancellationToken);
        if (p2pWallet == null || p2pWallet.Id == ObjectId.Empty) 
            throw new AccountNotFoundException($"P2P wallet with id {request.WalletId} is not found");
        var scryptService = new KeyStoreScryptService();
        var account = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(p2pWallet.KeyStore), p2pWallet.Hash);
        return await Transfer(request.RecipientAddress, request.Amount, account, cancellationToken);
    }

}