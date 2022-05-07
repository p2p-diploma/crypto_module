using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Crypto.Domain.Contracts.Ethereum.ContractDefinition;

public class EthereumTransferContractDeployment : EthereumTransferContractDeploymentBase
{
    public EthereumTransferContractDeployment() : base(BYTECODE) { }
    public EthereumTransferContractDeployment(string byteCode) : base(byteCode) { }
}

public class EthereumTransferContractDeploymentBase : ContractDeploymentMessage
{
    public static string BYTECODE = "608060405234801561001057600080fd5b5061043f806100206000396000f3fe6080604052600436106100345760003560e01c8063366b66251461003957806381349d9a1461005b5780638fc1a4c01461006e575b600080fd5b34801561004557600080fd5b506100596100543660046103d9565b61008e565b005b6100596100693660046103d9565b6101b0565b34801561007a57600080fd5b506100596100893660046103d9565b6102c4565b336000908152602081815260408083206001600160a01b038516845290915290205481906101035760405162461bcd60e51b815260206004820152601b60248201527f546865207061796d656e74206973206e6f7420696e20626c6f636b000000000060448201526064015b60405180910390fd5b336000908152602081815260408083206001600160a01b0386168085529252808320549051909283156108fc02918491818181858888f19350505050158015610150573d6000803e3d6000fd5b50336000818152602081815260408083206001600160a01b03881680855290835281842093909355518481529192917f8930ac7bcb101f94c05b13845098ae74383bfb9e348e73061b730040945cbb8291015b60405180910390a3505050565b6001600160a01b0381166102065760405162461bcd60e51b815260206004820152601e60248201527f526563697069656e742773206164647265737320697320696e76616c6964000060448201526064016100fa565b600034116102655760405162461bcd60e51b815260206004820152602660248201527f416d6f756e74206f66206574686572206d75737420626520677265617465722060448201526507468616e20360d41b60648201526084016100fa565b336000818152602081815260408083206001600160a01b03861680855290835292819020349081905590519081529192917ff4d61bf10cd7634ffb91cebcbd1828d9b0debaec1f35f00fac329be576fa795d910160405180910390a350565b336000908152602081815260408083206001600160a01b038516845290915290205481906103345760405162461bcd60e51b815260206004820152601b60248201527f546865207061796d656e74206973206e6f7420696e20626c6f636b000000000060448201526064016100fa565b336000818152602081815260408083206001600160a01b038716845290915280822054905190929183156108fc02918491818181858888f19350505050158015610382573d6000803e3d6000fd5b50336000818152602081815260408083206001600160a01b03881680855290835281842093909355518481529192917f30f49837c25b6071cbf15f0aae7956c50aca32d0337952975656645ffccec12191016101a3565b6000602082840312156103eb57600080fd5b81356001600160a01b038116811461040257600080fd5b939250505056fea26469706673582212205c733ec49bbaad9a358627359b3a0bff8e48fb0ab5461262efb03493ea71cfe464736f6c634300080d0033";
    public EthereumTransferContractDeploymentBase() : base(BYTECODE) { }
    public EthereumTransferContractDeploymentBase(string byteCode) : base(byteCode) { }

}

public class BlockSumFunction : BlockSumFunctionBase { }

[Function("block_sum")]
public class BlockSumFunctionBase : FunctionMessage
{
    [Parameter("address", "recipient", 1)]
    public string Recipient { get; set; }
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

public class BlockedEventDTO : BlockedEventDTOBase { }

[Event("Blocked")]
public class BlockedEventDTOBase : IEventDTO
{
    [Parameter("address", "sender", 1, true )]
    public string Sender { get; set; }
    [Parameter("address", "recipient", 2, true )]
    public string Recipient { get; set; }
    [Parameter("uint256", "amount", 3, false )]
    public BigInteger Amount { get; set; }
}

public class RevertedEventDTO : RevertedEventDTOBase { }

[Event("Reverted")]
public class RevertedEventDTOBase : IEventDTO
{
    [Parameter("address", "sender", 1, true )]
    public string Sender { get; set; }
    [Parameter("address", "recipient", 2, true )]
    public string Recipient { get; set; }
    [Parameter("uint256", "amount", 3, false )]
    public BigInteger Amount { get; set; }
}

public class TransferedEventDTO : TransferedEventDTOBase { }

[Event("Transfered")]
public class TransferedEventDTOBase : IEventDTO
{
    [Parameter("address", "sender", 1, true )]
    public string Sender { get; set; }
    [Parameter("address", "recipient", 2, true )]
    public string Recipient { get; set; }
    [Parameter("uint256", "amount", 3, false )]
    public BigInteger Amount { get; set; }
}