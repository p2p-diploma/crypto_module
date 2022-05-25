using Crypto.Application.Commands.ERC20;
using FluentValidation;
using Nethereum.Web3;
namespace Crypto.Server.Validators.ERC20;

public class TransferERC20FromP2PWalletValidator : AbstractValidator<TransferERC20FromP2PWalletCommand>
{
    public TransferERC20FromP2PWalletValidator()
    {
        RuleFor(c => c.WalletId).NotEmpty().Must(IsParsable).WithMessage("P2P wallet id is invalid");
        RuleFor(c => c.RecipientId).NotEmpty().Must(IsParsable).WithMessage("Recipient P2P wallet id is invalid");
        RuleFor(r => r.Amount).GreaterThan(0).WithMessage("Amount of tokens must be greater than 0");
    }
}