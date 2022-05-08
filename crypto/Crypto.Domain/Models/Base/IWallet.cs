namespace Crypto.Domain.Models.Base;

public interface IWallet<TId> 
{
    public TId Id { get; set; }
}