namespace Crypto.Application.Responses.ERC20;

public record Erc20P2PWalletResponse(string Address, decimal Balance);

public record Erc20P2PWalletWithIdResponse(string Id, string Address, decimal Balance)
: Erc20P2PWalletResponse(Address, Balance);