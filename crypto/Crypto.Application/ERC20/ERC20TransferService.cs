using Crypto.Application.Ethereum;
using Crypto.Domain.Configuration;
using Crypto.Domain.Contracts.ERC20;
using Crypto.Domain.Contracts.ERC20.ContractsDefinition;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.Contracts;
using Nethereum.KeyStore;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Crypto.Application.ERC20;

public class ERC20TransferService
{
    //ERC20 token
    private StandardERC20Service _token;
    private readonly string _tokenAddress;
    //Transfer contract which stores ERC20 token
    private ERC20TransferContractService _contract;
    private readonly string _contractAddress;
    private readonly IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> _repository;
    private readonly EthereumAccountManager _accountManager;
    public ERC20TransferService(SmartContractSettings contractOptions,
        IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> repository, EthereumAccountManager accountManager)
    {
        _repository = repository;
        _tokenAddress = contractOptions.StandardERC20Address;
        _contractAddress = contractOptions.ERC20TransferAddress;
        _accountManager = accountManager;
    }


    private async Task<bool> ApproveContractAsync(ApproveFunction message, Account signer, CancellationToken token = default)
    {
        if (message.Amount <= 0 || string.IsNullOrWhiteSpace(message.FromAddress)) return false;
        return await SendRequestFromContractAsync(message, new Web3(signer, _accountManager.BlockchainUrl){ TransactionManager = { UseLegacyAsDefault = true } }, 
            fromToken: true);
    }
    public async Task<bool> BlockSumAsync(BlockSumFunction message, CancellationToken token = default)
    {
        if (message.Amount <= 0 || string.IsNullOrWhiteSpace(message.FromAddress) || string.IsNullOrWhiteSpace(message.Recipient)) 
            return false;
        var signer = await GetSignerAsync(message.FromAddress, token);
        var approveRequest = new ApproveFunction
            { FromAddress = message.FromAddress, Amount = message.Amount, Spender = _contractAddress };
        if(await ApproveContractAsync(approveRequest, signer, token))
            return await SendRequestFromContractAsync(message, new Web3(signer, _accountManager.BlockchainUrl)
                { TransactionManager = { UseLegacyAsDefault = true } });
        return false;
    }
    public async Task<bool> TransferAsync(FinalTransferFunction message, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(message.FromAddress) || string.IsNullOrWhiteSpace(message.Recipient)) 
            return false;
        var signer = await GetSignerAsync(message.FromAddress, token);
        return await SendRequestFromContractAsync(message, new Web3(signer, _accountManager.BlockchainUrl){ TransactionManager = { UseLegacyAsDefault = true } });
    }
    public async Task<bool> RevertTransferAsync(RevertTransferFunction message, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(message.FromAddress) || string.IsNullOrWhiteSpace(message.Recipient)) 
            return false;
        var signer = await GetSignerAsync(message.FromAddress, token);
        return await SendRequestFromContractAsync(message, new Web3(signer, _accountManager.BlockchainUrl){ TransactionManager = { UseLegacyAsDefault = true } });
    }


    private async Task<Account> GetSignerAsync(string address, CancellationToken token = default)
    {
        var wallet = await _repository.FindOneAsync(w => w.KeyStore.Address == address, token);
        var scryptService = new KeyStoreScryptService();
        return _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
    }
    private async Task<bool> SendRequestFromContractAsync<TEthereumContractFunctionMessage>(
        TEthereumContractFunctionMessage message, Web3 web3, bool fromToken = false) where TEthereumContractFunctionMessage : FunctionMessage, new()
    {
        TransactionReceipt? transaction;
        if (!fromToken)
        {
            _contract = new(web3, _contractAddress);
            transaction = await _contract.ContractHandler.SendRequestAndWaitForReceiptAsync(message);
        } else {
            _token = new(web3, _tokenAddress);
            transaction = await _token.ContractHandler.SendRequestAndWaitForReceiptAsync(message);
        }
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