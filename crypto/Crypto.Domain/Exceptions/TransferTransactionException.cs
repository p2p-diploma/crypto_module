namespace Crypto.Domain.Exceptions;

public class TransferTransactionException : Exception
{
    public DateTime ErrorDate { get; set; } = DateTime.Now;
    public TransferTransactionException(string message) : base(message){}
}