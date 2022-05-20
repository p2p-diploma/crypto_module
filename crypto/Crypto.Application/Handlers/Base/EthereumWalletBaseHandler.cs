using Crypto.Application.Utils;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models.Base;
using MediatR;
using MongoDB.Bson;

namespace Crypto.Application.Handlers.Base;

public abstract class EthereumWalletBaseHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    protected readonly IEthereumWalletsRepository<ObjectId> _repository;
    protected EthereumWalletBaseHandler(IEthereumWalletsRepository<ObjectId> repository)
    {
        _repository = repository;
    }
    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}