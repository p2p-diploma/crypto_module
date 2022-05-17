using Crypto.Application.Responses.Ethereum;
using MediatR;

namespace Crypto.Application.Queries.Ethereum;

public record GetEthereumP2PWalletByEmailQuery(string Email) : IRequest<EthereumP2PWalletWithIdResponse>;