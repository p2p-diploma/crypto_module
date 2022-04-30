namespace Crypto.Interfaces;

public interface IWalletsRepository<TWallet, in TId> where TWallet : class
{
    Task CreateAsync(TWallet wallet, CancellationToken token = default);
    Task<TWallet> FindByIdAsync(TId id, CancellationToken token = default);
    Task<bool> ExistsAsync(string email, CancellationToken token = default);
}