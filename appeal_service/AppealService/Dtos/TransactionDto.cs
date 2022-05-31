namespace AppealService.Dtos;

public record TransactionDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string BuyerEmail { get; set; }
    public string SellerEmail { get; set; }
    public string Status { get; set; }
}