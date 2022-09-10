using Crypto.Application.Commands.Wallets.Sell;
using Crypto.Application.Handlers.Base;
using Crypto.Domain.Configuration;
using Crypto.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Crypto.Application.Handlers.Wallets.Sell;

public class ReduceAmountToSellHandler : EthereumP2PWalletBaseHandler<ReduceAmountToSellCommand, decimal>
{
    private readonly ILogger<ReduceAmountToSellHandler> _logger;
    public ReduceAmountToSellHandler(IEthereumP2PWalletsRepository<ObjectId> repository,
        ILogger<ReduceAmountToSellHandler> logger) : base(repository)
    {
        _logger = logger;
    }

    public override async Task<decimal> Handle(ReduceAmountToSellCommand request, CancellationToken cancellationToken)
    {
        var id = ObjectId.Parse(request.WalletId);
        _logger.LogInformation($"Reduce to sell wallet id: {id}");
        var amountToSell = await _repository.FindOneAsync(w => w.Id == id, wallet => wallet.EthToSell, cancellationToken);
        _logger.LogInformation($"Reduce to sell amount to sell of id: {id} => {amountToSell}");
        if (amountToSell < request.Amount)
        {
            _logger.LogWarning($"The amount to sell {amountToSell} exceeds the balance in P2P wallet {id}");
            throw new ArgumentException("The amount to sell exceeds the balance in P2P wallet");
        }
        amountToSell -= request.Amount;
        var updatedWallet = await _repository.UpdateAmountToSellAsync(id, amountToSell, CurrencyType.ETH, cancellationToken);
        return updatedWallet.EthToSell;
    }
}