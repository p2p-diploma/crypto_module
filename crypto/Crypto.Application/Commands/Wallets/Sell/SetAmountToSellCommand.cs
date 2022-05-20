using MediatR;

namespace Crypto.Application.Commands.Wallets.Sell;

public record SetAmountToSellCommand : P2PAmountBaseCommand, IRequest<decimal>;