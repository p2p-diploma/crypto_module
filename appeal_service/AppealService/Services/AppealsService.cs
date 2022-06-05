using System.Net;
using AppealService.Api;
using AppealService.Api.Config;
using AppealService.Contexts;
using AppealService.Dtos;
using AppealService.Models;
using Microsoft.EntityFrameworkCore;

namespace AppealService.Services;

public class AppealsService
{
    private readonly WalletsApi _walletsApi;
    private readonly TransactionsApi _transactionsApi;
    private readonly NotificationService _notificationService;
    private readonly AppealsContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AppealsService> _logger;
    public AppealsService(AppealsContext context, IWebHostEnvironment env, WalletsApi walletsApi,
        NotificationService notificationService, ILogger<AppealsService> logger, TransactionsApi transactionsApi)
    {
        _context = context;
        _env = env;
        _walletsApi = walletsApi;
        _notificationService = notificationService;
        _logger = logger;
        _transactionsApi = transactionsApi;
        if (!Directory.Exists(_env.WebRootPath + "/Receipts"))
            Directory.CreateDirectory(_env.WebRootPath + "/Receipts");
    }

    public async Task CreateAppealAsync(CreateAppealDto dto, string accessToken, IFormFile receipt, CancellationToken token)
    {
        var createdReceipt = await AddReceiptAsync(receipt, dto.BuyerEmail, token);
        var transaction = await _transactionsApi.GetById(dto.TransactionId, accessToken, token);
        if (transaction == null)
        {
            _logger.LogError($"Transaction with id {dto.TransactionId} not found");
            throw new ArgumentException($"Transaction with id {dto.TransactionId} not found");
        }

        var transactionId = Guid.Parse(dto.TransactionId);
        var appeal = new Appeal
        {  
            CreatedAt = DateTime.Now,
            BuyerEmail = dto.BuyerEmail, 
            SellerEmail = dto.SellerEmail, 
            AttachedReceipt = createdReceipt, Transaction = new Transaction
            {
                Amount = transaction.Amount, Status = transaction.Status, Id = transactionId,
                CreatedAt = transaction.CreatedAt, UpdatedAt = transaction.UpdatedAt
            }
        };
        _context.Appeals.Add(appeal);
        await _context.SaveChangesAsync(token);
    }
    private async Task<Receipt> AddReceiptAsync(IFormFile file, string email, CancellationToken token)
    {
        var receiptId = Guid.NewGuid();
        var receiptName = receiptId + "_" + email + "_receipt";
        string path = $"/Receipts/{receiptName}.pdf";
        using (var stream = new FileStream(_env.WebRootPath + path, FileMode.Create))
        {
            await file.CopyToAsync(stream, token);
            if(stream.Length > 0)
                _logger.LogInformation($"Copied file {receiptName}.pdf to {path} successfully");
        }
        var receipt = new Receipt { Id = receiptId, Name = receiptName, Path = path };
        return receipt;
    }
    public async Task DeleteAppealAsync(Guid appealId, CancellationToken token)
    {
        var appeal = await _context.Appeals.Include(a => a.AttachedReceipt).Include(a => a.Transaction)
            .FirstOrDefaultAsync(a => a.Id == appealId, token);
        if (appeal == null) throw new ArgumentException($"Appeal with id {appealId} is not found");
        _context.Appeals.Remove(appeal);
        _context.Receipts.Remove(appeal.AttachedReceipt);
        _context.Transactions.Remove(appeal.Transaction);
        try
        {
            await _context.SaveChangesAsync(token);
            File.Delete(_env.WebRootPath + appeal.AttachedReceipt.Path);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    public async Task<IEnumerable<AppealItemDto>> GetAppealsAsync(int page = 1, int pageCount = 5, CancellationToken token = default)
    {
        int rowsToSkip = 0;
        if (page >= 2) rowsToSkip = (page - 1) * pageCount;
        return await _context.Appeals.AsNoTracking()
            .OrderBy(a => a.Transaction.Id).Skip(rowsToSkip).Take(pageCount)
            .Select(a => new AppealItemDto(a.Transaction.Id.ToString(), a.BuyerEmail, a.SellerEmail, a.CreatedAt.ToString("f"), a.Id)).ToListAsync(token);
    }

    public async Task<AppealDto?> GetAppealByIdAsync(Guid appealId, CancellationToken token)
    {
        return await _context.Appeals.Include(a => a.AttachedReceipt).Include(a => a.Transaction)
            .Select(appeal => new AppealDto
            {
                Id = appealId, Receipt = appeal.AttachedReceipt.Name,
                Transaction = new TransactionDto
                {
                    CreatedAt = appeal.Transaction.CreatedAt, UpdatedAt = appeal.Transaction.UpdatedAt,
                    Amount = appeal.Transaction.Amount, Status = appeal.Transaction.Status, Id = appeal.Transaction.Id.ToString()
                },
                BuyerEmail = appeal.BuyerEmail, ReceiptId = appeal.AttachedReceipt.Id, SellerEmail = appeal.SellerEmail
            }).FirstOrDefaultAsync(a => a.Id == appealId, cancellationToken: token);
    }
    public async Task<Receipt?> GetReceiptById(Guid id)
    {
        return await _context.Receipts.FindAsync(id);
    }

    public async Task<ApiResult> FreezeAccount(string buyerEmail, string sellerEmail, string? accessToken, CancellationToken token)
    {
        var result = await _walletsApi.FreezeWallets(accessToken, sellerEmail, token);
        if(result.StatusCode == HttpStatusCode.OK)
            await Task.WhenAll(_notificationService.SendToSeller(sellerEmail, token), _notificationService.SendToBuyer(buyerEmail, token));
        return result;
    }
}