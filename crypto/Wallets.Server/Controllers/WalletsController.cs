using Crypto.Application.ERC20;
using Crypto.Application.Ethereum;
using Crypto.Domain.Dtos.ERC20;
using Crypto.Domain.Dtos.Wallets;
using Crypto.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Wallets.Server.Controllers;

/// <summary>
/// API for managing Ethereum and ERC20 wallets
/// </summary>
[ApiController]
[Route("/api/v1/[controller]")]
public class WalletsController : ControllerBase
{
    private readonly EthereumWalletService _walletService;
    private readonly ERC20WalletService _tokenWalletService;
    public WalletsController(EthereumWalletService walletService, ERC20WalletService tokenWalletService)
    {
        _walletService = walletService;
        _tokenWalletService = tokenWalletService;
    }

    /// <summary>
    /// Create wallet (Ethereum account) with zero balance
    /// </summary>
    /// <param name="userInfo">User's login and password </param>
    /// <param name="token"></param>
    /// <returns>Created wallet with wallet's ID, assigned address and private key</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /create
    ///     {
    ///         "email": "testemail@gmail.com",
    ///         "password": "testpassword123"
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
    [ProducesResponseType(typeof(CreatedWalletDto), 201)]
    [ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> CreateWallet([FromBody] CreateWalletDto userInfo, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        try
        {
            var createdWallet = await _walletService.CreateWalletAsync(userInfo, token);
            return createdWallet == null
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
    /// <param name="dto">User's login, password and provided private key of an account</param>
    /// <param name="token"></param>
    /// <returns>Created (loaded) wallet with wallet's ID, assigned address and private key</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /load
    ///     {
    ///         "email": "testemail@gmail.com",
    ///         "password": "testpassword123",
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
    [ProducesResponseType(typeof(CreatedWalletDto), 201)]
    [ProducesResponseType(400), ProducesResponseType(500)]
    public async Task<IActionResult> LoadWallet([FromBody] LoadWalletDto dto, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        try 
        {
            var loadedWallet = await _walletService.LoadWalletAsync(dto, token);
            return loadedWallet == null ? StatusCode(500, "Couldn't load wallet") :
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
    [ProducesResponseType(typeof(EthereumWalletDto), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetWallet(string id, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();
        try
        {
            return Ok(await _walletService.GetWalletAsync(id, token));
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
    [ProducesResponseType(typeof(ERC20WalletDto), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetTokenWallet(string id, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();
        try
        {
            return Ok(await _tokenWalletService.GetTokenWalletAsync(id, token));
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