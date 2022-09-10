using Crypto.Application.Commands.ERC20;
using Crypto.Application.Handlers.Base;
using Crypto.Domain.Accounts;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.ERC20;

public class TransferERC20FromP2PWalletHandler : EthereumP2PWalletBaseHandler<TransferERC20FromP2PWalletCommand, TransactionDetails>
{
    private readonly Erc20AccountManager _accountManager;

    public TransferERC20FromP2PWalletHandler(Erc20AccountManager accountManager, 
        IEthereumP2PWalletsRepository<ObjectId> repository) : base(repository)
    {
        _accountManager = accountManager;
    }
    public override async Task<TransactionDetails> Handle(TransferERC20FromP2PWalletCommand request, CancellationToken cancellationToken)
    {
        var parsedId = ObjectId.Parse(request.WalletId);
        var recipientId = ObjectId.Parse(request.RecipientId);
        var p2pWallet = await _repository.FindOneAsync(w => w.Id == parsedId, wallet => wallet, cancellationToken);
        if (p2pWallet == null) throw new NotFoundException($"P2P wallet with id {parsedId} is not found");
        if (p2pWallet.IsLocked) throw new AccountLockedException(p2pWallet.UnlockDate!.Value);
        if (p2pWallet == null || p2pWallet.Id == ObjectId.Empty) 
            throw new NotFoundException($"P2P wallet with id {request.WalletId} is not found");
        
        string? recipientAddress = await _repository.FindOneAsync(w => w.Id == recipientId, wallet => wallet.KeyStore.Address, cancellationToken);
        if(string.IsNullOrEmpty(recipientAddress))
            throw new NotFoundException($"Recipient P2P wallet with id {request.RecipientId} is not found");
        
        var scryptService = new KeyStoreScryptService();
        var account = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(p2pWallet.KeyStore), p2pWallet.Hash);
        return await _accountManager.TransferAsync(recipientAddress, request.Amount, account, cancellationToken);
    }
}