using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Crypto.Domain.Contracts.ERC20.ContractsDefinition;

public class ERC20TransferContractDeployment : ERC20TransferContractDeploymentBase
{
    public ERC20TransferContractDeployment() : base(BYTECODE) { }
    public ERC20TransferContractDeployment(string byteCode) : base(byteCode) { }
}

public class ERC20TransferContractDeploymentBase : ContractDeploymentMessage
{
    public static string BYTECODE = "608060405234801561001057600080fd5b5060405161058238038061058283398101604081905261002f91610054565b600080546001600160a01b0319166001600160a01b0392909216919091179055610084565b60006020828403121561006657600080fd5b81516001600160a01b038116811461007d57600080fd5b9392505050565b6104ef806100936000396000f3fe608060405234801561001057600080fd5b50600436106100415760003560e01c8063366b6625146100465780638fc1a4c01461005b578063c903777a1461006e575b600080fd5b6100596100543660046103e8565b610081565b005b6100596100693660046103e8565b6101e8565b61005961007c36600461040a565b6102d1565b60008054604051636eb1769f60e11b81523060048201526001600160a01b038085166024830152849392169063dd62ed3e90604401602060405180830381865afa1580156100d3573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906100f79190610434565b1161011d5760405162461bcd60e51b81526004016101149061044d565b60405180910390fd5b3360008181526001602090815260408083206001600160a01b038781168086529190935281842054935491516323b872dd60e01b8152600481019590955260248501526044840183905291929116906323b872dd906064015b6020604051808303816000875af1158015610195573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906101b99190610497565b50503360009081526001602090815260408083206001600160a01b039590951683529390529182209190915550565b60008054604051636eb1769f60e11b81523060048201526001600160a01b038085166024830152849392169063dd62ed3e90604401602060405180830381865afa15801561023a573d6000803e3d6000fd5b505050506040513d601f19601f8201168201806040525081019061025e9190610434565b1161027b5760405162461bcd60e51b81526004016101149061044d565b3360009081526001602090815260408083206001600160a01b0386811680865291909352818420549354915163a457c2d760e01b815260048101919091526024810184905291169063a457c2d790604401610176565b6001600160a01b0382166103325760405162461bcd60e51b815260206004820152602260248201527f526563697069656e742773206164647265737320646f6573206e6f74206578696044820152611cdd60f21b6064820152608401610114565b3360009081526001602090815260408083206001600160a01b03868116808652919093528184208590559254905163095ea7b360e01b8152600481019390935260248301849052169063095ea7b3906044016020604051808303816000875af11580156103a3573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906103c79190610497565b505050565b80356001600160a01b03811681146103e357600080fd5b919050565b6000602082840312156103fa57600080fd5b610403826103cc565b9392505050565b6000806040838503121561041d57600080fd5b610426836103cc565b946020939093013593505050565b60006020828403121561044657600080fd5b5051919050565b6020808252602a908201527f4f6e6c79207468652073656e64657220686173207065726d697373696f6e207460408201526937903a3930b739b332b960b11b606082015260800190565b6000602082840312156104a957600080fd5b8151801515811461040357600080fdfea26469706673582212208b7ed457ae0104c4536f59c9571fbfcc85eedcfdedb7834046232a28b9b01c7a64736f6c634300080d0033";
    public ERC20TransferContractDeploymentBase() : base(BYTECODE) { }
    public ERC20TransferContractDeploymentBase(string byteCode) : base(byteCode) { }
    [Parameter("address", "token_", 1)]
    public string Token { get; set; }
}

public class BlockSumFunction : BlockSumFunctionBase { }

[Function("block_sum")]
public class BlockSumFunctionBase : FunctionMessage
{
    [Parameter("address", "recipient", 1)]
    public string Recipient { get; set; }
    [Parameter("uint256", "amount", 2)]
    public BigInteger Amount { get; set; }
}

public class FinalTransferFunction : FinalTransferFunctionBase { }

[Function("final_transfer")]
public class FinalTransferFunctionBase : FunctionMessage
{
    [Parameter("address", "recipient", 1)]
    public string Recipient { get; set; }
}

public class RevertTransferFunction : RevertTransferFunctionBase { }

[Function("revert_transfer")]
public class RevertTransferFunctionBase : FunctionMessage
{
    [Parameter("address", "recipient", 1)]
    public string Recipient { get; set; }
}