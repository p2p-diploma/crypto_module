using MediatR;

namespace Crypto.Application.Commands.Wallets.Buy;

public record SetAmountToBuyCommand : P2PAmountBaseCommand, IRequest<decimal>;