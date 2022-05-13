using MediatR;

namespace Crypto.Application.Commands.ERC20;

public record RefundERC20FromP2PWalletCommand(string WalletId, decimal Amount) : IRequest<bool>;