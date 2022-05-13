using Crypto.Application.Commands.Wallets;
using Crypto.Application.Queries.ERC20;
using Crypto.Application.Queries.Ethereum;
using Crypto.Application.Responses.ERC20;
using Crypto.Application.Responses.Ethereum;
using Crypto.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace Wallets.Server.Controllers;

/// <summary>
/// API for managing Ethereum and ERC20 wallets:
/// Since ERC20 runs on the Ethereum blockchain, your platform wallet and P2P wallet will be one for Ethereum and ERC20
/// </summary>
[ApiController]
[Route("/api/v1/[controller]")]
public class WalletsController : ControllerBase
{
    private readonly IMediator _mediator;
    public WalletsController(IMediator mediator)
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
        catch (AccountNotFoundException e)
        {
            return BadRequest(e.Message);
        }
    }
    
    /// <summary>
    /// Returns information about user's Ethereum wallet: Ethereum account's address, private key, and balance in Ether
    /// </summary>
    /// <param name="id">Wallet's id in ObjectID format</param>
    /// <param name="token"></param>
    /// <returns>Wallet's information: address, private key, and balance in Ether</returns>
    /// <remarks>
    /// Sample request:
    ///     GET /6277d227108472b96eee5e56/ethereum
    /// 
    /// Sample response:
    /// 
    ///     {
    ///         "balance": 95.58789490,
    ///         "privateKey": "e13461ef741ab5f0367707d7f0e539b11c2957888888727a8da26e9e60a9a19f",
    ///         "address": "0xD2BE74365557b91070405d5007ed2922996CC5da"
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">Returns wallet information</response>
    /// <response code="400">When wallet id is invalid</response>
    /// <response code="404">When wallet is not found</response>
    /// <response code="500">When wallet info is not shown due to some error</response>
    [HttpGet("{id}/ethereum")]
    [ProducesResponseType(typeof(EthereumWalletResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetWallet(string id, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(id) || !IsParsable(id)) return BadRequest("Wallet id is invalid");
        try
        {
            return Ok(await _mediator.Send(new GetEthereumWalletQuery(id), token));
        }
        catch (AccountNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    /// <summary>
    /// Returns information about user's Ethereum P2P wallet: Ethereum account's address, balance in Ether
    /// </summary>
    /// <param name="id">Wallet's id in ObjectID format</param>
    /// <param name="token"></param>
    /// <returns>P2P wallet's information: address, and balance in Ether</returns>
    /// <remarks>
    /// Sample request:
    ///     GET /6277d227108472b96eee5e56/ethereum/p2p
    /// 
    /// Sample response:
    /// 
    ///     {
    ///         "balance": 95.58789490,
    ///         "address": "0xD2BE74365557b91070405d5007ed2922996CC5da"
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">Returns wallet information</response>
    /// <response code="400">When wallet id is invalid</response>
    /// <response code="404">When wallet is not found</response>
    /// <response code="500">When wallet info is not shown due to some error</response>
    [HttpGet("{id}/ethereum/p2p")]
    [ProducesResponseType(typeof(EthereumP2PWalletResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetP2PWallet(string id, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(id) || !IsParsable(id)) return BadRequest("Wallet id is invalid");
        try
        {
            return Ok(await _mediator.Send(new GetEthereumP2PWalletQuery(id), token));
        }
        catch (AccountNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    /// <summary>
    /// Returns information about user's ERC20 wallet: associated Ethereum account's address, balance in ERC20 tokens
    /// </summary>
    /// <param name="id">Wallet's id in ObjectID</param>
    /// <param name="token"></param>
    /// <returns>ERC20 wallet's information: address, balance in ERC20 tokens</returns>
    /// <remarks>
    /// Sample request:
    ///     GET /6277d227108472b96eee5e56/erc20
    /// 
    /// Sample response:
    /// 
    ///     {
    ///         "address": "0xD2BE74365557b91070405d5007ed2922996CC5da",
    ///         "balance": 0.095040493
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">Returns wallet information</response>
    /// <response code="400">When wallet id is invalid</response>
    /// <response code="404">When wallet is not found</response>
    /// <response code="500">When wallet info is not shown due to some error</response>
    [HttpGet("{id}/erc20")]
    [ProducesResponseType(typeof(ERC20WalletResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetTokenWallet(string id, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(id) || !IsParsable(id)) return BadRequest("Wallet id is invalid");
        try
        {
            return Ok(await _mediator.Send(new GetERC20WalletQuery(id), token));
        }
        catch (AccountNotFoundException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    
    /// <summary>
    /// Returns information about user's ERC20 P2P wallet: associated Ethereum account's address, balance in ERC20 tokens
    /// </summary>
    /// <param name="id">Wallet's id in ObjectID</param>
    /// <param name="token"></param>
    /// <returns>ERC20 wallet's information: address, balance in ERC20 tokens</returns>
    /// <remarks>
    /// Sample request:
    ///     GET /6277d227108472b96eee5e56/erc20/p2p
    /// 
    /// Sample response:
    /// 
    ///     {
    ///         "address": "0xD2BE74365557b91070405d5007ed2922996CC5da",
    ///         "balance": 0.095040493
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">Returns wallet information</response>
    /// <response code="400">When wallet id is invalid</response>
    /// <response code="404">When wallet is not found</response>
    /// <response code="500">When wallet info is not shown due to some error</response>
    [HttpGet("{id}/erc20/p2p")]
    [ProducesResponseType(typeof(ERC20P2PWalletResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetP2PTokenWallet(string id, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(id) || !IsParsable(id)) return BadRequest("Wallet id is invalid");
        try
        {
            return Ok(await _mediator.Send(new GetERC20P2PWalletQuery(id), token));
        }
        catch (AccountNotFoundException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
}