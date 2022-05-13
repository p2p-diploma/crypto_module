namespace Crypto.Application.Responses.Ethereum;

public record CreatedEthereumWalletResponse(string Address, string PrivateKey, string Id);