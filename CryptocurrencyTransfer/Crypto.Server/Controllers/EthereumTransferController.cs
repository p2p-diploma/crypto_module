using Crypto.Application.Ethereum;
using Crypto.Domain.Contracts.Ethereum.ContractDefinition;
using Crypto.Domain.Dtos.Ethereum;
using Microsoft.AspNetCore.Mvc;
using Nethereum.Web3;

namespace Crypto.Server.Controllers;

[ApiController]
[Route("/api/v1/ethereum")]
public class EthereumTransferController : ControllerBase
{
    private readonly EthereumTransferService _transferService;

    public EthereumTransferController(EthereumTransferService transferService)
    {
        _transferService = transferService;
    }
    [HttpPost("block")]
    public async Task<IActionResult> BlockSum([FromBody] BlockSumEthereumDto dto, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        var message = new BlockSumFunction
        {
            FromAddress = dto.Sender, Recipient = dto.Recipient,
            AmountToSend = Web3.Convert.ToWei(dto.EtherAmount)
        };
        return await _transferService.BlockSumAsync(message, token) ? Ok("Ether stored in block") : StatusCode(500, "Something went wrong");
    }
    [HttpPost("transfer")]
    public async Task<IActionResult> FinalTransfer([FromBody] FinalEthereumTransferDto dto, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        var message = new FinalTransferFunction
        {
            FromAddress = dto.Sender, Recipient = dto.Recipient,
        };
        return await _transferService.TransferAsync(message, token) ? Ok("Transferred") : StatusCode(500, "Something went wrong");
    }
    [HttpPost("revert")]
    public async Task<IActionResult> RevertTransfer([FromBody] RevertEthereumTransferDto dto, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        var message = new RevertTransferFunction
        {
            FromAddress = dto.Sender, Recipient = dto.Recipient,
        };
        return await _transferService.RevertTransferAsync(message, token) ? Ok("Reverted transfer") : StatusCode(500, "Something went wrong");
    }
}