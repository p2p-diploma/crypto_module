namespace AppealService.Api.Models;

public record User
{
    public string FullName { get; set; }
    public string Email { get; set; }
}