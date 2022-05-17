using Crypto.Application.Commands.ERC20;
using Crypto.Application.Handlers.Base;
using Crypto.Application.Utils;
using Crypto.Domain.Configuration;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.ERC20;

public class TransferERC20FromP2PWalletHandler : ERC20TransferHandlerBase<TransferERC20FromP2PWalletCommand,bool>
{
    private readonly IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> _repository;
    public TransferERC20FromP2PWalletHandler(EthereumAccountManager accountManager, SmartContractSettings settings, 
        IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> repository) : base(accountManager, settings)
    {
        _repository = repository;
    }

    public override async Task<bool> Handle(TransferERC20FromP2PWalletCommand request, CancellationToken cancellationToken)
    {
        var parsedId = ObjectId.Parse(request.WalletId);
        var p2pWallet = await _repository.FindOneAsync(w => w.Id == parsedId, cancellationToken);
        if (p2pWallet == null || p2pWallet.Id == ObjectId.Empty) 
            throw new AccountNotFoundException($"P2P wallet with id {request.WalletId} is not found");
        var scryptService = new KeyStoreScryptService();
        var account = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(p2pWallet.KeyStore), p2pWallet.Hash);
        return await Transfer(request.RecipientAddress, request.Amount, account, cancellationToken);
    }
}