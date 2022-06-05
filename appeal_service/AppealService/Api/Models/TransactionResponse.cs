using System.Text.Json.Serialization;

namespace AppealService.Api.Models;

public class TransactionResponse
{
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; }
}