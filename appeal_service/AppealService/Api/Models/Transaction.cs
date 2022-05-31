using System.Text.Json.Serialization;

namespace AppealService.Api.Models;

public class Transaction
{
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
    [JsonPropertyName("buyer_email")]
    public string BuyerEmail { get; set; }
    [JsonPropertyName("seller_email")]
    public string SellerEmail { get; set; }
    [JsonPropertyName("fiat_type")]
    public string FiatType { get; set; }
    [JsonPropertyName("fiat_amount")]
    public string FiatAmount { get; set; }
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
    [JsonPropertyName("crypto_type")]
    public string CryptoType { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; }
}