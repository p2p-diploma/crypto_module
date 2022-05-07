using Crypto.Domain.Contracts.Ethereum.ContractDefinition;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.RPC.Eth.DTOs;

namespace Crypto.Domain.Contracts.Ethereum;

public class EthereumTransferContractService
{
    public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.Web3 web3, EthereumTransferContractDeployment ethereumTransferContractDeployment, CancellationTokenSource cancellationTokenSource = null)
    {
        return web3.Eth.GetContractDeploymentHandler<EthereumTransferContractDeployment>().SendRequestAndWaitForReceiptAsync(ethereumTransferContractDeployment, cancellationTokenSource);
    }

    public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, EthereumTransferContractDeployment ethereumTransferContractDeployment)
    {
        return web3.Eth.GetContractDeploymentHandler<EthereumTransferContractDeployment>().SendRequestAsync(ethereumTransferContractDeployment);
    }

    public static async Task<EthereumTransferContractService> DeployContractAndGetServiceAsync(Nethereum.Web3.Web3 web3, EthereumTransferContractDeployment ethereumTransferContractDeployment, CancellationTokenSource cancellationTokenSource = null)
    {
        var receipt = await DeployContractAndWaitForReceiptAsync(web3, ethereumTransferContractDeployment, cancellationTokenSource);
        return new EthereumTransferContractService(web3, receipt.ContractAddress);
    }

    protected Nethereum.Web3.Web3 Web3{ get; }

    public ContractHandler ContractHandler { get; }

    public EthereumTransferContractService(Nethereum.Web3.Web3 web3, string contractAddress)
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

    public Task<string> BlockSumRequestAsync(string recipient)
    {
        var blockSumFunction = new BlockSumFunction
        {
            Recipient = recipient
        };

        return ContractHandler.SendRequestAsync(blockSumFunction);
    }

    public Task<TransactionReceipt> BlockSumRequestAndWaitForReceiptAsync(string recipient, CancellationTokenSource cancellationToken = null)
    {
        var blockSumFunction = new BlockSumFunction
        {
            Recipient = recipient
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