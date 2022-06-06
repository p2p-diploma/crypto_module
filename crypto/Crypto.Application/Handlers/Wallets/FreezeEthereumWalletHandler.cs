using Crypto.Application.Commands.Wallets;
using Crypto.Application.Handlers.Base;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;

namespace Crypto.Application.Handlers.Wallets;

public class FreezeEthereumWalletHandler : EthereumWalletBaseHandler<FreezeEthereumWalletCommand, bool>
{
    private readonly IEthereumP2PWalletsRepository<ObjectId> _p2PWalletsRepository;
    public FreezeEthereumWalletHandler(IEthereumWalletsRepository<ObjectId> repository, 
        IEthereumP2PWalletsRepository<ObjectId> p2PWalletsRepository) : base(repository)
    {
        _p2PWalletsRepository = p2PWalletsRepository;
    }

    public override async Task<bool> Handle(FreezeEthereumWalletCommand request, CancellationToken cancellationToken)
    {
        if (!await _repository.ExistsAsync(s => s.Email == request.Email, cancellationToken))
            throw new AccountNotFoundException($"Wallet with email {request.Email} is not found");
        return (await Task.WhenAll(_repository.Freeze(request.Email), _p2PWalletsRepository.Freeze(request.Email))).All(r => r);
    }
}