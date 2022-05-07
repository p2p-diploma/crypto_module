using Crypto.Domain.Models.Base;
using Nethereum.KeyStore.Model;

namespace Crypto.Domain.Models;
public record EthereumWallet<TId> : IWallet<TId> {
    public TId Id { get; set; }
    public string Email { get; set; }
    public string Hash { get; set; }
    public KeyStore<ScryptParams> KeyStore { get; set; }
}