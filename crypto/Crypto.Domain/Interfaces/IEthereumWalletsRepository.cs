using Crypto.Domain.Models;
using Crypto.Domain.Models.Documents;

namespace Crypto.Domain.Interfaces;

public interface IEthereumWalletsRepository<TId> : IWalletsRepository<EthereumWallet<TId>, TId>
{
    Task<bool> Freeze(string email);
    Task<bool> Unfreeze(TId walletId);
}