using Crypto.Domain.Configuration;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Models;
using Microsoft.Extensions.Logging;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.KeyStore;
using Nethereum.KeyStore.Model;
using Nethereum.Signer;
using Nethereum.Signer.Crypto;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Crypto.Domain.Accounts;
public class EthereumAccountManager
{
    protected readonly ILogger<EthereumAccountManager> _logger;
    public string BlockchainUrl { get; }
    public int ChainId { get; }
    public EthereumAccountManager(string blockchainUrl, BlockchainConnections blockchainConnections, ILogger<EthereumAccountManager> logger)
    {
        BlockchainUrl = blockchainUrl;
        _logger = logger;
        ChainId = blockchainConnections.GetChainId(BlockchainUrl);
    }
    public KeyStore<ScryptParams> GenerateKeyStore(string passwordHash, out string privateKey)
    {
        var key = EthECKey.GenerateKey();
        byte[] generatedPrivateKey = key.GetPrivateKeyAsBytes();
        var generatedAddress = key.GetPublicAddress();
        var keyStore = new KeyStoreScryptService();
        privateKey = key.GetPrivateKey();
        _logger.LogInformation($"Generated private key: {privateKey}, address: {generatedAddress}");
        return keyStore.EncryptAndGenerateKeyStore(passwordHash, generatedPrivateKey, generatedAddress);
    }

    public KeyStore<ScryptParams> GenerateKeyStoreFromKey(string passwordHash, string privateKey)
    {
        var key = new EthECKey(privateKey);
        byte[] generatedPrivateKey = key.GetPrivateKeyAsBytes();
        if (generatedPrivateKey.Length != 32) throw new ArgumentException("Private key is too short");
        var generatedAddress = key.GetPublicAddress();
        var keyStore = new KeyStoreScryptService();
        _logger.LogInformation($"Imported private key: {privateKey}, address: {generatedAddress}");
        return keyStore.EncryptAndGenerateKeyStore(passwordHash, generatedPrivateKey, generatedAddress);
    }
    public Account LoadAccountFromKeyStore(string json, string passwordHash) => Account.LoadFromKeyStore(json, passwordHash, ChainId);


    public virtual async Task<decimal> GetAccountBalanceAsync(Account account)
    {
        try
        {
            var web3 = new Web3(account, BlockchainUrl);
            var balanceInWei = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
            var balance = Web3.Convert.FromWei(balanceInWei.Value);
            _logger.LogInformation($"ETH balance of {account.Address}: {balance}");
            return balance;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return 0;
        }
    }

    public virtual async Task<TransactionResponse> TransferAsync(string recipient, decimal amount, Account sender, 
        CancellationToken cancellationToken = default) 
    {
        var web3 = new Web3(sender, BlockchainUrl){ TransactionManager = { UseLegacyAsDefault = true } };
        var balance = await web3.Eth.GetBalance.SendRequestAsync(sender.Address);
        var balanceInEth = Web3.Convert.FromWei(balance);
        if (balanceInEth < amount)
        {
            _logger.LogWarning($"Not enough ETH to transfer to {recipient}: {amount} from {sender.Address}: {balanceInEth}");
            throw new AccountBalanceException("Not enough balance on wallet");
        }
        //Transfer eth to recipient
        var transaction = await web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(recipient, amount);
        //if transaction is successful
        if (transaction.HasErrors() is false)
        {
            _logger.LogInformation($"Successful ETH transfer transaction for {sender.Address}");
            return new(sender.Address, recipient, CurrencyType.ETH, amount, transaction.TransactionHash, DateTime.Now);
        }
        _logger.LogError($"ETH transfer transaction error for {sender.Address}");
        throw new BlockchainTransactionException("Transaction error occured");
    }
}