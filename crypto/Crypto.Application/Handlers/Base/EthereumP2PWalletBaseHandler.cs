using Crypto.Domain.Interfaces;
using MediatR;
using MongoDB.Bson;

namespace Crypto.Application.Handlers.Base;

public abstract class EthereumP2PWalletBaseHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    protected readonly IEthereumP2PWalletsRepository<ObjectId> _repository;
    protected EthereumP2PWalletBaseHandler(IEthereumP2PWalletsRepository<ObjectId> repository)
    {
        _repository = repository;
    }
    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}