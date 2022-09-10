using Crypto.Domain.Models;
using MediatR;

namespace Crypto.Application.Commands.ERC20;
public record TransferERC20FromP2PWalletCommand(string WalletId, string RecipientId, decimal Amount) : IRequest<TransactionDetails>;