﻿using Crypto.Application.Commands.Ethereum;
using Crypto.Application.Handlers.Base;
using Crypto.Domain.Accounts;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.Ethereum;

public class TransferEtherToP2PWalletHandler : EthereumWalletBaseHandler<TransferEtherToP2PWalletCommand, TransactionDetails>
{
    private readonly EthereumAccountManager _accountManager;
    private readonly IEthereumP2PWalletsRepository<ObjectId> _p2pWalletsRepository;
    public TransferEtherToP2PWalletHandler(EthereumAccountManager accountManager, 
        IEthereumWalletsRepository<ObjectId> repository, 
        IEthereumP2PWalletsRepository<ObjectId> p2pWalletsRepository) : base(repository)
    {
        _accountManager = accountManager;
        _p2pWalletsRepository = p2pWalletsRepository;
    }
    public override async Task<TransactionDetails> Handle(TransferEtherToP2PWalletCommand request, CancellationToken cancellationToken)
    {
        var userId = ObjectId.Parse(request.WalletId);
        var userWallet = await _repository.FindOneAsync(w => w.Id == userId, wallet => wallet, cancellationToken);
        if (userWallet == null) throw new NotFoundException($"Wallet with id {userId} is not found");
        if (userWallet.IsLocked) throw new AccountLockedException(userWallet.UnlockDate!.Value);
        if (userWallet == null || userWallet.Id == ObjectId.Empty)
            throw new ArgumentException($"Wallet with id {request.WalletId} does not exist");
        
        var p2pWallet = await _p2pWalletsRepository.FindOneAsync(w => w.Id == userId, wallet => wallet, cancellationToken);
        if (p2pWallet == null) throw new NotFoundException($"P2P wallet with id {userId} is not found");
        var scryptService = new KeyStoreScryptService();
        var account = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(userWallet.KeyStore), userWallet.Hash);
        return await _accountManager.TransferAsync(p2pWallet.KeyStore.Address, request.Amount, account, cancellationToken);
    }

}