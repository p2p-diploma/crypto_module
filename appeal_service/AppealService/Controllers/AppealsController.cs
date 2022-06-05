using System.Net;
using AppealService.Contexts;
using AppealService.Dtos;
using AppealService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppealService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AppealsController : ControllerBase
{
    private readonly AppealsService _service;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AppealsController> _logger;
    public AppealsController(AppealsService service, IWebHostEnvironment env, ILogger<AppealsController> logger)
    {
        _service = service;
        _env = env;
        _logger = logger;
    }

    [HttpGet("count")]
    public async Task<int> GetAppealsCount([FromServices] AppealsContext context, CancellationToken token)
    {
        return await context.Appeals.CountAsync(token);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppeal([FromForm] CreateAppealDto appeal, CancellationToken token)
    {
        string? accessToken = HttpContext.Request.Cookies["jwt-access"];
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogWarning("Not found token in cookies");
            return BadRequest("Cookies not found");
        }
        _logger.LogInformation($"Found access token in cookies: {accessToken}");
        if (!ModelState.IsValid) return BadRequest();
        if (appeal.Receipt == null) return BadRequest("Receipt was not uploaded");
        if (appeal.Receipt.ContentType != "application/pdf") return BadRequest("Receipt should be in pdf format");
        try
        {
            await _service.CreateAppealAsync(appeal, accessToken:accessToken, appeal.Receipt, token);
            return Ok("Appeal submitted");
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e.Message);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return StatusCode(500, "Failed to submit appeal");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAppeals(CancellationToken token, int page = 1)
    {
        return Ok(await _service.GetAppealsAsync(page, token: token));
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
        string? accessToken = HttpContext.Request.Cookies["jwt-access"];
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogWarning("Not found token in cookies");
            return BadRequest("Cookies not found");
        }
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
        string? accessToken = HttpContext.Request.Cookies["jwt-access"];
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogWarning("Not found token in cookies");
            return BadRequest("Cookies not found");
        }
        var receipt = await _service.GetReceiptById(id);
        if (receipt == null) return BadRequest($"Receipt with id {id} is not found");
        _logger.LogInformation($"Found receipt by id {id}: {receipt.Name}, Path: {receipt.Path}");
        var file = await System.IO.File.ReadAllBytesAsync(_env.WebRootPath + receipt.Path, token);
        if (file.Length > 0)
        {
            _logger.LogInformation("Loaded file " + receipt.Name + ".pdf");
            return File(file, "application/pdf", receipt.Name + ".pdf");
        }
        return StatusCode(500, "File not found");
    }


    [HttpPost("freeze/{email}")]
    public async Task<IActionResult> FreezeAccount(string buyerEmail, string senderEmail,
        CancellationToken token)
    {
        string? accessToken = HttpContext.Request.Cookies["jwt-access"];
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogWarning("Not found token in cookies");
            return BadRequest("Cookies not found");
        }
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