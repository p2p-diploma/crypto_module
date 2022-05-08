using Crypto.Domain.Dtos.Ethereum;
using FluentValidation;
using Nethereum.Web3;

namespace Crypto.Server.Validators.Ethereum;

public class BlockSumEthereumDtoValidator : AbstractValidator<BlockSumEthereumDto>
{
    public BlockSumEthereumDtoValidator()
    {
        RuleFor(r => r.Recipient).NotEmpty().Must(Web3.IsChecksumAddress).WithMessage("Recipient's address is invalid");
        RuleFor(r => r.EtherAmount).Must(e => e > 0).WithMessage("Ethereum amount must be greater than 0");
        RuleFor(r => r.Sender).NotEmpty().Must(Web3.IsChecksumAddress).WithMessage("Recipient's address is invalid");
    }
}