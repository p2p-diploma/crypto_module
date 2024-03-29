﻿using Crypto.Application.Commands.Ethereum;
using Crypto.Application.Handlers.Base;
using Crypto.Domain.Accounts;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.Ethereum;

public class TransferEtherFromP2PWalletHandler 
    : EthereumP2PWalletBaseHandler<TransferEtherFromP2PWalletCommand, TransactionDetails>
{
    private readonly EthereumAccountManager _accountManager;
    public TransferEtherFromP2PWalletHandler(EthereumAccountManager accountManager, 
        IEthereumP2PWalletsRepository<ObjectId> repository) : base(repository)
    {
        _accountManager = accountManager;
    }
    public override async Task<TransactionDetails> Handle(TransferEtherFromP2PWalletCommand request, CancellationToken cancellationToken)
    {
        var id = ObjectId.Parse(request.WalletId);
        var recipientId = ObjectId.Parse(request.RecipientId);
        var sellerWallet = await _repository.FindOneAsync(w => w.Id == id, wallet => wallet, cancellationToken);
        if (sellerWallet == null) throw new NotFoundException($"P2P wallet with id {id} is not found");
        if (sellerWallet.IsLocked) throw new AccountLockedException(sellerWallet.UnlockDate!.Value);
        if (sellerWallet == null || sellerWallet.Id == ObjectId.Empty) 
            throw new NotFoundException($"P2P wallet with id {request.WalletId} is not found");
        
        string? recipientAddress = await _repository.FindOneAsync(w => w.Id == recipientId, wallet => wallet.KeyStore.Address, cancellationToken);
        if(string.IsNullOrEmpty(recipientAddress))
            throw new NotFoundException($"Recipient P2P wallet with id {request.RecipientId} is not found");
        
        var scryptService = new KeyStoreScryptService();
        var account = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(sellerWallet.KeyStore), sellerWallet.Hash);
        
        return await _accountManager.TransferAsync(recipientAddress, request.Amount, account, cancellationToken);
    }
    
}