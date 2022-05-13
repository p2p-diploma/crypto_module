using MediatR;

namespace Crypto.Application.Commands.Ethereum;

public record TransferEtherToP2PWalletCommand(string WalletId, decimal Amount) : IRequest<bool>;