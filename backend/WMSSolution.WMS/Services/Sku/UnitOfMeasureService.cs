using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Shared;
using WMSSolution.Shared.Excel;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.IServices.Sku;

namespace WMSSolution.WMS.Services.Sku;

/// <summary>
/// UnitOfMeasure Service
/// </summary>
/// <param name="dbContext"></param>
/// <param name="stringLocalizer"></param>
/// <param name="logger"></param>
public class UnitOfMeasureService(SqlDBContext dbContext, 
    IStringLocalizer<MultiLanguage> stringLocalizer,
    ILogger<UnitOfMeasureService> logger) : 
    BaseService<SkuUomEntity>, IUnitOfMeasureService
{
    private readonly SqlDBContext _dbContext = dbContext;
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;
    private readonly ILogger<UnitOfMeasureService> _logger = logger;
    /// <summary>
    /// Delete UnitOfMeasure
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<(bool flag, string msg)> DeleteUnitAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var dbSet = _dbContext.GetDbSet<SkuUomEntity>(tenantId, true);
        var entity = await dbSet.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }
        
        // check if reference?
        var skuUomLinkSet = _dbContext.GetDbSet<SkuUomLinkEntity>().AsNoTracking();
        if (await skuUomLinkSet.AnyAsync(t => t.SkuUomId == id, cancellationToken))
        {
            return (false, _stringLocalizer["delete_referenced"]);
        }

        _dbContext.GetDbSet<SkuUomEntity>().Remove(entity);
        var res = await _dbContext.SaveChangesAsync(cancellationToken);

        return res > 0 ? 
            (true, _stringLocalizer["delete_success"]): 
            (false, _stringLocalizer["delete_failed"]);
    }

    /// <summary>
    /// Get All
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<List<UnitDTO>> GetAllAsync(CurrentUser currentUser)
    {
        var dbSet = _dbContext.GetDbSet<SkuUomEntity>(currentUser.tenant_id);
        return await dbSet
            .Select(x => new UnitDTO
            {
                Id = x.Id,
                UnitName = x.UnitName,
                Description = x.Description
            })
            .ToListAsync();
    }

    /// <summary>
    /// Import Excel
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<int> ImportExcelData(List<InputUnitOfMeasure> request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        int index = 0;
        int insertedCount = 0;
        int totalUnits = request.Count;
        var tenantId = currentUser.tenant_id;
        var unitList = _dbContext.GetDbSet<SkuUomEntity>(tenantId);

        do
        {
            var items = request
                .Skip(index).Take(SystemDefine.BatchSize)
                .Select(x => new SkuUomEntity
                {
                    UnitName = x.Name,
                    Description = x.Description,
                    TenantId = tenantId
                }).ToList();

            try
            {
                var res = await SaveNewUnitsAsync(items, unitList);
                insertedCount += res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting batch starting at index {Index}", index);
                // Optionally, you can choose to break the loop or continue with the next batch
                // break;
            }

            // Xử lý batch
            Console.WriteLine($"Batch starting at {index}, count = {items.Count}");
            // Tăng index lên batchSize
            index += SystemDefine.BatchSize;

        } while (index < totalUnits);

        return insertedCount;
    }

    private async Task<int> SaveNewUnitsAsync(List<SkuUomEntity> items, IQueryable<SkuUomEntity> unitList)
    {
        var newUnits = new List<SkuUomEntity>();
        foreach (var item in items)
        {
            bool exists = await unitList.AnyAsync(s => s.UnitName == item.UnitName);
            if (!exists)
            {
                newUnits.Add(item);
            }
        }

        _dbContext.GetDbSet<SkuUomEntity>().AddRange(newUnits);
        return await _dbContext.SaveChangesAsync();
    }
}

