using System.Linq.Expressions;
using Crypto.Domain.Models.Base;

namespace Crypto.Domain.Interfaces;

public interface IWalletsRepository<TWallet, in TId> where TWallet : IWallet<TId>
{
    Task CreateAsync(TWallet wallet, CancellationToken token = default);
    Task<TWallet> FindOneAsync(Expression<Func<TWallet, bool>> expr, CancellationToken token = default);
    Task<bool> ExistsAsync(Expression<Func<TWallet, bool>> expr, CancellationToken token = default);
}