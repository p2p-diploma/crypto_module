using Crypto.Domain.Dtos.Ethereum;
using FluentValidation;
using Nethereum.Web3;

namespace Crypto.Server.Validators.Ethereum;

public class RevertEthereumTransferDtoValidator : AbstractValidator<RevertEthereumTransferDto>
{
    public RevertEthereumTransferDtoValidator()
    {
        RuleFor(r => r.Recipient).NotEmpty().Must(Web3.IsChecksumAddress).WithMessage("Recipient's address is invalid");
        RuleFor(r => r.Sender).NotEmpty().Must(Web3.IsChecksumAddress).WithMessage("Recipient's address is invalid");
    }
}