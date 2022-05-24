using Crypto.Domain.Configuration;
using Crypto.Domain.Models;
using Crypto.Domain.Models.Documents;

namespace Crypto.Domain.Interfaces;

public interface IEthereumP2PWalletsRepository<TId> : IWalletsRepository<EthereumP2PWallet<TId>, TId>
{
    Task<EthereumP2PWallet<TId>> UpdateAmountToBuyAsync(TId walletId, decimal amount, string currencyType, CancellationToken token = default);
    Task<EthereumP2PWallet<TId>> UpdateAmountToSellAsync(TId walletId, decimal amount, string currencyType, CancellationToken token = default);
    Task<bool> Freeze(TId walletId);
}