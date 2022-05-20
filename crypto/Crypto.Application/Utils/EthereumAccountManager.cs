using Crypto.Application.Responses;
using Crypto.Domain.Configuration;
using Crypto.Domain.Exceptions;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.KeyStore;
using Nethereum.KeyStore.Model;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Crypto.Application.Utils;
public class EthereumAccountManager
{
    public string BlockchainUrl { get; }
    public string TokenAddress { get; }
    public EthereumAccountManager(string blockchainUrl, BlockchainConnections blockchainConnections, SmartContractSettings settings)
    {
        BlockchainUrl = blockchainUrl;
        ChainId = blockchainConnections.GetChainId(blockchainUrl);
        TokenAddress = settings.StandardERC20Address;
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

    public async Task<decimal> GetAccountBalanceInEtherAsync(Account account)
    {
        var web3 = new Web3(account, BlockchainUrl);
        var balanceInWei = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
        return Web3.Convert.FromWei(balanceInWei.Value);
    }
    public int ChainId { get; }
    
    
    public async Task<TransactionResponse> Transfer(string recipient, decimal amount, Account signer, CancellationToken cancellationToken) 
    {
        var web3 = new Web3(signer, BlockchainUrl){ TransactionManager = { UseLegacyAsDefault = true } };
        var token = web3.Eth.ERC20.GetContractService(TokenAddress);
        var tokenBalance = await token.BalanceOfQueryAsync(signer.Address);
        var amountInWei = Web3.Convert.ToWei(amount, UnitConversion.EthUnit.Gwei);
        if (tokenBalance < amountInWei)
            throw new AccountBalanceException($"Amount to transfer exceeds balance on wallet {signer.Address}");

        var transfer = new TransferFunction { To = recipient, Value = amountInWei, FromAddress = signer.Address };
        var etherValues = await Task.WhenAll(web3.Eth.GetBalance.SendRequestAsync(signer.Address), token.ContractHandler.EstimateGasAsync(transfer));
        if (etherValues[0].Value > etherValues[1].Value)
            throw new AccountBalanceException("Not enough ether on P2P wallet to transfer or refund ERC20");
        
        var transaction = await token.ContractHandler.SendRequestAndWaitForReceiptAsync(transfer,
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
        if (transaction.HasErrors() is false)
            return new(signer.Address, recipient, CurrencyType.ERC20, amount, transaction.TransactionHash, DateTime.Now);
        
        throw new BlockchainTransactionException("Transaction error occured");
    }
}