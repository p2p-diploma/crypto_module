using System.ComponentModel.DataAnnotations;

namespace AppealService.Models;

public class Transaction
{
    [Key]
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
}