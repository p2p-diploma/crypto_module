using Crypto.Application.Responses.Ethereum;
using MediatR;

namespace Crypto.Application.Queries.Ethereum;

public record GetEthereumP2PWalletQuery(string WalletId) : IRequest<EthereumP2PWalletResponse>;