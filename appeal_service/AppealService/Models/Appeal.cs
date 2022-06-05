using System.ComponentModel.DataAnnotations.Schema;

namespace AppealService.Models;

public class Appeal
{
    public Guid Id { get; set; }
    public string BuyerEmail { get; set; }
    public string SellerEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    [Column("Receipt")]
    public Receipt AttachedReceipt { get; set; }
    [Column("Transaction")]
    public Transaction Transaction { get; set; }
}