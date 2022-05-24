using Crypto.Domain.Configuration;
using Crypto.Domain.Contracts;
using Crypto.Domain.Contracts.ContractDefinition;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Models;
using Microsoft.Extensions.Logging;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Crypto.Domain.Accounts;

public class Erc20AccountManager : EthereumAccountManager
{
    private StandardERC20Service _token;
    public string TokenAddress { get; }
    
    public Erc20AccountManager(string blockchainUrl, BlockchainConnections blockchainConnections, SmartContractSettings settings,
        ILogger<Erc20AccountManager> logger)
        : base(blockchainUrl, blockchainConnections, logger)
    {
        TokenAddress = settings.StandardERC20Address;
    }
    public override async Task<decimal> GetAccountBalanceAsync(Account account)
    {
        var web3 = new Web3(account, BlockchainUrl);
        _token = new StandardERC20Service(web3, TokenAddress);
        try
        {
            var amount = await _token.BalanceOfQueryAsync(account.Address);
            _logger.LogInformation($"ERC20 balance of {account.Address}: {amount}");
            return Web3.Convert.FromWei(amount, UnitConversion.EthUnit.Gwei);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return 0;
        }
    }

    public override async Task<TransactionResponse> TransferAsync(string recipient, decimal amount, Account sender, 
        CancellationToken cancellationToken = default)
    {
        var web3 = new Web3(sender, BlockchainUrl){ TransactionManager = { UseLegacyAsDefault = true } };
        //Connect to blockchain using Web3 provider
        //Check the balance of account for ERC20, if balance is less than the amount in lot, throw error
        _token = new StandardERC20Service(web3, TokenAddress);
        var balanceMessage = new BalanceOfFunction { FromAddress = sender.Address, Account = sender.Address };
        var tokenBalance = await _token.BalanceOfQueryAsync(balanceMessage);
        var amountInWei = Web3.Convert.ToWei(amount, UnitConversion.EthUnit.Gwei);
        if (tokenBalance < amountInWei)
        {
            _logger.LogWarning($"Not enough ERC20 to transfer to {recipient}: {amount} from {sender.Address}: {tokenBalance}");
            throw new AccountBalanceException("Not enough balance on wallet");
        }
        //Check the balance and compare with estimated gas to transfer ERC20
        var values = await Task.WhenAll(web3.Eth.GetBalance.SendRequestAsync(sender.Address), _token.ContractHandler.EstimateGasAsync(balanceMessage));
        if (values[0].Value < values[1].Value)
        {
            _logger.LogWarning($"ETH amount of {sender.Address}: {values[0].Value} is less than gas: {values[1].Value}");
            throw new AccountBalanceException("Not enough balance on wallet to pay gas");
        }
        //Try transfer ERC20 to recipient
        var transaction = await _token.ContractHandler
            .SendRequestAndWaitForReceiptAsync(new TransferFunction { To = recipient, Amount = amountInWei, FromAddress = sender.Address },
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
        //if transaction is successful
        if (transaction.HasErrors() is false)
        {
            _logger.LogInformation($"Successful ERC20 transfer transaction for {sender.Address}");
            return new(sender.Address, recipient, CurrencyType.ERC20, amount, transaction.TransactionHash, DateTime.Now);
        }
        //else throw transaction error
        _logger.LogError($"ERC20 transfer transaction error for {sender.Address}");
        throw new BlockchainTransactionException("Transaction error occured");
    }

}