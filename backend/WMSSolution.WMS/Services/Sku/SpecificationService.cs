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
/// Specification
/// </summary>
/// <param name="dbContext"></param>
/// <param name="logger"></param>
/// <param name="stringLocalizer"></param>
public class SpecificationService(SqlDBContext dbContext,
    ILogger<SpecificationService> logger,
    IStringLocalizer<MultiLanguage> stringLocalizer) : 
    BaseService<SpecificationEntity>, ISpecificationService
{
    private readonly SqlDBContext _dbContext = dbContext;
    private readonly ILogger<SpecificationService> _logger = logger;
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    /// <summary>
    /// Delete Specification
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool flag, string? msg)> DeleteSpecificationAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var dbSet = _dbContext.GetDbSet<SpecificationEntity>();
        var entity = await dbSet.FirstOrDefaultAsync(t => t.Id == id, cancellationToken: cancellationToken);

        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }

        entity.is_delete = true;
        entity.update_time = DateTime.UtcNow;

        var qty = await _dbContext.SaveChangesAsync(cancellationToken);
        if (qty > 0)
            return (true, _stringLocalizer["delete_success"]);

        return (false, _stringLocalizer["delete_failed"]);
    }

    /// <summary>
    /// Import excel
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<int> ImportExcelData(List<InputSpecification> request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        int index = 0;
        int insertedCount = 0;
        int totalSpecs = request.Count;
        var tenantId = currentUser.tenant_id;
        var specList = _dbContext.GetDbSet<SpecificationEntity>(tenantId, true);

        do
        {
            var items = request
                .Skip(index).Take(SystemDefine.BatchSize)
                .Select(x => new SpecificationEntity
                {
                    specification_code = x.Code,
                    specification_name = x.DisplayName,
                    TenantId = tenantId
                }).ToList();

            try
            {
                var res = await SaveNewSpecificationsAsync(items, specList);
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

        } while (index < totalSpecs);

        return insertedCount;
    }

    private async Task<int> SaveNewSpecificationsAsync(List<SpecificationEntity> items,
        IQueryable<SpecificationEntity> specList)
    {
        var newSpecs = new List<SpecificationEntity>();
        foreach (var item in items)
        {
            var entity = await specList
                .FirstOrDefaultAsync(s => s.specification_code == item.specification_code &&
                    s.specification_name == item.specification_name);
            if (entity == null)
            {
                newSpecs.Add(item);
            }
            else
            {
                if (entity.is_delete)
                {
                    _logger.LogInformation("Restoring deleted specification with code {Code} and name {Name}",
                        entity.specification_code, entity.specification_name);

                    entity.is_delete = false;
                    entity.update_time = DateTime.UtcNow;
                    _dbContext.GetDbSet<SpecificationEntity>().Update(entity);
                }
            }
        }

        _dbContext.GetDbSet<SpecificationEntity>().AddRange(newSpecs);
        return await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Get all
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<List<SpecificationDTO>> GetAllAsync(CurrentUser currentUser)
    {
        var dbSet = _dbContext.GetDbSet<SpecificationEntity>(currentUser.tenant_id);
        return await dbSet.Where(t => !t.is_delete)
            .Select(x => new SpecificationDTO
            {
                Id = x.Id,
                DisplayName = x.specification_name,
                Code = x.specification_code ?? ""
            })
            .ToListAsync();
    }
}
