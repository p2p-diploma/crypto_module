﻿using System.Linq.Expressions;
using Crypto.Domain.Models.Base;

namespace Crypto.Domain.Interfaces;

public interface IWalletsRepository<TWallet, in TId> where TWallet : IWallet<TId>
{
    Task<TWallet> CreateAsync(TWallet wallet, CancellationToken token = default);
    Task<TProjection?> FindOneAsync<TProjection>(Expression<Func<TWallet, bool>> expr, Expression<Func<TWallet, TProjection>> projection,
        CancellationToken token = default);
    
    Task<bool> ExistsAsync(Expression<Func<TWallet, bool>> expr, CancellationToken token = default);
    Task<TWallet> UpdateAsync(TWallet wallet, CancellationToken token = default);
}