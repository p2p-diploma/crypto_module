using Crypto.Application.Commands.Ethereum;
using Crypto.Application.Handlers.Base;
using Crypto.Application.Responses;
using Crypto.Application.Utils;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.Ethereum;

public class TransferEtherFromP2PWalletHandler 
    : EthereumP2PWalletBaseHandler<TransferEtherFromP2PWalletCommand, TransactionResponse>
{
    private readonly EthereumAccountManager _accountManager;
    public TransferEtherFromP2PWalletHandler(EthereumAccountManager accountManager, 
        IEthereumP2PWalletsRepository<ObjectId> repository) : base(repository)
    {
        _accountManager = accountManager;
    }
    public override async Task<TransactionResponse> Handle(TransferEtherFromP2PWalletCommand request, CancellationToken cancellationToken)
    {
        var id = ObjectId.Parse(request.WalletId);
        var p2pWallet = await _repository.FindOneAndProjectAsync(w => w.Id == id, wallet => wallet, cancellationToken);
        if (p2pWallet == null || p2pWallet.Id == ObjectId.Empty) 
            throw new AccountNotFoundException($"P2P wallet with id {request.WalletId} is not found");
        var scryptService = new KeyStoreScryptService();
        var account = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(p2pWallet.KeyStore), p2pWallet.Hash);
        return await _accountManager.Transfer(request.RecipientAddress, request.Amount, account, cancellationToken);
    }
}