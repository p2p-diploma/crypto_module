using Crypto.Configuration;
using Crypto.Interfaces;
using Crypto.Models;
using Crypto.Protos.Contracts;
using Crypto.Services.Ethereum;
using Crypto.SmartContracts;
using Crypto.SmartContracts.Ethereum;
using Crypto.SmartContracts.Ethereum.ContractDefinition;
using Grpc.Core;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Nethereum.KeyStore;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace Crypto.Services.Grpc;

public class TransferAgreementGrpcService : TransferAgreementProtoService.TransferAgreementProtoServiceBase
{
    private readonly ILogger<TransferAgreementGrpcService> _logger;
    private readonly string _blockchainUrl;
    private readonly string _contractAddress;
    private readonly EthereumAccountManager _accountManager;
    private readonly IWalletsRepository<EthereumWallet,ObjectId> _repository;

    public TransferAgreementGrpcService(ILogger<TransferAgreementGrpcService> logger, 
        IOptions<BlockchainConnections> options, IOptions<SmartContractSettings> contractOptions,
        IWalletsRepository<EthereumWallet, ObjectId> repository)
    {
        _logger = logger;
        _contractAddress = contractOptions.Value.Address;
        _blockchainUrl = options.Value.Ganache;
        _accountManager = new(_blockchainUrl, options);
        _repository = repository;
    }

    public override async Task<StoreInBlockResponse> StoreInBlock(StoreInBlockRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Recipient))
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new RpcException(new(StatusCode.InvalidArgument, "This recipient's address does not exist"));
        }
        if (!Guid.TryParse(request.SlotId, out _))
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new RpcException(new(StatusCode.InvalidArgument, "Slot id is invalid"));
        }
        if (!ObjectId.TryParse(request.SenderId, out var senderId))
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new RpcException(new(StatusCode.InvalidArgument, "Id of sender is either invalid or wallet is not found"));
        }
        var wallet = await _repository.FindByIdAsync(senderId, context.CancellationToken);
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        /*var loadedAccount = new Account(_senderPrivateKey, _chainId);*/
        Web3 web3 = new(loadedAccount, _blockchainUrl) { TransactionManager = { UseLegacyAsDefault = true } };
        TransferAgreementContractService contractService = new(web3, _contractAddress);
        var transaction = await contractService.StoreInBlockRequestAndWaitForReceiptAsync(new StoreInBlockFunction
        {
            SlotId = request.SlotId,
            Recipient = request.Recipient,
            AmountToSend = Web3.Convert.ToWei(request.EtherAmount),
            FromAddress = loadedAccount.Address,
        });
        LogTransaction(transaction);
        var response = new StoreInBlockResponse();
        var hasErrors = transaction.HasErrors();
        if (!hasErrors.HasValue || hasErrors.Value)
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status500InternalServerError;
            response.IsStored = false; response.ErrorMessage = "Transaction error occured";
        }
        else response.IsStored = true;
        return response;
    }

    public override async Task<TransferToRecipientResponse> TransferToRecipient(TransferToRecipientRequest request, ServerCallContext context)
    {
        if (!ObjectId.TryParse(request.SenderId, out var senderId))
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new RpcException(new(StatusCode.InvalidArgument, "Id of sender is either invalid or wallet is not found"));
        }
        if (!Guid.TryParse(request.SlotId, out _))
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new RpcException(new(StatusCode.InvalidArgument, "Slot id is invalid"));
        }
        var wallet = await _repository.FindByIdAsync(senderId, context.CancellationToken);
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        Web3 web3 = new(loadedAccount, _blockchainUrl) { TransactionManager = { UseLegacyAsDefault = true } };
        var contractService = new TransferAgreementContractService(web3, _contractAddress);
        var transaction = await contractService.TransferToRecipientRequestAndWaitForReceiptAsync(new TransferToRecipientFunction
        {
            SlotId = request.SlotId,
            FromAddress = loadedAccount.Address,
        });
        LogTransaction(transaction);
        var response = new TransferToRecipientResponse();
        var hasErrors = transaction.HasErrors();
        if (!hasErrors.HasValue || hasErrors.Value)
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status500InternalServerError;
            response.IsTransferred = false; response.ErrorMessage = "Transaction error occured";
        }
        else response.IsTransferred = true;
        return response;
    }

    public override async Task<DenyTransferResponse> DenyTransfer(DenyTransferRequest request, ServerCallContext context)
    {
        if (!ObjectId.TryParse(request.SenderId, out var senderId))
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new RpcException(new(StatusCode.InvalidArgument, "Id of sender is either invalid or wallet is not found"));
        }
        if (!Guid.TryParse(request.SlotId, out _))
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new RpcException(new(StatusCode.InvalidArgument, "Slot id is invalid"));
        }
        var wallet = await _repository.FindByIdAsync(senderId, context.CancellationToken);
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        Web3 web3 = new(loadedAccount, _blockchainUrl) { TransactionManager = { UseLegacyAsDefault = true } };
        TransferAgreementContractService contractService = new(web3, _contractAddress);
        var transaction = await contractService.DenyTransferRequestAndWaitForReceiptAsync(new DenyTransferFunction
        {
            SlotId = request.SlotId, FromAddress = loadedAccount.Address
        });
        LogTransaction(transaction);
        var response = new DenyTransferResponse();
        var hasErrors = transaction.HasErrors();
        if (!hasErrors.HasValue || hasErrors.Value)
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status500InternalServerError;
            response.IsDenied = false; response.ErrorMessage = "Transaction error occured";
        }
        else response.IsDenied = true;
        return response;
    }



    private void LogTransaction(TransactionReceipt transaction)
    {
        _logger.LogInformation($"Transaction №{transaction.TransactionHash}: \n" +
                               $"Hash: {transaction.TransactionHash} \n" +
                               $"Gas used: {transaction.GasUsed} \n" + 
                               $"Contract: {transaction.ContractAddress} \n");
    }
}