namespace Crypto.Application.Responses.Ethereum;

public record EthereumWalletResponse(decimal Balance, string Address);
public record EthereumWalletWithIdResponse(string Id, decimal Balance, 
    string Address) : EthereumWalletResponse(Balance, Address);