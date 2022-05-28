namespace Crypto.Application.Responses.Ethereum;

public record EthereumP2PWalletResponse(string Address, decimal Balance, bool IsFrozen);

public record EthereumP2PWalletWithIdResponse(string Id, string Address, decimal Balance, bool IsFrozen) 
    : EthereumP2PWalletResponse(Address, Balance, IsFrozen);