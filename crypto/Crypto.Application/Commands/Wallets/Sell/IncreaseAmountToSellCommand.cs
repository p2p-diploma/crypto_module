using MediatR;

namespace Crypto.Application.Commands.Wallets.Sell;

public record IncreaseAmountToSellCommand : P2PAmountBaseCommand, IRequest<decimal>;