namespace Crypto.Domain.Models;

public record TransactionDetails(string SenderAddress, string RecipientAddress, string CurrencyType, 
                                    decimal Amount, string TransactionHash, DateTime TransactionDate);