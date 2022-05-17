namespace Crypto.Application.Responses.Ethereum;

public record EthereumP2PWalletResponse(string Address, decimal Balance);

public record EthereumP2PWalletWithIdResponse(string Id, string Address, decimal Balance) 
    : EthereumP2PWalletResponse(Address, Balance);