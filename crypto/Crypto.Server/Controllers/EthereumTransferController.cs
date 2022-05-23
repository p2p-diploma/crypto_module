using Crypto.Application.Commands.Ethereum;
using Crypto.Application.Responses;
using Crypto.Domain.Configuration;
using Crypto.Domain.Exceptions;
using Crypto.Server.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Crypto.Server.Controllers;
/// <summary>
/// Ethereum transfer operations
/// </summary>
[ApiController]
[Route("/api/v1/eth/transfer")]
//[TokenAuthorize(Roles.USER)]
public class EthereumTransferController : ControllerBase
{
    private readonly IMediator _mediator;
    public EthereumTransferController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Transfer Ether from platform wallet to P2P wallet
    /// </summary>
    /// <param name="command">Platform wallet id; Address of P2P wallet (Ethereum account); Amount of ether </param>
    /// <param name="token"></param>
    /// <returns>Transfer to P2P wallet result message</returns>
    /// <response code="200">Ether is successfully transferred to P2P wallet</response>
    /// <response code="400">When wallet id is invalid; when P2P wallet's address is invalid; when amount of ether is less or equal to 0</response>
    /// <response code="500">Error with transferring ether: transaction error</response>
    [HttpPost("to_p2p")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> TransferToP2P([FromBody] TransferEtherToP2PWalletCommand command, CancellationToken token) 
        => await Transfer(command, token);

    /// <summary>
    /// Refund ether from P2P wallet back to platform's wallet
    /// </summary>
    /// <param name="command">Platform wallet id; Amount of ether </param>
    /// <param name="token"></param>
    /// <returns>Refund result message</returns>
    /// <response code="200">Ether is successfully transferred back to platform wallet</response>
    /// <response code="400">When wallet id is invalid; when amount of ether is less or equal to 0</response>
    /// <response code="500">Error with transferring ether: transaction error</response>
    [HttpPost("refund")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> Refund([FromBody] RefundEtherFromP2PWalletCommand command, CancellationToken token) 
        => await Transfer(command, token);

    /// <summary>
    /// Transfer ether from P2P wallet to recipient's wallet
    /// </summary>
    /// <param name="command">P2P wallet id; Address of recipient's wallet (Ethereum account); Amount of ether </param>
    /// <param name="token"></param>
    /// <returns>Transfer to recipient's wallet result message</returns>
    /// <response code="200">Ether are successfully transferred to recipient's wallet</response>
    /// <response code="400">When P2P wallet id is invalid; when recipient's address is invalid; when amount of ether is less or equal to 0</response>
    /// <response code="500">Error with transferring ether: transaction error</response>
    [HttpPost("from_p2p")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> TransferFromP2P([FromBody] TransferEtherFromP2PWalletCommand command, CancellationToken token) 
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