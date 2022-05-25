using System.Net;
using AppealService.Dtos;
using AppealService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppealService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
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
    public async Task<IActionResult> CreateAppeal([FromForm] CreateAppealDto appeal, CancellationToken token)
    {
        //string? accessToken = HttpContext.Request.Cookies["jwt-access"];
        //if (string.IsNullOrEmpty(accessToken)) return BadRequest("Cookies not found");
        if (!ModelState.IsValid) return BadRequest();
        if (appeal.Receipt == null) return BadRequest("Receipt was not uploaded");
        if (appeal.Receipt.ContentType != "application/pdf") return BadRequest("Receipt should be in pdf format");
        try
        {
            await _service.CreateAppealAsync(appeal, appeal.Receipt, "", token);
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
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
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
        var file = await System.IO.File.ReadAllBytesAsync(_env.WebRootPath + receipt.Path, token);
        return File(file, "application/pdf", receipt.Name + ".pdf");
    }


    [HttpPost("freeze/{email}")]
    public async Task<IActionResult> FreezeAccount(string buyerEmail, string senderEmail,
        CancellationToken token)
    {
        string? accessToken = HttpContext.Request.Cookies["jwt-access"];
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