using System.Numerics;
using Crypto.Domain.Contracts.ERC20.ContractsDefinition;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace Crypto.Domain.Contracts.ERC20;
public class StandardERC20Service
{
    public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Web3 web3, 
        StandardERC20Deployment standardERC20Deployment, CancellationTokenSource cancellationTokenSource = null)
        => web3.Eth.GetContractDeploymentHandler<StandardERC20Deployment>()
            .SendRequestAndWaitForReceiptAsync(standardERC20Deployment, cancellationTokenSource);

    public static Task<string> DeployContractAsync(Web3 web3, StandardERC20Deployment standardERC20Deployment) => 
        web3.Eth.GetContractDeploymentHandler<StandardERC20Deployment>().SendRequestAsync(standardERC20Deployment);

    public static async Task<StandardERC20Service> DeployContractAndGetServiceAsync(Web3 web3, 
        StandardERC20Deployment standardERC20Deployment, CancellationTokenSource cancellationTokenSource = null)
    {
        var receipt = await DeployContractAndWaitForReceiptAsync(web3, standardERC20Deployment, cancellationTokenSource);
        return new StandardERC20Service(web3, receipt.ContractAddress);
    }

    protected Web3 Web3{ get; }

    public ContractHandler ContractHandler { get; }

    public StandardERC20Service(Web3 web3, string contractAddress)
    {
        Web3 = web3;
        ContractHandler = web3.Eth.GetContractHandler(contractAddress);
    }

    public Task<BigInteger> AllowanceQueryAsync(AllowanceFunction allowanceFunction, BlockParameter blockParameter = null) => 
        ContractHandler.QueryAsync<AllowanceFunction, BigInteger>(allowanceFunction, blockParameter);


    public Task<BigInteger> AllowanceQueryAsync(string owner, string spender, BlockParameter blockParameter = null)
    {
        var allowanceFunction = new AllowanceFunction { Owner = owner, Spender = spender };
        return ContractHandler.QueryAsync<AllowanceFunction, BigInteger>(allowanceFunction, blockParameter);
    }

    public Task<string> ApproveRequestAsync(ApproveFunction approveFunction) => ContractHandler.SendRequestAsync(approveFunction);

    public Task<TransactionReceipt> ApproveRequestAndWaitForReceiptAsync(ApproveFunction approveFunction, CancellationTokenSource cancellationToken = null)
        => ContractHandler.SendRequestAndWaitForReceiptAsync(approveFunction, cancellationToken);

    public Task<string> ApproveRequestAsync(string spender, BigInteger amount)
    {
        var approveFunction = new ApproveFunction { Spender = spender, Amount = amount };
        return ContractHandler.SendRequestAsync(approveFunction);
    }

    public Task<TransactionReceipt> ApproveRequestAndWaitForReceiptAsync(string spender, BigInteger amount, CancellationTokenSource cancellationToken = null)
    {
        var approveFunction = new ApproveFunction { Spender = spender, Amount = amount };
        return ContractHandler.SendRequestAndWaitForReceiptAsync(approveFunction, cancellationToken);
    }

    public Task<BigInteger> BalanceOfQueryAsync(BalanceOfFunction balanceOfFunction, BlockParameter blockParameter = null) => 
        ContractHandler.QueryAsync<BalanceOfFunction, BigInteger>(balanceOfFunction, blockParameter);


    public Task<BigInteger> BalanceOfQueryAsync(string account, BlockParameter blockParameter = null)
    {
        var balanceOfFunction = new BalanceOfFunction { Account = account };
        return ContractHandler.QueryAsync<BalanceOfFunction, BigInteger>(balanceOfFunction, blockParameter);
    }

    public Task<byte> DecimalsQueryAsync(DecimalsFunction decimalsFunction, BlockParameter blockParameter = null) => 
        ContractHandler.QueryAsync<DecimalsFunction, byte>(decimalsFunction, blockParameter);


    public Task<byte> DecimalsQueryAsync(BlockParameter blockParameter = null) => 
        ContractHandler.QueryAsync<DecimalsFunction, byte>(null, blockParameter);

    public Task<string> DecreaseAllowanceRequestAsync(DecreaseAllowanceFunction decreaseAllowanceFunction)
    {
        return ContractHandler.SendRequestAsync(decreaseAllowanceFunction);
    }

    public Task<TransactionReceipt> DecreaseAllowanceRequestAndWaitForReceiptAsync(DecreaseAllowanceFunction decreaseAllowanceFunction, CancellationTokenSource cancellationToken = null) => 
        ContractHandler.SendRequestAndWaitForReceiptAsync(decreaseAllowanceFunction, cancellationToken);

    public Task<string> DecreaseAllowanceRequestAsync(string spender, BigInteger subtractedValue)
    {
        var decreaseAllowanceFunction = new DecreaseAllowanceFunction { Spender = spender, SubtractedValue = subtractedValue }; 
        return ContractHandler.SendRequestAsync(decreaseAllowanceFunction);
    }

    public Task<TransactionReceipt> DecreaseAllowanceRequestAndWaitForReceiptAsync(string spender, BigInteger subtractedValue, CancellationTokenSource cancellationToken = null)
    {
        var decreaseAllowanceFunction = new DecreaseAllowanceFunction { Spender = spender, SubtractedValue = subtractedValue };
        return ContractHandler.SendRequestAndWaitForReceiptAsync(decreaseAllowanceFunction, cancellationToken);
    }

    public Task<string> IncreaseAllowanceRequestAsync(IncreaseAllowanceFunction increaseAllowanceFunction) => 
        ContractHandler.SendRequestAsync(increaseAllowanceFunction);

    public Task<TransactionReceipt> IncreaseAllowanceRequestAndWaitForReceiptAsync(IncreaseAllowanceFunction increaseAllowanceFunction, CancellationTokenSource cancellationToken = null) => 
        ContractHandler.SendRequestAndWaitForReceiptAsync(increaseAllowanceFunction, cancellationToken);

    public Task<string> IncreaseAllowanceRequestAsync(string spender, BigInteger addedValue)
    {
        var increaseAllowanceFunction = new IncreaseAllowanceFunction { Spender = spender, AddedValue = addedValue };
        return ContractHandler.SendRequestAsync(increaseAllowanceFunction);
    }

    public Task<TransactionReceipt> IncreaseAllowanceRequestAndWaitForReceiptAsync(string spender, BigInteger addedValue, CancellationTokenSource cancellationToken = null)
    {
        var increaseAllowanceFunction = new IncreaseAllowanceFunction
        {
            Spender = spender,
            AddedValue = addedValue
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(increaseAllowanceFunction, cancellationToken);
    }

    public Task<string> NameQueryAsync(NameFunction nameFunction, BlockParameter blockParameter = null)
    {
        return ContractHandler.QueryAsync<NameFunction, string>(nameFunction, blockParameter);
    }

        
    public Task<string> NameQueryAsync(BlockParameter blockParameter = null)
    {
        return ContractHandler.QueryAsync<NameFunction, string>(null, blockParameter);
    }

    public Task<string> SymbolQueryAsync(SymbolFunction symbolFunction, BlockParameter blockParameter = null)
    {
        return ContractHandler.QueryAsync<SymbolFunction, string>(symbolFunction, blockParameter);
    }

        
    public Task<string> SymbolQueryAsync(BlockParameter blockParameter = null)
    {
        return ContractHandler.QueryAsync<SymbolFunction, string>(null, blockParameter);
    }

    public Task<BigInteger> TotalSupplyQueryAsync(TotalSupplyFunction totalSupplyFunction, BlockParameter blockParameter = null)
    {
        return ContractHandler.QueryAsync<TotalSupplyFunction, BigInteger>(totalSupplyFunction, blockParameter);
    }

        
    public Task<BigInteger> TotalSupplyQueryAsync(BlockParameter blockParameter = null)
    {
        return ContractHandler.QueryAsync<TotalSupplyFunction, BigInteger>(null, blockParameter);
    }

    public Task<string> TransferRequestAsync(TransferFunction transferFunction)
    {
        return ContractHandler.SendRequestAsync(transferFunction);
    }

    public Task<TransactionReceipt> TransferRequestAndWaitForReceiptAsync(TransferFunction transferFunction, CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(transferFunction, cancellationToken);
    }

    public Task<string> TransferRequestAsync(string to, BigInteger amount)
    {
        var transferFunction = new TransferFunction
        {
            To = to,
            Amount = amount
        };

        return ContractHandler.SendRequestAsync(transferFunction);
    }

    public Task<TransactionReceipt> TransferRequestAndWaitForReceiptAsync(string to, BigInteger amount, CancellationTokenSource cancellationToken = null)
    {
        var transferFunction = new TransferFunction
        {
            To = to,
            Amount = amount
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(transferFunction, cancellationToken);
    }

    public Task<string> TransferFromRequestAsync(TransferFromFunction transferFromFunction)
    {
        return ContractHandler.SendRequestAsync(transferFromFunction);
    }

    public Task<TransactionReceipt> TransferFromRequestAndWaitForReceiptAsync(TransferFromFunction transferFromFunction, CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(transferFromFunction, cancellationToken);
    }

    public Task<string> TransferFromRequestAsync(string from, string to, BigInteger amount)
    {
        var transferFromFunction = new TransferFromFunction
        {
            From = from,
            To = to,
            Amount = amount
        };

        return ContractHandler.SendRequestAsync(transferFromFunction);
    }

    public Task<TransactionReceipt> TransferFromRequestAndWaitForReceiptAsync(string from, string to, BigInteger amount, CancellationTokenSource cancellationToken = null)
    {
        var transferFromFunction = new TransferFromFunction
        {
            From = from,
            To = to,
            Amount = amount
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(transferFromFunction, cancellationToken);
    }
}