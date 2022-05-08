using Crypto.Application.ERC20;
using Crypto.Domain.Contracts.ERC20.ContractsDefinition;
using Crypto.Domain.Dtos.ERC20;
using Microsoft.AspNetCore.Mvc;

namespace Crypto.Server.Controllers;

/// <summary>
/// ERC20 transfer operations
/// </summary>
[ApiController]
[Route("/api/v1/erc20")]
public class ERC20TransferController : ControllerBase
{
    private readonly ERC20TransferService _transferService;
    public ERC20TransferController(ERC20TransferService transferService)
    {
        _transferService = transferService;
    }
    
    /// <summary>
    /// Block sum ERC20 between sender and recipient
    /// </summary>
    /// <param name="usersInfo">Ethereum accounts' addresses of sender and recipient and the amount of tokens to be transferred </param>
    /// <param name="token"></param>
    /// <returns>Block sum transaction result message</returns>
    /// <response code="200">ERC20 tokens are successfully stored in block</response>
    /// <response code="400">When sender's or recipient's addresses are invalid, or amount of tokens is invalid</response>
    /// <response code="500">Error with storing tokens: transaction error</response>
    [HttpPost("block")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> BlockSum([FromBody] BlockSumERC20Dto usersInfo, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        var message = new BlockSumFunction
        {
            FromAddress = usersInfo.Sender, Recipient = usersInfo.Recipient,
            Amount = usersInfo.TokensAmount
        };
        return await _transferService.BlockSumAsync(message, token) ? Ok("Tokens stored in block") : StatusCode(500, "Something went wrong");
    }
    
    /// <summary>
    /// Final transfer of ERC20 from block to recipient
    /// </summary>
    /// <param name="usersInfo">Ethereum accounts' addresses of sender and recipient and the amount of tokens to be transferred </param>
    /// <param name="token"></param>
    /// <returns>Transfer transaction result message</returns>
    /// <response code="200">ERC20 tokens are successfully transferred</response>
    /// <response code="400">When sender's or recipient's addresses are invalid, or amount of tokens is invalid</response>
    /// <response code="500">Error with transferring tokens: transaction error</response>
    [HttpPost("transfer")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> FinalTransfer([FromBody] FinalERC20TransferDto usersInfo, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        var message = new FinalTransferFunction
        {
            FromAddress = usersInfo.Sender, Recipient = usersInfo.Recipient,
        };
        return await _transferService.TransferAsync(message, token) ? Ok("Transferred") : StatusCode(500, "Something went wrong");
    }
    
    /// <summary>
    /// Revert of transfer from block to recipient
    /// </summary>
    /// <param name="usersInfo">Ethereum accounts' addresses of sender and recipient and the amount of tokens to be transferred </param>
    /// <param name="token"></param>
    /// <returns>Revert transaction result message</returns>
    /// <response code="200">Transfer is successfully reverted</response>
    /// <response code="400">When sender's or recipient's addresses are invalid, or amount of tokens is invalid</response>
    /// <response code="500">Error with reverting transfer: transaction error</response>
    [HttpPost("revert")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> RevertTransfer([FromBody] RevertERC20TransferDto usersInfo, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        var message = new RevertTransferFunction
        {
            FromAddress = usersInfo.Sender, Recipient = usersInfo.Recipient,
        };
        return await _transferService.RevertTransferAsync(message, token) ? Ok("Reverted transfer") : StatusCode(500, "Something went wrong");
    }
}