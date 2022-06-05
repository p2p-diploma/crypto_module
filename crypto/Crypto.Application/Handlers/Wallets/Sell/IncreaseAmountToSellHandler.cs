using Crypto.Application.Commands.Wallets.Sell;
using Crypto.Application.Handlers.Base;
using Crypto.Domain.Configuration;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;

namespace Crypto.Application.Handlers.Wallets.Sell;

public class IncreaseAmountToSellHandler : EthereumP2PWalletBaseHandler<IncreaseAmountToSellCommand, decimal>
{
    public IncreaseAmountToSellHandler(IEthereumP2PWalletsRepository<ObjectId> repository) : base(repository)
    {
    }

    public override async Task<decimal> Handle(IncreaseAmountToSellCommand request, CancellationToken cancellationToken)
    {
        var id = ObjectId.Parse(request.WalletId);
        var amountToSell = await _repository.FindOneAndProjectAsync(w => w.Id == id, wallet => wallet.EthToSell, cancellationToken);
        amountToSell += request.Amount;
        var updatedWallet = await _repository.UpdateAmountToSellAsync(id, amountToSell, CurrencyType.ETH, cancellationToken);
        return updatedWallet.EthToSell;
    }
}