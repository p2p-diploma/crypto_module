using Crypto.Domain.Models;
using MediatR;
namespace Crypto.Application.Commands.Ethereum;

public record RefundEtherFromP2PWalletCommand(string WalletId, decimal Amount) : IRequest<TransactionDetails>;