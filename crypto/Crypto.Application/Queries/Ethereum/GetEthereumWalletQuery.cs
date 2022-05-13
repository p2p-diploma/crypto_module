using Crypto.Application.Responses.Ethereum;
using MediatR;

namespace Crypto.Application.Queries.Ethereum;

public record GetEthereumWalletQuery(string Id) : IRequest<EthereumWalletResponse>;