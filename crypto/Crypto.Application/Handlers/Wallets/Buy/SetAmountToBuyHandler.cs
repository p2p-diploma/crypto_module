using Crypto.Application.Commands.Wallets.Buy;
using Crypto.Application.Handlers.Base;
using Crypto.Domain.Configuration;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;

namespace Crypto.Application.Handlers.Wallets.Buy;

public class SetAmountToBuyHandler : EthereumP2PWalletBaseHandler<SetAmountToBuyCommand, decimal>
{
    public SetAmountToBuyHandler(IEthereumP2PWalletsRepository<ObjectId> repository) : base(repository)
    {
    }

    public override async Task<decimal> Handle(SetAmountToBuyCommand request, CancellationToken cancellationToken)
    {
        var updatedWallet = await _repository.UpdateAmountToBuyAsync(ObjectId.Parse(request.WalletId), request.Amount,
            CurrencyType.ETH, cancellationToken);
        return updatedWallet.EthToBuy;
    }
}