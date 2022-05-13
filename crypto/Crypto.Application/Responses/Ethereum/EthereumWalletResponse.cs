namespace Crypto.Application.Responses.Ethereum;

public record EthereumWalletResponse(decimal Balance, string Address, string PrivateKey);