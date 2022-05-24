using Crypto.Application.Handlers.Base;
using Crypto.Application.Queries;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;

namespace Crypto.Application.Handlers.Wallets.Buy;

public class GetAmountToBuyHandler : EthereumP2PWalletBaseHandler<GetAmountToBuyQuery, decimal>
{
    public GetAmountToBuyHandler(IEthereumP2PWalletsRepository<ObjectId> repository) : base(repository)
    {
    }

    public override async Task<decimal> Handle(GetAmountToBuyQuery request, CancellationToken cancellationToken)
    {
        var id = ObjectId.Parse(request.WalletId);
        return await _repository.FindOneAndProjectAsync(w => w.Id == id, wallet => wallet.EthToBuy, cancellationToken);
    }
}