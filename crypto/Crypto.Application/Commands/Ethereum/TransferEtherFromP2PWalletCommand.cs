using MediatR;

namespace Crypto.Application.Commands.Ethereum;

public record TransferEtherFromP2PWalletCommand(string WalletId, string RecipientAddress, decimal Amount) : IRequest<bool>;