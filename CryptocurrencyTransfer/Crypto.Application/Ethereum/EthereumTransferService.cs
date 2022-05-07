using Crypto.Domain.Configuration;
using Crypto.Domain.Contracts.Ethereum;
using Crypto.Domain.Contracts.Ethereum.ContractDefinition;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.Contracts;
using Nethereum.KeyStore;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Crypto.Application.Ethereum;

public class EthereumTransferService
{
    private EthereumTransferContractService _contract;
    private readonly string _contractAddress;
    private readonly IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> _repository;
    private readonly EthereumAccountManager _accountManager;
    public EthereumTransferService(SmartContractSettings contractOptions, IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> repository,
        EthereumAccountManager accountManager)
    {
        _repository = repository;
        _contractAddress = contractOptions.EthereumTransferAddress;
        _accountManager = accountManager;
    }
    
    public async Task<bool> BlockSumAsync(BlockSumFunction message, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(message.Recipient) || !Web3.IsChecksumAddress(message.Recipient) || message.AmountToSend <= 0)
            return false;
        var signer = await GetSignerAsync(message.FromAddress);
        return await SendRequestFromContractAsync(message, new Web3(signer, _accountManager.BlockchainUrl){ TransactionManager = { UseLegacyAsDefault = true } });
    }

    public async Task<bool> TransferAsync(FinalTransferFunction message, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(message.Recipient) || !Web3.IsChecksumAddress(message.Recipient)) return false;
        var signer = await GetSignerAsync(message.FromAddress);
        return await SendRequestFromContractAsync(message, new Web3(signer, _accountManager.BlockchainUrl)
            { TransactionManager = { UseLegacyAsDefault = true } });
    }
    public async Task<bool> RevertTransferAsync(RevertTransferFunction message, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(message.Recipient) || !Web3.IsChecksumAddress(message.Recipient)) return false;
        var signer = await GetSignerAsync(message.FromAddress);
        return await SendRequestFromContractAsync(message, new Web3(signer, _accountManager.BlockchainUrl)
            { TransactionManager = { UseLegacyAsDefault = true } });
    }

    private async Task<Account> GetSignerAsync(string address)
    {
        var wallet = await _repository.FindOneAsync(w => w.KeyStore.Address == address);
        var scryptService = new KeyStoreScryptService();
        return _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
    }
    private async Task<bool> SendRequestFromContractAsync<TEthereumContractFunctionMessage>(
        TEthereumContractFunctionMessage message, Web3 web3) where TEthereumContractFunctionMessage : FunctionMessage, new()
    {
        _contract = new(web3, _contractAddress); 
        var transaction = await _contract.ContractHandler.SendRequestAndWaitForReceiptAsync(message);
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