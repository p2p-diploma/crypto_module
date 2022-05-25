using System.ComponentModel.DataAnnotations;

namespace AppealService.Dtos;

public record CreateAppealDto
{
    [EmailAddress(ErrorMessage = "Email is invalid")]
    public string BuyerEmail { get; set; }
    [EmailAddress(ErrorMessage = "Email is invalid")]
    public string SellerEmail { get; set; }
    [Required(ErrorMessage = "Lot id is empty")]
    public string LotId { get; set; }
    [Required(ErrorMessage = "Receipt is empty")]
    public IFormFile? Receipt { get; set; }
}