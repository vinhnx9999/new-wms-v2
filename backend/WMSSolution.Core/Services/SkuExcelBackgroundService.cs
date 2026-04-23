using Microsoft.Extensions.Hosting;
using System.Threading.Channels;
using WMSSolution.Core.DBContext;
using WMSSolution.Shared.Excel;

namespace WMSSolution.Core.Services;

/// <summary>
/// Sku Excel Background
/// </summary>
/// <param name="sqlDbContext"></param>
public class SkuExcelBackgroundService(SqlDBContext sqlDbContext) : BackgroundService
{
    private readonly Channel<List<InputSku>> _queue = Channel.CreateUnbounded<List<InputSku>>();
    private readonly SqlDBContext _sqlDbContext = sqlDbContext;

    /// <summary>
    /// Queue Import
    /// </summary>
    /// <param name="skus"></param>
    /// <returns></returns>
    public async Task QueueImportAsync(List<InputSku> skus)
    {
        await _queue.Writer.WriteAsync(skus);
    }

    public async Task ExecuteImportAsync(List<InputSku> skus)
    {
        var units = skus.Select(x => x.Unit).Distinct();
        var specifications = skus.Select(x => x.Specification).Distinct();
        var productCodes = skus.Select(x => x.ProductCode).Distinct();
        var categories = skus.Select(x => x.Category).Distinct();
        var properties = skus.Select(x => x.Property).Distinct();

        using var transaction = await _sqlDbContext.Database.BeginTransactionAsync();
        try
        {
            // Thực hiện các thao tác import dữ liệu vào database
            // Ví dụ: _dbContext.Skus.AddRange(skus);
            // Sau đó gọi _dbContext.SaveChangesAsync() để lưu vào database
            //CheckUnits(units);
            //CheckSpecifications(specifications);
            //await _sqlDbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            await transaction.RollbackAsync();
            // Xử lý lỗi nếu cần thiết
        }
    }

    private void CheckUnits(IEnumerable<string> units)
    {
        //_sqlDbContext.GetDbSet<SpuEntity>
        //        .AsNoTracking()
        //        .Select(x => x.Unit)
        //        .Distinct()
        //        .ToList();
        //foreach (var unit in units)
        //{
        //    // Kiểm tra nếu unit chưa tồn tại trong database thì thêm mới
        //    if (!_sqlDbContext.Units.Any(u => u.Name == unit))
        //    {
        //        _sqlDbContext.Units.Add(new Unit { Name = unit });
        //    }   
        //}
    }

    /// <summary>
    /// Execute
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //var skus = await _queue.Reader.ReadAllAsync(stoppingToken);
        await foreach (var skus in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            // chạy import trong background
            var units = skus.Select(x => x.Unit).Distinct();
            var specifications = skus.Select(x => x.Specification).Distinct();
            var productCodes = skus.Select(x => x.ProductCode).Distinct();
            var categories = skus.Select(x => x.Category).Distinct();
            var properties = skus.Select(x => x.Property).Distinct();

            //await _skuService.ImportExcelData(skus);
        }
    }
}
