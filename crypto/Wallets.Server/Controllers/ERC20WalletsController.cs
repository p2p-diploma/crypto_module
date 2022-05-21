using System.ComponentModel.DataAnnotations;
using Crypto.Application.Commands.Wallets.Buy;
using Crypto.Application.Commands.Wallets.Sell;
using Crypto.Application.Queries;
using Crypto.Application.Queries.ERC20;
using Crypto.Application.Responses.ERC20;
using Crypto.Domain.Configuration;
using Crypto.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wallets.Server.Filters;

namespace Wallets.Server.Controllers;

[ApiController]
[Route("api/v1/wallets/erc20")]
[RoleAuthorize("user")]
public class ERC20WalletsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ERC20WalletsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns information about user's ERC20 wallet: associated Ethereum account's address, balance in ERC20 tokens
    /// </summary>
    /// <param name="id">Wallet's id in ObjectID</param>
    /// <param name="token"></param>
    /// <returns>ERC20 wallet's information: address, balance in ERC20 tokens</returns>
    /// <remarks>
    /// Sample request:
    ///     GET /erc20/6277d227108472b96eee5e56
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
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Erc20WalletResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetTokenWalletById(string id, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(id) || !IsParsable(id)) return BadRequest("Wallet id is invalid");
        try
        {
            return Ok(await _mediator.Send(new GetErc20WalletByIdQuery(id), token));
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
    /// Returns information about user's ERC20 wallet: associated Ethereum account's address, balance in ERC20 tokens
    /// </summary>
    /// <param name="email">User's email</param>
    /// <param name="token"></param>
    /// <returns>ERC20 wallet's information: address, balance in ERC20 tokens</returns>
    /// <remarks>
    /// Sample request:
    ///     GET /erc20/email={email}
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
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(Erc20WalletResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetTokenWalletByEmail([EmailAddress] string email, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest("Email is invalid");
        try
        {
            return Ok(await _mediator.Send(new GetErc20WalletByEmailQuery(email), token));
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
    ///     GET /erc20/6277d227108472b96eee5e56/p2p
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
    [HttpGet("{id}/p2p")]
    [ProducesResponseType(typeof(Erc20P2PWalletResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetP2PTokenWallet(string id, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(id) || !IsParsable(id)) return BadRequest("Wallet id is invalid");
        try
        {
            return Ok(await _mediator.Send(new GetErc20P2PWalletByIdQuery(id), token));
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
    /// <param name="email">User's email</param>
    /// <param name="token"></param>
    /// <returns>ERC20 wallet's information: address, balance in ERC20 tokens</returns>
    /// <remarks>
    /// Sample request:
    ///     GET /erc20/6277d227108472b96eee5e56/p2p
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
    [HttpGet("email/{email}/p2p")]
    [ProducesResponseType(typeof(Erc20P2PWalletResponse), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetP2PTokenWalletByEmail([EmailAddress] string email, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest("Email is invalid");
        try
        {
            return Ok(await _mediator.Send(new GetErc20P2PWalletByEmailQuery(email), token));
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
    
    
    
    [HttpPut("p2p/setToBuy")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> SetAmountToBuy([FromBody] SetAmountToBuyCommand command, CancellationToken token)
    {
        try
        {
            command.CurrencyType = CurrencyType.ERC20;
            return Ok(await _mediator.Send(command, token));
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
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
    
    [HttpPut("p2p/reduceToBuy")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> ReduceAmountToBuy([FromBody] ReduceAmountToBuyCommand command, CancellationToken token)
    {
        try
        {
            command.CurrencyType = CurrencyType.ERC20;
            return Ok(await _mediator.Send(command, token));
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
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
    
    
    [HttpPut("p2p/increaseToBuy")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> IncreaseAmountToBuy([FromBody] IncreaseAmountToBuyCommand command, CancellationToken token)
    {
        try
        {
            command.CurrencyType = CurrencyType.ERC20;
            return Ok(await _mediator.Send(command, token));
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
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
    
    
    [HttpGet("{id}/p2p/amountToBuy")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetAmountToBuy(string id, CancellationToken token)
    {
        if (!IsParsable(id)) return BadRequest("Wallet id is invalid");
        try
        {
            var query = new GetAmountToBuyQuery { WalletId = id, CurrencyType = CurrencyType.ETHER};
            query.CurrencyType = CurrencyType.ERC20;
            return Ok(await _mediator.Send(query, token));
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
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
    
    
    
    [HttpPut("p2p/setToSell")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> SetAmountToSell([FromBody] SetAmountToSellCommand command, CancellationToken token)
    {
        try
        {
            command.CurrencyType = CurrencyType.ERC20;
            return Ok(await _mediator.Send(command, token));
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
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
    
    [HttpPut("p2p/reduceToSell")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> ReduceAmountToSell([FromBody] ReduceAmountToSellCommand command, CancellationToken token)
    {
        try
        {
            command.CurrencyType = CurrencyType.ERC20;
            return Ok(await _mediator.Send(command, token));
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
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
    
    
    [HttpPut("p2p/increaseToSell")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> IncreaseAmountToSell([FromBody] IncreaseAmountToSellCommand command, CancellationToken token)
    {
        try
        {
            command.CurrencyType = CurrencyType.ERC20;
            return Ok(await _mediator.Send(command, token));
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
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
    
    
    [HttpGet("{id}/p2p/amountToSell")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(500)]
    public async Task<IActionResult> GetAmountToSell(string id, CancellationToken token)
    {
        if (!IsParsable(id)) return BadRequest("Wallet id is invalid");
        try
        {
            var query = new GetAmountToBuyQuery { WalletId = id, CurrencyType = CurrencyType.ERC20 };
            return Ok(await _mediator.Send(query, token));
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
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
}