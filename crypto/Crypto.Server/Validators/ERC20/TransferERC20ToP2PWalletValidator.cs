using Crypto.Application.Commands.ERC20;
using FluentValidation;

namespace Crypto.Server.Validators.ERC20;
public class TransferERC20ToP2PWalletValidator : AbstractValidator<TransferERC20ToP2PWalletCommand>
{
    public TransferERC20ToP2PWalletValidator()
    {
        RuleFor(r => r.WalletId).NotEmpty().Must(IsParsable).WithMessage("Wallet id is invalid");
        RuleFor(r => r.Amount).GreaterThan(0).WithMessage("Amount of tokens must be greater than 0");
    }
}