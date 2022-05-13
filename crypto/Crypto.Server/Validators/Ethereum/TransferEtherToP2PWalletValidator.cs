using Crypto.Application.Commands.Ethereum;
using FluentValidation;

namespace Crypto.Server.Validators.Ethereum;

public class TransferEtherToP2PWalletValidator : AbstractValidator<TransferEtherToP2PWalletCommand>
{
    public TransferEtherToP2PWalletValidator()
    {
        RuleFor(c => c.WalletId).NotEmpty().Must(IsParsable).WithMessage("User wallet id is invalid");
        RuleFor(r => r.Amount).GreaterThan(0).WithMessage("Amount of ether must be greater than 0");
    }
}