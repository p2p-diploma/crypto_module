using MongoDB.Bson;
using Nethereum.KeyStore.Model;

namespace Crypto.Models;

public record EthereumWallet {
    public ObjectId Id { get; set; }
    public string Email { get; set; }
    public string Hash { get; set; }
    public KeyStore<ScryptParams> KeyStore { get; set; }
}