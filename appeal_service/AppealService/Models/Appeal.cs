using System.ComponentModel.DataAnnotations.Schema;

namespace AppealService.Models;

public class Appeal
{
    public Guid Id { get; set; }
    public string TransactionId { get; set; }
    public string BuyerEmail { get; set; }
    public string BuyerName { get; set; }
    public string SellerEmail { get; set; }
    public string SellerName { get; set; }
    public DateTime CreatedAt { get; set; }
    [Column("Receipt")]
    public Receipt AttachedReceipt { get; set; }
}