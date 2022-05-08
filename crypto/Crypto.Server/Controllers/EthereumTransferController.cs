using Crypto.Application.Ethereum;
using Crypto.Domain.Contracts.Ethereum.ContractDefinition;
using Crypto.Domain.Dtos.Ethereum;
using Microsoft.AspNetCore.Mvc;
using Nethereum.Web3;

namespace Crypto.Server.Controllers;
/// <summary>
/// Ethereum transfer operations
/// </summary>
[ApiController]
[Route("/api/v1/ethereum")]
public class EthereumTransferController : ControllerBase
{
    private readonly EthereumTransferService _transferService;
    public EthereumTransferController(EthereumTransferService transferService)
    {
        _transferService = transferService;
    }
    
    /// <summary>
    /// Block sum Ethereum between sender and recipient
    /// </summary>
    /// <param name="usersInfo">Ethereum accounts' addresses of sender and recipient and the amount of ether to be transferred </param>
    /// <param name="token"></param>
    /// <returns>Block sum transaction result message</returns>
    /// <response code="200">Ether is successfully stored in block</response>
    /// <response code="400">When sender's or recipient's addresses are invalid, or amount of ether is invalid</response>
    /// <response code="500">Error with storing Ether: transaction error</response>
    [HttpPost("block")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> BlockSum([FromBody] BlockSumEthereumDto usersInfo, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        var message = new BlockSumFunction
        {
            FromAddress = usersInfo.Sender, Recipient = usersInfo.Recipient,
            AmountToSend = Web3.Convert.ToWei(usersInfo.EtherAmount)
        };
        return await _transferService.BlockSumAsync(message, token) ? Ok("Ether stored in block") : StatusCode(500, "Something went wrong");
    }
    
    /// <summary>
    /// Final transfer of ether from block to recipient
    /// </summary>
    /// <param name="usersInfo">Ethereum accounts' addresses of sender and recipient </param>
    /// <param name="token"></param>
    /// <returns>Transfer transaction result message</returns>
    /// <response code="200">Ether is successfully transferred</response>
    /// <response code="400">When sender's or recipient's addresses are invalid</response>
    /// <response code="500">Error with transferring Ether: transaction error</response>
    [HttpPost("transfer")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> FinalTransfer([FromBody] FinalEthereumTransferDto usersInfo, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        var message = new FinalTransferFunction
        {
            FromAddress = usersInfo.Sender, Recipient = usersInfo.Recipient,
        };
        return await _transferService.TransferAsync(message, token) ? Ok("Transferred") : StatusCode(500, "Something went wrong");
    }
    
    /// <summary>
    /// Revert of transfer of ether from block to recipient
    /// </summary>
    /// <param name="usersInfo">Ethereum accounts' addresses of sender and recipient </param>
    /// <param name="token"></param>
    /// <returns>Revert transaction result message</returns>
    /// <response code="200">Transfer is successfully reverted</response>
    /// <response code="400">When sender's or recipient's addresses are invalid</response>
    /// <response code="500">Error with reverting transfer: transaction error</response>
    [HttpPost("revert")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> RevertTransfer([FromBody] RevertEthereumTransferDto usersInfo, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        var message = new RevertTransferFunction
        {
            FromAddress = usersInfo.Sender, Recipient = usersInfo.Recipient,
        };
        return await _transferService.RevertTransferAsync(message, token) ? Ok("Reverted transfer") : StatusCode(500, "Something went wrong");
    }
}