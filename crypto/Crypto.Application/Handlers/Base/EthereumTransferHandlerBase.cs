using Crypto.Application.Utils;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models.Base;
using MediatR;
using MongoDB.Bson;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Crypto.Application.Handlers.Base;

public abstract class EthereumTransferHandlerBase<TRequest, TResponse, TWallet> 
    : WalletHandlerBase<TRequest, TResponse, TWallet> where TRequest : IRequest<TResponse> where TWallet : IWallet<ObjectId>
{
    protected readonly EthereumAccountManager _accountManager;
    protected EthereumTransferHandlerBase(EthereumAccountManager accountManager, IWalletsRepository<TWallet, ObjectId> repository) 
        : base(repository)
    {
        _accountManager = accountManager;
    }
    
    protected async Task<bool> Transfer(string address, decimal amount, Account signer, CancellationToken token)
    {
        var web3 = new Web3(signer, _accountManager.BlockchainUrl){ TransactionManager = { UseLegacyAsDefault = true } };
        var balance = Web3.Convert.FromWei(await web3.Eth.GetBalance.SendRequestAsync(signer.Address));
        if (balance < amount) 
            throw new AccountBalanceException($"Amount to transfer exceeds balance on wallet {signer.Address}");

        var transaction = await web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(address, amount,
            tokenSource: CancellationTokenSource.CreateLinkedTokenSource(token));
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