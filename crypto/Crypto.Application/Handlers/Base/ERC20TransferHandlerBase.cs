using Crypto.Application.Utils;
using Crypto.Domain.Configuration;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models.Base;
using MediatR;
using MongoDB.Bson;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Crypto.Application.Handlers.Base;

public abstract class ERC20TransferHandlerBase<TRequest, TResponse, TWallet> : WalletHandlerBase<TRequest, TResponse, TWallet> 
    where TRequest : IRequest<TResponse> where TWallet : IWallet<ObjectId>
{
    private readonly string _tokenAddress;
    protected readonly EthereumAccountManager _accountManager;
    protected ERC20TransferHandlerBase(EthereumAccountManager accountManager, 
        IWalletsRepository<TWallet, ObjectId> repository, SmartContractSettings settings) : base(repository)
    {
        _accountManager = accountManager;
        _tokenAddress = settings.StandardERC20Address;
    }
    protected async Task<bool> Transfer(string toAddress, decimal amount, Account signer, CancellationToken cancellationToken) 
    {
        var web3 = new Web3(signer, _accountManager.BlockchainUrl){ TransactionManager = { UseLegacyAsDefault = true } };
        var token = web3.Eth.ERC20.GetContractService(_tokenAddress);
        var tokenBalance = await token.BalanceOfQueryAsync(signer.Address);
        var amountInWei = Web3.Convert.ToWei(amount, UnitConversion.EthUnit.Gwei);
        if (tokenBalance < amountInWei)
            throw new AccountBalanceException($"Amount to transfer exceeds balance on wallet {signer.Address}");

        var transfer = new TransferFunction { To = toAddress, Value = amountInWei, FromAddress = signer.Address };
        var etherValues = await Task.WhenAll(web3.Eth.GetBalance.SendRequestAsync(signer.Address), token.ContractHandler.EstimateGasAsync(transfer));
        if (etherValues[0].Value > etherValues[1].Value)
            throw new AccountBalanceException("Not enough ether on P2P wallet to transfer or refund ERC20");
        
        var transaction = await token.ContractHandler.SendRequestAndWaitForReceiptAsync(transfer,
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
        var blockSumAnyError = transaction.HasErrors();
        if (blockSumAnyError is false)
        {
            var successMessage = "Transaction success: \n " + $"hash: {transaction.TransactionHash}\n" +
                                 $"from: {transaction.From}\n" + $"to: {transaction.To}\n";
            Console.WriteLine(successMessage);
            return true;
        }
        var errorMessage = "Transaction error: \n " + $"hash: {transaction.TransactionHash}\n" +
                           $"from: {transaction.From}\n" + $"to: {transaction.To}\n";
        Console.WriteLine(errorMessage);
        throw new BlockchainTransactionException(errorMessage);
    }
}