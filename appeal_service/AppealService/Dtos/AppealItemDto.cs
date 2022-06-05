namespace AppealService.Dtos;

public record AppealItemDto(string TransactionId, string BuyerEmail, string SellerEmail, string CreatedAt, Guid Id);