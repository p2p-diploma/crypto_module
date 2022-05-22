using MediatR;

namespace Crypto.Application.Queries.Ethereum;

public record GetPrivateKeyQuery(string Id, string Email) : IRequest<string>;