namespace Crypto.Application.Responses.Ethereum;

public record EthereumWalletResponse(decimal Balance, string Address, string PrivateKey);
public record EthereumWalletWithIdResponse(string Id, decimal Balance, 
    string Address, string PrivateKey) : EthereumWalletResponse(Balance, Address, PrivateKey);