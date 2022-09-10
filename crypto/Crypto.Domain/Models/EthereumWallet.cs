using Crypto.Domain.Models.Base;
using Nethereum.KeyStore.Model;

namespace Crypto.Domain.Models;
public record EthereumWallet<TId> : IWallet<TId> {
    public TId Id { get; set; }
    public string Email { get; set; }
    public string Hash { get; set; }
    public KeyStore<ScryptParams> KeyStore { get; set; }
    public bool IsLocked { get; set; } = false;
    public DateTime? UnlockDate { get; set; } = null;
}