using Crypto.Application.Commands.Wallets.Sell;
using Crypto.Application.Handlers.Base;
using Crypto.Domain.Configuration;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;

namespace Crypto.Application.Handlers.Wallets.Sell;

public class ReduceAmountToSellHandler : EthereumP2PWalletBaseHandler<ReduceAmountToSellCommand, decimal>
{
    public ReduceAmountToSellHandler(IEthereumP2PWalletsRepository<ObjectId> repository) : base(repository)
    {
    }

    public override async Task<decimal> Handle(ReduceAmountToSellCommand request, CancellationToken cancellationToken)
    {
        var id = ObjectId.Parse(request.WalletId);
        var amountToSell = await _repository.FindOneAndProjectAsync(w => w.Id == id, wallet => wallet.EthToSell, cancellationToken);
        if (amountToSell < request.Amount)
            throw new ArgumentException("The amount to sell exceeds the balance in P2P wallet");
        amountToSell -= request.Amount;
        var updatedWallet = await _repository.UpdateAmountToSellAsync(id, amountToSell, CurrencyType.ETH, cancellationToken);
        return updatedWallet.EthToBuy;
    }
}