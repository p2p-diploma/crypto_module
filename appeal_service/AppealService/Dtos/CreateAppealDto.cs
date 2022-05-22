namespace AppealService.Dtos;

public record CreateAppealDto
{
    public string BuyerEmail { get; set; }
    public string SellerEmail { get; set; }
    public string LotId { get; set; }
    public IFormFile Receipt { get; set; }
}