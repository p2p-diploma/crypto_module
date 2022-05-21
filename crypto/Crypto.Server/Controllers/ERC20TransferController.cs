using Crypto.Application.Commands.ERC20;
using Crypto.Application.Commands.Ethereum;
using Crypto.Application.Responses;
using Crypto.Domain.Exceptions;
using Crypto.Server.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Crypto.Server.Controllers;

/// <summary>
/// ERC20 transfer operations
/// </summary>
[ApiController]
[Route("/api/v1/erc20/transfer")]
[RoleAuthorize("user")]
public class ERC20TransferController : ControllerBase
{
    private readonly IMediator _mediator;
    public ERC20TransferController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Transfer ERC20 from platform wallet to P2P wallet
    /// </summary>
    /// <param name="command">Platform wallet id; Amount of ERC20 tokens </param>
    /// <param name="token"></param>
    /// <returns>Transfer to P2P wallet result message</returns>
    /// <response code="200">ERC20 tokens are successfully transferred to P2P wallet</response>
    /// <response code="400">When wallet id is invalid; when amount of tokens is less or equal to 0</response>
    /// <response code="500">Error with transferring tokens: transaction error</response>
    [HttpPost("to_p2p")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> TransferToP2P([FromBody] TransferERC20ToP2PWalletCommand command, CancellationToken token)
        => await Transfer(command, token);

    /// <summary>
    /// Refund tokens from P2P wallet back to platform's wallet
    /// </summary>
    /// <param name="command">Platform wallet id; Amount of ERC20 tokens </param>
    /// <param name="token"></param>
    /// <returns>Refund result message</returns>
    /// <response code="200">ERC20 tokens are successfully transferred back to platform wallet</response>
    /// <response code="400">When wallet id is invalid; when amount of tokens is less or equal to 0</response>
    /// <response code="500">Error with transferring tokens: transaction error</response>
    [HttpPost("refund")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> Refund([FromBody] RefundERC20FromP2PWalletCommand command, CancellationToken token)
        => await Transfer(command, token);

    /// <summary>
    /// Transfer ERC20 from P2P wallet to recipient's wallet
    /// </summary>
    /// <param name="command">P2P wallet id; Address of recipient's wallet (Ethereum account); Amount of ERC20 tokens </param>
    /// <param name="token"></param>
    /// <returns>Transfer to recipient's wallet result message</returns>
    /// <response code="200">ERC20 tokens are successfully transferred to recipient's wallet</response>
    /// <response code="400">When P2P wallet id is invalid; when recipient's address is invalid; when amount of tokens is less or equal to 0</response>
    /// <response code="500">Error with transferring tokens: transaction error</response>
    [HttpPost("from_p2p")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> TransferFromP2P([FromBody] TransferERC20FromP2PWalletCommand command, CancellationToken token)
        => await Transfer(command, token);




    /// <summary>
    /// Fund ERC20 P2P wallet with ether: WARNING! For transferring and refunding ERC20 tokens you need to
    /// have some ether on your P2P wallet for paying gas - transaction fee.
    /// Therefore, you have to fund P2P wallet by some ether from your platform wallet.
    /// </summary>
    /// <param name="command">Wallet (platform) id; Amount of ether to fund</param>
    /// <param name="token"></param>
    /// <returns>Transfer to P2P wallet result message</returns>
    /// <response code="200">P2P wallet is successfully funded</response>
    /// <response code="400">When wallet id is invalid; when amount of ether is less or equal to 0</response>
    /// <response code="500">Error with funding: transaction error</response>
    [HttpPost("fund")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> FundP2P([FromBody] TransferEtherToP2PWalletCommand command, CancellationToken token)
        => await Transfer(command, token);
    
    
    private async Task<IActionResult> Transfer<TRequest>(TRequest command, CancellationToken token) where TRequest : IRequest<TransactionResponse>
    {
        if (!ModelState.IsValid) return BadRequest();
        try
        {
            return Ok(await _mediator.Send(command, token));
        }
        catch (AccountBalanceException e)
        {
            return BadRequest(e.Message);
        }
        catch (BlockchainTransactionException e)
        {
            return StatusCode(500, e.Message);
        }
    }
}