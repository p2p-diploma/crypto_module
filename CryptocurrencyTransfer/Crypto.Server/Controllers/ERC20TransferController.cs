using Crypto.Application.ERC20;
using Crypto.Domain.Contracts.ERC20.ContractsDefinition;
using Crypto.Domain.Dtos.ERC20;
using Microsoft.AspNetCore.Mvc;

namespace Crypto.Server.Controllers;

[ApiController]
[Route("/api/v1/erc20")]
public class ERC20TransferController : ControllerBase
{
    private readonly ERC20TransferService _transferService;
    public ERC20TransferController(ERC20TransferService transferService)
    {
        _transferService = transferService;
    }
    [HttpPost("block")]
    public async Task<IActionResult> BlockSum([FromBody] BlockSumERC20Dto dto, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        var message = new BlockSumFunction
        {
            FromAddress = dto.Sender, Recipient = dto.Recipient,
            Amount = dto.TokensAmount
        };
        return await _transferService.BlockSumAsync(message, token) ? Ok("Tokens stored in block") : StatusCode(500, "Something went wrong");
    }
    [HttpPost("transfer")]
    public async Task<IActionResult> FinalTransfer([FromBody] FinalERC20TransferDto dto, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        var message = new FinalTransferFunction
        {
            FromAddress = dto.Sender, Recipient = dto.Recipient,
        };
        return await _transferService.TransferAsync(message, token) ? Ok("Transferred") : StatusCode(500, "Something went wrong");
    }
    [HttpPost("revert")]
    public async Task<IActionResult> RevertTransfer([FromBody] RevertERC20TransferDto dto, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        var message = new RevertTransferFunction
        {
            FromAddress = dto.Sender, Recipient = dto.Recipient,
        };
        return await _transferService.RevertTransferAsync(message, token) ? Ok("Reverted transfer") : StatusCode(500, "Something went wrong");
    }
}