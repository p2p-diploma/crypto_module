namespace Crypto.Application.Responses.ERC20;

public record Erc20WalletResponse(string Address, decimal Balance, bool IsFrozen);

public record Erc20WalletWithIdResponse(string Id, string Address, decimal Balance, bool IsFrozen)
: Erc20WalletResponse(Address, Balance, IsFrozen);