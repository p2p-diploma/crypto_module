using Crypto.Domain.Dtos.ERC20;
using FluentValidation;
using Nethereum.Web3;

namespace Crypto.Server.Validators.ERC20;
public class RevertERC20TransferDtoValidator : AbstractValidator<RevertERC20TransferDto>
{
    public RevertERC20TransferDtoValidator()
    {
        RuleFor(r => r.Recipient).NotEmpty().Must(Web3.IsChecksumAddress).WithMessage("Recipient's address is invalid");
        RuleFor(r => r.Sender).NotEmpty().Must(Web3.IsChecksumAddress).WithMessage("Recipient's address is invalid");
    }
}