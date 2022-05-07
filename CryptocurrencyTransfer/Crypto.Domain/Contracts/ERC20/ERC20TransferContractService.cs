using System.Numerics;
using Crypto.Domain.Contracts.ERC20.ContractsDefinition;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.RPC.Eth.DTOs;

namespace Crypto.Domain.Contracts.ERC20;

public class ERC20TransferContractService
{
    public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.Web3 web3, ERC20TransferContractDeployment eRC20TransferContractDeployment, CancellationTokenSource cancellationTokenSource = null)
    {
        return web3.Eth.GetContractDeploymentHandler<ERC20TransferContractDeployment>().SendRequestAndWaitForReceiptAsync(eRC20TransferContractDeployment, cancellationTokenSource);
    }

    public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, ERC20TransferContractDeployment eRC20TransferContractDeployment)
    {
        return web3.Eth.GetContractDeploymentHandler<ERC20TransferContractDeployment>().SendRequestAsync(eRC20TransferContractDeployment);
    }

    public static async Task<ERC20TransferContractService> DeployContractAndGetServiceAsync(Nethereum.Web3.Web3 web3, ERC20TransferContractDeployment eRC20TransferContractDeployment, CancellationTokenSource cancellationTokenSource = null)
    {
        var receipt = await DeployContractAndWaitForReceiptAsync(web3, eRC20TransferContractDeployment, cancellationTokenSource);
        return new ERC20TransferContractService(web3, receipt.ContractAddress);
    }

    protected Nethereum.Web3.Web3 Web3{ get; }

    public ContractHandler ContractHandler { get; }

    public ERC20TransferContractService(Nethereum.Web3.Web3 web3, string contractAddress)
    {
        Web3 = web3;
        ContractHandler = web3.Eth.GetContractHandler(contractAddress);
    }

    public Task<string> BlockSumRequestAsync(BlockSumFunction blockSumFunction)
    {
        return ContractHandler.SendRequestAsync(blockSumFunction);
    }

    public Task<TransactionReceipt> BlockSumRequestAndWaitForReceiptAsync(BlockSumFunction blockSumFunction, CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(blockSumFunction, cancellationToken);
    }

    public Task<string> BlockSumRequestAsync(string recipient, BigInteger amount)
    {
        var blockSumFunction = new BlockSumFunction
        {
            Recipient = recipient,
            Amount = amount
        };

        return ContractHandler.SendRequestAsync(blockSumFunction);
    }

    public Task<TransactionReceipt> BlockSumRequestAndWaitForReceiptAsync(string recipient, BigInteger amount, CancellationTokenSource cancellationToken = null)
    {
        var blockSumFunction = new BlockSumFunction
        {
            Recipient = recipient,
            Amount = amount
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(blockSumFunction, cancellationToken);
    }

    public Task<string> FinalTransferRequestAsync(FinalTransferFunction finalTransferFunction)
    {
        return ContractHandler.SendRequestAsync(finalTransferFunction);
    }

    public Task<TransactionReceipt> FinalTransferRequestAndWaitForReceiptAsync(FinalTransferFunction finalTransferFunction, CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(finalTransferFunction, cancellationToken);
    }

    public Task<string> FinalTransferRequestAsync(string recipient)
    {
        var finalTransferFunction = new FinalTransferFunction
        {
            Recipient = recipient
        };

        return ContractHandler.SendRequestAsync(finalTransferFunction);
    }

    public Task<TransactionReceipt> FinalTransferRequestAndWaitForReceiptAsync(string recipient, CancellationTokenSource cancellationToken = null)
    {
        var finalTransferFunction = new FinalTransferFunction
        {
            Recipient = recipient
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(finalTransferFunction, cancellationToken);
    }

    public Task<string> RevertTransferRequestAsync(RevertTransferFunction revertTransferFunction)
    {
        return ContractHandler.SendRequestAsync(revertTransferFunction);
    }

    public Task<TransactionReceipt> RevertTransferRequestAndWaitForReceiptAsync(RevertTransferFunction revertTransferFunction, CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(revertTransferFunction, cancellationToken);
    }

    public Task<string> RevertTransferRequestAsync(string recipient)
    {
        var revertTransferFunction = new RevertTransferFunction
        {
            Recipient = recipient
        };

        return ContractHandler.SendRequestAsync(revertTransferFunction);
    }

    public Task<TransactionReceipt> RevertTransferRequestAndWaitForReceiptAsync(string recipient, CancellationTokenSource cancellationToken = null)
    {
        var revertTransferFunction = new RevertTransferFunction
        {
            Recipient = recipient
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(revertTransferFunction, cancellationToken);
    }
}