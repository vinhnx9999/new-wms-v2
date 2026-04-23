using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text;
using System.Text.Json;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Models.IntegrationWCS;
using WMSSolution.Core.Services;
using WMSSolution.Shared;
using WMSSolution.Shared.Enums;
using WMSSolution.Shared.Enums.Location;
using WMSSolution.Shared.Excel;
using WMSSolution.Shared.MasterData;
using WMSSolution.Shared.Planning;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.OutboundGateway;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.Models.Warehouse;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Dashboard;
using WMSSolution.WMS.Entities.ViewModels.Warehouse;
using WMSSolution.WMS.Entities.ViewModels.Warehouse.Invertory;
using WMSSolution.WMS.IServices.ActionLog;
using WMSSolution.WMS.IServices.Warehouse;

namespace WMSSolution.WMS.Services.Warehouse;


/// <summary>
///  Warehouse Service
/// </summary>
/// <remarks>
///Warehouse  constructor
/// </remarks>
/// <param name="dbContext">The DBContext</param>
/// <param name="actionLogService"></param>
/// <param name="stringLocalizer">Localizer</param>
/// <param name="logger">Logger</param>
public class WarehouseService(SqlDBContext dbContext,
    IActionLogService actionLogService,
        IStringLocalizer<MultiLanguage> stringLocalizer,
        ILogger<WarehouseService> logger) :
    BaseService<WarehouseEntity>, IWarehouseService
{
    #region Args
    /// <summary>
    /// The DBContext
    /// </summary>
    private readonly SqlDBContext _dbContext = dbContext;

    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    /// <summary>
    /// Logger Service
    /// </summary>
    private readonly ILogger<WarehouseService> _logger = logger;
    private readonly IActionLogService _actionLogService = actionLogService;
    #endregion

    #region Api
    /// <summary>
    /// get select items
    /// </summary>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<List<FormSelectItem>> GetSelectItemsAsnyc(CurrentUser currentUser)
    {
        var res = new List<FormSelectItem>();
        var DBSet = _dbContext.GetDbSet<WarehouseEntity>();
        res.AddRange(await (from db in DBSet.AsNoTracking()
                            where db.is_valid == true && db.TenantId == currentUser.tenant_id
                            select new FormSelectItem
                            {
                                code = "WarehouseName",
                                name = db.WarehouseName,
                                value = db.Id.ToString(),
                                comments = "warehouse datas",
                            }).ToListAsync());
        return res;
    }

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    public async Task<(List<WarehouseViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count != 0)
        {
            pageSearch.searchObjects.ForEach(s =>
            {
                queries.Add(s);
            });
        }

        var tenantId = currentUser.tenant_id;
        var query = _dbContext.GetDbSet<WarehouseEntity>(tenantId).Where(t => t.is_valid);
        var expression = queries.AsExpression<WarehouseEntity>();

        if (expression != null)
        {
            query = query.Where(expression);
        }

        int totals = await query.CountAsync(cancellationToken);
        var list = await query.OrderByDescending(t => t.create_time)
                   .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                   .Take(pageSearch.pageSize)
                   .ToListAsync();

        var items = await ConvertData2Model(list, tenantId);
        return (items, totals);
    }

    /// <summary>
    /// Get all records
    /// </summary>
    /// <returns></returns>
    public async Task<List<WarehouseViewModel>> GetAllAsync(CurrentUser currentUser)
    {
        var tenantId = currentUser.tenant_id;
        var data = await _dbContext.GetDbSet<WarehouseEntity>(tenantId).ToListAsync();
        return await ConvertData2Model(data, tenantId);
    }

    private async Task<List<WarehouseViewModel>> ConvertData2Model(List<WarehouseEntity> data, long tenantId)
    {
        var results = data.Adapt<List<WarehouseViewModel>>();
        var queryable = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);
        var queryStock = _dbContext.GetDbSet<StockEntity>(tenantId).Where(s => s.qty > 0);

        foreach (var item in results)
        {
            var locationIds = queryable
                .Where(x => x.WarehouseId == item.Id)
                .Select(x => x.Id)
                .Distinct();

            item.TotalPallet = await SumTotalPalletAsync(queryStock, locationIds);
            item.TotalInventory = await SumTotalInventoryAsync(queryStock, locationIds);
            item.city = item.city;
            item.WarehouseName = item.WarehouseName;
            item.tenant_id = tenantId;
            item.is_valid = item.is_valid;
            item.LocationCount = await queryable.Where(t => t.WarehouseId == item.Id).CountAsync();
        }

        return results;
    }

    private async Task<int> SumTotalInventoryAsync(IQueryable<StockEntity> queryStock,
        IQueryable<int> locationIds)
    {
        return await queryStock
            .Where(s => locationIds.Contains(s.goods_location_id))
            .SumAsync(s => s.qty);
    }
    private async Task<int> SumTotalPalletAsync(IQueryable<StockEntity> queryStock,
        IQueryable<int> locationIds)
    {
        return await queryStock
            .Where(s => locationIds.Contains(s.goods_location_id))
            .CountAsync();
    }

    /// <summary>
    /// Get a record by id
    /// </summary>
    /// <returns></returns>
    public async Task<WarehouseViewModel> GetAsync(int id)
    {
        var DbSet = _dbContext.GetDbSet<WarehouseEntity>();
        var entity = await DbSet.AsNoTracking().FirstOrDefaultAsync(t => t.Id.Equals(id));
        if (entity == null)
        {
            return new WarehouseViewModel();
        }
        return entity.Adapt<WarehouseViewModel>();
    }
    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">current user</param>
    /// <param name="cancellationToken" >cancellationToken</param>
    /// <returns></returns>
    public async Task<(int id, string msg)> AddAsync(WarehouseVM viewModel, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var warehouseDbSet = _dbContext.GetDbSet<WarehouseEntity>();
        var isDuplicateWarehouseName = await warehouseDbSet.AnyAsync(t => t.WarehouseName.ToLower() == viewModel.WarehouseName.ToLower()
                                                             && t.TenantId == currentUser.tenant_id, cancellationToken);
        if (isDuplicateWarehouseName)
        {
            _logger.LogError("Duplicate warehouse name: {WarehouseName}", viewModel.WarehouseName);
            return (0, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["WarehouseName"], viewModel.WarehouseName));
        }

        // mapping anual 
        var entity = new WarehouseEntity
        {
            Id = 0,
            WarehouseName = viewModel.WarehouseName,
            city = viewModel.City,
            address = viewModel.Address,
            email = viewModel.Email,
            manager = viewModel.Manager,
            contact_tel = viewModel.ContactTel,
            creator = currentUser.user_name,
            create_time = DateTime.UtcNow,
            last_update_time = DateTime.UtcNow,
            is_valid = true,
            TenantId = currentUser.tenant_id
        };

        await warehouseDbSet.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        if (entity.Id > 0)
        {
            await _actionLogService.AddLogAsync(
                $"Creating new warehouse {entity.WarehouseName} by {currentUser.user_name}",
                "Warehouse", currentUser);
            return (entity.Id, _stringLocalizer["save_success"]);
        }
        else
        {
            return (0, _stringLocalizer["save_failed"]);
        }
    }
    /// <summary>
    /// update a record
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> UpdateAsync(WarehouseViewModel viewModel, CurrentUser currentUser)
    {
        var DbSet = _dbContext.GetDbSet<WarehouseEntity>();
        if (await DbSet.AnyAsync(t => t.Id != viewModel.Id && t.WarehouseName == viewModel.WarehouseName && t.TenantId == currentUser.tenant_id))
        {
            return (false, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["WarehouseName"], viewModel.WarehouseName));
        }
        var entity = await DbSet.FirstOrDefaultAsync(t => t.Id.Equals(viewModel.Id));
        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }

        string jsonString = JsonSerializer.Serialize(new
        {
            id = entity.Id,
            name = entity.WarehouseName,
            address = entity.address,
            contact = entity.contact_tel,
            city = entity.city
        });

        entity.WarehouseName = viewModel.WarehouseName;
        entity.city = viewModel.city;
        entity.address = viewModel.address;
        entity.email = viewModel.email;
        entity.manager = viewModel.manager;
        entity.contact_tel = viewModel.contact_tel;
        entity.is_valid = viewModel.is_valid;
        entity.last_update_time = DateTime.UtcNow;
        var warehousearea_DBSet = _dbContext.GetDbSet<WarehouseareaEntity>();
        var wadatas = await warehousearea_DBSet.Where(t => t.WarehouseId == entity.Id).ToListAsync();
        wadatas.ForEach(t =>
        {
            t.is_valid = entity.is_valid;
        });
        var goodslocation_DBSet = _dbContext.GetDbSet<GoodslocationEntity>();
        var gldatas = await goodslocation_DBSet.Where(t => t.WarehouseAreaId == entity.Id).ToListAsync();
        gldatas.ForEach(t =>
        {
            t.WarehouseName = entity.WarehouseName;
            t.IsValid = entity.is_valid;
        });
        var qty = await _dbContext.SaveChangesAsync();
        if (qty > 0)
        {
            string jsonModel = JsonSerializer.Serialize(viewModel);
            await _actionLogService.AddLogAsync(
                $"Updated warehouse {jsonString} => {jsonModel} by {currentUser.user_name}",
                "Warehouse", currentUser);

            return (true, _stringLocalizer["save_success"]);
        }
        else
        {
            return (false, _stringLocalizer["save_failed"]);
        }
    }
    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> DeleteAsync(int id, CurrentUser currentUser)
    {
        if (await _dbContext.GetDbSet<GoodslocationEntity>().AnyAsync(t => t.WarehouseId == id))
        {
            return (false, _stringLocalizer["exist_warehousearea_not_delete"]);
        }

        var entity = await _dbContext.GetDbSet<WarehouseEntity>()
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == currentUser.tenant_id);
        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }

        string jsonString = JsonSerializer.Serialize(new
        {
            id = entity.Id,
            name = entity.WarehouseName,
            address = entity.address,
            contact = entity.contact_tel,
            city = entity.city
        });

        var qty = await _dbContext.GetDbSet<WarehouseEntity>().Where(t => t.Id.Equals(id)).ExecuteDeleteAsync();
        if (qty > 0)
        {
            await _actionLogService.AddLogAsync(
                $"Delete warehouse {jsonString} by {currentUser.user_name}",
                "Warehouse", currentUser);
            return (true, _stringLocalizer["delete_success"]);
        }
        else
        {
            return (false, _stringLocalizer["delete_failed"]);
        }
    }

    /// <summary>
    /// import warehouses by excel
    /// </summary>
    /// <param name="datas">excel datas</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ExcelAsync(List<WarehouseExcelImportViewModel> datas, CurrentUser currentUser)
    {
        StringBuilder sb = new();
        var DbSet = _dbContext.GetDbSet<WarehouseEntity>();
        var WarehouseName_repeat_excel = datas.GroupBy(t => t.WarehouseName).Select(t => new { WarehouseName = t.Key, cnt = t.Count() }).Where(t => t.cnt > 1).ToList();
        foreach (var repeat in WarehouseName_repeat_excel)
        {
            sb.AppendLine(string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["WarehouseName"], repeat.WarehouseName));
        }
        if (WarehouseName_repeat_excel.Count > 0)
        {
            return (false, sb.ToString());
        }

        var WarehouseName_repeat_exists = await DbSet.Where(t => t.TenantId == currentUser.tenant_id).Where(t => datas.Select(t => t.WarehouseName).ToList().Contains(t.WarehouseName)).Select(t => t.WarehouseName).ToListAsync();
        foreach (var repeat in WarehouseName_repeat_exists)
        {
            sb.AppendLine(string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["WarehouseName"], repeat));
        }
        if (WarehouseName_repeat_exists.Count > 0)
        {
            return (false, sb.ToString());
        }

        var entities = datas.Adapt<List<WarehouseEntity>>();
        entities.ForEach(t =>
        {
            t.creator = currentUser.user_name;
            t.TenantId = currentUser.tenant_id;
            t.create_time = DateTime.UtcNow;
            t.last_update_time = DateTime.UtcNow;
            t.is_valid = true;
        });
        await DbSet.AddRangeAsync(entities);
        var res = await _dbContext.SaveChangesAsync();
        if (res > 0)
        {
            return (true, _stringLocalizer["save_success"]);
        }
        return (false, _stringLocalizer["save_failed"]);
    }

    /// <summary>
    /// Get Rule Settings
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<IEnumerable<RuleSettingsViewModel>> GetRuleSettingsAsync(int id, CurrentUser currentUser)
    {
        var tenantId = currentUser.tenant_id;
        var dbSet = _dbContext.GetDbSet<WarehouseRuleSettingsEntity>(tenantId);
        var rs = await dbSet.Where(t => t.WarehouseId == id).ToListAsync();
        var data = new List<RuleSettingsViewModel>();
        var skus = _dbContext.GetDbSet<SkuEntity>(tenantId);
        var suppliers = _dbContext.GetDbSet<SupplierEntity>();
        var categories = _dbContext.GetDbSet<CategoryEntity>().AsNoTracking();

        foreach (var r in rs)
        {
            data.Add(new RuleSettingsViewModel
            {
                Id = r.Id,
                BlockId = r.BlockId,
                CategoryId = r.CategoryId,
                FloorId = r.FloorId,
                RuleSettings = r.RuleSettings,
                SkuId = r.SkuId,
                SupplierId = r.SupplierId,
                FloorName = GetFloorName(r.FloorId),
                SkuName = GetSkuName(skus, r.SkuId),
                SupplierName = GetSupplierName(suppliers, r.SupplierId),
                CategoryName = GetCategoryName(categories, r.CategoryId)
            });
        }

        return data;
    }

    private string GetCategoryName(IQueryable<CategoryEntity> categories, int? categoryId)
    {
        if (!categoryId.HasValue) return "";
        return categories.Where(t => t.Id == categoryId).Select(t => t.category_name).FirstOrDefault() ?? "";
    }

    private string GetSupplierName(DbSet<SupplierEntity> suppliers, int? supplierId)
    {
        if (!supplierId.HasValue) return "";

        return suppliers.Where(t => t.Id == supplierId).Select(t => t.supplier_name).FirstOrDefault() ?? "";
    }

    private string GetSkuName(IQueryable<SkuEntity> skus, int? skuId)
    {
        if (!skuId.HasValue) return "";
        return skus.Where(t => t.Id == skuId).Select(t => $"{t.sku_code}-{t.sku_name}").FirstOrDefault() ?? "";
    }

    private string GetFloorName(int? floorId)
    {
        if (floorId.GetValueOrDefault() < 1) return "";

        return floorId.GetValueOrDefault() == 1 ? "First Floor" : $"Floor {floorId}";
    }

    /// <summary>
    /// Warehouse rule settings
    /// </summary>
    /// <param name="model"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<(int id, string msg)> CreateRuleSettingsAsync(WarehouseSettingsViewModel model, CurrentUser currentUser)
    {
        var dbSet = _dbContext.GetDbSet<WarehouseRuleSettingsEntity>();
        bool isCurrentRule = await CheckRuleSettingsBeforeSave(dbSet, currentUser.tenant_id, model);
        if (isCurrentRule) return (0, _stringLocalizer["InvalidData"]);

        var entity = new WarehouseRuleSettingsEntity
        {
            WarehouseId = model.WarehouseId,
            BlockId = model.BlockId,
            CategoryId = model.CategoryId,
            FloorId = model.FloorId,
            RuleSettings = model.RuleSettings,
            SkuId = model.SkuId,
            SupplierId = model.SupplierId,
            TenantId = currentUser.tenant_id
        };

        await dbSet.AddAsync(entity);
        var res = await _dbContext.SaveChangesAsync();

        string jsonString = JsonSerializer.Serialize(entity);
        await _actionLogService.AddLogAsync(
                $"[Rule Settings] {jsonString} by {currentUser.user_name}",
                "Warehouse", currentUser);

        return res > 0 ?
            (entity.Id, _stringLocalizer["save_success"]) :
            (0, _stringLocalizer["save_failed"]);
    }


    /// <summary>
    /// Synchronous Wcs Locations
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<(int id, string msg)> SynchronousWcsLocationsAsync(SyncWcsLocationViewModel viewModel, CurrentUser currentUser)
    {
        var tenantId = currentUser.tenant_id;
        int storeId = viewModel.WarehouseId;

        var dbSet = _dbContext.GetDbSet<WarehouseEntity>()
            .Where(x => x.TenantId == tenantId);
        var warehouse = await dbSet.FirstOrDefaultAsync(x => x.Id == storeId);
        if (warehouse == null)
        {
            return (0, _stringLocalizer["not_exists_entity"]);
        }

        if (!string.IsNullOrEmpty(warehouse.WcsBlockId) && warehouse.WcsBlockId != viewModel.WcsBlockId)
        {
            return (0, _stringLocalizer["WcsBlock_exists"]);
        }

        warehouse.WcsBlockId = viewModel.WcsBlockId;
        _dbContext.GetDbSet<WarehouseEntity>().Update(warehouse);

        var locationDbSet = _dbContext
            .GetDbSet<GoodslocationEntity>(tenantId, true)
            .Where(x => x.WarehouseId == storeId);

        var locationList = await locationDbSet.ToListAsync();

        var stockDbSet = _dbContext.GetDbSet<StockEntity>(tenantId);

        int batchSize = 100;
        int index = 0;
        int insertedCount = 0;
        int totalLocations = viewModel.Locations.Count();

        do
        {
            var wcsLocations = viewModel.Locations
                .Skip(index).Take(batchSize)
                .Select(x => new GoodslocationEntity
                {
                    WarehouseId = storeId,
                    WarehouseName = warehouse.WarehouseName,
                    LocationName = x.Address,
                    GoodsLocationType = x.Type.GetLocationType(),
                    CoordinateX = $"{x.CoordX}",
                    CoordinateY = $"{x.CoordY}",
                    CoordinateZ = $"{x.CoordZ}",
                    Priority = x.StoragePriority.GetValueOrDefault(),
                    IsValid = true,
                    WarehouseAreaName = x.Zone,
                    TenantId = tenantId,
                    LocationStatus = (byte)x.Status.GetLocationStatus()
                }).ToList();
            SaveNewLocations(locationList, wcsLocations);
            UpdateLocations(locationList, wcsLocations, stockDbSet);

            var res = await _dbContext.SaveChangesAsync();
            insertedCount += res;
            // Xử lý batch
            Console.WriteLine($"Batch starting at {index}, count = {wcsLocations.Count}");
            // Tăng index lên batchSize
            index += batchSize;

        } while (index < totalLocations);

        await _actionLogService.AddLogAsync(
                $"[Synchronous Wcs] Locations {totalLocations} has been sync {insertedCount} rows by {currentUser.user_name}",
                "Warehouse", currentUser);

        return insertedCount > 0 ?
            (insertedCount, _stringLocalizer["save_success"]) :
            (0, _stringLocalizer["save_failed"]);
    }

    /// <summary>
    /// Only Update location type and priority, 
    /// other fields will not be updated to avoid affecting WMS operation, such as location name, coordinate, etc.
    /// </summary>
    /// <param name="locationList"></param>
    /// <param name="wcsLocations"></param>
    /// <param name="stockDbSet"></param>
    private void UpdateLocations(List<GoodslocationEntity> locationList,
        List<GoodslocationEntity> wcsLocations,
        IQueryable<StockEntity> stockDbSet)
    {
        var items = wcsLocations
            .Where(x => locationList.Any(l => l.LocationName == x.LocationName))
            .ToList();

        if (items.Count == 0)
        {
            return;
        }

        var syncConflictDbSet = _dbContext.GetDbSet<LocationSyncConflictEntity>();

        foreach (var item in items)
        {
            var current = locationList
                .FirstOrDefault(l => l.LocationName == item.LocationName);

            if (current != null)
            {
                //The same value of priority and location type, no need to update
                if (current.Priority == item.Priority &&
                    current.GoodsLocationType == item.GoodsLocationType
                    )
                {
                    continue;
                }

                var hasPallet = stockDbSet
                    .Where(s => s.goods_location_id == current.Id)
                    .Any();

                if (!hasPallet)
                {
                    current.GoodsLocationType = item.GoodsLocationType;
                }
                current.Priority = item.Priority;


                _dbContext.GetDbSet<GoodslocationEntity>().Update(current);
            }
        }
    }

    private void SaveNewLocations(List<GoodslocationEntity> locationList,
        List<GoodslocationEntity> wcsLocations)
    {
        var items = wcsLocations
            .Where(x => !locationList.Any(l => l.LocationName == x.LocationName))
            .ToList();

        _dbContext.GetDbSet<GoodslocationEntity>().AddRange(items);
    }

    /// <summary>
    /// DeleteRuleSettings
    /// </summary>
    /// <param name="id"></param>
    /// <param name="settingRuleId"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<(bool flag, string msg)> DeleteRuleSettings(int id, int settingRuleId, CurrentUser currentUser)
    {
        var dbSet = _dbContext.GetDbSet<WarehouseRuleSettingsEntity>();
        var ruleSettings = await dbSet.FirstOrDefaultAsync(t => t.Id == settingRuleId &&
                t.WarehouseId == id && t.TenantId == currentUser.tenant_id);

        if (ruleSettings == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }

        string jsonString = JsonSerializer.Serialize(ruleSettings);
        await _actionLogService.AddLogAsync(
                $"[Delete Rule Settings] WarehouseId {id} has deleted RuleId {settingRuleId} = {jsonString} by {currentUser.user_name}",
                "Warehouse", currentUser);

        dbSet.Remove(ruleSettings);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0 ?
            (true, _stringLocalizer["save_success"]) :
            (false, _stringLocalizer["save_failed"]);
    }


    /// <summary>
    /// Warehouse Info
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<InventoryDTO> GetWarehouseInfo(CurrentUser currentUser)
    {
        var tenantId = currentUser.tenant_id;

        var warehouseCount = await _dbContext.GetDbSet<WarehouseEntity>(tenantId)
            .CountAsync(t => t.is_valid);

        var totalLocations = await _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
            .CountAsync(t => t.IsValid);

        var totalPallets = await (from stock in _dbContext.GetDbSet<StockEntity>(tenantId)
                                  join location in _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
                                  on stock.goods_location_id equals location.Id
                                  where location.IsValid && stock.qty > 0
                                  select stock.Id)
                                  .CountAsync();

        var storageSlot = await _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
                .Where(t => t.IsValid && t.GoodsLocationType == GoodsLocationTypeEnum.StorageSlot)
                .CountAsync();

        // This can be moved to configuration
        var curInfoIds = (from stock in _dbContext.GetDbSet<StockEntity>(tenantId)
                          join location in _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
                          on stock.goods_location_id equals location.Id
                          where location.IsValid && stock.qty > 0
                          group stock by new { stock.sku_id, location.WarehouseId } into g
                          select new
                          {
                              SkuId = g.Key.sku_id,
                              g.Key.WarehouseId,
                              Total = g.Sum(s => s.qty)
                          });

        var skuIds = curInfoIds.Select(s => s.SkuId).Distinct();
        var warehouseIds = curInfoIds.Select(s => s.WarehouseId).Distinct();

        var safetyStocks = await _dbContext
            .GetDbSet<SkuSafetyStockEntity>()
            .Where(x => warehouseIds.Contains(x.WarehouseId))
            .ToListAsync();

        var lowInventoryAlert = 0;
        foreach (var safetyStock in safetyStocks)
        {
            var total = curInfoIds
                .Where(x => x.WarehouseId == safetyStock.WarehouseId && x.SkuId == safetyStock.sku_id)
                .Sum(x => x.Total);

            if (total < safetyStock.safety_stock_qty) lowInventoryAlert++;
        }

        return new InventoryDTO
        {
            TotalWarehouses = warehouseCount,
            TotalLocations = totalLocations,
            TotalPallets = totalPallets,
            WarehouseCapacity = storageSlot,
            LowInventoryAlert = lowInventoryAlert
        };
    }

    #endregion

    #region private methods

    private async Task<bool> CheckRuleSettingsBeforeSave(DbSet<WarehouseRuleSettingsEntity> dbSet,
        long tenantId, WarehouseSettingsViewModel model)
    {
        var query = dbSet.AsNoTracking().Where(t => t.TenantId == tenantId);

        return await query.Where(t => t.WarehouseId == model.WarehouseId
            && t.BlockId == model.BlockId
            && t.CategoryId == model.CategoryId
            && t.FloorId == model.FloorId
            && t.SkuId == model.SkuId
            && t.SupplierId == model.SupplierId).AnyAsync();
    }

    /// <summary>
    /// Activates the specified warehouse for the current tenant and returns the result of the operation.
    /// </summary>
    /// <remarks>If the warehouse does not exist for the current tenant, the method returns a message
    /// indicating that the entity does not exist. The warehouse's validity status is updated upon successful
    /// activation.</remarks>
    /// <param name="wareHouseId">The unique identifier of the warehouse to activate.</param>
    /// <param name="currentUser">The current user context containing tenant information used to scope the warehouse activation.</param>
    /// <returns>A tuple containing the number of affected rows and a message indicating whether the activation was successful or
    /// if the warehouse does not exist.</returns>
    public async Task<(int id, string msg)> ActiveWareHouseAsync(int wareHouseId, CurrentUser currentUser)
    {
        var dbSet = _dbContext.GetDbSet<WarehouseEntity>()
            .Where(x => x.TenantId == currentUser.tenant_id);
        var warehouse = await dbSet.FirstOrDefaultAsync(x => x.Id == wareHouseId);
        if (warehouse == null)
        {
            return (0, _stringLocalizer["not_exists_entity"]);
        }

        warehouse.is_valid = true;
        _dbContext.Update(warehouse);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0 ?
            (res, _stringLocalizer["save_success"]) :
            (0, _stringLocalizer["save_failed"]);
    }

    /// <summary>
    /// De-active WareHouse
    /// </summary>
    /// <param name="wareHouseId"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<(int id, string msg)> DeActiveWareHouseAsync(int wareHouseId, CurrentUser currentUser)
    {
        var dbSet = _dbContext.GetDbSet<WarehouseEntity>()
             .Where(x => x.TenantId == currentUser.tenant_id);
        var warehouse = await dbSet.FirstOrDefaultAsync(x => x.Id == wareHouseId);
        if (warehouse == null)
        {
            return (0, _stringLocalizer["not_exists_entity"]);
        }

        warehouse.is_valid = false;
        _dbContext.Update(warehouse);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0 ?
            (res, _stringLocalizer["save_success"]) :
            (0, _stringLocalizer["save_failed"]);
    }

    /// <summary>
    /// Get general info of warehouse, include total inventory, total pallet, location count, etc.
    /// </summary>
    /// <param name="wareHouseId"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<WarehouseGeneralInfo> GetGeneralInfoAsync(int wareHouseId, CurrentUser currentUser)
    {
        var tenantId = currentUser.tenant_id;

        var warehouse = await _dbContext.GetDbSet<WarehouseEntity>(tenantId)
            .FirstOrDefaultAsync(t => t.is_valid && t.Id == wareHouseId);
        if (warehouse == null)
        {
            return new WarehouseGeneralInfo();
        }

        var stockQuery = _dbContext.GetDbSet<StockEntity>(tenantId)
            .Where(s => s.qty > 0);
        var locationQuery = _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
            .Where(t => t.IsValid && t.WarehouseId == wareHouseId);

        var totalLocations = await locationQuery.CountAsync();

        var totalPallets = await (from stock in stockQuery
                                  join location in locationQuery
                                  on stock.goods_location_id equals location.Id
                                  where location.IsValid && stock.qty > 0
                                  select stock.Id)
                                  .CountAsync();

        var storageSlot = await locationQuery
                .Where(t => t.GoodsLocationType == GoodsLocationTypeEnum.StorageSlot)
                .CountAsync();

        var processingStatuses = new[]
        {
            ReceiptStatus.NEW,
            ReceiptStatus.DRAFT,
            ReceiptStatus.PROCESSING
        };

        var queryInbound = _dbContext.GetDbSet<InboundReceiptEntity>(tenantId)
            .Where(t => t.WarehouseId == wareHouseId);

        DateTime today = DateTime.UtcNow.Date;
        var fromDate = today.AddDays(-7);

        var queryReceipt = from receipt in queryInbound
                           where processingStatuses.Contains(receipt.Status)
                           && receipt.LastUpdateTime.Date >= fromDate.Date
                           && receipt.LastUpdateTime.Date <= today.Date
                           group receipt by receipt.Status into g
                           select new OrderItemDTO
                           {
                               ItemStatus = g.Key,
                               TotalCount = g.Count()
                           };

        var receipts = await queryReceipt.ToListAsync();

        var processing = receipts.Sum(x => x.TotalCount);

        return new WarehouseGeneralInfo
        {
            Id = warehouse.Id,
            Name = warehouse.WarehouseName ?? "",
            Code = $"WH-{warehouse.Id:000}",
            Address = warehouse.address ?? "",
            City = warehouse.city ?? "",
            WcsBlockId = warehouse.WcsBlockId,
            LocationCount = totalLocations,
            TotalInventory = storageSlot,
            TotalPallet = totalPallets,
            ProcessingOrders = processing,
        };
    }

    #endregion

    /// <summary>
    /// Get Receipt Details
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <param name="pageSearch"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<InboundInfoModel>> GetReceiptDetailsByIdAsync(int warehouseId,
        PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;

        var warehouse = _dbContext.GetDbSet<WarehouseEntity>(tenantId)
            .Where(x => x.Id == warehouseId);
        var suppliers = _dbContext.GetDbSet<SupplierEntity>(tenantId);

        var details = _dbContext.GetDbSet<InboundReceiptDetailEntity>();
        var skus = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var locations = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);
        var receipts = _dbContext.GetDbSet<InboundReceiptEntity>(tenantId)
            .Where(x => x.Status == ReceiptStatus.COMPLETE);
        var receiptIds = receipts
            .Where(r => r.WarehouseId == warehouseId).Select(r => r.Id)
            .Distinct();

        var baseQuery = details
            .Where(x => receiptIds.Contains(x.ReceiptId))
            .Select(x => new InboundInfoModel
            {
                ExpiryDate = x.ExpiryDate,
                Id = x.Id,
                ReceiptId = x.ReceiptId,
                LocationId = x.LocationId,
                SkuId = x.SkuId,
                SupplierId = x.Receipt.SupplierId,
                Quantity = x.Quantity,
                SkuUomId = x.SkuUomId,
                PalletCode = x.PalletCode,
                Status = (int)x.Receipt.Status,
                CreateDate = x.Receipt.CreateDate
            });

        //TODO search query later
        int totals = await baseQuery.CountAsync(cancellationToken);
        if (totals == 0)
        {
            return [];
        }

        var results = await baseQuery.OrderBy(x => x.SkuId)
                .ThenBy(t => t.ExpiryDate)
                .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                .Take(pageSearch.pageSize).ToListAsync(cancellationToken);

        var uoms = _dbContext.GetDbSet<SkuUomEntity>(tenantId);
        foreach (var item in results)
        {
            var receipt = await receipts.FirstOrDefaultAsync(r => r.Id == item.ReceiptId, cancellationToken);
            if (receipt == null) continue;

            var supplier = await suppliers.FirstOrDefaultAsync(s => s.Id == receipt.SupplierId, cancellationToken);
            var sku = await skus.FirstOrDefaultAsync(s => s.Id == item.SkuId, cancellationToken);
            var uom = await uoms.FirstOrDefaultAsync(u => u.Id == item.SkuUomId, cancellationToken);
            var location = await locations.FirstOrDefaultAsync(l => l.Id == item.LocationId, cancellationToken);

            item.LocationName = location != null ? location.LocationName : string.Empty;
            item.ReceiptNo = receipt.ReceiptNumber ?? "";
            item.UnitName = uom != null ? uom.UnitName : string.Empty;
            item.SupplierName = supplier?.supplier_name ?? string.Empty;
            item.SkuName = sku != null ? sku.sku_name : string.Empty;
            item.SkuCode = sku != null ? sku.sku_code : string.Empty;
            item.Status = (int)receipt.Status;
            item.CreateDate = receipt.CreateDate;
        }

        return results;
    }

    /// <summary>
    /// Order Details = Outbound details, currently only for outbound, can be extended for other order types in the future
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <param name="pageSearch"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<OutboundInfoModel>> GetOrderDetailsByIdAsync(int warehouseId,
        PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;

        var warehouse = _dbContext.GetDbSet<WarehouseEntity>(tenantId)
            .Where(x => x.Id == warehouseId);
        var outboundGateways = _dbContext.GetDbSet<OutboundGatewayEntity>(tenantId);
        var customers = _dbContext.GetDbSet<CustomerEntity>(tenantId);

        var outboundReceipts = _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId)
            .Where(x => x.WarehouseId == warehouseId &&
                x.Status == ReceiptStatus.COMPLETE);
        var outboundIds = outboundReceipts.Select(x => x.Id).Distinct();
        var details = _dbContext.GetDbSet<OutBoundReceiptDetailEntity>()
            .AsNoTracking()
            .Where(x => outboundIds.Contains(x.ReceiptId));

        var query = details
            .Select(x => new OutboundInfoModel
            {
                Id = x.Id,
                CustomerId = x.Receipt.CustomerId,
                CreateDate = x.Receipt.CreateDate,
                OrderNo = x.Receipt.ReceiptNumber,
                OrderId = x.ReceiptId,
                LocationId = x.LocationId,
                Status = (int)x.Receipt.Status,
                SkuId = x.SkuId,
                SkuUomId = x.SkuUomId,
                Type = x.Receipt.Type,
                Quantity = x.Quantity,
            });

        var totals = await query.CountAsync(cancellationToken);
        if (totals == 0)
        {
            return [];
        }

        var results = await query
           .OrderBy(t => t.SkuId)
            .ThenByDescending(x => x.CreateDate)
            .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
            .Take(pageSearch.pageSize)
            .ToListAsync(cancellationToken);

        var suppliers = _dbContext.GetDbSet<SupplierEntity>(tenantId);
        var skus = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var locations = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);
        var uoms = _dbContext.GetDbSet<SkuUomEntity>(tenantId);

        foreach (var item in results)
        {
            var outbound = await outboundReceipts.FirstOrDefaultAsync(r => r.Id == item.OrderId, cancellationToken);
            if (outbound == null) continue;

            item.OrderNo = outbound.ReceiptNumber ?? "";

            var customer = await customers.FirstOrDefaultAsync(s => s.Id == item.CustomerId, cancellationToken);
            var sku = await skus.FirstOrDefaultAsync(s => s.Id == item.SkuId, cancellationToken);
            var uom = await uoms.FirstOrDefaultAsync(u => u.Id == item.SkuUomId, cancellationToken);
            var location = await locations.FirstOrDefaultAsync(l => l.Id == item.LocationId, cancellationToken);
            var gateway = await outboundGateways.FirstOrDefaultAsync(g => g.Id == outbound.OutboundGatewayId, cancellationToken);

            item.GatewayName = gateway != null ? gateway.GatewayName : string.Empty;
            item.CustomerName = customer != null ? customer.customer_name : string.Empty;
            item.LocationName = location != null ? location.LocationName : string.Empty;
            item.UnitName = uom != null ? uom.UnitName : string.Empty;
            //item.SupplierName = supplier != null ? supplier.supplier_name : string.Empty;
            item.SkuName = sku != null ? sku.sku_name : string.Empty;
            item.SkuCode = sku != null ? sku.sku_code : string.Empty;
        }

        return results;
    }

    /// <summary>
    /// Inventory Overview
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <param name="pageSearch"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<InventoryOverview>> GetInventoryOverviewByIdAsync(int warehouseId,
        PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var warehouse = _dbContext.GetDbSet<WarehouseEntity>(tenantId)
            .Where(x => x.Id == warehouseId && x.is_valid);
        var dbSet = _dbContext.GetDbSet<StockEntity>(tenantId);

        var query = dbSet.Where(x => x.qty > 0)
            .Select(x => new InventoryOverview
            {
                SkuId = x.sku_id,
                Quantity = x.qty,
                ExpiryDate = x.expiry_date,
                LocationId = x.goods_location_id,
                InventoryStatus = "In Stock"
            });

        var processingStatus = new List<ReceiptStatus>
        {
            ReceiptStatus.PROCESSING, ReceiptStatus.NEW
        };

        var receiptIds = _dbContext.GetDbSet<InboundReceiptEntity>(tenantId)
            .Where(x => x.WarehouseId == warehouseId && processingStatus.Contains(x.Status))
            .Select(r => r.Id)
            .Distinct();

        var incomingStock = _dbContext.GetDbSet<InboundReceiptDetailEntity>()
            .Select(x => new InventoryOverview
            {
                SkuId = x.SkuId,
                Quantity = x.Quantity,
                ExpiryDate = x.ExpiryDate,
                LocationId = x.LocationId,
                OrderId = x.ReceiptId,
                SupplierId = x.Receipt.SupplierId,
                InventoryStatus = "Incoming stock",
            });

        var outboundReceipts = _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId)
            .Where(x => x.WarehouseId == warehouseId && processingStatus.Contains(x.Status));
        var outboundIds = outboundReceipts.Select(x => x.Id).Distinct();
        var details = _dbContext.GetDbSet<OutBoundReceiptDetailEntity>()
            .AsNoTracking()
            .Where(x => outboundIds.Contains(x.ReceiptId));

        var outgoingStock = details
            .Select(x => new InventoryOverview
            {
                SkuId = x.SkuId,
                Quantity = x.Quantity,
                LocationId = x.LocationId,
                OrderId = x.ReceiptId,
                InventoryStatus = "Outgoing stock",
            });

        var rs1 = await query.ToListAsync(cancellationToken);
        var rs2 = await incomingStock.ToListAsync(cancellationToken);
        var rs3 = await outgoingStock.ToListAsync(cancellationToken);

        var results = //await query.Union(incomingStock).Union(outgoingStock)
            rs1.Union(rs2).Union(rs3)
           .OrderBy(t => t.SkuId)
            .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
            .Take(pageSearch.pageSize)
            .ToList();

        var suppliers = _dbContext.GetDbSet<SupplierEntity>(tenantId);
        var skus = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var locations = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);
        var uoms = _dbContext.GetDbSet<SkuUomEntity>(tenantId);

        foreach (var item in results)
        {
            var supplier = await suppliers.FirstOrDefaultAsync(s => s.Id == item.SupplierId, cancellationToken);
            var sku = await skus.FirstOrDefaultAsync(s => s.Id == item.SkuId, cancellationToken);
            var uom = await uoms.FirstOrDefaultAsync(u => u.Id == item.SkuUomId, cancellationToken);
            var location = await locations.FirstOrDefaultAsync(l => l.Id == item.LocationId, cancellationToken);

            item.LocationName = location != null ? location.LocationName : string.Empty;
            item.UnitName = uom != null ? uom.UnitName : "Cái";
            item.SupplierName = supplier?.supplier_name ?? string.Empty;
            item.SkuName = sku != null ? sku.sku_name : string.Empty;
            item.SkuCode = sku != null ? sku.sku_code : string.Empty;
        }

        return results;
    }

    /// <summary>
    /// Get Safety Stock Config Asnyc
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="warehouseId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<SkuSafetyStockDto>> GetSafetyStockConfigAsnyc(CurrentUser currentUser, int warehouseId)
    {
        var tenantId = currentUser.tenant_id;
        var warehouse = await _dbContext.GetDbSet<WarehouseEntity>(tenantId)
            .FirstOrDefaultAsync(x => x.Id == warehouseId && x.is_valid);

        if (warehouse == null) return [];

        var query = _dbContext.GetDbSet<SkuSafetyStockEntity>().AsNoTracking()
            .Where(x => x.WarehouseId == warehouseId);

        return await query.Select(x => new SkuSafetyStockDto
        {
            WarehouseId = warehouseId,
            WarehouseAddress = warehouse.address,
            WarehouseName = warehouse.WarehouseName,
            Id = x.Id,
            SafetyStockQty = x.safety_stock_qty,
            SkuId = x.sku_id,
            SkuCode = x.Sku.sku_code,
            SkuName = x.Sku.sku_name,
        }).ToListAsync();
    }

    /// <summary>
    /// Import Excel Safety Stock
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="warehouseId"></param>
    /// <param name="safetyStocks"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<int> ImportExcelSafetyStockConfig(CurrentUser currentUser,
        int warehouseId, List<InputSkuSafetyStock> safetyStocks)
    {
        var query = _dbContext.GetDbSet<SkuSafetyStockEntity>()
            .Where(x => x.WarehouseId == warehouseId);

        int index = 0;
        int insertedCount = 0;
        int totalRequests = safetyStocks.Count;
        var tenantId = currentUser.tenant_id;

        var wareHouseList = _dbContext.GetDbSet<WarehouseEntity>(tenantId);
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantId);

        do
        {
            var batchOrders = safetyStocks.Skip(index).Take(SystemDefine.BatchSize).ToList();
            var items = new List<SkuSafetyStockEntity>();
            foreach (var order in batchOrders)
            {
                var wareHouse = wareHouseList.FirstOrDefault(w => w.WarehouseName == order.WareHouseName);
                if (wareHouse == null) continue;

                var sku = skuList.FirstOrDefault(x => x.sku_code == order.SkuCode || x.sku_name == order.SkuCode);
                if (sku == null) continue;

                items.Add(new SkuSafetyStockEntity
                {
                    sku_id = sku.Id,
                    WarehouseId = wareHouse.Id,
                    safety_stock_qty = int.TryParse(order.Qty, out var qty) ? qty : 0,
                });
            }

            try
            {
                // Save Stock Config
                var res = await SaveSafetyStockConfigAsync(items, query);
                insertedCount += res;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting batch starting at index {Index}", index);
            }

            // Xử lý batch
            Console.WriteLine($"Batch starting at {index}, count = {items.Count}");
            // Tăng index lên batchSize
            index += SystemDefine.BatchSize;

        } while (index < totalRequests);

        await _actionLogService.AddLogAsync(
                $"[ImportExcel Safety Stock] WarehouseId {warehouseId} has requesting import {safetyStocks.Count} rows => executed {insertedCount} by {currentUser.user_name}",
                "Warehouse", currentUser);

        return insertedCount;
    }

    private async Task<int> SaveSafetyStockConfigAsync(List<SkuSafetyStockEntity> items,
        IQueryable<SkuSafetyStockEntity> query)
    {
        foreach (var item in items)
        {
            var existing = await query
                .Where(x => x.sku_id == item.sku_id && x.WarehouseId == item.WarehouseId)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                _dbContext.GetDbSet<SkuSafetyStockEntity>().Add(item);
            }
            else
            {
                existing.safety_stock_qty = item.safety_stock_qty;
                _dbContext.GetDbSet<SkuSafetyStockEntity>().Update(existing);
            }
        }

        return await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Update Qty Safety Stock
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="warehouseId"></param>
    /// <param name="safetyStock"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<int> UpdateQtySafetyStockConfig(CurrentUser currentUser, int warehouseId, SkuSafetyStockDto safetyStock)
    {
        var tenantId = currentUser.tenant_id;
        var warehouse = await _dbContext.GetDbSet<WarehouseEntity>(tenantId)
            .FirstOrDefaultAsync(x => x.Id == warehouseId && x.is_valid);

        if (warehouse == null) return 0;

        var query = _dbContext.GetDbSet<SkuSafetyStockEntity>()
            .Where(x => x.Id == safetyStock.Id && x.WarehouseId == warehouseId);

        var entity = await query.FirstOrDefaultAsync();
        if (entity == null) { return 0; }

        string jsonString = JsonSerializer.Serialize(new
        {
            skuId = entity.sku_id,
            warehouseId = entity.WarehouseId,
            qty = entity.safety_stock_qty
        });

        await _actionLogService.AddLogAsync(
                $"[Update Qty Safety Stock] WarehouseId {warehouseId} has update {jsonString} => {safetyStock.SafetyStockQty} by {currentUser.user_name}",
                "Warehouse", currentUser);

        entity.safety_stock_qty = safetyStock.SafetyStockQty ?? 0;
        return await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Delete Safety Stock Config
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="warehouseId"></param>
    /// <param name="skuSafetyId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<int> DeleteSafetyStockConfig(CurrentUser currentUser, int warehouseId, int skuSafetyId)
    {
        var tenantId = currentUser.tenant_id;
        var warehouse = await _dbContext.GetDbSet<WarehouseEntity>(tenantId)
            .FirstOrDefaultAsync(x => x.Id == warehouseId && x.is_valid);

        if (warehouse == null) return 0;

        var query = _dbContext.GetDbSet<SkuSafetyStockEntity>()
            .Where(x => x.Id == skuSafetyId && x.WarehouseId == warehouseId);

        var entity = await query.FirstOrDefaultAsync();
        if (entity == null) { return 0; }

        string jsonString = JsonSerializer.Serialize(new
        {
            id = skuSafetyId,
            skuId = entity.sku_id,
            warehouseId = entity.WarehouseId,
            qty = entity.safety_stock_qty
        });
        await _actionLogService.AddLogAsync(
                $"[Delete Safety Stock {skuSafetyId} ] Safety {jsonString} has been deleted by {currentUser.user_name}",
                "Warehouse", currentUser);

        _dbContext.GetDbSet<SkuSafetyStockEntity>().Remove(entity);
        return await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Calculator Pallets
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<AvailablePallet>> GetCalculatorPalletsAsync(CalculatorPalletRequest request,
        CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var warehouseId = request.WarehouseId;
        var warehouse = await _dbContext.GetDbSet<WarehouseEntity>(tenantId)
            .FirstOrDefaultAsync(x => x.Id == warehouseId && x.is_valid, cancellationToken);

        if (warehouse == null) return [];
        var ruleSettings = _dbContext
            .GetDbSet<WarehouseRuleSettingsEntity>(tenantId)
            .Where(x => x.WarehouseId == warehouseId);

        var locationList = _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
            .Where(x => x.WarehouseId == warehouseId && x.IsValid
            && x.GoodsLocationType == GoodsLocationTypeEnum.StorageSlot);

        var queryStock = _dbContext.GetDbSet<StockEntity>(tenantId)
            .Where(s => s.qty > 0);

        var planningList = _dbContext.GetDbSet<InboundPalletEntity>(tenantId);

        var results = new List<AvailablePallet>();
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantId);
        var locIds = new List<int>();
        foreach (var detail in request.Details)
        {
            if (detail == null) continue;

            var sku = skuList.FirstOrDefault(x => x.Id == detail.SkuId);
            if (sku == null) continue;

            var myRules = ruleSettings.Where(x => x.SkuId == detail.SkuId);
            if (detail.SupplierId.GetValueOrDefault() > 0)
            {
                myRules = myRules.Where(x => x.SupplierId == detail.SupplierId);
            }

            var locations = await GetAvailableLocationsAsync(locIds, detail,
                sku, myRules, locationList, queryStock, planningList);
            locIds.AddRange(locations.Select(x => x.LocationId));
            results.AddRange(locations);
        }

        return results;
    }

    private async Task<IEnumerable<AvailablePallet>> GetAvailableLocationsAsync(List<int> excludedLocIds,
        InboundDetailItem detail,
        SkuEntity sku, IQueryable<WarehouseRuleSettingsEntity> ruleSettings,
        IQueryable<GoodslocationEntity> locationList, IQueryable<StockEntity> queryStock,
        IQueryable<InboundPalletEntity> planningList)
    {
        int maxQtyPerPallet = sku.maxQtyPerPallet.GetValueOrDefault(1);
        var qty = detail.Quantity;
        var availablePallets = new List<AvailablePallet>();
        var locIds = excludedLocIds ?? [];
        var fromDate = DateTime.UtcNow.AddDays(-1).Date;
        var toDate = DateTime.UtcNow.AddDays(1).Date;

        var planLocIds = planningList
            .Where(x => x.PlanningDate == null || (x.PlanningDate > fromDate && x.PlanningDate < toDate))
            .Select(x => x.LocationId);

        //Find rule settings
        foreach (var rule in ruleSettings)
        {
            var locations = await locationList
                .Where(x => x.CoordinateZ == $"{rule.FloorId}")
                .Where(x => !planLocIds.Contains(x.Id))
                .OrderBy(x => x.Priority)
                .ToListAsync();

            foreach (var item in locations)
            {
                var itemStock = await queryStock
                    .Where(x => x.goods_location_id == item.Id && x.sku_id == sku.Id)
                    .ToListAsync();

                var curQty = itemStock.Count == 0 ? 0 : itemStock.Sum(x => x.qty);

                if (maxQtyPerPallet <= curQty) continue;

                var balance = Math.Min(maxQtyPerPallet, maxQtyPerPallet - curQty);
                var palletName = itemStock?.FirstOrDefault()?.Palletcode ?? "";
                availablePallets.Add(new AvailablePallet
                {
                    LocationId = item.Id,
                    PalletName = palletName,
                    Quantity = curQty,
                    Balance = Math.Min(balance, qty),
                    SkuCode = sku.sku_code,
                    LocationName = item.LocationName,
                    SkuId = sku.Id,
                    SkuName = sku.sku_name,
                    AvailableQty = maxQtyPerPallet - curQty,
                });

                qty -= balance;
                locIds.Add(item.Id);

                if (qty < 0) return availablePallets;
            }
        }

        //Find space empty
        if (qty > 0)
        {
            var locations = await locationList
                .Where(x => !planLocIds.Contains(x.Id))
                .Where(x => !locIds.Contains(x.Id))
                .OrderBy(x => x.CoordinateZ)
                .ThenBy(x => x.Priority)
                .ToListAsync();

            foreach (var item in locations)
            {
                var itemStock = await queryStock
                    .Where(x => x.goods_location_id == item.Id && x.sku_id == sku.Id)
                    .ToListAsync();

                var curQty = itemStock.Count == 0 ? 0 : itemStock.Sum(x => x.qty);

                if (maxQtyPerPallet <= curQty) continue;

                var balance = Math.Min(maxQtyPerPallet, maxQtyPerPallet - curQty);
                var palletName = itemStock?.FirstOrDefault()?.Palletcode ?? "";
                availablePallets.Add(new AvailablePallet
                {
                    LocationId = item.Id,
                    PalletName = palletName,
                    Quantity = curQty,
                    Balance = Math.Min(balance, qty),
                    SkuCode = sku.sku_code,
                    LocationName = item.LocationName,
                    SkuId = sku.Id,
                    SkuName = sku.sku_name,
                    AvailableQty = maxQtyPerPallet - curQty,
                });

                qty -= balance;
                locIds.Add(item.Id);

                if (qty < 0) return availablePallets;
            }
        }

        //Find location already stored 
        if (qty > 0)
        {
            var plans = await planningList
                .Where(x => x.Quantity < maxQtyPerPallet && x.SkuId == detail.SkuId)
                .Where(x => x.PlanningDate == null || (x.PlanningDate > fromDate && x.PlanningDate < toDate))
                .ToListAsync();

            var planIds = plans.Select(x => x.LocationId);
            var storedLocations = await locationList
                .Where(x => planIds.Contains(x.Id))
                .OrderBy(x => x.CoordinateZ).ThenBy(x => x.Priority)
                .ToListAsync();

            foreach (var storedLocation in storedLocations)
            {
                var itemStock = await queryStock
                    .Where(x => x.goods_location_id == storedLocation.Id && x.sku_id == sku.Id)
                    .ToListAsync();

                var curQty = itemStock.Count == 0 ? 0 : itemStock.Sum(x => x.qty);
                var incomming = plans.FirstOrDefault(x => x.LocationId == storedLocation.Id);
                curQty += (int)(incomming?.Quantity ?? 0);

                if (maxQtyPerPallet <= curQty) continue;

                var balance = Math.Min(maxQtyPerPallet, maxQtyPerPallet - curQty);
                var palletName = itemStock?.FirstOrDefault()?.Palletcode ?? "";
                availablePallets.Add(new AvailablePallet
                {
                    LocationId = storedLocation.Id,
                    PalletName = palletName,
                    Quantity = curQty,
                    Balance = Math.Min(balance, qty),
                    SkuCode = sku.sku_code,
                    LocationName = storedLocation.LocationName,
                    SkuId = sku.Id,
                    SkuName = sku.sku_name,
                    AvailableQty = maxQtyPerPallet - curQty,
                });

                qty -= balance;
                if (qty < 0) return availablePallets;
            }
        }

        return availablePallets;
    }

    /// <summary>
    /// Get Floors
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<int>> GetFloors(int warehouseId, CurrentUser currentUser)
    {
        var locationList = _dbContext.GetDbSet<GoodslocationEntity>(currentUser.tenant_id)
            .Where(x => x.WarehouseId == warehouseId && x.IsValid
            && x.GoodsLocationType == GoodsLocationTypeEnum.StorageSlot);

        var ids = await locationList
            .Select(x => x.CoordinateZ).Distinct()
            .ToListAsync();

        return ids.Select(x => Convert2Int(x)).OrderBy(x => x);
    }

    private int Convert2Int(string x)
    {
        try
        {
            return int.Parse(x);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Get Master Data
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<LocationMaster>> GetMasterData(CurrentUser currentUser)
    {
        var query = _dbContext.GetDbSet<GoodslocationEntity>(currentUser.tenant_id)
            .Where(x => x.IsValid);

        return await query.Select(x => new LocationMaster
        {
            LocationId = x.Id,
            LocationName = x.LocationName,
            LocationType = x.GoodsLocationType,
            WarehouseId = x.WarehouseId
        })
            .ToListAsync();
    }
}

