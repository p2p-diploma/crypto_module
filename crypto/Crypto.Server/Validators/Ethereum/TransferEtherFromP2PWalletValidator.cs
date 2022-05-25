using Crypto.Application.Commands.Ethereum;
using FluentValidation;
using Nethereum.Web3;

namespace Crypto.Server.Validators.Ethereum;

public class TransferEtherFromP2PWalletValidator : AbstractValidator<TransferEtherFromP2PWalletCommand>
{
    public TransferEtherFromP2PWalletValidator()
    {
        RuleFor(c => c.WalletId).NotEmpty().Must(IsParsable).WithMessage("P2P wallet id is invalid");
        RuleFor(c => c.RecipientId).NotEmpty().Must(IsParsable).WithMessage("Recipient P2P wallet id is invalid");
        RuleFor(r => r.Amount).GreaterThan(0).WithMessage("Amount of ether must be greater than 0");
    }
}