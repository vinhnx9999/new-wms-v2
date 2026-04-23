using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Core.Utility;
using WMSSolution.Shared.Enums;
using WMSSolution.Shared.Enums.Location;
using WMSSolution.Shared.Excel;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Pallet;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.Models.Warehouse;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Goodslocation;
using WMSSolution.WMS.IServices;

namespace WMSSolution.WMS.Services.Goodslocation;

/// <summary>
///  Goodslocation Service
/// </summary>
/// <remarks>
///Goodslocation  constructor
/// </remarks>
/// <param name="dBContext">The DBContext</param>
/// <param name="stringLocalizer">Localizer</param>
/// <param name="logger">Logger</param>
public class GoodslocationService(
                SqlDBContext dBContext,
                IStringLocalizer<MultiLanguage> stringLocalizer,
                ILogger<GoodslocationService> logger
            ) : BaseService<GoodslocationEntity>, IGoodslocationService
{
    #region Args
    /// <summary>
    /// The DBContext
    /// </summary>
    private readonly SqlDBContext _dBContext = dBContext;

    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    /// <summary>
    /// Logger Service 
    /// </summary>
    private readonly ILogger<GoodslocationService> _logger = logger;

    #endregion

    #region Api
    /// <summary>
    /// get goodslocation of the warehousearea by WarehouseId and warehousearea_id
    /// </summary>
    /// <param name="warehouse_area_id">warehousearea's id</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<List<FormSelectItem>> GetGoodslocationByWarehouse_area_id(int warehouse_area_id, CurrentUser currentUser)
    {
        var res = new List<FormSelectItem>();
        var DbSet = _dBContext.GetDbSet<GoodslocationEntity>();
        res = await (from g in DbSet.AsNoTracking()
                     where g.IsValid == true && g.TenantId == currentUser.tenant_id && g.WarehouseId == warehouse_area_id
                     select new FormSelectItem
                     {
                         code = "goodslocation",
                         comments = "goodslocations of the warehousearea",
                         name = g.LocationName,
                         value = g.Id.ToString(),
                     }).ToListAsync();
        return res;
    }

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(List<GoodslocationViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count != 0)
        {
            pageSearch.searchObjects.ForEach(s =>
            {
                queries.Add(s);
            });
        }
        var dbSet = _dBContext.GetDbSet<GoodslocationEntity>().AsNoTracking();

        var query = dbSet.Where(t => t.TenantId == currentUser.tenant_id);

        var expression = queries.AsExpression<GoodslocationEntity>();
        if (expression is not null)
        {
            query = query.Where(expression);
        }
        if (pageSearch.sqlTitle == "select")
        {
            query = query.Where(t => t.IsValid == true);
        }
        int totals = await query.CountAsync();
        var list = await query.OrderByDescending(t => t.CreateTime)
                   .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                   .Take(pageSearch.pageSize)
                   .ToListAsync();
        return (list.Adapt<List<GoodslocationViewModel>>(), totals);
    }

    /// <summary>
    /// Get all records
    /// </summary>
    /// <returns></returns>
    public async Task<List<GoodslocationViewModel>> GetAllAsync(CurrentUser currentUser)
    {
        var DbSet = _dBContext.GetDbSet<GoodslocationEntity>();
        var data = await DbSet.AsNoTracking().Where(t => t.TenantId.Equals(currentUser.tenant_id)).ToListAsync();
        return data.Adapt<List<GoodslocationViewModel>>();
    }

    /// <summary>
    /// Get a record by id
    /// </summary>
    /// <returns></returns>
    public async Task<GoodslocationViewModel> GetAsync(int id)
    {
        var DbSet = _dBContext.GetDbSet<GoodslocationEntity>();
        var entity = await DbSet.AsNoTracking().FirstOrDefaultAsync(t => t.Id.Equals(id));
        if (entity == null)
        {
            return new GoodslocationViewModel();
        }
        return entity.Adapt<GoodslocationViewModel>();
    }

    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="request">request</param>
    /// <param name="currentUser">current user</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    public async Task<(int id, string msg)> AddAsync(AddLocationRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return (0, _stringLocalizer["invalid_request"]);
        }
        if (request.WarehouseId <= 0)
        {
            return (0, _stringLocalizer["invalid_warehouse_id"]);
        }

        var tenantId = currentUser.tenant_id;
        var locationDbSet = _dBContext.GetDbSet<GoodslocationEntity>();
        GoodslocationEntity newLocation;

        var warehouse = await _dBContext.GetDbSet<WarehouseEntity>(tenantId).AsNoTracking().FirstOrDefaultAsync(t => t.Id == request.WarehouseId
                                        , cancellationToken);
        if (warehouse is null)
        {
            return (0, _stringLocalizer["warehouse_not_found"]);
        }

        // Handle Physical (Shelf) Location
        if (!request.IsVirtualLocation)
        {
            if (string.IsNullOrWhiteSpace(request.CoordinateX) ||
                string.IsNullOrWhiteSpace(request.CoordinateY) ||
                string.IsNullOrWhiteSpace(request.CoordinateZ))
            {
                return (0, _stringLocalizer["missing_coordinates_error"]);
            }


            var exist = await locationDbSet.AnyAsync(t => t.WarehouseId == request.WarehouseId
                                               && t.CoordinateX == request.CoordinateX
                                               && t.CoordinateY == request.CoordinateY
                                               && t.CoordinateZ == request.CoordinateZ
                                               && t.TenantId == tenantId, cancellationToken);
            if (exist)
            {
                return (0, _stringLocalizer["exists_entity_with_coordinates"]);
            }

            newLocation = new GoodslocationEntity
            {
                WarehouseId = request.WarehouseId,
                LocationName = $"{request.CoordinateZ}.{request.CoordinateX}.{request.CoordinateY}",
                CoordinateX = request.CoordinateX,
                CoordinateY = request.CoordinateY,
                CoordinateZ = request.CoordinateZ,
                GoodsLocationType = GoodsLocationTypeEnum.StorageSlot,
                IsValid = true,
                TenantId = tenantId,
                CreateTime = DateTime.UtcNow,
                Priority = request.Priority,
                WarehouseName = warehouse.WarehouseName,
            };
        }
        // Handle Virtual Location
        else
        {
            newLocation = new GoodslocationEntity
            {
                WarehouseId = request.WarehouseId,
                LocationName = request.LocationName ?? string.Empty,
                CoordinateX = string.Empty,
                CoordinateY = string.Empty,
                CoordinateZ = string.Empty,
                GoodsLocationType = GoodsLocationTypeEnum.VirtualLocation,
                IsValid = true,
                TenantId = tenantId,
                CreateTime = DateTime.UtcNow,
                Priority = request.Priority,
                WarehouseName = warehouse.WarehouseName,
            };
        }

        await locationDbSet.AddAsync(newLocation, cancellationToken);
        var rowsAffected = await _dBContext.SaveChangesAsync(cancellationToken);

        if (rowsAffected > 0)
        {

            return (newLocation.Id, _stringLocalizer["save_success"]);
        }

        return (0, _stringLocalizer["save_failed"]);
    }


    /// <summary>
    /// update a record
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> UpdateAsync(GoodslocationViewModel viewModel, CurrentUser currentUser)
    {
        var DbSet = _dBContext.GetDbSet<GoodslocationEntity>();
        if (await DbSet.AnyAsync(t => t.Id != viewModel.Id && t.WarehouseId == viewModel.WarehouseId && t.LocationName == viewModel.LocationName && t.TenantId == currentUser.tenant_id))
        {
            return (false, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["LocationName"], viewModel.LocationName));
        }
        var entity = await DbSet.FirstOrDefaultAsync(t => t.Id.Equals(viewModel.Id));
        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }
        entity.Id = viewModel.Id;
        entity.WarehouseId = viewModel.WarehouseId;
        entity.WarehouseName = viewModel.WarehouseName;
        entity.WarehouseAreaName = viewModel.WarehouseAreaName;
        entity.WarehouseAreaProperty = viewModel.WarehouseAreaProperty;
        entity.LocationName = viewModel.LocationName;
        entity.LocationLength = viewModel.LocationLength;
        entity.LocationWidth = viewModel.LocationWidth;
        entity.LocationHeigth = viewModel.LocationHeigth;
        entity.LocationVolume = viewModel.LocationVolume;
        entity.LocationLoad = viewModel.LocationLoad;
        entity.CoordinateX = viewModel.CoordinateX;
        entity.CoordinateY = viewModel.CoordinateY;
        entity.CoordinateZ = viewModel.CoordinateZ;
        entity.LocationStatus = viewModel.LocationStatus;
        entity.IsValid = viewModel.IsValid;
        entity.WarehouseAreaId = viewModel.WarehouseAreaId;
        entity.LastUpdateTime = DateTime.UtcNow;
        var qty = await _dBContext.SaveChangesAsync();
        if (qty > 0)
        {
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
    /// <returns></returns>
    public async Task<(bool flag, string msg)> DeleteAsync(int id)
    {
        var exist_stock = await _dBContext.GetDbSet<StockEntity>().AsNoTracking().Where(t => t.qty > 0 && t.goods_location_id == id).AnyAsync();
        if (exist_stock)
        {
            return (false, _stringLocalizer["location_exist_stock_not_delete"]);
        }
        var qty = await _dBContext.GetDbSet<GoodslocationEntity>().Where(t => t.Id.Equals(id)).ExecuteDeleteAsync();
        if (qty > 0)
        {
            return (true, _stringLocalizer["delete_success"]);
        }
        else
        {
            return (false, _stringLocalizer["delete_failed"]);
        }
    }

    /// <summary>
    /// Get location for pick putaway logic handle for Robot excute Task
    /// default value with inbound task
    /// inbound high priority using with value 1
    /// outbound high priority using with value 6
    /// using greedy algorithm to get location (future improvement)
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="type"></param>
    /// <param name="totalPalletNeed"></param>
    /// <returns></returns>
    public async Task<List<GoodslocationViewModel>> GetLocationForPallet(CurrentUser currentUser, GetLocationPalletTypeEnum type = GetLocationPalletTypeEnum.Inbound, int totalPalletNeed = 1)
    {
        if (totalPalletNeed <= 0)
        {
            return [];
        }
        try
        {
            var baseQuery = _dBContext.GetDbSet<GoodslocationEntity>(currentUser.tenant_id)
                                .Where(g => g.IsValid
                                && g.LocationStatus == (int)GoodLocationStatusEnum.AVAILABLE
                                && g.GoodsLocationType == GoodsLocationTypeEnum.StorageSlot);

            var counts = await baseQuery
                .GroupBy(g => g.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToListAsync();

            if (counts.Count == 0)
            {
                return [];
            }

            var ordered = (type == GetLocationPalletTypeEnum.Inbound)
                ? [.. counts.OrderBy(x => x.Priority)]
                : counts.OrderByDescending(x => x.Priority).ToList();

            var need = totalPalletNeed;
            var boundaryPriority = ordered[0].Priority;
            var takeOnlyFromFirstGroup = false;

            if (ordered[0].Count >= need)
            {
                boundaryPriority = ordered[0].Priority;
                takeOnlyFromFirstGroup = true;
            }
            else
            {
                need -= ordered[0].Count;
                boundaryPriority = ordered[0].Priority;

                for (int i = 1; i < ordered.Count; i++)
                {
                    boundaryPriority = ordered[i].Priority;
                    if (ordered[i].Count >= need)
                    {
                        break;
                    }
                    need -= ordered[i].Count;
                }
            }

            IQueryable<GoodslocationEntity> fetchQuery;

            if (takeOnlyFromFirstGroup)
            {
                fetchQuery = baseQuery.Where(g => g.Priority == boundaryPriority);

                fetchQuery = (type == GetLocationPalletTypeEnum.Inbound)
                    ? fetchQuery.OrderBy(g => g.Id)
                    : fetchQuery.OrderBy(g => g.Id);

                var picked = await fetchQuery.ToListAsync();
                return picked.Adapt<List<GoodslocationViewModel>>();
            }
            else
            {

                fetchQuery = (type == GetLocationPalletTypeEnum.Inbound)
                    ? baseQuery.Where(g => g.Priority <= boundaryPriority)
                    : baseQuery.Where(g => g.Priority >= boundaryPriority);

                fetchQuery = (type == GetLocationPalletTypeEnum.Inbound)
                    ? fetchQuery.OrderBy(g => g.Priority).ThenBy(g => g.Id)
                    : fetchQuery.OrderByDescending(g => g.Priority).ThenBy(g => g.Id);

                var picked = await fetchQuery.ToListAsync();
                return picked.Adapt<List<GoodslocationViewModel>>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetLocationForPallet error: {message}", ex.Message);
            return [];
        }
    }

    /// <summary>
    /// Get Available Store Locations
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<List<StoreLocationViewModel>> GetAvailableStoreLocations(int warehouseId, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;

        var locations = await _dBContext.GetDbSet<GoodslocationEntity>(tenantId)
            .Where(x => x.WarehouseId == warehouseId && x.IsValid)
            .ToListAsync(cancellationToken);

        var locationIds = locations.Select(l => l.Id).ToList();

        var stocks = await _dBContext.GetDbSet<StockEntity>(tenantId)
            .Where(s => locationIds.Contains(s.goods_location_id) && s.qty > 0)
            .ToListAsync(cancellationToken);

        var skuIds = stocks.Select(s => s.sku_id).Distinct().ToList();
        var skuDict = await _dBContext.GetDbSet<SkuEntity>(tenantId)
                            .Where(s => skuIds.Contains(s.Id))
                            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var stocksGroupedByLocation = stocks.GroupBy(s => s.goods_location_id).ToDictionary(g => g.Key, g => g.ToList());

        var results = new List<StoreLocationViewModel>();
        foreach (var location in locations)
        {
            var entity = new StoreLocationViewModel
            {
                Id = location.Id,
                Address = $"{location.LocationName}",
                CoordX = location.CoordinateX.ObjToInt(),
                CoordY = location.CoordinateY.ObjToInt(),
                CoordZ = location.CoordinateZ.ObjToInt(),
                Level = location.CoordinateZ.ObjToInt(),
                Status = location.LocationStatus,
                Type = location.GoodsLocationType.GetDescription(),
                StoragePriority = location.Priority,
                VirtualLocation = location.GoodsLocationType == GoodsLocationTypeEnum.VirtualLocation
            };
            if (stocksGroupedByLocation.TryGetValue(location.Id, out var storeProducts))
            {

                entity.PalletCode = storeProducts.First().Palletcode ?? "";

                foreach (var stock in storeProducts)
                {
                    skuDict.TryGetValue(stock.sku_id, out var skuInfo);

                    entity.Products.Add(new ProductDto
                    {
                        SkuId = stock.sku_id,
                        SkuCode = skuInfo?.sku_code ?? "",
                        SkuName = skuInfo?.sku_name ?? "",
                        Quantity = stock.qty,
                        ExpiryDate = stock.expiry_date ?? DateTime.UtcNow
                    });
                }
            }

            results.Add(entity);
        }

        return results;
    }


    /// <summary>
    /// Suggest for location with Pallet
    /// Request :
    /// warehouseId ? Qty ?
    /// Rule :
    /// Location has pallet -> show Pallet Code -> show each skus ? Qty in each ?  -> can put in
    /// Location empty -> show empty -> can put in
    /// Response : 
    /// list Location   
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task<List<LocationWithPalletViewModel>> GetLocationWithPalletAsync(GetLocationWithPalletRequest request, CurrentUser currentUser, CancellationToken cancellation)
    {
        if (request is null)
        {
            _logger.LogError("GetLocationWithPalletAsync request is null");
            return [];
        }

        var tenantId = currentUser.tenant_id;

        var receipt = await _dBContext.GetDbSet<InboundReceiptEntity>(tenantId)
                                        .Where(r => r.WarehouseId == request.WarehouseId
                                                    && (r.Status == ReceiptStatus.NEW || r.Status == ReceiptStatus.DRAFT))
                                        .Include(r => r.Details)
                                        .ToListAsync(cancellation);

        var locationIdsInUse = receipt.SelectMany(r => r.Details)
                                    .Where(d => d.LocationId.HasValue)
                                    .Select(d => d.LocationId.GetValueOrDefault())
                                    .Distinct()
                                    .ToList();

        var locations = await _dBContext.GetDbSet<GoodslocationEntity>(tenantId)
                                        .Where(x => x.WarehouseId == request.WarehouseId
                                                      && x.LocationStatus == (byte)GoodLocationStatusEnum.AVAILABLE
                                                      && x.IsValid
                                                      && x.GoodsLocationType == GoodsLocationTypeEnum.StorageSlot
                                                      && x.GoodsLocationType != GoodsLocationTypeEnum.VirtualLocation
                                                      && !locationIdsInUse.Contains(x.Id))
                                        .OrderBy(x => x.CoordinateZ)
                                        .ThenBy(x => x.CoordinateX)
                                        .ThenBy(x => x.CoordinateY)
                                        .ToListAsync(cancellation);

        if (locations is null)
        {
            _logger.LogError("GetLocationWithPalletAsync no locations found for warehouseId: {warehouseId}", request.WarehouseId);
            return [];
        }

        var locationIds = locations.Select(l => l.Id).Distinct().ToList();
        var stocks = await _dBContext.GetDbSet<StockEntity>(tenantId)
            .Where(s => locationIds.Contains(s.goods_location_id) && s.qty > 0)
            .ToListAsync(cancellation);

        var palletCodes = stocks.Where(s => !string.IsNullOrEmpty(s.Palletcode))
                                .Select(s => s.Palletcode!)
                                .Distinct()
                                .ToList();

        var pallets = palletCodes.Count > 0
                    ? await _dBContext.GetDbSet<PalletEntity>(tenantId)
                        .AsNoTracking()
                        .Where(p => palletCodes.Contains(p.PalletCode) && !p.IsFull)
                        .ToListAsync(cancellation)
                    : [];

        var skuIds = stocks.Select(s => s.sku_id).Distinct().ToList();
        var skus = skuIds.Count > 0
                    ? await _dBContext.GetDbSet<SkuEntity>()
                        .AsNoTracking()
                        .Where(s => skuIds.Contains(s.Id))
                        .ToListAsync(cancellation)
                    : [];

        var stockByLocation = stocks.GroupBy(s => s.goods_location_id).ToDictionary(g => g.Key, g => g.ToList());
        var palletLookup = pallets.ToDictionary(p => p.PalletCode);
        var skuLookup = skus.ToDictionary(s => s.Id);

        var results = new List<LocationWithPalletViewModel>();
        foreach (var location in locations)
        {
            if (!stockByLocation.TryGetValue(location.Id, out var locationStocks))
            {
                results.Add(new LocationWithPalletViewModel
                {
                    GoodLocationId = location.Id,
                    GoodLocationName = location.LocationName,
                    PalletId = 0,
                    PalletCode = "",
                    Items = null,
                });
                continue;
            }

            var palletCode = locationStocks
                .Select(s => s.Palletcode)
                .FirstOrDefault(p => !string.IsNullOrEmpty(p));

            if (palletCode is null || !palletLookup.TryGetValue(palletCode, out var pallet))
            {
                continue;
            }

            var items = locationStocks.Select(st =>
            {
                skuLookup.TryGetValue(st.sku_id, out var sku);
                return new ItemInPalletViewModel
                {
                    SkuId = st.sku_id,
                    SkuName = sku?.sku_name ?? "",
                    Qty = st.qty,
                };
            }).ToList();

            results.Add(new LocationWithPalletViewModel
            {
                GoodLocationId = location.Id,
                GoodLocationName = location.LocationName,
                PalletId = pallet.Id,
                PalletCode = pallet.PalletCode,
                Items = items,
            });
        }

        if (request.Qty > 0 && results.Count > request.Qty)
        {
            results = [.. results.Take(request.Qty)];
        }

        return results;
    }

    #endregion

    /// <summary>
    /// Get Locations With Sku Async
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task<List<LocationWithPalletViewModel>> GetLocationWithSkuAsync(GetLocationWithSkuIdRequest request, CurrentUser currentUser, CancellationToken cancellation)
    {
        if (request is null)
        {
            _logger.LogError("GetLocationWithSkuAsync request is null");
            return [];
        }

        var tenantId = currentUser.tenant_id;
        var locations = await _dBContext.GetDbSet<GoodslocationEntity>(tenantId)
                                        .Where(x => x.WarehouseId == request.WarehouseId
                                                      && x.LocationStatus == (byte)GoodLocationStatusEnum.AVAILABLE
                                                      && x.IsValid
                                                      && x.GoodsLocationType == GoodsLocationTypeEnum.StorageSlot
                                                      && x.GoodsLocationType != GoodsLocationTypeEnum.VirtualLocation)
                                        .OrderBy(x => x.CoordinateZ)
                                        .ToListAsync(cancellation);

        if (locations is null)
        {
            _logger.LogError("GetLocationWithPalletAsync no locations found for warehouseId: {warehouseId}", request.WarehouseId);
            return [];
        }

        var queryRuleSettings = _dBContext.GetDbSet<WarehouseRuleSettingsEntity>(tenantId)
                                        .Where(r => r.WarehouseId == request.WarehouseId);

        int skuId = request.SkuId.GetValueOrDefault();
        var applyRuleSettings = request.ApplyRuleSettings.GetValueOrDefault();
        var supplierId = request.SupplierId.GetValueOrDefault();
        var reqQuantity = request.RequestedQuantity.GetValueOrDefault();

        var maxQty = await _dBContext.GetDbSet<SkuEntity>().AsNoTracking()
                                    .Where(s => s.Id == skuId)
                                    .Select(g => g.maxQtyPerPallet)
                                    .MaxAsync(cancellation);
        if (supplierId > 0)
        {
            queryRuleSettings = queryRuleSettings.Where(r => r.SupplierId == supplierId);
        }
        var queryStock = _dBContext.GetDbSet<StockEntity>(tenantId)
                                    .Where(s => s.qty > 0);
        if (skuId > 0)
        {
            queryStock = queryStock.Where(x => x.sku_id == skuId);
            queryRuleSettings = queryRuleSettings.Where(r => r.SkuId == skuId);
        }

        if (applyRuleSettings)
        {
            var ruleSettings = await queryRuleSettings.ToListAsync(cancellation);

            if (ruleSettings.Count != 0)
            {
                var storageFloorIds = ruleSettings.Select(r => r.FloorId).Distinct().ToList();

                locations = [.. locations.Where(l => storageFloorIds.Contains(l.CoordinateZ.ObjToInt()))];
            }
        }

        var locationIds = locations.Select(l => l.Id).Distinct().ToList();
        queryStock = queryStock.Where(s => locationIds.Contains(s.goods_location_id));

        var stocks = await queryStock.ToListAsync(cancellation);

        var palletCodes = stocks.Where(s => !string.IsNullOrEmpty(s.Palletcode))
                                .Select(s => s.Palletcode!)
                                .Distinct()
                                .ToList();

        var pallets = palletCodes.Count > 0
                    ? await _dBContext.GetDbSet<PalletEntity>(tenantId)
                        .AsNoTracking()
                        .Where(p => palletCodes.Contains(p.PalletCode) && !p.IsFull)
                        .ToListAsync(cancellation)
                    : [];

        var skuIds = stocks.Select(s => s.sku_id).Distinct().ToList();
        var skus = skuIds.Count > 0
                    ? await _dBContext.GetDbSet<SkuEntity>()
                        .AsNoTracking()
                        .Where(s => skuIds.Contains(s.Id))
                        .ToListAsync(cancellation)
                    : [];

        var stockByLocation = stocks.GroupBy(s => s.goods_location_id).ToDictionary(g => g.Key, g => g.ToList());
        var palletLookup = pallets.ToDictionary(p => p.PalletCode);
        var skuLookup = skus.ToDictionary(s => s.Id);

        var results = new List<LocationWithPalletViewModel>();
        //Add rule settings logic here to filter locations if needed in the future
        // For now, we just order by CoordinateZ and Priority as default logic to suggest locations
        // Empty location will be added in the end of the list with empty pallet code and null items, which means can put in
        var resultIds = new List<int>();

        //TODO : check available quantity logic here, for now we just suggest locations with requested sku,
        //but not check the quantity, which means the location with requested sku but not enough quantity will also be suggested,
        //need to confirm with PM and whether need to consider the total available quantity in the location
        //(which means the location with requested sku and other skus, if the total available quantity in the location is enough for the request,
        //then we also suggest this location)
        foreach (var location in locations.OrderBy(x => x.CoordinateZ).ThenBy(x => x.Priority))
        {
            if (!stockByLocation.TryGetValue(location.Id, out var locationStocks))
            {
                resultIds.Add(location.Id);
                reqQuantity -= maxQty.GetValueOrDefault();

                results.Add(new LocationWithPalletViewModel
                {
                    GoodLocationId = location.Id,
                    GoodLocationName = location.LocationName,
                    PalletId = 0,
                    PalletCode = "",
                    Items = null,
                    AvailableQuantity = maxQty.GetValueOrDefault()
                });
            }
        }

        foreach (var location in locations.Where(x => !resultIds.Contains(x.Id)))
        {
            if (stockByLocation.TryGetValue(location.Id, out var locationStocks))
            {
                var items = locationStocks.Select(st =>
                {
                    skuLookup.TryGetValue(st.sku_id, out var sku);
                    return new ItemInPalletViewModel
                    {
                        SkuId = st.sku_id,
                        SkuName = sku?.sku_name ?? "",
                        Qty = st.qty,
                    };
                }).ToList();

                var palletCode = locationStocks
                    .Select(s => s.Palletcode)
                    .FirstOrDefault(p => !string.IsNullOrEmpty(p));

                if (palletCode is null || !palletLookup.TryGetValue(palletCode, out var pallet))
                {
                    continue;
                }

                var curVal = locationStocks.Sum(x => x.qty);
                var availableQty = Math.Max(0, maxQty.GetValueOrDefault() - curVal);
                if (availableQty <= 0) continue;

                if (reqQuantity > curVal)
                {
                    reqQuantity -= curVal;
                }
                else
                {
                    reqQuantity = 0;
                }

                results.Add(new LocationWithPalletViewModel
                {
                    GoodLocationId = location.Id,
                    GoodLocationName = location.LocationName,
                    PalletId = pallet.Id,
                    PalletCode = pallet.PalletCode,
                    Items = items,
                    AvailableQuantity = availableQty
                });
            }
        }



        if (request.Qty > 0)
        {
            var data = new List<LocationWithPalletViewModel>();
            var quantity = request.RequestedQuantity.GetValueOrDefault();
            var num = 0;
            foreach (var rs in results)
            {
                bool addItem = (num < quantity + 1) || (data.Count < request.Qty);
                if (addItem)
                {
                    data.Add(rs);
                    num += rs.AvailableQuantity;
                }
            }

            return data;
        }

        return results;
    }

    /// <summary>
    /// Import Excel Data
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> ImportExcelData(List<InputLocationExcel> request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (request == null || request.Count == 0)
        {
            return 0;
        }

        var tenantId = currentUser.tenant_id;
        var userName = currentUser.user_name;

        var locationDbSet = _dBContext.GetDbSet<GoodslocationEntity>(tenantId);
        var warehouseDbSet = _dBContext.GetDbSet<WarehouseEntity>(tenantId);

        var warehouseNames = request
            .Select(x => x.WarehouseName?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var warehouses = await warehouseDbSet.AsNoTracking()
            .Where(x => warehouseNames.Contains(x.WarehouseName))
            .ToListAsync(cancellationToken);

        var warehouseDict = warehouses.ToDictionary(x => x.WarehouseName, x => x, StringComparer.OrdinalIgnoreCase);
        var warehouseIds = warehouses.Select(x => x.Id).Distinct().ToList();

        var existing = await locationDbSet.AsNoTracking()
            .Where(x => warehouseIds.Contains(x.WarehouseId))
            .Select(x => new
            {
                x.WarehouseId,
                x.LocationName,
                x.CoordinateX,
                x.CoordinateY,
                x.CoordinateZ,
                x.GoodsLocationType
            })
            .ToListAsync(cancellationToken);

        var existingPhysical = existing
        .Where(x => x.GoodsLocationType != GoodsLocationTypeEnum.VirtualLocation)
        .Select(x => $"P|{x.WarehouseId}|{x.CoordinateX}|{x.CoordinateY}|{x.CoordinateZ}")
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingVirtual = existing
            .Where(x => x.GoodsLocationType == GoodsLocationTypeEnum.VirtualLocation)
            .Select(x => $"V|{x.WarehouseId}|{x.LocationName}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toInsert = new List<GoodslocationEntity>();

        foreach (var row in request)
        {
            if (string.IsNullOrWhiteSpace(row.WarehouseName))
            {
                continue;
            }

            if (!warehouseDict.TryGetValue(row.WarehouseName.Trim(), out var warehouse))
            {
                continue;
            }

            var priority = row.Priority;
            if (priority < 1 || priority > 6)
            {
                priority = 1;
            }

            if (row.IsVirtualLocation)
            {
                if (string.IsNullOrWhiteSpace(row.LocationName))
                {
                    continue;
                }

                var key = $"V|{warehouse.Id}|{row.LocationName.Trim()}";
                if (!existingVirtual.Add(key))
                {
                    continue;
                }

                toInsert.Add(new GoodslocationEntity
                {
                    WarehouseId = warehouse.Id,
                    WarehouseName = warehouse.WarehouseName,
                    LocationName = row.LocationName.Trim(),
                    CoordinateX = string.Empty,
                    CoordinateY = string.Empty,
                    CoordinateZ = string.Empty,
                    GoodsLocationType = GoodsLocationTypeEnum.VirtualLocation,
                    LocationStatus = (byte)GoodLocationStatusEnum.AVAILABLE,
                    IsValid = true,
                    Priority = priority,
                    TenantId = tenantId,

                    CreateTime = DateTime.UtcNow,
                    LastUpdateTime = DateTime.UtcNow
                });

                continue;
            }

            if (string.IsNullOrWhiteSpace(row.CoordinateX)
                || string.IsNullOrWhiteSpace(row.CoordinateY)
                || string.IsNullOrWhiteSpace(row.CoordinateZ))
            {
                continue;
            }

            var x = row.CoordinateX.Trim();
            var y = row.CoordinateY.Trim();
            var z = row.CoordinateZ.Trim();

            var physicalKey = $"P|{warehouse.Id}|{x}|{y}|{z}";
            if (!existingPhysical.Add(physicalKey))
            {
                continue;
            }

            toInsert.Add(new GoodslocationEntity
            {
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.WarehouseName,
                LocationName = string.IsNullOrWhiteSpace(row.LocationName) ? $"{z}.{x}.{y}" : row.LocationName.Trim(),
                CoordinateX = x,
                CoordinateY = y,
                CoordinateZ = z,
                GoodsLocationType = GoodsLocationTypeEnum.StorageSlot,
                LocationStatus = (byte)GoodLocationStatusEnum.AVAILABLE,
                IsValid = true,
                Priority = priority,
                TenantId = tenantId,

                CreateTime = DateTime.UtcNow,
                LastUpdateTime = DateTime.UtcNow
            });
        }

        if (toInsert.Count == 0)
        {
            return 0;
        }

        await _dBContext.GetDbSet<GoodslocationEntity>().AddRangeAsync(toInsert, cancellationToken);
        return await _dBContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Get Locations By Warehouse Async
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<LocationOnlyViewModel>> GetLocationsByWarehouseAsync(
    int warehouseId,
    CurrentUser currentUser,
    CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;

        var locations = await _dBContext.GetDbSet<GoodslocationEntity>(tenantId)
            .AsNoTracking()
            .Where(x => x.WarehouseId == warehouseId && x.IsValid)
            .OrderBy(x => x.CoordinateZ)
            .ThenBy(x => x.CoordinateX)
            .ThenBy(x => x.CoordinateY)
            .ToListAsync(cancellationToken);

        return locations.Select(location => new LocationOnlyViewModel
        {
            Id = location.Id,
            Address = location.LocationName,
            CoordX = location.CoordinateX.ObjToInt(),
            CoordY = location.CoordinateY.ObjToInt(),
            CoordZ = location.CoordinateZ.ObjToInt(),
            Level = location.CoordinateZ.ObjToInt(),
            Status = location.LocationStatus,
            Type = location.GoodsLocationType.GetDescription(),
            StoragePriority = location.Priority,
            VirtualLocation = location.GoodsLocationType == GoodsLocationTypeEnum.VirtualLocation
        }).ToList();
    }
}

