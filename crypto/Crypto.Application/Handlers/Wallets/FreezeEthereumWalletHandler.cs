﻿using Crypto.Application.Commands.Wallets;
using Crypto.Application.Handlers.Base;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;

namespace Crypto.Application.Handlers.Wallets;

public class FreezeEthereumWalletHandler : EthereumWalletBaseHandler<FreezeEthereumWalletCommand, bool>
{
    private readonly IEthereumP2PWalletsRepository<ObjectId> _p2PWalletsRepository;
    public FreezeEthereumWalletHandler(IEthereumWalletsRepository<ObjectId> repository, 
        IEthereumP2PWalletsRepository<ObjectId> p2PWalletsRepository) : base(repository)
    {
        _p2PWalletsRepository = p2PWalletsRepository;
    }

    public override async Task<bool> Handle(FreezeEthereumWalletCommand request, CancellationToken cancellationToken)
    {
        var walletId = ObjectId.Parse(request.WalletId);
        if (!await _repository.ExistsAsync(s => s.Id == walletId, cancellationToken))
            throw new AccountNotFoundException($"Wallet with id {walletId} is not found");
        return (await Task.WhenAll(_repository.Freeze(walletId), _p2PWalletsRepository.Freeze(walletId))).All(r => r);
    }
}