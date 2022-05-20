using Crypto.Application.Commands.Wallets.Sell;
using Crypto.Application.Handlers.Base;
using Crypto.Domain.Configuration;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;

namespace Crypto.Application.Handlers.Wallets.Sell;

public class SetAmountToSellHandler : EthereumP2PWalletBaseHandler<SetAmountToSellCommand, decimal>
{
    public SetAmountToSellHandler(IEthereumP2PWalletsRepository<ObjectId> repository) : base(repository)
    {
    }

    public override async Task<decimal> Handle(SetAmountToSellCommand request, CancellationToken cancellationToken)
    {
        var updatedWallet = await _repository.UpdateAmountToSellAsync(ObjectId.Parse(request.WalletId), request.Amount,
            CurrencyType.ETHER, cancellationToken);
        return updatedWallet.EthereumAmountToSell;
    }
}