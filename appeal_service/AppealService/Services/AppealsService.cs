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
    private readonly UsersApi _usersApi;
    private readonly WalletsApi _walletsApi;
    private readonly NotificationService _notificationService;
    private readonly AppealsContext _context;
    private readonly IWebHostEnvironment _env;
    public AppealsService(UsersApi usersApi, AppealsContext context, IWebHostEnvironment env, WalletsApi walletsApi,
        NotificationService notificationService)
    {
        _usersApi = usersApi;
        _context = context;
        _env = env;
        _walletsApi = walletsApi;
        _notificationService = notificationService;
    }

    public async Task CreateAppealAsync(CreateAppealDto dto, IFormFile receipt, string? accessToken, CancellationToken token)
    {
        var buyer = await _usersApi.GetByEmail(dto.BuyerEmail, accessToken, token);
        var seller = await _usersApi.GetByEmail(dto.SellerEmail, accessToken, token);
        if (buyer == null) throw new ArgumentException($"User with email {dto.BuyerEmail} is not found");
        if (seller == null) throw new ArgumentException($"User with email {dto.SellerEmail} is not found");
        var createdReceipt = await AddReceiptAsync(receipt, buyer.FullName, token);
        var appeal = new Appeal
        {
            LotId = dto.LotId, CreatedAt = DateTime.Now,
            BuyerEmail = buyer.Email, BuyerName = buyer.FullName,
            SellerEmail = seller.Email, SellerName = seller.FullName,
            AttachedReceipt = createdReceipt
        };
        _context.Appeals.Add(appeal);
        await _context.SaveChangesAsync(token);
    }
    private async Task<Receipt> AddReceiptAsync(IFormFile file, string buyerName, CancellationToken token)
    {
        var receiptId = Guid.NewGuid();
        var receiptName = receiptId + "_" + buyerName + "_receipt";
        string path = $"/Receipts/{receiptName}.pdf";
        using (var stream = new FileStream(_env.WebRootPath + path, FileMode.Create))
        {
            await file.CopyToAsync(stream, token);
        }
        var receipt = new Receipt { Id = receiptId, Name = receiptName, Path = path };
        return receipt;
    }
    public async Task DeleteAppealAsync(Guid appealId, CancellationToken token)
    {
        var appeal = await _context.Appeals.Include(a => a.AttachedReceipt).FirstOrDefaultAsync(a => a.Id == appealId, token);
        if (appeal == null) throw new ArgumentException($"Appeal with id {appealId} is not found");
        _context.Appeals.Remove(appeal);
        _context.Receipts.Remove(appeal.AttachedReceipt);
        await _context.SaveChangesAsync(token);
        File.Delete(_env.WebRootPath + appeal.AttachedReceipt.Path);
    }

    public async Task<IEnumerable<AppealElementDto>> GetAppealsAsync(int page, CancellationToken token)
    {
        int rowsToSkip = 0;
        if (page >= 2) rowsToSkip = (page - 1) * 10;
        return await _context.Appeals.AsNoTracking()
            .OrderBy(a => a.LotId).Skip(rowsToSkip).Take(10)
            .Select(a => new AppealElementDto(a.LotId, a.BuyerEmail, a.SellerEmail, a.CreatedAt.ToShortDateString(), a.Id)).ToListAsync(token);
    }

    public async Task<AppealDto?> GetAppealByIdAsync(Guid appealId, CancellationToken token)
    {
       return await _context.Appeals.Include(a => a.AttachedReceipt).Select(a => new AppealDto
           {
               BuyerEmail = a.BuyerEmail, Id = a.Id, LotId = a.LotId, Receipt = a.AttachedReceipt.Name,
               ReceiptId = a.AttachedReceipt.Id, SellerEmail = a.SellerEmail
           })
           .FirstOrDefaultAsync(a => a.Id == appealId, cancellationToken: token);
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