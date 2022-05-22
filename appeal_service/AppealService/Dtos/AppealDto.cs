namespace AppealService.Dtos;

public record AppealDto
{
    public Guid Id { get; set; }
    public string LotId { get; set; }
    public string BuyerEmail { get; set; }
    public string SellerEmail { get; set; }
    public Guid ReceiptId { get; set; }
    public string Receipt { get; set; }
}