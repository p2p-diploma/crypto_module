using MediatR;

namespace Crypto.Application.Commands.Wallets.Sell;

public record ReduceAmountToSellCommand : P2PAmountBaseCommand, IRequest<decimal>;