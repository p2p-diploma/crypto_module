namespace Crypto.Application.Responses.ERC20;

public record Erc20WalletResponse(string Address, decimal Balance);

public record Erc20WalletWithIdResponse(string Id, string Address, decimal Balance)
: Erc20WalletResponse(Address, Balance);