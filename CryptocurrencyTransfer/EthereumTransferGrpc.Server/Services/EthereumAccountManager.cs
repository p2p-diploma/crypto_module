using System.Numerics;
using EthereumTransferGrpc.Configuration;
using Microsoft.Extensions.Options;
using Nethereum.KeyStore;
using Nethereum.KeyStore.Model;
using Nethereum.RPC.Eth;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace EthereumTransferGrpc.Services;

public class EthereumAccountManager
{
    private readonly int _chainId;
    private readonly string _blockchainUrl;
    public EthereumAccountManager(string blockchainUrl, IOptions<BlockchainConnections> options)
    {
        _blockchainUrl = blockchainUrl;
        _chainId = options.Value.GetChainId(blockchainUrl);
    }
    public KeyStore<ScryptParams> GenerateKeyStore(string password, out string privateKey)
    {
        var key = EthECKey.GenerateKey();
        byte[] generatedPrivateKey = key.GetPrivateKeyAsBytes();
        var generatedAddress = key.GetPublicAddress();
        var keyStore = new KeyStoreScryptService();
        privateKey = key.GetPrivateKey();
        return keyStore.EncryptAndGenerateKeyStore(password, generatedPrivateKey, generatedAddress);
    }

    public Account LoadAccountFromKeyStore(string json, string password) => Account.LoadFromKeyStore(json, password, _chainId);

    public async Task<decimal> GetAccountBalanceAsync(Account account)
    {
        var web3 = new Web3(account, _blockchainUrl);
        var balanceInWei = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
        return Web3.Convert.FromWei(balanceInWei.Value);
    }
}