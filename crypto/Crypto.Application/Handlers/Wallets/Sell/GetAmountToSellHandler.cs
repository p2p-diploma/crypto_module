using Crypto.Application.Handlers.Base;
using Crypto.Application.Queries;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;

namespace Crypto.Application.Handlers.Wallets.Sell;

public class GetAmountToSellHandler : EthereumP2PWalletBaseHandler<GetAmountToSellQuery, decimal>
{
    public GetAmountToSellHandler(IEthereumP2PWalletsRepository<ObjectId> repository) : base(repository)
    {
    }

    public override async Task<decimal> Handle(GetAmountToSellQuery request, CancellationToken cancellationToken)
    {
        var id = ObjectId.Parse(request.WalletId);
        return await _repository.FindOneAsync(w => w.Id == id, wallet => wallet.EthToSell, cancellationToken);
    }
}