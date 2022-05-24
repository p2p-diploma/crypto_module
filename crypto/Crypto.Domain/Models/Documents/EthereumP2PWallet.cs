namespace Crypto.Domain.Models.Documents;

public record EthereumP2PWallet<TId> : EthereumWallet<TId>
{
    public decimal EthToBuy { get; set; }
    public decimal EthToSell { get; set; }
    public decimal Erc20ToBuy { get; set; }
    public decimal Erc20ToSell { get; set; }
}