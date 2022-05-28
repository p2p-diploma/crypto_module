using Crypto.Domain.Models.Documents.Base;
using Nethereum.KeyStore.Model;

namespace Crypto.Domain.Models.Documents;
public record EthereumWallet<TId> : IWallet<TId> {
    public TId Id { get; set; }
    public string Email { get; set; }
    public string Hash { get; set; }
    public KeyStore<ScryptParams> KeyStore { get; set; }
    public bool IsFrozen { get; set; } = false;
    public DateTime? DateOfUnfreeze { get; set; } = null;
}