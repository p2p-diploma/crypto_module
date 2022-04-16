using EthereumTransferGrpc.Configuration;
using EthereumTransferGrpc.Protos.Contracts;
using EthereumTransferGrpc.SmartContracts;
using EthereumTransferGrpc.SmartContracts.ContractDefinition;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace EthereumTransferGrpc.Services.Grpc;

public class TransferAgreementGrpcService : TransferAgreementProtoService.TransferAgreementProtoServiceBase
{
    private readonly ILogger<TransferAgreementGrpcService> _logger;
    private readonly string _blockchainUrl;
    private readonly string _contractAddress;
    private readonly int _chainId;
    private readonly string _senderPrivateKey;
    //private readonly EthereumAccountManager _accountManager;
    public TransferAgreementGrpcService(ILogger<TransferAgreementGrpcService> logger, 
        IOptions<BlockchainConnections> options, IOptions<SmartContractSettings> contractOptions
        /*EthereumAccountManager accountManager, EthereumWalletsRepository repository*/)
    {
        _logger = logger;
        _contractAddress = contractOptions.Value.Address;
        _blockchainUrl = options.Value.Ganache;
        _chainId = options.Value.GetChainId(_blockchainUrl);
        _senderPrivateKey = contractOptions.Value.TestSenderPrivateKey;
       // _accountManager = accountManager;
        //_repository = repository;
    }

    public override async Task<StoreInBlockResponse> StoreInBlock(StoreInBlockRequest request, ServerCallContext context)
    {
        if(string.IsNullOrEmpty(request.Recipient))
            throw new RpcException(new(StatusCode.InvalidArgument, "This recipient's address does not exist"));
        if(!Guid.TryParse(request.SlotId, out _))
            throw new RpcException(new(StatusCode.InvalidArgument, "Slot id is invalid"));
        /*if(!ObjectId.TryParse(request.SenderId, out var senderId))
            throw new RpcException(new(StatusCode.InvalidArgument, "Id of sender is either invalid or wallet is not found"));
        
        var wallet = await _repository.FindByIdAsync(senderId, context.CancellationToken);
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager
            .LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash, _blockchainUrl);*/
        var loadedAccount = new Account(_senderPrivateKey, _chainId);
        Web3 web3 = new(loadedAccount, _blockchainUrl);
        web3.TransactionManager.UseLegacyAsDefault = true;
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
        bool? hasErrors = transaction.HasErrors();
        if (!hasErrors.HasValue || hasErrors.Value)
        {
            response.IsStored = false;
            response.ErrorMessage = "Transaction error occured";
        }
        response.IsStored = true;
        return response;
    }

    public override async Task<TransferToRecipientResponse> TransferToRecipient(TransferToRecipientRequest request, ServerCallContext context)
    {
        /*if(!ObjectId.TryParse(request.SenderId, out var senderId))
            throw new RpcException(new(StatusCode.InvalidArgument, "Id of sender is either invalid or wallet is not found"));*/
        if (!Guid.TryParse(request.SlotId, out _))
            throw new RpcException(new(StatusCode.InvalidArgument, "Slot id is invalid"));
        
        /*var wallet = await _repository.FindByIdAsync(senderId, context.CancellationToken);
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager
            .LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash, _blockchainUrl);*/
        var loadedAccount = new Account(_senderPrivateKey, _chainId);
        Web3 web3 = new(loadedAccount, _blockchainUrl);
        web3.TransactionManager.UseLegacyAsDefault = true;
        var contractService = new TransferAgreementContractService(web3, _contractAddress);
        var transaction = await contractService.TransferToRecipientRequestAndWaitForReceiptAsync(new TransferToRecipientFunction
        {
            SlotId = request.SlotId,
            FromAddress = loadedAccount.Address,
        });
        LogTransaction(transaction);
        var response = new TransferToRecipientResponse();
        bool? hasErrors = transaction.HasErrors();
        if (!hasErrors.HasValue || hasErrors.Value)
        {
            response.IsTransferred = false;
            response.ErrorMessage = "Transaction error occured";
        }
        response.IsTransferred = true;
        return response;
    }

    public override async Task<DenyTransferResponse> DenyTransfer(DenyTransferRequest request, ServerCallContext context)
    {
        /*if(!ObjectId.TryParse(request.SenderId, out _))
            throw new RpcException(new(StatusCode.InvalidArgument, "Id of sender is either invalid or wallet is not found"));*/
        if (!Guid.TryParse(request.SlotId, out _))
            throw new RpcException(new(StatusCode.InvalidArgument, "Slot id is invalid"));
        
        /*var wallet = await _repository.FindByIdAsync(senderId, context.CancellationToken);
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager
            .LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash, _blockchainUrl);*/
        var loadedAccount = new Account(_senderPrivateKey, _chainId);
        Web3 web3 = new(loadedAccount, _blockchainUrl);
        web3.TransactionManager.UseLegacyAsDefault = true;
        TransferAgreementContractService contractService = new(web3, _contractAddress);
        var transaction = await contractService.DenyTransferRequestAndWaitForReceiptAsync(new DenyTransferFunction
        {
            SlotId = request.SlotId,
            FromAddress = loadedAccount.Address
        });
        LogTransaction(transaction);
        var response = new DenyTransferResponse();
        bool? hasErrors = transaction.HasErrors();
        if (!hasErrors.HasValue || hasErrors.Value)
        {
            response.IsDenied = false;
            response.ErrorMessage = "Transaction error occured";
        }
        response.IsDenied = true;
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