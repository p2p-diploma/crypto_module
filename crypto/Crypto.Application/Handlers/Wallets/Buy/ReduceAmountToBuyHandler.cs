using Crypto.Application.Commands.Wallets.Buy;
using Crypto.Application.Handlers.Base;
using Crypto.Domain.Configuration;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;

namespace Crypto.Application.Handlers.Wallets.Buy;

public class ReduceAmountToBuyHandler : EthereumP2PWalletBaseHandler<ReduceAmountToBuyCommand, decimal>
{
    public ReduceAmountToBuyHandler(IEthereumP2PWalletsRepository<ObjectId> repository) : base(repository)
    {
    }

    public override async Task<decimal> Handle(ReduceAmountToBuyCommand request, CancellationToken cancellationToken)
    {
        var id = ObjectId.Parse(request.WalletId);
        var amountToBuy = await _repository.FindOneAndProjectAsync(w => w.Id == id, wallet => wallet.EthereumAmountToBuy, cancellationToken);
        if (amountToBuy < request.Amount)
            throw new ArgumentException("The amount to buy exceeds the balance in P2P wallet");
        amountToBuy -= request.Amount;
        var updatedWallet = await _repository.UpdateAmountToBuyAsync(id, amountToBuy, CurrencyType.ETHER, cancellationToken);
        return updatedWallet.EthereumAmountToBuy;
    }
}