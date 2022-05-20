﻿using Crypto.Application.Commands.Wallets.Buy;
using Crypto.Application.Handlers.Base;
using Crypto.Domain.Configuration;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;

namespace Crypto.Application.Handlers.Wallets.Buy;

public class IncreaseAmountToBuyHandler : EthereumP2PWalletBaseHandler<IncreaseAmountToBuyCommand, decimal>
{
    public IncreaseAmountToBuyHandler(IEthereumP2PWalletsRepository<ObjectId> repository) : base(repository)
    {
    }

    public override async Task<decimal> Handle(IncreaseAmountToBuyCommand request, CancellationToken cancellationToken)
    {
        var id = ObjectId.Parse(request.WalletId);
        var amountToBuy = await _repository.FindOneAndProjectAsync(w => w.Id == id, wallet => wallet.EthereumAmountToBuy, cancellationToken);
        amountToBuy += request.Amount;
        var updatedWallet = await _repository.UpdateAmountToBuyAsync(id, amountToBuy, CurrencyType.ETHER, cancellationToken);
        return updatedWallet.EthereumAmountToBuy;
    }
}