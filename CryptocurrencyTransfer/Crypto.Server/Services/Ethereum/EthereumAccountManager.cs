using Crypto.Configuration;
using Microsoft.Extensions.Options;
using Nethereum.KeyStore;
using Nethereum.KeyStore.Model;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Crypto.Services.Ethereum;

public class EthereumAccountManager
{
    private readonly string _blockchainUrl;
    public EthereumAccountManager(string blockchainUrl, IOptions<BlockchainConnections> options)
    {
        _blockchainUrl = blockchainUrl;
        ChainId = options.Value.GetChainId(blockchainUrl);
    }
    public static KeyStore<ScryptParams> GenerateKeyStore(string passwordHash, out string privateKey)
    {
        var key = EthECKey.GenerateKey();
        byte[] generatedPrivateKey = key.GetPrivateKeyAsBytes();
        var generatedAddress = key.GetPublicAddress();
        var keyStore = new KeyStoreScryptService();
        privateKey = key.GetPrivateKey();
        return keyStore.EncryptAndGenerateKeyStore(passwordHash, generatedPrivateKey, generatedAddress);
    }

    public static KeyStore<ScryptParams> GenerateKeyStore(string passwordHash, string privateKey)
    {
        var key = new EthECKey(privateKey);
        byte[] generatedPrivateKey = key.GetPrivateKeyAsBytes();
        var generatedAddress = key.GetPublicAddress();
        var keyStore = new KeyStoreScryptService();
        return keyStore.EncryptAndGenerateKeyStore(passwordHash, generatedPrivateKey, generatedAddress);
    }

    public Account LoadAccountFromKeyStore(string json, string passwordHash) => Account.LoadFromKeyStore(json, passwordHash, ChainId);

    public async Task<decimal> GetAccountBalanceInEtherAsync(Account account)
    {
        var web3 = new Web3(account, _blockchainUrl);
        var balanceInWei = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
        return Web3.Convert.FromWei(balanceInWei.Value);
    }
    public int ChainId { get; }
}