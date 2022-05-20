using MediatR;

namespace Crypto.Application.Commands.Wallets.Buy;

public record ReduceAmountToBuyCommand : P2PAmountBaseCommand, IRequest<decimal>;