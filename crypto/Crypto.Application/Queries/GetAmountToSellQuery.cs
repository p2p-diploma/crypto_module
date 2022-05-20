using System.Text.Json.Serialization;
using MediatR;

namespace Crypto.Application.Queries;

public record GetAmountToSellQuery : IRequest<decimal>{
    public string WalletId { get; set; }
    [JsonIgnore] 
    public string CurrencyType { get; set; } = string.Empty;
}