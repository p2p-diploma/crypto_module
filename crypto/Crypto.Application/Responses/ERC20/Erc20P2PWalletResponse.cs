namespace Crypto.Application.Responses.ERC20;

public record Erc20P2PWalletResponse(string Address, decimal Balance, bool IsFrozen);

public record Erc20P2PWalletWithIdResponse(string Id, string Address, decimal Balance, bool IsFrozen)
: Erc20P2PWalletResponse(Address, Balance, IsFrozen);