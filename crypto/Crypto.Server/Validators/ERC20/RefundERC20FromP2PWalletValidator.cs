using Crypto.Application.Commands.ERC20;
using FluentValidation;

namespace Crypto.Server.Validators.ERC20;

public class RefundERC20FromP2PWalletValidator : AbstractValidator<RefundERC20FromP2PWalletCommand>
{
    public RefundERC20FromP2PWalletValidator()
    {
        RuleFor(c => c.WalletId).NotEmpty().Must(IsParsable).WithMessage("P2P wallet id is invalid");
        RuleFor(r => r.Amount).GreaterThan(0).WithMessage("Amount of tokens must be greater than 0");
    }
}