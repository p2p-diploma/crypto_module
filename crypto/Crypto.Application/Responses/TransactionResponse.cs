namespace Crypto.Application.Responses;

public record TransactionResponse(string SenderAddress, string RecipientAddress, string CurrencyType, 
    decimal Amount, string TransactionHash, DateTime TransactionDate);