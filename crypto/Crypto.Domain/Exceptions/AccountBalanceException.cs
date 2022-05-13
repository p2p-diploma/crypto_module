namespace Crypto.Domain.Exceptions;

public class AccountBalanceException : Exception
{
    public AccountBalanceException(string message) : base(message)
    {
        
    }
}