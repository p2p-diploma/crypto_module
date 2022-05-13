using Crypto.Application.Commands.Ethereum;
using FluentValidation;

namespace Crypto.Server.Validators.Ethereum;

public class RefundEtherFromP2PWalletValidator : AbstractValidator<RefundEtherFromP2PWalletCommand>
{
    public RefundEtherFromP2PWalletValidator()
    {
        RuleFor(c => c.WalletId).NotEmpty().Must(IsParsable).WithMessage("P2P wallet id is invalid");
        RuleFor(r => r.Amount).GreaterThan(0).WithMessage("Amount of ether must be greater than 0");
    }
}