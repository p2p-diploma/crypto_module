using System.Text.Json.Serialization;

namespace Crypto.Application.Commands.Wallets;

public record P2PAmountBaseCommand
{
    public string WalletId { get; set; }
    public decimal Amount { get; set; }
    [JsonIgnore]
    public string CurrencyType { get; set; } = string.Empty;
}