namespace AppealService.Dtos;

public record AppealElementDto(string TransactionId, string BuyerEmail, string SellerEmail, string CreatedAt, Guid Id);