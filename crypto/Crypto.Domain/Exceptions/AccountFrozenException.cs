namespace Crypto.Domain.Exceptions;

public class AccountFrozenException : Exception
{
    public AccountFrozenException() : base("Account is frozen. Transfer operations are unavailable")
    {
        
    }

    public AccountFrozenException(string message) : base(message)
    {
        
    }
}