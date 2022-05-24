using Crypto.Domain.Configuration;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Models;
using Nethereum.KeyStore;
using Nethereum.KeyStore.Model;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Crypto.Domain.Accounts;
public class EthereumAccountManager
{
    public string BlockchainUrl { get; }
    public int ChainId { get; }
    public EthereumAccountManager(string blockchainUrl, BlockchainConnections blockchainConnections)
    {
        BlockchainUrl = blockchainUrl;
        ChainId = blockchainConnections.GetChainId(BlockchainUrl);
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

    public static KeyStore<ScryptParams> GenerateKeyStoreFromKey(string passwordHash, string privateKey)
    {
        var key = new EthECKey(privateKey);
        byte[] generatedPrivateKey = key.GetPrivateKeyAsBytes();
        var generatedAddress = key.GetPublicAddress();
        var keyStore = new KeyStoreScryptService();
        return keyStore.EncryptAndGenerateKeyStore(passwordHash, generatedPrivateKey, generatedAddress);
    }
    public Account LoadAccountFromKeyStore(string json, string passwordHash) => Account.LoadFromKeyStore(json, passwordHash, ChainId);


    public virtual async Task<decimal> GetAccountBalanceAsync(Account account)
    {
        try
        {
            var web3 = new Web3(account, BlockchainUrl);
            var balanceInWei = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
            return Web3.Convert.FromWei(balanceInWei.Value);
        }
        catch
        {
            return 0;
        }
    }

    public virtual async Task<TransactionResponse> TransferAsync(string recipient, decimal amount, Account sender, 
        CancellationToken cancellationToken = default) 
    {
        var web3 = new Web3(sender, BlockchainUrl){ TransactionManager = { UseLegacyAsDefault = true } };
        var balance = await web3.Eth.GetBalance.SendRequestAsync(sender.Address);
        if (Web3.Convert.FromWei(balance) < amount)
            throw new AccountBalanceException("Not enough balance on wallet");
        //Transfer eth to recipient
        var transaction = await web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(recipient, amount);
        //if transaction is successful
        if (transaction.HasErrors() is false)
            return new(sender.Address, recipient, CurrencyType.ETH, amount, transaction.TransactionHash, DateTime.Now);
        throw new BlockchainTransactionException("Transaction error occured");
    }
}