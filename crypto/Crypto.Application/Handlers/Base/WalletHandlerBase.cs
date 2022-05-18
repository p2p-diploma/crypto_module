using Crypto.Application.Utils;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models.Base;
using MediatR;
using MongoDB.Bson;

namespace Crypto.Application.Handlers.Base;

public abstract class WalletHandlerBase<TRequest, TResponse, TWallet> : IRequestHandler<TRequest, TResponse> 
    where TRequest : IRequest<TResponse> where TWallet : IWallet<ObjectId>
{
    protected readonly IWalletsRepository<TWallet, ObjectId> _repository;
    protected WalletHandlerBase(IWalletsRepository<TWallet, ObjectId> repository)
    {
        _repository = repository;
    }
    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}