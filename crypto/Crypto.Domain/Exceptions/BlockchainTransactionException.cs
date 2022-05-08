namespace Crypto.Domain.Exceptions;

public class BlockchainTransactionException : Exception
{
    public BlockchainTransactionException(string message) : base(message){}
}