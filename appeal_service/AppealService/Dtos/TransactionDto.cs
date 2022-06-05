namespace AppealService.Dtos;

public record TransactionDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Status { get; set; }
    public decimal Amount { get; set; }
    public string Id { get; set; }
}