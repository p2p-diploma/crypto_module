namespace AppealService.Dtos;

public record AppealElementDto(string LotId, string BuyerEmail, string SellerEmail, string CreatedAt, Guid Id);