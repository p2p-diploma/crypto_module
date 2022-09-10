namespace Crypto.Domain.Exceptions;

public class AccountLockedException : Exception
{
    public AccountLockedException(DateTime unlockDate) : base("Account is frozen. Transfer operations are unavailable") => 
        UnlockDate = unlockDate;

    public AccountLockedException(DateTime unlockDate, string message) : base(message) => 
        UnlockDate = unlockDate;
    public DateTime UnlockDate { get; }
}