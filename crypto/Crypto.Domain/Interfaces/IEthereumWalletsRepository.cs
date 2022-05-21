using Crypto.Domain.Models;

namespace Crypto.Domain.Interfaces;

public interface IEthereumWalletsRepository<TId> : IWalletsRepository<EthereumWallet<TId>, TId>
{
    Task<bool> Freeze(TId walletId);
}