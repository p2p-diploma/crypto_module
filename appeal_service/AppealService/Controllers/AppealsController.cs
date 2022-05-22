using System.Net;
using AppealService.Dtos;
using AppealService.Filters;
using AppealService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppealService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[TokenAuthorize("A")]
public class AppealsController : ControllerBase
{
    private readonly AppealsService _service;
    private readonly IWebHostEnvironment _env;
    public AppealsController(AppealsService service, IWebHostEnvironment env)
    {
        _service = service;
        _env = env;
    }

    [HttpPost]
    [TokenAuthorize("U")]
    public async Task<IActionResult> CreateAppeal([FromBody] CreateAppealDto appeal, [FromHeader(Name = "Bearer")] string accessToken,
        IFormFile? receipt, CancellationToken token)
    {
        accessToken = accessToken.Split(' ').Last();
        if (!ModelState.IsValid) return BadRequest();
        if (receipt == null) return BadRequest("Receipt was not uploaded");
        if (receipt.ContentType != "application/pdf") return BadRequest("Receipt should be in pdf format");
        appeal.Receipt = receipt;
        try
        {
            await _service.CreateAppealAsync(appeal, accessToken, token);
            return Ok("Appeal submitted");
        }
        catch (HttpRequestException e)
        {
            return StatusCode(500, e.Message);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch
        {
            return StatusCode(500, "Failed to submit appeal");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAppeals(CancellationToken token, int page = 1)
    {
        return Ok(await _service.GetAppealsAsync(page, token));
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppeal(Guid id, CancellationToken token)
    {
        if (id == Guid.Empty) return BadRequest("Appeal id is invalid");
        return Ok(await _service.GetAppealByIdAsync(id, token));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppeal(Guid id, CancellationToken token)
    {
        if (id == Guid.Empty) return BadRequest("Appeal id is invalid");
        try
        {
            await _service.DeleteAppealAsync(id, token);
            return Ok("Appeal deleted");
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("receipt/{id}")]
    public async Task<IActionResult> DownloadReceipt(Guid id, CancellationToken token)
    {
        var receipt = await _service.GetReceiptById(id);
        if (receipt == null) return BadRequest($"Receipt with id {id} is not found");
        using var memoryStream = new MemoryStream();
        var path = receipt.Path + receipt.Name;
        using var stream = new FileStream(_env.WebRootPath + receipt.Path, FileMode.Open);
        await stream.CopyToAsync(memoryStream, token);
        memoryStream.Position = 0;
        return File(memoryStream, "application/pdf", Path.GetFileName(path));
    }


    [HttpPost("freeze/{email}")]
    public async Task<IActionResult> FreezeAccount(string buyerEmail, string senderEmail, [FromHeader(Name = "Bearer")] string accessToken,
        CancellationToken token)
    {
        accessToken = accessToken.Split(' ').Last();
        var result = await _service.FreezeAccount(buyerEmail, senderEmail, accessToken, token);
        return result.StatusCode switch
        {
            HttpStatusCode.OK => Ok("Account is frozen"),
            HttpStatusCode.BadRequest => BadRequest("Something went wrong"),
            HttpStatusCode.Unauthorized => Unauthorized("Unauthorized to freeze account"),
            _ => StatusCode(500, "Server error. Please, try later")
        };
    }
}