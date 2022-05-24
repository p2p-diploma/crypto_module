using Crypto.Application.Commands.Wallets;
using Crypto.Application.Commands.Wallets.Buy;
using Crypto.Application.Responses.Ethereum;
using Crypto.Domain.Configuration;
using Crypto.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wallets.Server.Filters;

namespace Wallets.Server.Controllers;

/// <summary>
/// API for managing Ethereum and ERC20 wallets:
/// Since ERC20 runs on the Ethereum blockchain, your platform wallet and P2P wallet will be one for Ethereum and ERC20
/// </summary>
[ApiController]
[Route("/api/v1/wallets")]
public class BaseWalletsController : ControllerBase
{
    private readonly IMediator _mediator;
    public BaseWalletsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create wallet (Ethereum account) with zero balance
    /// </summary>
    /// <param name="command">User's login and password </param>
    /// <param name="token"></param>
    /// <returns>Created wallet with wallet's ID, assigned address and private key</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /create
    ///     {
    ///         "email": "testemail@gmail.com",
    ///         "password": "test_password123"
    ///     }
    /// Sample response:
    /// 
    ///     {
    ///         "address": "0xD2BE74365557b91070405d5007ed2922996CC5da",
    ///         "privateKey": "e13461ef741ab5f0367707d7f0e539b11c2957888888727a8da26e9e60a9a19f",
    ///         "id": "6277d227108472b96eee5e56"
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">Returns newly created wallet</response>
    /// <response code="400">When user's email or password are invalid</response>
    /// <response code="500">When user's wallet couldn't be created</response>
    [HttpPost("create")]
    [ProducesResponseType(typeof(CreatedEthereumWalletResponse), 201)]
    [ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> CreateWallet([FromBody] CreateEthereumWalletCommand command, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        try
        {
            var createdWallet = await _mediator.Send(command, token);
            return string.IsNullOrEmpty(createdWallet.Id)
                ? StatusCode(500, "Couldn't create wallet")
                : CreatedAtRoute(new { id = createdWallet.Id, privateKey = createdWallet.PrivateKey, address = createdWallet.Address }, createdWallet);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }
    /// <summary>
    /// Load existing wallet (Ethereum account) by private key
    /// </summary>
    /// <param name="command">User's login, password and provided private key of an account</param>
    /// <param name="token"></param>
    /// <returns>Created (loaded) wallet with wallet's ID, assigned address and private key</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /load
    ///     {
    ///         "email": "testemail@gmail.com",
    ///         "password": "test_password123",
    ///         "privateKey": "e13461ef741ab5f0367707d7f0e539b11c2957888888727a8da26e9e60a9a19f"
    ///     }
    /// Sample response:
    /// 
    ///     {
    ///         "address": "0xD2BE74365557b91070405d5007ed2922996CC5da",
    ///         "privateKey": "e13461ef741ab5f0367707d7f0e539b11c2957888888727a8da26e9e60a9a19f",
    ///         "id": "6277d227108472b96eee5e56"
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">Returns loaded wallet</response>
    /// <response code="400">When user's email or password are invalid; When provided private key is invalid</response>
    /// <response code="500">When user's wallet cannot be loaded due to some error</response>
    [HttpPost("load")]
    [ProducesResponseType(typeof(CreatedEthereumWalletResponse), 201)]
    [ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> LoadWallet([FromBody] LoadEthereumWalletCommand command, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        try 
        {
            var loadedWallet = await _mediator.Send(command, token);
            return string.IsNullOrEmpty(loadedWallet.Id) ?
                StatusCode(500, "Couldn't load wallet") :
                CreatedAtRoute(new { id = loadedWallet.Id, privateKey = loadedWallet.PrivateKey, address = loadedWallet.Address }, loadedWallet);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }


    /// <summary>
    /// Freeze Ethereum and ERC20 wallets of user
    /// </summary>
    /// <param name="id">User wallet's id</param>
    /// <param name="token"></param>
    /// <returns>Status code whether the wallets are frozen or not</returns>
    /// <remarks>
    /// WARNING: only admin has permission to freeze accounts
    /// 
    /// </remarks>
    /// <response code="200">Wallets are successfully frozen</response>
    /// <response code="400">When wallet id is invalid</response>
    /// <response code="500">When user's wallet cannot be loaded due to some error</response>
    //[TokenAuthorize(Roles.ADMIN)]
    [HttpPut("freeze/{id}")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401), ProducesResponseType(500)]
    public async Task<IActionResult> FreezeWallet(string id, CancellationToken token)
    {
        if (!IsParsable(id)) return BadRequest("Wallet id is invalid");
        var allFrozen = await _mediator.Send(new FreezeEthereumWalletCommand(id), token);
        return allFrozen ? Ok("Wallets are successfully frozen") : StatusCode(500, "Failed to freeze wallets");
    }
}