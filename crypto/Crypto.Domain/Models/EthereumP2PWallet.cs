namespace Crypto.Domain.Models;

public record EthereumP2PWallet<TId> : EthereumWallet<TId>
{
    public decimal EthereumAmountToBuy { get; set; }
    public decimal EthereumAmountToSell { get; set; }
    public decimal Erc20AmountToBuy { get; set; }
    public decimal Erc20AmountToSell { get; set; }
}