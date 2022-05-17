using Crypto.Application.Responses.Ethereum;
using MediatR;

namespace Crypto.Application.Queries.Ethereum;

public record GetEthereumWalletByIdQuery(string Id) : IRequest<EthereumWalletResponse>;