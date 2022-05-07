using Crypto.Application.ERC20;
using Crypto.Application.Ethereum;
using Crypto.Domain.Dtos.Wallets;
using Crypto.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Crypto.Server.Controllers;

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

    [HttpPost("create")]
    public async Task<IActionResult> CreateWallet([FromBody] CreateWalletDto dto, CancellationToken token)
    {
        if (!ModelState.IsValid) return BadRequest();
        try
        {
            var createdWallet = await _walletService.CreateWalletAsync(dto, token);
            return createdWallet == null
                ? StatusCode(500, "Couldn't create wallet")
                : CreatedAtRoute(new { id = createdWallet.Id, privateKey = createdWallet.PrivateKey, address = createdWallet.Address }, createdWallet);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpPost("load")]
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
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpGet("{id}/ethereum")]
    public async Task<IActionResult> GetWallet(string id, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();
        try
        {
            return Ok(await _walletService.GetWalletAsync(id, token));
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
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
    
    [HttpGet("{id}/erc20")]
    public async Task<IActionResult> GetTokenWallet(string id, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();
        try
        {
            return Ok(await _tokenWalletService.GetTokenWalletAsync(id, token));
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
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