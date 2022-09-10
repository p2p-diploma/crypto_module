using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Crypto.Application.Commands.Wallets.Buy;
using Crypto.Application.Commands.Wallets.Sell;
using Crypto.Application.Queries;
using Crypto.Application.Queries.Ethereum;
using Crypto.Application.Responses.Ethereum;
using Crypto.Domain.Configuration;
using Crypto.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wallets.Server.Filters;

namespace Wallets.Server.Controllers;
/// <summary>
/// Controller for getting eth wallets and setting amount to sell and buy.
/// </summary>
[ApiController]
[Route("api/v1/wallets/eth")]
//[TokenAuthorize(Roles.USER)]
public class EthereumWalletsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EthereumWalletsController> _logger;
    public EthereumWalletsController(IMediator mediator, ILogger<EthereumWalletsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{id}/privateKey")]
    public async Task<IActionResult> GetWalletPrivateKey(string id, CancellationToken token)
    {
        if (!IsParsable(id)) return BadRequest("Wallet id is invalid");
        var accessToken = HttpContext.Request.Cookies["jwt-access"]?.Split(' ').Last();
        if (!TryGetEmailFromToken(accessToken, out var email)) return Unauthorized();
        var privateKey = await _mediator.Send(new GetPrivateKeyQuery(id, email), token); 
        return Ok(privateKey);
    }

    private static bool TryGetEmailFromToken(string? token, out string email)
    {
        if (string.IsNullOrEmpty(token)) { email = ""; return false; }
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token)) { email = ""; return false; }
        var user = handler.ReadJwtToken(token);
        var matchEmail = user.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
        if (string.IsNullOrEmpty(matchEmail)) { email = ""; return false; }
        email = matchEmail;
        return true;
    }
    /// <summary>
    /// Returns information about user's Ethereum wallet: Ethereum account's address, private key, and balance in Ether
    /// </summary>
    /// <param name="id">Wallet's id in ObjectID format</param>
    /// <param name="token"></param>
    /// <returns>Wallet's information: address, private key, and balance in Ether</returns>
    /// <remarks>
    /// Sample request:
    ///     GET /eth/6277d227108472b96eee5e56
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
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EthereumWalletResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetEthereumWalletById(string id, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(id) || !IsParsable(id)) return BadRequest("Wallet id is invalid");
        return Ok(await _mediator.Send(new GetEthereumWalletByIdQuery(id), token));
    }

    /// <summary>
    /// Returns information about user's Ethereum wallet: Ethereum account's address, private key, and balance in Ether
    /// </summary>
    /// <param name="email">User's email</param>
    /// <param name="token"></param>
    /// <param name="includeBalance">For Appeals service: to include only wallet's id for freezing</param>
    /// <returns>Wallet's information: address, private key, and balance in Ether</returns>
    /// <remarks>
    /// Sample request:
    ///     GET /eth/email={email}
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
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(EthereumWalletResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetEthereumWalletByEmail([EmailAddress] string email, CancellationToken token,
        [FromQuery] bool includeBalance = true) =>
        Ok(await _mediator.Send(new GetEthereumWalletByEmailQuery(email, includeBalance), token));

    /// <summary>
    /// Returns information about user's Ethereum P2P wallet: Ethereum account's address, balance in Ether
    /// </summary>
    /// <param name="id">Wallet's id in ObjectID format</param>
    /// <param name="token"></param>
    /// <returns>P2P wallet's information: address, and balance in Ether</returns>
    /// <remarks>
    /// Sample request:
    ///     GET /eth/6277d227108472b96eee5e56/p2p
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
    [HttpGet("{id}/p2p")]
    [ProducesResponseType(typeof(EthereumP2PWalletResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetP2PWalletById(string id, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(id) || !IsParsable(id)) return BadRequest("Wallet id is invalid");
        return Ok(await _mediator.Send(new GetEthereumP2PWalletByIdQuery(id), token));
    }
    
    
    /// <summary>
    /// Returns information about user's Ethereum P2P wallet: Ethereum account's address, balance in Ether
    /// </summary>
    /// <param name="email">User's email</param>
    /// <param name="token"></param>
    /// <returns>P2P wallet's information: address, and balance in Ether</returns>
    /// <remarks>
    /// Sample request:
    ///     GET /eth/email={email}/p2p
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
    [HttpGet("email/{email}/p2p")]
    [ProducesResponseType(typeof(EthereumP2PWalletResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetP2PWalletByEmail([EmailAddress] string email, CancellationToken token) => 
        Ok(await _mediator.Send(new GetEthereumP2PWalletByEmailQuery(email), token));


    /// <summary>
    /// For lots: set amount of ether to buy in lot. This is the copy of balance in real wallet.
    /// </summary>
    /// <param name="command">Wallet id and amount of ether</param>
    /// <param name="token"></param>
    /// <returns>Status code of setting amount to buy</returns>
    /// <remarks>
    /// Sample request:
    ///     POST /eth/p2p/setToBuy
    /// 
    /// Sample request:
    /// 
    ///     {
    ///         "walletId": "",
    ///         "amount": 95.695
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">When amount to buy is set</response>
    /// <response code="400">When wallet id is invalid or amount is not greater than 0</response>
    /// <response code="404">When wallet is not found</response>
    /// <response code="500">When amount to buy is not set due to server error</response>
    [HttpPut("p2p/setToBuy")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> SetAmountToBuy([FromBody] SetAmountToBuyCommand command, CancellationToken token)
    {
        command.CurrencyType = CurrencyType.ETH;
        return Ok(await _mediator.Send(command, token));
    }
    
    
    /// <summary>
    /// For lots: reduce amount of ether to buy in lot. This is the copy of balance in real wallet.
    /// </summary>
    /// <param name="command">Wallet id and amount of ether</param>
    /// <param name="token"></param>
    /// <returns>Status code of reducing amount to buy</returns>
    /// <remarks>
    /// Sample request:
    ///     POST /eth/p2p/reduceToBuy
    /// 
    /// Sample request:
    /// 
    ///     {
    ///         "walletId": "",
    ///         "amount": 95.695
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">When amount to buy is reduced</response>
    /// <response code="400">When wallet id is invalid or amount is not greater than 0</response>
    /// <response code="404">When wallet is not found</response>
    /// <response code="500">When amount to buy is not reduced due to server error</response>
    [HttpPut("p2p/reduceToBuy")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> ReduceAmountToBuy([FromBody] ReduceAmountToBuyCommand command, CancellationToken token)
    {
        command.CurrencyType = CurrencyType.ETH;
        return Ok(await _mediator.Send(command, token));
    }
    
    /// <summary>
    /// For lots: increase amount of ether to buy in lot. This is the copy of balance in real wallet.
    /// </summary>
    /// <param name="command">Wallet id and amount of ether</param>
    /// <param name="token"></param>
    /// <returns>Status code of increased amount to buy</returns>
    /// <remarks>
    /// Sample request:
    ///     POST /eth/p2p/increaseToBuy
    /// 
    /// Sample request:
    /// 
    ///     {
    ///         "walletId": "",
    ///         "amount": 95.695
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">When amount to buy is increased</response>
    /// <response code="400">When wallet id is invalid or amount is not greater than 0</response>
    /// <response code="404">When wallet is not found</response>
    /// <response code="500">When amount to buy is not increased due to server error</response>
    [HttpPut("p2p/increaseToBuy")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> IncreaseAmountToBuy([FromBody] IncreaseAmountToBuyCommand command, CancellationToken token)
    {
        command.CurrencyType = CurrencyType.ETH;
        return Ok(await _mediator.Send(command, token));
    }
    
    
    /// <summary>
    /// For lots: get amount to buy in lot
    /// </summary>
    /// <param name="id">Wallet id</param>
    /// <param name="token"></param>
    /// <returns>Amount to buy in ether</returns>
    /// <response code="200">Amount to buy</response>
    /// <response code="400">When wallet id is invalid or amount is not greater than 0</response>
    /// <response code="404">When wallet is not found</response>
    /// <response code="500">When amount to buy is not reduced due to server error</response>
    [HttpGet("{id}/p2p/amountToBuy")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetAmountToBuy(string id, CancellationToken token)
    {
        if (!IsParsable(id)) return BadRequest("Wallet id is invalid");
        var query = new GetAmountToBuyQuery { WalletId = id, CurrencyType = CurrencyType.ETH};
        return Ok(await _mediator.Send(query, token));
    }
    
    
    
    
    
    /// <summary>
    /// For lots: set amount of ether to sell in lot. This is the copy of balance in real wallet.
    /// </summary>
    /// <param name="command">Wallet id and amount of ether</param>
    /// <param name="token"></param>
    /// <returns>Status code of setting amount to sell</returns>
    /// <remarks>
    /// Sample request:
    ///     POST /eth/p2p/setToSell
    /// 
    /// Sample request:
    /// 
    ///     {
    ///         "walletId": "",
    ///         "amount": 95.695
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">When amount to sell is set</response>
    /// <response code="400">When wallet id is invalid or amount is not greater than 0</response>
    /// <response code="404">When wallet is not found</response>
    /// <response code="500">When amount to sell is not set due to server error</response>
    [HttpPut("p2p/setToSell")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> SetAmountToSell([FromBody] SetAmountToSellCommand command, CancellationToken token)
    {
        command.CurrencyType = CurrencyType.ETH;
        return Ok(await _mediator.Send(command, token));
    }
    
    /// <summary>
    /// For lots: reduce amount of ether to sell in lot. This is the copy of balance in real wallet.
    /// </summary>
    /// <param name="command">Wallet id and amount of ether</param>
    /// <param name="token"></param>
    /// <returns>Status code of reducing amount to sell</returns>
    /// <remarks>
    /// Sample request:
    ///     POST /eth/p2p/reduceToSell
    /// 
    /// Sample request:
    /// 
    ///     {
    ///         "walletId": "",
    ///         "amount": 95.695
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">When amount to sell is reduced</response>
    /// <response code="400">When wallet id is invalid or amount is not greater than 0</response>
    /// <response code="404">When wallet is not found</response>
    /// <response code="500">When amount to sell is not reduced due to server error</response>
    [HttpPut("p2p/reduceToSell")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> ReduceAmountToSell([FromBody] ReduceAmountToSellCommand command, CancellationToken token)
    {
        command.CurrencyType = CurrencyType.ETH;
        return Ok(await _mediator.Send(command, token));
    }
    
    /// <summary>
    /// For lots: increase amount of ether to sell in lot. This is the copy of balance in real wallet.
    /// </summary>
    /// <param name="command">Wallet id and amount of ether</param>
    /// <param name="token"></param>
    /// <returns>Status code of increased amount to sell</returns>
    /// <remarks>
    /// Sample request:
    ///     POST /eth/p2p/increaseToSell
    /// 
    /// Sample request:
    /// 
    ///     {
    ///         "walletId": "",
    ///         "amount": 95.695
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">When amount to sell is increased</response>
    /// <response code="400">When wallet id is invalid or amount is not greater than 0</response>
    /// <response code="404">When wallet is not found</response>
    /// <response code="500">When amount to sell is not increased due to server error</response>
    [HttpPut("p2p/increaseToSell")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> IncreaseAmountToSell([FromBody] IncreaseAmountToSellCommand command, CancellationToken token)
    {
        command.CurrencyType = CurrencyType.ETH;
        return Ok(await _mediator.Send(command, token));
    }
    
    
    /// <summary>
    /// For lots: get amount to sell in lot
    /// </summary>
    /// <param name="id">Wallet id</param>
    /// <param name="token"></param>
    /// <returns>Amount to sell in ether</returns>
    /// <response code="200">Amount to sell</response>
    /// <response code="400">When wallet id is invalid or amount is not greater than 0</response>
    /// <response code="404">When wallet is not found</response>
    /// <response code="500">When amount to sell is not reduced due to server error</response>
    [HttpGet("{id}/p2p/amountToSell")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetAmountToSell(string id, CancellationToken token)
    {
        if (!IsParsable(id)) return BadRequest("Wallet id is invalid");
        var query = new GetAmountToSellQuery { WalletId = id, CurrencyType = CurrencyType.ETH};
        return Ok(await _mediator.Send(query, token));
    }
}