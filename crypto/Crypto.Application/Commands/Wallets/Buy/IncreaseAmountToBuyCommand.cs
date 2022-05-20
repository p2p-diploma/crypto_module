using MediatR;

namespace Crypto.Application.Commands.Wallets.Buy;

public record IncreaseAmountToBuyCommand : P2PAmountBaseCommand, IRequest<decimal>;