using System.Numerics;

namespace Crypto.Domain.Exceptions;

public class AccountBalanceException : Exception
{
    public AccountBalanceException(decimal current, decimal required, string currencyType) 
        : base($"Not enough funds on your wallet: require {required - current} {currencyType}")
    {
        CurrentBalance = current;
        RequiredBalance = required;
    }
    public AccountBalanceException(BigInteger current, BigInteger required, string currencyType) 
        : base($"Not enough funds on your wallet: require {required - current} {currencyType}")
    {
        CurrentBalanceInWei = current;
        RequiredBalanceInWei = required;
    }
    
    public decimal CurrentBalance { get; set; }
    public decimal RequiredBalance { get; set; }
    
    public BigInteger CurrentBalanceInWei { get; set; }
    public BigInteger RequiredBalanceInWei { get; set; }
}