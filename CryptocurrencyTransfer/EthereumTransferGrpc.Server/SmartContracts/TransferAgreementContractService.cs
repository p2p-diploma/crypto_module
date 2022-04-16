using EthereumTransferGrpc.SmartContracts.ContractDefinition;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.RPC.Eth.DTOs;

namespace EthereumTransferGrpc.SmartContracts;

public class TransferAgreementContractService
{
    protected Nethereum.Web3.Web3 Web3{ get; }

    public ContractHandler ContractHandler { get; }

    public TransferAgreementContractService(Nethereum.Web3.Web3 web3, string contractAddress)
    {
        Web3 = web3;
        ContractHandler = web3.Eth.GetContractHandler(contractAddress);
    }
    public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.Web3 web3, TransferAgreementContractDeployment transferAgreementContractDeployment, CancellationTokenSource cancellationTokenSource = null)
    {
        return web3.Eth.GetContractDeploymentHandler<TransferAgreementContractDeployment>()
            .SendRequestAndWaitForReceiptAsync(transferAgreementContractDeployment, cancellationTokenSource);
    }

    public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, TransferAgreementContractDeployment transferAgreementContractDeployment)
    {
        return web3.Eth.GetContractDeploymentHandler<TransferAgreementContractDeployment>()
            .SendRequestAsync(transferAgreementContractDeployment);
    }

    public static async Task<TransferAgreementContractService> DeployContractAndGetServiceAsync(Nethereum.Web3.Web3 web3, TransferAgreementContractDeployment transferAgreementContractDeployment, CancellationTokenSource cancellationTokenSource = null)
    {
        var receipt = await DeployContractAndWaitForReceiptAsync(web3, transferAgreementContractDeployment, cancellationTokenSource);
        return new TransferAgreementContractService(web3, receipt.ContractAddress);
    }

    public Task<string> DenyTransferRequestAsync(DenyTransferFunction denyTransferFunction)
    {
        return ContractHandler.SendRequestAsync(denyTransferFunction);
    }

    public Task<TransactionReceipt> DenyTransferRequestAndWaitForReceiptAsync(DenyTransferFunction denyTransferFunction, CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(denyTransferFunction, cancellationToken);
    }

    public Task<string> DenyTransferRequestAsync(string slotId)
    {
        var denyTransferFunction = new DenyTransferFunction
        {
            SlotId = slotId
        };
        return ContractHandler.SendRequestAsync(denyTransferFunction);
    }

    public Task<TransactionReceipt> DenyTransferRequestAndWaitForReceiptAsync(string slotId, CancellationTokenSource cancellationToken = null)
    {
        var denyTransferFunction = new DenyTransferFunction
        {
            SlotId = slotId
        };
        return ContractHandler.SendRequestAndWaitForReceiptAsync(denyTransferFunction, cancellationToken);
    }

    public Task<string> StoreInBlockRequestAsync(StoreInBlockFunction storeInBlockFunction)
    {
        return ContractHandler.SendRequestAsync(storeInBlockFunction);
    }

    public Task<TransactionReceipt> StoreInBlockRequestAndWaitForReceiptAsync(StoreInBlockFunction storeInBlockFunction, CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(storeInBlockFunction, cancellationToken);
    }

    public Task<string> StoreInBlockRequestAsync(string slotId, string recipient)
    {
        var storeInBlockFunction = new StoreInBlockFunction
        {
            SlotId = slotId,
            Recipient = recipient
        };
        return ContractHandler.SendRequestAsync(storeInBlockFunction);
    }

    public Task<TransactionReceipt> StoreInBlockRequestAndWaitForReceiptAsync(string slotId, string recipient, CancellationTokenSource cancellationToken = null)
    {
        var storeInBlockFunction = new StoreInBlockFunction
        {
            SlotId = slotId,
            Recipient = recipient
        };
        return ContractHandler.SendRequestAndWaitForReceiptAsync(storeInBlockFunction, cancellationToken);
    }

    public Task<string> TransferToRecipientRequestAsync(TransferToRecipientFunction transferToRecipientFunction)
    {
        return ContractHandler.SendRequestAsync(transferToRecipientFunction);
    }

    public Task<TransactionReceipt> TransferToRecipientRequestAndWaitForReceiptAsync(TransferToRecipientFunction transferToRecipientFunction, CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(transferToRecipientFunction, cancellationToken);
    }

    public Task<string> TransferToRecipientRequestAsync(string slotId)
    {
        var transferToRecipientFunction = new TransferToRecipientFunction
        {
            SlotId = slotId
        };
        return ContractHandler.SendRequestAsync(transferToRecipientFunction);
    }

    public Task<TransactionReceipt> TransferToRecipientRequestAndWaitForReceiptAsync(string slotId, CancellationTokenSource cancellationToken = null)
    {
        var transferToRecipientFunction = new TransferToRecipientFunction
        {
            SlotId = slotId
        };
        return ContractHandler.SendRequestAndWaitForReceiptAsync(transferToRecipientFunction, cancellationToken);
    }
}