using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Models.IntegrationWCS;
using WMSSolution.Core.Services;
using WMSSolution.Shared.Enums;
using WMSSolution.Shared.Enums.Location;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Dispatchlist;
using WMSSolution.WMS.Entities.Models.Pallet;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.Models.Stock;
using WMSSolution.WMS.Entities.Models.Warehouse;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Goodslocation;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS;
using WMSSolution.WMS.Entities.ViewModels.Stock;
using WMSSolution.WMS.Entities.ViewModels.Stock.DuyPhatSolution;
using WMSSolution.WMS.IServices.Stock;

namespace WMSSolution.WMS.Services.Stock;

/// <summary>
///  Stock Service
/// </summary>
public class StockService : BaseService<StockEntity>, IStockService
{
    #region Args

    /// <summary>
    /// The DBContext
    /// </summary>
    private readonly SqlDBContext _dbContext;

    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer;

    /// <summary>
    /// Logger Service
    /// </summary>
    private readonly ILogger<StockService> _logger;

    /// <summary>
    /// configuration
    /// </summary>
    public IConfiguration Configuration { get; }

    #endregion Args

    #region constructor

    /// <summary>
    ///Stock  constructor
    /// </summary>
    /// <param name="dBContext">The DBContext</param>
    /// <param name="stringLocalizer">Localizer</param>
    /// <param name="logger">Logger</param>
    /// <param name="configuration">Configuration</param>
    public StockService(
        SqlDBContext dBContext
      , IStringLocalizer<MultiLanguage> stringLocalizer
      , ILogger<StockService> logger
        , IConfiguration configuration
        )
    {
        this._dbContext = dBContext;
        this._stringLocalizer = stringLocalizer;
        this._logger = logger;
        this.Configuration = configuration;
    }

    #endregion constructor

    #region Api

    /// <summary>
    /// stock page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="cancellationToken">cancellation token </param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(List<StockManagementViewModel> data, int totals)> StockPageAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;

        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count > 0)
        {
            pageSearch.searchObjects.ForEach(queries.Add);
        }

        var stockDbSet = _dbContext.GetDbSet<StockEntity>(tenantId);
        var asnDBSet = _dbContext.GetDbSet<AsnEntity>(tenantId);
        var skuDBSet = _dbContext.GetDbSet<SkuEntity>(tenantId);
        var spuDBSet = _dbContext.GetDbSet<SpuEntity>(tenantId);
        var processdetailDBSet = _dbContext.GetDbSet<StockprocessdetailEntity>(tenantId);
        var moveDBSet = _dbContext.GetDbSet<StockmoveEntity>(tenantId);
        var locationDbSet = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);
        var outboundReceiptDbSet = _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId);
        var outboundReceiptDetailDbSet = _dbContext.GetDbSet<OutBoundReceiptDetailEntity>().AsNoTracking();


        var stockGroupDatas = from stock in stockDbSet
                              join gl in locationDbSet on stock.goods_location_id equals gl.Id into gl_left
                              from gl in gl_left.DefaultIfEmpty()
                              group new { stock, gl } by stock.sku_id into sg
                              select new
                              {
                                  sku_id = sg.Key,
                                  series_number = sg.Select(x => x.stock.series_number).FirstOrDefault() ?? "",
                                  location_name = sg.Select(x => x.gl.LocationName).FirstOrDefault() ?? "",
                                  warehouse_name = sg.Select(x => x.gl.WarehouseName).FirstOrDefault() ?? "",
                                  qty_frozen = sg.Where(t => t.stock.is_freeze == true).Sum(e => e.stock.qty),
                                  qty = sg.Sum(t => t.stock.qty),
                                  qty_normal = sg.Where(t => t.gl.WarehouseAreaProperty != 5).Sum(t => t.stock.qty),
                                  qty_normal_frozen = sg.Where(t => t.gl.WarehouseAreaProperty != 5 && t.stock.is_freeze == true).Sum(t => t.stock.qty),
                                  expiry_date = sg.Min(t => t.stock.expiry_date)
                              };

        var asnGroupDatas = from asn in asnDBSet
                            group asn by asn.sku_id into ag
                            select new
                            {
                                sku_id = ag.Key,
                                qty_asn = ag.Where(t => t.asn_status == 0).Sum(t => t.asn_qty),
                                qty_to_unload = ag.Where(t => t.asn_status == 1).Sum(t => t.asn_qty),
                                qty_to_sort = ag.Where(t => t.asn_status == 2).Sum(t => t.asn_qty),
                                qty_sorted = ag.Where(t => t.asn_status == 3).Sum(t => t.sorted_qty),
                                shortage_qty = ag.Where(t => t.asn_status == 4).Sum(t => t.shortage_qty)
                            };

        var outbound_group_datas = from r in outboundReceiptDbSet
                                   join rd in outboundReceiptDetailDbSet on r.Id equals rd.ReceiptId
                                   where r.Status == ReceiptStatus.DRAFT
                                      || r.Status == ReceiptStatus.NEW
                                      || r.Status == ReceiptStatus.PROCESSING
                                   group rd by rd.SkuId into og
                                   select new
                                   {
                                       sku_id = (int?)og.Key,
                                       qty_locked = og.Sum(x => (decimal?)x.Quantity) ?? 0m
                                   };

        var processLockedGroupDatas = from pd in processdetailDBSet
                                      join gl in locationDbSet on pd.goods_location_id equals gl.Id
                                      where pd.is_update_stock == false && pd.is_source == true
                                      group new { pd, gl } by pd.sku_id into pdg
                                      select new
                                      {
                                          sku_id = pdg.Key,
                                          qty_locked = pdg.Sum(t => t.pd.qty),
                                          qty_normal_locked = pdg.Where(t => t.gl.WarehouseAreaProperty != 5).Sum(t => t.pd.qty),
                                      };

        var moveLockedGroupDatas = from m in moveDBSet.AsNoTracking()
                                   join gl in locationDbSet on m.orig_goods_location_id equals gl.Id
                                   where m.move_status == 0
                                   group new { m, gl } by m.sku_id into mg
                                   select new
                                   {
                                       sku_id = mg.Key,
                                       qty_locked = mg.Sum(t => t.m.qty),
                                       qty_normal_locked = mg.Where(t => t.gl.WarehouseAreaProperty != 5).Sum(t => t.m.qty),
                                   };

        var query = from sku in skuDBSet
                    join ag in asnGroupDatas on sku.Id equals ag.sku_id into ag_left
                    from ag in ag_left.DefaultIfEmpty()
                    join sg in stockGroupDatas on sku.Id equals sg.sku_id into sg_left
                    from sg in sg_left.DefaultIfEmpty()
                    join dp in outbound_group_datas on sku.Id equals dp.sku_id into dp_left
                    from dp in dp_left.DefaultIfEmpty()
                    join pl in processLockedGroupDatas on sku.Id equals pl.sku_id into pl_left
                    from pl in pl_left.DefaultIfEmpty()
                    join m in moveLockedGroupDatas on sku.Id equals m.sku_id into m_left
                    from m in m_left.DefaultIfEmpty()
                    join spu in spuDBSet on sku.spu_id equals spu.Id into spu_left
                    from spu in spu_left.DefaultIfEmpty()
                    select new StockManagementViewModel
                    {
                        sku_id = sku.Id,
                        spu_name = spu.spu_name ?? string.Empty,
                        spu_code = spu.spu_code ?? string.Empty,
                        sku_code = sku.sku_code,
                        sku_name = sku.sku_name,
                        unit = sku.unit ?? string.Empty,
                        location_name = sg.location_name,
                        warehouse_name = sg.warehouse_name,
                        series_number = sg.series_number,
                        qty_asn = (int?)ag.qty_asn ?? 0,

                        qty_available = ((int?)sg.qty_normal ?? 0)
                                  - ((int?)sg.qty_normal_frozen ?? 0)
                                  - ((int?)dp.qty_locked ?? 0)
                                  - ((int?)pl.qty_normal_locked ?? 0)
                                  - ((int?)m.qty_normal_locked ?? 0),

                        qty_frozen = (int?)sg.qty_frozen ?? 0,

                        qty_locked = ((int?)dp.qty_locked ?? 0)
                                   + ((int?)pl.qty_locked ?? 0)
                                   + ((int?)m.qty_locked ?? 0),

                        qty_sorted = (int?)ag.qty_sorted ?? 0,
                        qty_to_sort = (int?)ag.qty_to_sort ?? 0,
                        shortage_qty = (int?)ag.shortage_qty ?? 0,
                        qty_to_unload = (int?)ag.qty_to_unload ?? 0,
                        qty = (int?)sg.qty ?? 0,
                        expiry_date = (DateTime?)sg.expiry_date ?? DateTime.UtcNow,

                    };

        var expression = queries.AsGroupedExpression<StockManagementViewModel>();

        if (expression is not null)
        {
            query = query
                .Where(t => t.qty_available > 0)
                .Where(expression);
        }


        int totals = await query.CountAsync(cancellationToken);
        var list = await query.OrderBy(t => t.sku_code).ThenByDescending(c => c.expiry_date)
                   .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                   .Take(pageSearch.pageSize)
                   .ToListAsync(cancellationToken);

        return (list, totals);
    }

    /// <summary>
    /// location stock page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    public async Task<(List<LocationStockManagementViewModel> data, int totals)> LocationStockPageAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count > 0)
        {
            pageSearch.searchObjects.ForEach(queries.Add);
        }

        var tenantId = currentUser.tenant_id;
        var dbSet = _dbContext.GetDbSet<StockEntity>(tenantId);
        var spuDbSet = _dbContext.GetDbSet<SpuEntity>(tenantId);
        var locationDbSet = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);
        var moveDbSet = _dbContext.GetDbSet<StockmoveEntity>(tenantId);
        var skuDbSet = _dbContext.GetDbSet<SkuEntity>(tenantId);
        var processDetailDbSet = _dbContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();
        var categoryDbSet = _dbContext.GetDbSet<CategoryEntity>(tenantId);
        var goodsOwnerDbSet = _dbContext.GetDbSet<GoodsownerEntity>().AsNoTracking();
        var outboundReceiptDbSet = _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId);
        var outboundReceiptDetailDbSet = _dbContext.GetDbSet<OutBoundReceiptDetailEntity>().AsNoTracking();


        // Main Stock Group
        var qStock = from stock in dbSet
                     join gw in goodsOwnerDbSet on stock.goods_owner_id equals gw.Id into gw_left
                     from gw in gw_left.DefaultIfEmpty()
                     group new { stock, gw } by new
                     {
                         sku_id = (int?)stock.sku_id,
                         goods_location_id = (int?)stock.goods_location_id,
                         goods_owner_id = (int?)stock.goods_owner_id,
                         series_number = stock.series_number,
                         pallet_code = stock.Palletcode,
                         expiry_date = (DateTime?)stock.expiry_date,
                         price = stock.price,
                         putaway_date = (DateTime?)stock.PutAwayDate.Date,
                         supplierId = stock.SupplierId,
                     } into sg
                     select new
                     {
                         Key = sg.Key,
                         GoodsOwnerName = sg.Select(s => s.gw.goods_owner_name).FirstOrDefault(), // Aggregate Name
                         QtyFrozen = sg.Where(t => t.stock.is_freeze == true).Sum(e => e.stock.qty),
                         QtyTotal = sg.Sum(t => t.stock.qty),
                         LastUpdate = sg.Max(t => t.stock.last_update_time)
                     };

        // lock Outbound
        var outboundLock = from r in outboundReceiptDbSet
                           join rd in outboundReceiptDetailDbSet on r.Id equals rd.ReceiptId
                           where r.Status == ReceiptStatus.DRAFT
                              || r.Status == ReceiptStatus.NEW
                              || r.Status == ReceiptStatus.PROCESSING
                           group rd by new
                           {
                               sku_id = (int?)rd.SkuId,
                               goods_location_id = (int?)rd.LocationId,

                           } into dg
                           select new
                           {
                               Key = dg.Key,
                               QtyLocked = dg.Sum(x => (int)x.Quantity)
                           };

        // lock Transfer internal
        var qProcessLock = from pd in processDetailDbSet
                           where pd.is_update_stock == false && pd.is_source == true
                           group pd by new
                           {
                               sku_id = (int?)pd.sku_id,
                               goods_location_id = (int?)pd.goods_location_id,
                               goods_owner_id = (int?)pd.goods_owner_id,
                               series_number = pd.series_number,
                               expiry_date = (DateTime?)pd.expiry_date,
                               price = pd.price,
                               putaway_date = (DateTime?)pd.putaway_date.Date
                           } into pdg
                           select new
                           {
                               Key = pdg.Key,
                               QtyLocked = pdg.Sum(t => t.qty)
                           };

        // lock Move Stock  
        var qMoveLock = from m in moveDbSet
                        where m.move_status == 0
                        group m by new
                        {
                            sku_id = (int?)m.sku_id,
                            goods_location_id = (int?)m.orig_goods_location_id,
                            goods_owner_id = (int?)m.goods_owner_id,
                            series_number = m.series_number,
                            expiry_date = (DateTime?)m.expiry_date,
                            price = m.price,
                            putaway_date = (DateTime?)m.putaway_date.Date
                        } into mg
                        select new
                        {
                            Key = mg.Key,
                            QtyLocked = mg.Sum(t => t.qty)
                        };

        var query = from s in qStock
                    join d in outboundLock on new
                    {
                        s.Key.sku_id,
                        s.Key.goods_location_id
                    }
                    equals new
                    {
                        d.Key.sku_id,
                        d.Key.goods_location_id
                    } into d_left

                    from d in d_left.DefaultIfEmpty()
                    join p in qProcessLock on new
                    {
                        s.Key.sku_id,
                        s.Key.goods_location_id,
                        s.Key.goods_owner_id,
                        s.Key.series_number,
                        s.Key.expiry_date,
                        s.Key.price,
                        s.Key.putaway_date
                    } equals new
                    {
                        p.Key.sku_id,
                        p.Key.goods_location_id,
                        p.Key.goods_owner_id,
                        p.Key.series_number,
                        p.Key.expiry_date,
                        p.Key.price,
                        p.Key.putaway_date
                    } into p_left
                    from p in p_left.DefaultIfEmpty()

                    join m in qMoveLock on new
                    {
                        s.Key.sku_id,
                        s.Key.goods_location_id,
                        s.Key.goods_owner_id,
                        s.Key.series_number,
                        s.Key.expiry_date,
                        s.Key.price,
                        s.Key.putaway_date
                    } equals new
                    {
                        m.Key.sku_id,
                        m.Key.goods_location_id,
                        m.Key.goods_owner_id,
                        m.Key.series_number,
                        m.Key.expiry_date,
                        m.Key.price,
                        m.Key.putaway_date
                    } into m_left
                    from m in m_left.DefaultIfEmpty()

                    join sku in skuDbSet on (int?)s.Key.sku_id equals sku.Id into sku_left
                    from sku in sku_left.DefaultIfEmpty()

                    join spu in spuDbSet on sku.spu_id equals spu.Id into spu_left
                    from spu in spu_left.DefaultIfEmpty()

                    join cate in categoryDbSet on spu.category_id equals cate.Id into cate_left
                    from cate in cate_left.DefaultIfEmpty()

                    join gl in locationDbSet on (int?)s.Key.goods_location_id equals gl.Id into gl_left
                    from gl in gl_left.DefaultIfEmpty()


                    let lockedOutbound = (int?)d.QtyLocked ?? 0
                    let lockedProcess = (int?)p.QtyLocked ?? 0
                    let lockedMove = (int?)m.QtyLocked ?? 0
                    let totalLocked = lockedOutbound + lockedProcess + lockedMove
                    let isVirtualArea = gl != null && gl.WarehouseAreaProperty == 5

                    select new LocationStockManagementViewModel
                    {
                        sku_id = s.Key.sku_id ?? 0,
                        goods_owner_name = s.GoodsOwnerName,
                        spu_name = spu.spu_name ?? string.Empty,
                        spu_code = spu.spu_code ?? string.Empty,
                        sku_code = sku.sku_code,
                        sku_name = sku.sku_name,
                        qty_available = isVirtualArea ? 0 : (s.QtyTotal - s.QtyFrozen - totalLocked),
                        qty_frozen = s.QtyFrozen,
                        qty_locked = totalLocked,
                        qty = s.QtyTotal,
                        LocationName = gl != null ? gl.LocationName : "",
                        WarehouseName = gl != null ? gl.WarehouseName : "",
                        location_name = gl != null ? gl.LocationName : "",
                        warehouse_name = gl != null ? gl.WarehouseName : "",
                        series_number = s.Key.series_number,
                        pallet_code = s.Key.pallet_code ?? "",
                        expiry_date = s.Key.expiry_date,
                        price = s.Key.price,
                        putaway_date = s.Key.putaway_date,
                        last_update_time = s.LastUpdate,
                        category_id = cate == null ? 0 : cate.Id,
                        category_name = cate == null ? "" : cate.category_name,
                        goods_location_id = s.Key.goods_location_id ?? 0
                    };

        // 5. Execution
        var expression = queries.AsGroupedExpression<LocationStockManagementViewModel>(Condition.AndAlso);
        if (expression != null)
        {
            query = query.Where(expression);
        }
        int totals = await query.CountAsync(cancellationToken);

        // using Order By FEFO
        List<LocationStockManagementViewModel> list = await query
                   .Where(t => t.qty_available > 0)
                   .OrderByDescending(t => t.expiry_date)
                   .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                   .Take(pageSearch.pageSize)
                   .ToListAsync(cancellationToken);

        return (list, totals);
    }

    /// <summary>
    /// safety stock
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(List<SafetyStockManagementViewModel> data, int totals)> SafetyStockPageAsync(PageSearch pageSearch, CurrentUser currentUser)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count != 0)
        {
            pageSearch.searchObjects.ForEach(queries.Add);
        }
        var tenantId = currentUser.tenant_id;
        var DbSet = _dbContext.GetDbSet<StockEntity>(tenantId);
        var dispatch_DBSet = _dbContext.GetDbSet<DispatchlistEntity>(tenantId);

        var dispatchpick_DBSet = _dbContext.GetDbSet<DispatchpicklistEntity>().AsNoTracking();
        var sku_DBSet = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var spu_DBSet = _dbContext.GetDbSet<SpuEntity>().AsNoTracking();
        var location_DBSet = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
        var processdetail_DBSet = _dbContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();
        var move_DBSet = _dbContext.GetDbSet<StockmoveEntity>().AsNoTracking();
        var sku_safety_DBSet = _dbContext.GetDbSet<SkuSafetyStockEntity>().AsNoTracking();

        var stock_group_datas = from stock in DbSet
                                join gl in location_DBSet on stock.goods_location_id equals gl.Id
                                where stock.TenantId == currentUser.tenant_id
                                group new { stock, gl } by new { stock.sku_id, gl.WarehouseId } into sg
                                select new
                                {
                                    sku_id = sg.Key.sku_id,
                                    WarehouseId = sg.Key.WarehouseId,
                                    qty_frozen = sg.Where(t => t.stock.is_freeze == true).Sum(e => e.stock.qty),
                                    qty = sg.Sum(t => t.stock.qty)
                                };

        var dispatch_group_datas = from dp in dispatch_DBSet
                                   join dpp in dispatchpick_DBSet on dp.Id equals dpp.dispatchlist_id
                                   join gl in location_DBSet on dpp.goods_location_id equals gl.Id
                                   where dp.dispatch_status > 1 && dp.dispatch_status < 6
                                   group dpp by new { dpp.sku_id, gl.WarehouseId } into dg
                                   select new
                                   {
                                       sku_id = dg.Key.sku_id,
                                       WarehouseId = dg.Key.WarehouseId,
                                       qty_locked = dg.Sum(t => t.pick_qty)
                                   };
        var process_locked_group_datas = from pd in processdetail_DBSet
                                         join gl in location_DBSet on pd.goods_location_id equals gl.Id
                                         where pd.is_update_stock == false && pd.is_source == true
                                         group new { pd, gl } by new { pd.sku_id, gl.WarehouseId } into pdg
                                         select new
                                         {
                                             sku_id = pdg.Key.sku_id,
                                             WarehouseId = pdg.Key.WarehouseId,
                                             qty_locked = pdg.Sum(t => t.pd.qty)
                                         };

        var move_locked_group_datas = from m in move_DBSet
                                      join gl in location_DBSet on m.orig_goods_location_id equals gl.Id
                                      where m.move_status == 0
                                      group new { m, gl } by new { m.sku_id, gl.WarehouseId } into mg
                                      select new
                                      {
                                          sku_id = mg.Key.sku_id,
                                          WarehouseId = mg.Key.WarehouseId,
                                          qty_locked = mg.Sum(t => t.m.qty)
                                      };
        var query = from sg in stock_group_datas
                    join dp in dispatch_group_datas on new { sg.sku_id, sg.WarehouseId } equals new { dp.sku_id, dp.WarehouseId } into dp_left
                    from dp in dp_left.DefaultIfEmpty()
                    join pl in process_locked_group_datas on new { sg.sku_id, sg.WarehouseId } equals new { pl.sku_id, pl.WarehouseId } into pl_left
                    from pl in pl_left.DefaultIfEmpty()
                    join m in move_locked_group_datas on new { sg.sku_id, sg.WarehouseId } equals new { m.sku_id, m.WarehouseId } into m_left
                    from m in m_left.DefaultIfEmpty()
                    join sku in sku_DBSet on sg.sku_id equals sku.Id
                    join spu in spu_DBSet on sku.spu_id equals spu.Id
                    join gl in location_DBSet on sg.WarehouseId equals gl.Id
                    join sss in sku_safety_DBSet on new { sg.sku_id, sg.WarehouseId } equals new { sss.sku_id, sss.WarehouseId } into sss_left
                    from sss in sss_left.DefaultIfEmpty()
                    select new SafetyStockManagementViewModel
                    {
                        sku_id = sg.sku_id,
                        spu_name = spu.spu_name,
                        spu_code = spu.spu_code,
                        sku_code = sku.sku_code,
                        sku_name = sku.sku_name,
                        qty_available = gl.WarehouseAreaProperty == 5 ? 0 : (
                                        sg.qty
                                        - sg.qty_frozen
                                        - ((int?)dp.qty_locked ?? 0)  // Sửa: Thêm (int?)
                                        - ((int?)pl.qty_locked ?? 0)  // Sửa: Thêm (int?)
                                        - ((int?)m.qty_locked ?? 0)   // Sửa: Thêm (int?)
                                    ),

                        qty_frozen = sg.qty_frozen,

                        qty_locked = ((int?)dp.qty_locked ?? 0)
                                       + ((int?)pl.qty_locked ?? 0)
                                       + ((int?)m.qty_locked ?? 0),
                        qty = sg.qty,
                        WarehouseName = gl.WarehouseName,
                        safety_stock_qty = (int?)sss.safety_stock_qty ?? 0,
                    };
        query = query.Where(queries.AsExpression<SafetyStockManagementViewModel>());
        int totals = await query.CountAsync();
        var list = await query.OrderBy(t => t.sku_code)
                   .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                   .Take(pageSearch.pageSize)
                   .ToListAsync();
        return (list, totals);
    }

    /// <summary>
    ///  page search select
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(List<StockViewModel> data, int totals)> SelectPageAsync(PageSearch pageSearch, CurrentUser currentUser)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count > 0)
        {
            pageSearch.searchObjects.ForEach(queries.Add);
        }

        var tenantId = currentUser.tenant_id;

        var stockDbSet = _dbContext.GetDbSet<StockEntity>(tenantId);
        var dispatchDbSet = _dbContext.GetDbSet<DispatchlistEntity>(tenantId);

        var dispatchPickDbSet = _dbContext.GetDbSet<DispatchpicklistEntity>().AsNoTracking();
        var skuDbSet = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var spuDbSet = _dbContext.GetDbSet<SpuEntity>().AsNoTracking();
        var locationDbSet = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
        var processDetailDbSet = _dbContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();
        var moveDbSet = _dbContext.GetDbSet<StockmoveEntity>().AsNoTracking();
        var ownerDb = _dbContext.GetDbSet<GoodsownerEntity>().AsNoTracking();

        var dispatch_group_datas = from dp in dispatchDbSet
                                   join dpp in dispatchPickDbSet on dp.Id equals dpp.dispatchlist_id
                                   where dp.dispatch_status > 1 && dp.dispatch_status < 6
                                   group dpp by new { dpp.sku_id, dpp.goods_location_id, dpp.goods_owner_id, dpp.series_number, dpp.expiry_date, dpp.price, dpp.putaway_date } into dg
                                   select new
                                   {
                                       goods_owner_id = dg.Key.goods_owner_id,
                                       sku_id = dg.Key.sku_id,
                                       goods_location_id = dg.Key.goods_location_id,
                                       series_number = dg.Key.series_number,
                                       dg.Key.expiry_date,
                                       dg.Key.price,
                                       putaway_date = dg.Key.putaway_date.Date,
                                       qty_locked = dg.Sum(t => t.pick_qty)
                                   };

        var process_locked_group_datas = from pd in processDetailDbSet
                                         where pd.is_update_stock == false && pd.is_source == true
                                         group pd by new { pd.sku_id, pd.goods_location_id, pd.goods_owner_id, pd.series_number, pd.expiry_date, pd.price, pd.putaway_date } into pdg
                                         select new
                                         {
                                             goods_owner_id = pdg.Key.goods_owner_id,
                                             sku_id = pdg.Key.sku_id,
                                             goods_location_id = pdg.Key.goods_location_id,
                                             series_number = pdg.Key.series_number,
                                             pdg.Key.expiry_date,
                                             pdg.Key.price,
                                             putaway_date = pdg.Key.putaway_date.Date,
                                             qty_locked = pdg.Sum(t => t.qty)
                                         };

        var move_locked_group_datas = from m in moveDbSet
                                      where m.move_status == 0
                                      group m by new { m.sku_id, m.orig_goods_location_id, m.goods_owner_id, m.series_number, m.expiry_date, m.price, m.putaway_date } into mg
                                      select new
                                      {
                                          goods_owner_id = mg.Key.goods_owner_id,
                                          sku_id = mg.Key.sku_id,
                                          goods_location_id = mg.Key.orig_goods_location_id,
                                          series_number = mg.Key.series_number,
                                          mg.Key.expiry_date,
                                          mg.Key.price,
                                          putaway_date = mg.Key.putaway_date.Date,
                                          qty_locked = mg.Sum(t => t.qty)
                                      };


        var query = from sg in stockDbSet
                    join dp in dispatch_group_datas on
             new
             {
                 sku_id = (int?)sg.sku_id,
                 goods_location_id = (int?)sg.goods_location_id,
                 goods_owner_id = (int?)sg.goods_owner_id,
                 series_number = sg.series_number,
                 expiry_date = (DateTime?)sg.expiry_date,
                 price = sg.price,
                 putaway_date = (DateTime?)sg.PutAwayDate.Date
             } equals
            new
            {
                sku_id = (int?)dp.sku_id,
                goods_location_id = (int?)dp.goods_location_id,
                goods_owner_id = (int?)dp.goods_owner_id,
                series_number = dp.series_number,
                expiry_date = (DateTime?)dp.expiry_date,
                price = dp.price,
                putaway_date = (DateTime?)dp.putaway_date.Date
            } into dp_left
                    from dp in dp_left.DefaultIfEmpty()
                    join pl in process_locked_group_datas on
                    new
                    {
                        sku_id = (int?)sg.sku_id,
                        goods_location_id = (int?)sg.goods_location_id,
                        goods_owner_id = (int?)sg.goods_owner_id,
                        series_number = sg.series_number,
                        expiry_date = (DateTime?)sg.expiry_date,
                        price = sg.price,
                        putaway_date = (DateTime?)sg.PutAwayDate.Date
                    } equals
                    new
                    {
                        sku_id = (int?)pl.sku_id,
                        goods_location_id = (int?)pl.goods_location_id,
                        goods_owner_id = (int?)pl.goods_owner_id,
                        series_number = pl.series_number,
                        expiry_date = (DateTime?)pl.expiry_date,
                        price = pl.price,
                        putaway_date = (DateTime?)pl.putaway_date.Date
                    } into pl_left

                    from pl in pl_left.DefaultIfEmpty()


                    join m in move_locked_group_datas on
                    new
                    {
                        sku_id = (int?)sg.sku_id,
                        goods_location_id = (int?)sg.goods_location_id,
                        goods_owner_id = (int?)sg.goods_owner_id,
                        series_number = sg.series_number,
                        expiry_date = (DateTime?)sg.expiry_date,
                        price = sg.price,
                        putaway_date = (DateTime?)sg.PutAwayDate.Date
                    }
                    equals
                    new
                    {
                        sku_id = (int?)m.sku_id,
                        goods_location_id = (int?)m.goods_location_id,
                        goods_owner_id = (int?)m.goods_owner_id,
                        series_number = m.series_number,
                        expiry_date = (DateTime?)m.expiry_date,
                        price = m.price,
                        putaway_date = (DateTime?)m.putaway_date.Date
                    } into m_left
                    from m in m_left.DefaultIfEmpty()
                    join sku in skuDbSet on sg.sku_id equals sku.Id
                    join spu in spuDbSet on sku.spu_id equals spu.Id
                    join gl in locationDbSet on sg.goods_location_id equals gl.Id
                    join owner in ownerDb on sg.goods_owner_id equals owner.Id into o_left
                    from owner in o_left.DefaultIfEmpty()
                    where sg.TenantId == currentUser.tenant_id
                    group new { sg, dp, pl, m, sku, spu, gl } by new
                    {
                        sg.sku_id,
                        spu.spu_name,
                        spu.spu_code,
                        sku.sku_code,
                        sku.sku_name,
                        sg.goods_location_id,
                        sg.goods_owner_id,
                        owner.goods_owner_name,
                        sg.qty,
                        gl.LocationName,
                        sg.is_freeze,
                        gl.WarehouseName,
                        sg.Id,
                        sku.unit,
                        sg.series_number,
                        sg.expiry_date,
                        sg.price,
                        sg.PutAwayDate,
                        sg.TenantId
                    } into g

                    select new StockViewModel
                    {
                        sku_id = g.Key.sku_id,
                        spu_name = g.Key.spu_name,
                        spu_code = g.Key.spu_code,
                        sku_code = g.Key.sku_code,
                        sku_name = g.Key.sku_name,
                        qty_available = g.Key.is_freeze ? 0 : (
                                        g.Key.qty
                                        - g.Sum(t => (int?)t.dp.qty_locked ?? 0)
                                        - g.Sum(t => (int?)t.pl.qty_locked ?? 0)
                                        - g.Sum(t => (int?)t.m.qty_locked ?? 0)
                                    ),
                        qty = g.Key.qty,
                        goods_location_id = g.Key.goods_location_id,
                        goods_owner_id = g.Key.goods_owner_id,
                        LocationName = g.Key.LocationName,
                        WarehouseName = g.Key.WarehouseName,
                        series_number = g.Key.series_number,
                        expiry_date = g.Key.expiry_date,
                        price = g.Key.price,
                        putaway_date = g.Key.PutAwayDate,
                        is_freeze = g.Key.is_freeze,
                        id = g.Key.Id,
                        tenant_id = g.Key.TenantId,
                        unit = g.Key.unit,
                        goods_owner_name = g.Key.goods_owner_name == null ? "" : g.Key.goods_owner_name,
                    };

        if (pageSearch.sqlTitle == "")
        {
            query = query.Where(t => t.qty_available > 0);
        }
        else if (pageSearch.sqlTitle == "all")
        {
        }
        else if (pageSearch.sqlTitle == "frozen")
        {
            query = query.Where(t => t.is_freeze == true);
        }
        query = query.Where(queries.AsExpression<StockViewModel>());
        int totals = await query.CountAsync();
        var list = await query.OrderBy(t => t.sku_code)
                   .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                   .Take(pageSearch.pageSize)
                   .ToListAsync();
        return (list, totals);
    }

    /// <summary>
    ///  sku page search select
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>   
    /// <returns></returns>
    public async Task<(List<SkuSelectViewModel> data, int totals)> SkuSelectPageAsync(PageSearch pageSearch, CurrentUser currentUser)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count > 0)
        {
            pageSearch.searchObjects.ForEach(s =>
            {
                queries.Add(s);
            });
        }
        var sku_DBSet = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var spu_DBSet = _dbContext.GetDbSet<SpuEntity>(currentUser.tenant_id);

        var query = from sku in sku_DBSet
                    join spu in spu_DBSet on sku.spu_id equals spu.Id
                    select new SkuSelectViewModel
                    {
                        spu_id = sku.spu_id.GetValueOrDefault(),
                        sku_code = sku.sku_code,
                        sku_name = sku.sku_name,
                        unit = sku.unit ?? "",
                        spu_code = spu.spu_code,
                        spu_name = spu.spu_name,
                        supplier_id = spu.supplier_id.GetValueOrDefault(),
                        supplier_name = spu.supplier_name,
                        brand = spu.brand,
                        origin = spu.origin,
                        sku_id = sku.Id,
                        cost = sku.cost.GetValueOrDefault(),
                        price = sku.price.GetValueOrDefault(),
                        height = sku.height.GetValueOrDefault(),
                        weight = sku.weight.GetValueOrDefault(),
                        volume = sku.volume.GetValueOrDefault(),
                    };
        query = query.Where(queries.AsExpression<SkuSelectViewModel>());
        int totals = await query.CountAsync();
        var list = await query.OrderBy(t => t.sku_code)
                   .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                   .Take(pageSearch.pageSize)
                   .ToListAsync();
        return (list, totals);
    }

    /// <summary>
    ///  sku page search select available (qty > 0)
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <returns></returns>
    public async Task<(List<SkuSelectViewModel> data, int totals)> SkuSelectAvailablePageAsync(PageSearch pageSearch)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count > 0)
        {
            pageSearch.searchObjects.ForEach(s =>
            {
                queries.Add(s);
            });
        }
        var sku_DBSet = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var spu_DBSet = _dbContext.GetDbSet<SpuEntity>().AsNoTracking();
        var stock_DBSet = _dbContext.GetDbSet<StockEntity>().AsNoTracking();
        var locationDbSet = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking(); // Cần bảng này
        var dispatchListDbSet = _dbContext.GetDbSet<DispatchlistEntity>().AsNoTracking();
        var dispatchPickDbSet = _dbContext.GetDbSet<DispatchpicklistEntity>().AsNoTracking();
        var processDetailDbSet = _dbContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();
        var moveDbSet = _dbContext.GetDbSet<StockmoveEntity>().AsNoTracking();


        var sourceStock = from s in stock_DBSet
                          join l in locationDbSet on s.goods_location_id equals l.Id
                          where l.WarehouseAreaProperty != 5
                          select new
                          {
                              SkuId = s.sku_id,
                              Qty = (decimal)s.qty // (+) Cộng vào
                          };


        var sourceFrozen = from s in stock_DBSet
                           join l in locationDbSet on s.goods_location_id equals l.Id
                           where l.WarehouseAreaProperty != 5 && s.is_freeze == true
                           select new
                           {
                               SkuId = s.sku_id,
                               Qty = -(decimal)s.qty // (-) Trừ đi
                           };


        var sourceDispatch = from dp in dispatchListDbSet
                             join dpp in dispatchPickDbSet on dp.Id equals dpp.dispatchlist_id
                             where dp.dispatch_status > 1 && dp.dispatch_status < 6
                             select new
                             {
                                 SkuId = dpp.sku_id,
                                 Qty = -(decimal)dpp.pick_qty // (-) Trừ đi
                             };


        var sourceProcess = from pd in processDetailDbSet
                            where pd.is_update_stock == false && pd.is_source == true
                            select new
                            {
                                SkuId = pd.sku_id,
                                Qty = -(decimal)pd.qty // (-) Trừ đi
                            };


        var sourceMove = from m in moveDbSet
                         where m.move_status == 0
                         select new
                         {
                             SkuId = m.sku_id,
                             Qty = -(decimal)m.qty // (-) Trừ đi
                         };



        var allSources = sourceStock
                         .Concat(sourceFrozen)
                         .Concat(sourceDispatch)
                         .Concat(sourceProcess)
                         .Concat(sourceMove);

        var qAvailableStock = from x in allSources
                              group x by x.SkuId into g
                              select new
                              {
                                  SkuId = g.Key,
                                  AvailableQty = g.Sum(y => y.Qty)
                              };

        var qFinalAvailable = qAvailableStock.Where(x => x.AvailableQty > 0);



        var query = from sku in sku_DBSet
                    join spu in spu_DBSet on sku.spu_id equals spu.Id
                    join sq in qFinalAvailable on sku.Id equals sq.SkuId

                    select new SkuSelectViewModel
                    {
                        spu_id = sku.spu_id.GetValueOrDefault(),
                        sku_code = sku.sku_code,
                        sku_name = sku.sku_name,
                        unit = sku.unit ?? "",
                        spu_code = spu.spu_code,
                        spu_name = spu.spu_name,
                        supplier_id = spu.supplier_id.GetValueOrDefault(),
                        supplier_name = spu.supplier_name,
                        brand = spu.brand,
                        origin = spu.origin,
                        sku_id = sku.Id,

                        // Ép kiểu về int nếu ViewModel yêu cầu int
                        qty_available = (int)sq.AvailableQty,

                        cost = sku.cost.GetValueOrDefault(),
                        price = sku.price.GetValueOrDefault(),
                        weight = sku.weight.GetValueOrDefault(),
                        volume = sku.volume.GetValueOrDefault(),
                    };

        // Áp dụng tìm kiếm động
        query = query.Where(queries.AsExpression<SkuSelectViewModel>());

        // Debug SQL để kiểm tra
        var sqlString = query.ToString();
        int totals = await query.CountAsync();
        var list = await query.OrderBy(t => t.sku_code)
                                       .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                                       .Take(pageSearch.pageSize)
                                       .ToListAsync();
        return (list, totals);
    }

    /// <summary>
    /// get stock infomation by phone
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<List<LocationStockManagementViewModel>> LocationStockForPhoneAsync(LocationStockForPhoneSearchViewModel input, CurrentUser currentUser)
    {
        var tenantId = currentUser.tenant_id;
        var DbSet = _dbContext.GetDbSet<StockEntity>(tenantId);
        var dispatch_DBSet = _dbContext.GetDbSet<DispatchlistEntity>(tenantId);

        var dispatchpick_DBSet = _dbContext.GetDbSet<DispatchpicklistEntity>().AsNoTracking();
        var sku_DBSet = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var spu_DBSet = _dbContext.GetDbSet<SpuEntity>().AsNoTracking();
        var location_DBSet = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
        var processdetailDBSet = _dbContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();
        var moveDBSet = _dbContext.GetDbSet<StockmoveEntity>().AsNoTracking();
        var ownerDb = _dbContext.GetDbSet<GoodsownerEntity>().AsNoTracking();


        var stock_group_datas = from stock in DbSet
                                join gw in ownerDb on stock.goods_owner_id equals gw.Id into gw_left
                                from gw in gw_left.DefaultIfEmpty()
                                join gl in location_DBSet on stock.goods_location_id equals gl.Id
                                join sku in sku_DBSet on stock.sku_id equals sku.Id
                                join spu in spu_DBSet on sku.spu_id equals spu.Id
                                where (input.sku_id == 0 || stock.sku_id == input.sku_id)
                                && (input.goods_location_id == 0 || stock.goods_location_id == input.goods_location_id)
                                && (input.WarehouseId == 0 || gl.WarehouseId == input.WarehouseId)
                                && (input.spu_name == "" || spu.spu_name.Contains(input.spu_name))
                                && (input.LocationName == "" || gl.LocationName.Contains(input.LocationName))
                                && (input.series_number == "" || stock.series_number == input.series_number)
                                group new { stock, gw } by new { stock.sku_id, stock.goods_location_id, stock.goods_owner_id, gw.goods_owner_name, stock.series_number, stock.expiry_date, stock.price, putaway_date = (DateTime?)stock.PutAwayDate } into sg
                                select new
                                {
                                    sku_id = sg.Key.sku_id,
                                    goods_location_id = sg.Key.goods_location_id,
                                    goods_owner_id = sg.Key.goods_owner_id,
                                    goods_owner_name = sg.Key.goods_owner_name,
                                    series_number = sg.Key.series_number,
                                    sg.Key.expiry_date,
                                    sg.Key.price,
                                    sg.Key.putaway_date,
                                    qty_frozen = sg.Where(t => t.stock.is_freeze == true).Sum(e => e.stock.qty),
                                    qty = sg.Sum(t => t.stock.qty)
                                };

        var dispatch_group_datas = from dp in dispatch_DBSet
                                   join dpp in dispatchpick_DBSet on dp.Id equals dpp.dispatchlist_id
                                   where dp.dispatch_status > 1 && dp.dispatch_status < 6
                                   group dpp by new { dpp.sku_id, dpp.goods_location_id, dpp.goods_owner_id, dpp.series_number, dpp.expiry_date, dpp.price, putaway_date = (DateTime?)dpp.putaway_date } into dg
                                   select new
                                   {
                                       sku_id = dg.Key.sku_id,
                                       goods_location_id = dg.Key.goods_location_id,
                                       goods_owner_id = dg.Key.goods_owner_id,
                                       series_number = dg.Key.series_number,
                                       dg.Key.expiry_date,
                                       dg.Key.price,
                                       dg.Key.putaway_date,
                                       qty_locked = dg.Sum(t => t.pick_qty)
                                   };
        var process_locked_group_datas = from pd in processdetailDBSet
                                         join gl in location_DBSet on pd.goods_location_id equals gl.Id
                                         where pd.is_update_stock == false && pd.is_source == true
                                         group pd by new
                                         {
                                             pd.sku_id,
                                             pd.goods_location_id,
                                             pd.goods_owner_id,
                                             pd.series_number,
                                             pd.expiry_date,
                                             pd.price,
                                             putaway_date = (DateTime?)pd.putaway_date
                                         } into pdg
                                         select new
                                         {
                                             sku_id = pdg.Key.sku_id,
                                             goods_location_id = pdg.Key.goods_location_id,
                                             goods_owner_id = pdg.Key.goods_owner_id,
                                             series_number = pdg.Key.series_number,
                                             pdg.Key.expiry_date,
                                             pdg.Key.price,
                                             pdg.Key.putaway_date,
                                             qty_locked = (int?)pdg.Sum(t => t.qty)
                                         };

        var move_locked_group_datas = from m in moveDBSet.AsNoTracking()
                                      where m.move_status == 0
                                      group m by new { m.sku_id, m.orig_goods_location_id, m.goods_owner_id, m.series_number, m.expiry_date, m.price, m.putaway_date } into mg
                                      select new
                                      {
                                          sku_id = mg.Key.sku_id,
                                          goods_location_id = mg.Key.orig_goods_location_id,
                                          goods_owner_id = mg.Key.goods_owner_id,
                                          series_number = mg.Key.series_number,
                                          mg.Key.expiry_date,
                                          mg.Key.price,
                                          mg.Key.putaway_date,
                                          qty_locked = mg.Sum(t => t.qty)
                                      };
        var query = from sg in stock_group_datas
                    join dp in dispatch_group_datas on
               new
               {
                   sku_id = (int?)sg.sku_id,
                   goods_location_id = (int?)sg.goods_location_id,
                   goods_owner_id = (int?)sg.goods_owner_id,
                   series_number = sg.series_number,
                   expiry_date = (DateTime?)sg.expiry_date,
                   price = sg.price,
                   putaway_date = (DateTime?)sg.putaway_date
               } equals new
               {
                   sku_id = (int?)dp.sku_id,
                   goods_location_id = (int?)dp.goods_location_id,
                   goods_owner_id = (int?)dp.goods_owner_id,
                   series_number = dp.series_number,
                   expiry_date = (DateTime?)dp.expiry_date,
                   price = dp.price,
                   putaway_date = (DateTime?)dp.putaway_date
               } into dp_left
                    from dp in dp_left.DefaultIfEmpty()
                    join pl in process_locked_group_datas on
                new
                {
                    sku_id = (int?)sg.sku_id,
                    goods_location_id = (int?)sg.goods_location_id,
                    goods_owner_id = (int?)sg.goods_owner_id,
                    series_number = sg.series_number,
                    expiry_date = (DateTime?)sg.expiry_date,
                    price = sg.price,
                    putaway_date = (DateTime?)sg.putaway_date
                } equals
             new
             {
                 sku_id = (int?)pl.sku_id,
                 goods_location_id = (int?)pl.goods_location_id,
                 goods_owner_id = (int?)pl.goods_owner_id,
                 series_number = pl.series_number,
                 expiry_date = (DateTime?)pl.expiry_date,
                 price = pl.price,
                 putaway_date = (DateTime?)pl.putaway_date
             } into pl_left
                    from pl in pl_left.DefaultIfEmpty()
                    join m in move_locked_group_datas on
              new
              {
                  sku_id = (int?)sg.sku_id,
                  goods_location_id = (int?)sg.goods_location_id,
                  goods_owner_id = (int?)sg.goods_owner_id,
                  series_number = sg.series_number,
                  expiry_date = (DateTime?)sg.expiry_date,
                  price = sg.price,
                  putaway_date = (DateTime?)sg.putaway_date
              } equals
             new
             {
                 sku_id = (int?)m.sku_id,
                 goods_location_id = (int?)m.goods_location_id,
                 goods_owner_id = (int?)m.goods_owner_id,
                 series_number = m.series_number,
                 expiry_date = (DateTime?)m.expiry_date,
                 price = m.price,
                 putaway_date = (DateTime?)m.putaway_date
             } into m_left
                    from m in m_left.DefaultIfEmpty()
                    join sku in sku_DBSet on sg.sku_id equals sku.Id
                    join spu in spu_DBSet on sku.spu_id equals spu.Id
                    join gl in location_DBSet on sg.goods_location_id equals gl.Id
                    select new LocationStockManagementViewModel
                    {
                        sku_id = sg.sku_id,
                        goods_owner_name = sg.goods_owner_name,
                        spu_name = spu.spu_name,
                        spu_code = spu.spu_code,
                        sku_code = sku.sku_code,
                        sku_name = sku.sku_name,
                        qty_available = gl.WarehouseAreaProperty == 5 ? 0 : (
                                            sg.qty
                                            - sg.qty_frozen
                                            - (dp == null ? 0 : (int?)dp.qty_locked ?? 0)
                                            - (pl == null ? 0 : (int?)pl.qty_locked ?? 0)
                                            - (m == null ? 0 : (int?)m.qty_locked ?? 0)
                                        ),
                        qty_frozen = sg.qty_frozen,
                        qty_locked = (dp == null ? 0 : (int?)dp.qty_locked ?? 0)
                                                       + (pl == null ? 0 : (int?)pl.qty_locked ?? 0)
                                                       + (m == null ? 0 : (int?)m.qty_locked ?? 0),
                        qty = sg.qty,
                        LocationName = gl.LocationName,
                        WarehouseName = gl.WarehouseName,
                        location_name = gl.LocationName,
                        warehouse_name = gl.WarehouseName,
                        series_number = sg.series_number,
                        expiry_date = sg.expiry_date,
                        price = sg.price,
                        putaway_date = sg.putaway_date,
                        goods_location_id = sg.goods_location_id
                    };

        var list = await query.OrderBy(t => t.sku_code)
                   .ToListAsync();
        return list;
    }

    /// <summary>
    /// delivery statistic
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns> 
    public async Task<(List<DeliveryStatisticViewModel> datas, int totals)> DeliveryStatistic(DeliveryStatisticSearchViewModel input, CurrentUser currentUser)
    {
        var tenantId = currentUser.tenant_id;
        var dispatch_DBSet = _dbContext.GetDbSet<DispatchlistEntity>(tenantId);
        var dispatchpick_DBSet = _dbContext.GetDbSet<DispatchpicklistEntity>().AsNoTracking();
        var sku_DBSet = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var spu_DBSet = _dbContext.GetDbSet<SpuEntity>(tenantId);
        var location_DBSet = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
        var warehouse_DBSet = _dbContext.GetDbSet<WarehouseEntity>().AsNoTracking();
        var owner_DbSet = _dbContext.GetDbSet<GoodsownerEntity>().AsNoTracking();
        if (input.delivery_date_from > DateTime.UtcNow)
        {
            dispatch_DBSet = dispatch_DBSet.Where(t => t.create_time >= input.delivery_date_from);
        }
        if (input.delivery_date_to > DateTime.UtcNow)
        {
            dispatch_DBSet = dispatch_DBSet.Where(t => t.create_time <= input.delivery_date_to);
        }
        var query = from dp in dispatch_DBSet
                    join dpp in dispatchpick_DBSet on dp.Id equals dpp.dispatchlist_id
                    join sku in sku_DBSet on dp.sku_id equals sku.Id
                    join spu in spu_DBSet on sku.spu_id equals spu.Id
                    join location in location_DBSet on dpp.goods_location_id equals location.Id
                    join wh in warehouse_DBSet on location.WarehouseId equals wh.Id
                    join go in owner_DbSet on dpp.goods_owner_id equals go.Id into go_left
                    from go in go_left.DefaultIfEmpty()
                    where dp.dispatch_status >= 6 && spu.spu_name.Contains(input.spu_name) && spu.spu_code.Contains(input.spu_code)
                    && sku.sku_name.Contains(input.sku_name) && sku.sku_code.Contains(input.sku_code) && wh.WarehouseName.Contains(input.WarehouseName)
                    && dp.customer_name.Contains(input.customer_name) && (go.goods_owner_name == null ? "" : go.goods_owner_name).Contains(input.goods_owner_name)
                    group new { dpp, dp, spu, sku } by
                    new
                    {
                        dp.dispatch_no,
                        wh.WarehouseName,
                        LocationName = location.LocationName,
                        spu.spu_name,
                        spu.spu_code,
                        sku.sku_name,
                        sku.sku_code,
                        dpp.series_number,
                        dpp.price,
                        dpp.expiry_date,
                        dpp.putaway_date,
                        dp.customer_name,
                        dp.create_time,
                        dpp.goods_owner_id,
                        goods_owner_name = (go.goods_owner_name == null ? "" : go.goods_owner_name)
                    }
                    into dg
                    select new DeliveryStatisticViewModel
                    {
                        dispatch_no = dg.Key.dispatch_no,
                        WarehouseName = dg.Key.WarehouseName,
                        LocationName = dg.Key.LocationName,
                        spu_name = dg.Key.spu_name,
                        spu_code = dg.Key.spu_code,
                        sku_name = dg.Key.sku_name,
                        sku_code = dg.Key.sku_code,
                        series_number = dg.Key.series_number,
                        expiry_date = dg.Key.expiry_date,
                        price = dg.Key.price,
                        putaway_date = dg.Key.putaway_date,
                        customer_name = dg.Key.customer_name,
                        delivery_date = dg.Key.create_time,
                        goods_owner_name = dg.Key.goods_owner_name == null ? "" : dg.Key.goods_owner_name,
                        delivery_qty = dg.Sum(t => t.dpp.picked_qty),
                        delivery_amount = dg.Sum(t => t.dpp.picked_qty * t.sku.price.GetValueOrDefault())
                    };
        int totals = await query.CountAsync();
        var list = await query.OrderByDescending(t => t.delivery_date)
                                          .Skip((input.pageIndex - 1) * input.pageSize)
                                          .Take(input.pageSize).ToListAsync();
        return (list, totals);
    }

    /// <summary>
    /// stock age page search
    /// </summary>
    /// <param name="input">args</param>
    /// <param name="currentUser">currentUser</param> 
    /// <returns></returns>
    public async Task<(List<StockAgeViewModel> data, int totals)> StockAgePageAsync(StockAgeSearchViewModel input, CurrentUser currentUser)
    {
        var database_config = (Configuration.GetSection("Database")["db"] ?? "").ToUpper();

        var tenantId = currentUser.tenant_id;
        var DbSet = _dbContext.GetDbSet<StockEntity>(tenantId);
        var sku_DBSet = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var spu_DBSet = _dbContext.GetDbSet<SpuEntity>(tenantId);
        var location_DBSet = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
        var ownerDb = _dbContext.GetDbSet<GoodsownerEntity>().AsNoTracking();

        if (input.expiry_date_from > DateTime.UtcNow)
        {
            DbSet = DbSet.Where(t => t.expiry_date >= input.expiry_date_from);
        }
        if (input.expiry_date_to > DateTime.UtcNow)
        {
            DbSet = DbSet.Where(t => t.expiry_date <= input.expiry_date_to);
        }
        var stock_group_datas = from stock in DbSet
                                join gw in ownerDb on stock.goods_owner_id equals gw.Id into gw_left
                                from gw in gw_left.DefaultIfEmpty()
                                group new { stock, gw } by new { stock.sku_id, stock.goods_location_id, stock.goods_owner_id, stock.series_number, gw.goods_owner_name, stock.expiry_date, stock.price, stock.PutAwayDate } into sg
                                select new
                                {
                                    sku_id = sg.Key.sku_id,
                                    goods_location_id = sg.Key.goods_location_id,
                                    goods_owner_id = sg.Key.goods_owner_id,
                                    goods_owner_name = sg.Key.goods_owner_name,
                                    series_number = sg.Key.series_number,
                                    sg.Key.expiry_date,
                                    sg.Key.price,
                                    PutAwayDate = sg.Key.PutAwayDate.Date,
                                    qty_frozen = sg.Where(t => t.stock.is_freeze == true).Sum(e => e.stock.qty),
                                    qty = sg.Sum(t => t.stock.qty)
                                };

        var today = DateTime.Today;
        var query = from sg in stock_group_datas
                    join sku in sku_DBSet on sg.sku_id equals sku.Id
                    join spu in spu_DBSet on sku.spu_id equals spu.Id
                    join gl in location_DBSet on sg.goods_location_id equals gl.Id
                    where spu.spu_name.Contains(input.spu_name) && sku.sku_name.Contains(input.sku_name)
                    && sku.sku_code.Contains(input.sku_code) && spu.spu_code.Contains(input.spu_code)
                    && gl.WarehouseName.Contains(input.WarehouseName)
                    select new StockAgeViewModel
                    {
                        sku_id = sg.sku_id,
                        goods_owner_name = sg.goods_owner_name,
                        spu_name = spu.spu_name,
                        spu_code = spu.spu_code,
                        sku_code = sku.sku_code,
                        sku_name = sku.sku_name,
                        qty = sg.qty,
                        LocationName = gl.LocationName,
                        WarehouseName = gl.WarehouseName,
                        series_number = sg.series_number,
                        expiry_date = sg.expiry_date,
                        price = sg.price,
                        putaway_date = sg.PutAwayDate,
                        stock_age = sg.PutAwayDate == DateTime.UtcNow ? 0 : database_config == "MYSQL" ? Microsoft.EntityFrameworkCore.MySqlDbFunctionsExtensions.DateDiffDay(EF.Functions, sg.PutAwayDate.Date, today) : Microsoft.EntityFrameworkCore.SqlServerDbFunctionsExtensions.DateDiffDay(EF.Functions, sg.PutAwayDate.Date, today),
                    };

        if (input.stock_age_from > 0)
        {
            query = query.Where(t => t.stock_age >= input.stock_age_from);
        }
        if (input.stock_age_to > 0)
        {
            query = query.Where(t => t.stock_age <= input.stock_age_to);
        }
        query = query.Where(t => t.qty > 0);
        int totals = await query.CountAsync();
        var list = await query.OrderBy(t => t.sku_code)
                   .Skip((input.pageIndex - 1) * input.pageSize)
                   .Take(input.pageSize)
                   .ToListAsync();
        return (list, totals);
    }

    #endregion Api

    #region New Flow Api update 8/12/2025

    /// <summary>
    /// Get inventory dashboard badge
    /// </summary>
    /// <returns></returns>
    public async Task<InventoryDashboardViewModel> GetDashboardStatsAsync(CurrentUser currentUser)
    {
        var result = new InventoryDashboardViewModel();

        var tenantId = currentUser.tenant_id;
        var stockDbSet = _dbContext.GetDbSet<StockEntity>().AsNoTracking();
        var skuDbSet = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var spuDbSet = _dbContext.GetDbSet<SpuEntity>(tenantId).AsNoTracking();
        var locationDbSet = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
        var warehouseDbSet = _dbContext.GetDbSet<WarehouseEntity>().AsNoTracking();

        var skuCountQuery = from sku in skuDbSet
                            join spu in spuDbSet on sku.spu_id equals spu.Id
                            select sku.Id;
        result.total_sku = await skuCountQuery.CountAsync();

        var stockSummary = await (from stock in stockDbSet
                                  join gl in locationDbSet on stock.goods_location_id equals gl.Id
                                  group new { stock, gl } by 1 into g
                                  select new
                                  {
                                      TotalQty = g.Sum(t => t.stock.qty),
                                      TotalNormalQty = g.Where(t => t.gl.WarehouseAreaProperty != 5).Sum(t => t.stock.qty),
                                      TotalNormalFrozenQty = g.Where(t => t.gl.WarehouseAreaProperty != 5 && t.stock.is_freeze == true).Sum(t => t.stock.qty)
                                  }).FirstOrDefaultAsync();

        result.total_stock_qty = stockSummary?.TotalQty ?? 0;
        var totalNormalQty = stockSummary?.TotalNormalQty ?? 0;
        var totalNormalFrozenQty = stockSummary?.TotalNormalFrozenQty ?? 0;

        var totalDispatchLocked = await _dbContext.GetDbSet<DispatchlistEntity>(tenantId)
            .Where(t => t.dispatch_status > 1 && t.dispatch_status < 6)
            .SumAsync(t => (decimal?)t.lock_qty) ?? 0;

        var totalProcessLocked = await (from pd in _dbContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking()
                                        join gl in locationDbSet on pd.goods_location_id equals gl.Id
                                        where pd.is_update_stock == false && pd.is_source == true && gl.WarehouseAreaProperty != 5
                                        select pd.qty).SumAsync(q => (decimal?)q) ?? 0;

        var totalMoveLocked = await (from m in _dbContext.GetDbSet<StockmoveEntity>(tenantId)
                                     join gl in locationDbSet on m.orig_goods_location_id equals gl.Id
                                     where m.move_status == 0 && gl.WarehouseAreaProperty != 5
                                     select m.qty).SumAsync(q => (decimal?)q) ?? 0;

        var totalLocked = totalDispatchLocked + totalProcessLocked + totalMoveLocked;
        result.total_available_qty = totalNormalQty - totalNormalFrozenQty - totalLocked;
        if (result.total_available_qty < 0) result.total_available_qty = 0;

        var today = DateTime.Today;
        var soonDate = today.AddDays(30);

        result.expired_qty = await (from stock in stockDbSet
                                    join gl in locationDbSet on stock.goods_location_id equals gl.Id
                                    where stock.expiry_date < today && gl.WarehouseAreaProperty != 5
                                    select stock.qty).SumAsync(q => (decimal?)q) ?? 0;

        result.soon_expired_qty = await (from stock in stockDbSet
                                         join gl in locationDbSet on stock.goods_location_id equals gl.Id
                                         where stock.expiry_date >= today && stock.expiry_date < soonDate && gl.WarehouseAreaProperty != 5
                                         select stock.qty).SumAsync(q => (decimal?)q) ?? 0;

        var skuInventory = await stockDbSet
            .GroupBy(s => s.sku_id)
            .Select(g => new { SkuId = g.Key, TotalQty = g.Sum(s => s.qty) })
            .ToListAsync();

        var activeSkuIdsWithStock = skuInventory.Where(s => s.TotalQty > 0).Select(s => s.SkuId).ToList();
        result.out_of_stock_sku_count = result.total_sku - activeSkuIdsWithStock.Count;

        var safetyStocks = await _dbContext.GetDbSet<SkuSafetyStockEntity>().AsNoTracking()
            .ToListAsync();

        result.low_stock_sku_count = skuInventory
            .Join(safetyStocks, inv => inv.SkuId, safety => safety.sku_id, (inv, safety) => new { inv.TotalQty, safety.safety_stock_qty })
            .Count(x => x.TotalQty < x.safety_stock_qty);

        result.total_locations = await (from loc in locationDbSet
                                        join wh in warehouseDbSet on loc.WarehouseId equals wh.Id
                                        select loc.Id).CountAsync();

        result.used_locations = await stockDbSet
                        .Where(s => s.qty > 0)
                        .Select(s => s.goods_location_id)
                        .Distinct()
                        .CountAsync();

        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<(List<ProductLifeCycleViewModel> data, int totals)> GetStockProductTraceabilityAsync(PageSearch pageSearch, CurrentUser currentUser)
    {
        QueryCollection queries = [];
        if (pageSearch?.searchObjects.Count > 0)
        {
            pageSearch.searchObjects.ForEach(s => queries.Add(s));
        }

        long tenantId = currentUser.tenant_id;
        var asnsort_DBSet = _dbContext.GetDbSet<AsnsortEntity>(tenantId);
        var asn_DBSet = _dbContext.GetDbSet<AsnEntity>(tenantId);
        var asnmaster_DBSet = _dbContext.GetDbSet<AsnmasterEntity>(tenantId);
        var move_DBSet = _dbContext.GetDbSet<StockmoveEntity>(tenantId);
        var adjust_DBSet = _dbContext.GetDbSet<StockadjustEntity>(tenantId);
        var dispatchlist_DBSet = _dbContext.GetDbSet<DispatchlistEntity>(tenantId);
        var stock_DBSet = _dbContext.GetDbSet<StockEntity>(tenantId);

        var supplier_DBSet = _dbContext.GetDbSet<SupplierEntity>().AsNoTracking();
        var dispatchpick_DBSet = _dbContext.GetDbSet<DispatchpicklistEntity>().AsNoTracking();
        var customer_DBSet = _dbContext.GetDbSet<CustomerEntity>().AsNoTracking();
        var location_DBSet = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
        var sku_DBSet = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();

        // --- 0. CURRENT STOCK (Tồn kho hiện tại) ---
        var stock_query = from s in stock_DBSet
                          join sku in sku_DBSet on s.sku_id equals sku.Id
                          join l in location_DBSet on s.goods_location_id equals l.Id
                          where s.qty > 0
                          select new ProductLifeCycleViewModel
                          {
                              sku_id = s.sku_id,
                              sku_code = sku.sku_code,
                              sku_name = sku.sku_name,
                              series_number = s.series_number,
                              ActivityType = "CURRENT_STOCK",
                              EventTime = DateTime.Now, // Hoặc s.last_update_time
                              DocNumber = "-",
                              PartnerName = "-",
                              FromLocation = l.LocationName,
                              ToLocation = "Current Position",
                              Qty = s.qty,
                              PutAwayDate = s.PutAwayDate
                          };

        // --- 1. INBOUND ---
        var inbound_query = from sort in asnsort_DBSet
                            join asn in asn_DBSet on sort.asn_id equals asn.Id
                            join master in asnmaster_DBSet on asn.asnmaster_id equals master.Id
                            join sup in supplier_DBSet on asn.supplier_id equals sup.Id into sup_left
                            from sup in sup_left.DefaultIfEmpty()
                            join sku in sku_DBSet on asn.sku_id equals sku.Id
                            // 1. Gom nhóm
                            group new { sort, asn, master, sup, sku } by new
                            {
                                asn.sku_id,
                                sku.sku_code,
                                sku.sku_name,
                                sort.series_number,
                                master.asn_no,
                                sup.supplier_name,
                                sort.create_time
                            } into g
                            select new ProductLifeCycleViewModel
                            {
                                sku_id = g.Key.sku_id,
                                sku_code = g.Key.sku_code,
                                sku_name = g.Key.sku_name,
                                series_number = g.Key.series_number,
                                ActivityType = "INBOUND",
                                EventTime = g.Key.create_time,
                                DocNumber = g.Key.asn_no,
                                PartnerName = g.Key.supplier_name,
                                FromLocation = "-",
                                ToLocation = "RECEIVING_AREA",
                                Qty = g.Sum(x => x.sort.sorted_qty),
                                PutAwayDate = null
                            };

        // --- 2. MOVE ---
        var move_query = from m in move_DBSet
                         join l_from in location_DBSet on m.orig_goods_location_id equals l_from.Id
                         join l_to in location_DBSet on m.dest_googs_location_id equals l_to.Id
                         join sku in sku_DBSet on m.sku_id equals sku.Id
                         where m.move_status == 1
                         group new { m, l_from, l_to, sku } by new
                         {
                             m.sku_id,
                             sku.sku_code,
                             sku.sku_name,
                             m.series_number,
                             m.job_code,
                             m.handler,
                             m.handle_time,
                             FromLoc = l_from.LocationName,
                             ToLoc = l_to.LocationName,
                             m.putaway_date
                         } into g
                         select new ProductLifeCycleViewModel
                         {
                             sku_id = g.Key.sku_id,
                             sku_code = g.Key.sku_code,
                             sku_name = g.Key.sku_name,
                             series_number = g.Key.series_number,
                             ActivityType = "MOVE",
                             EventTime = g.Key.handle_time,
                             DocNumber = g.Key.job_code,
                             PartnerName = g.Key.handler,
                             FromLocation = g.Key.FromLoc,
                             ToLocation = g.Key.ToLoc,
                             Qty = g.Sum(x => x.m.qty),
                             PutAwayDate = g.Key.putaway_date
                         };

        // --- 3. ADJUST ---
        var adjust_query = from a in adjust_DBSet
                           join l in location_DBSet on a.goods_location_id equals l.Id into l_left
                           from l in l_left.DefaultIfEmpty()
                           join sku in sku_DBSet on a.sku_id equals sku.Id
                           group new { a, l, sku } by new
                           {
                               a.sku_id,
                               sku.sku_code,
                               sku.sku_name,
                               a.series_number,
                               a.job_code,
                               a.creator,
                               a.create_time,
                               LocName = l.LocationName,
                               a.putaway_date
                           } into g
                           select new ProductLifeCycleViewModel
                           {
                               sku_id = g.Key.sku_id,
                               sku_code = g.Key.sku_code,
                               sku_name = g.Key.sku_name,
                               series_number = g.Key.series_number,
                               ActivityType = "ADJUST",
                               EventTime = g.Key.create_time,
                               DocNumber = g.Key.job_code,
                               PartnerName = g.Key.creator,
                               FromLocation = g.Key.LocName,
                               ToLocation = "-", // Adjust usually happens in place
                               Qty = g.Sum(x => x.a.qty),
                               PutAwayDate = g.Key.putaway_date
                           };

        // --- 4. OUTBOUND ---
        var outbound_query = from pick in dispatchpick_DBSet
                             join dispatch in dispatchlist_DBSet on pick.dispatchlist_id equals dispatch.Id
                             join cust in customer_DBSet on dispatch.customer_id equals cust.Id into cust_left
                             from cust in cust_left.DefaultIfEmpty()
                             join l in location_DBSet on pick.goods_location_id equals l.Id
                             join sku in sku_DBSet on pick.sku_id equals sku.Id
                             group new { pick, dispatch, cust, l, sku } by new
                             {
                                 pick.sku_id,
                                 sku.sku_code,
                                 sku.sku_name,
                                 pick.series_number,
                                 dispatch.dispatch_no,
                                 cust.customer_name,
                                 dispatch.last_update_time,
                                 LocName = l.LocationName,
                                 pick.putaway_date
                             } into g
                             select new ProductLifeCycleViewModel
                             {
                                 sku_id = g.Key.sku_id,
                                 sku_code = g.Key.sku_code,
                                 sku_name = g.Key.sku_name,
                                 series_number = g.Key.series_number,
                                 ActivityType = "OUTBOUND",
                                 EventTime = g.Key.last_update_time,
                                 DocNumber = g.Key.dispatch_no,
                                 PartnerName = g.Key.customer_name,
                                 FromLocation = g.Key.LocName,
                                 ToLocation = "DISPATCH_AREA",
                                 Qty = g.Sum(x => -x.pick.picked_qty),
                                 PutAwayDate = g.Key.putaway_date
                             };

        var fullQuery = stock_query
                        .Concat(inbound_query)
                        .Concat(move_query)
                        .Concat(adjust_query)
                        .Concat(outbound_query);

        fullQuery = fullQuery.Where(queries.AsGroupedExpression<ProductLifeCycleViewModel>());

        int totals = await fullQuery.CountAsync();
        pageSearch ??= new PageSearch();

        var list = await fullQuery
                        .OrderByDescending(t => t.EventTime)
                        .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                        .Take(pageSearch.pageSize)
                        .ToListAsync();
        return (list, totals);
    }
    #endregion


    #region Duy Phát 

    /// <summary>
    /// Gets the total available quantity for a specific SKU.
    /// Used to show user how much stock is available before they input required quantity.
    /// </summary>
    /// <param name="pageSearch">page search parameters</param>
    /// <param name="currentUser">Current user context</param>
    /// <returns>Summary of available stock for the SKU</returns>
    public async Task<(List<SkuAvailableSummaryViewModel> data, int totals)> GetSkuAvailableSummaryAsync(
        PageSearch pageSearch,
        CurrentUser currentUser)
    {
        var tenantId = currentUser.tenant_id;

        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count > 0)
        {
            pageSearch.searchObjects.ForEach(queries.Add);
        }

        // Get DbSets
        var stockDbSet = _dbContext.GetDbSet<StockEntity>().AsNoTracking();
        var locationDbSet = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
        var dispatchPickDbSet = _dbContext.GetDbSet<DispatchpicklistEntity>().AsNoTracking();
        var dispatchDbSet = _dbContext.GetDbSet<DispatchlistEntity>().AsNoTracking();
        var dispatchDetailDbSet = _dbContext.GetDbSet<DispatchListDetailEntity>().AsNoTracking();
        var skuDbSet = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var spuDbSet = _dbContext.GetDbSet<SpuEntity>().AsNoTracking();

        var stockGrouped = from stock in stockDbSet
                           join loc in locationDbSet on stock.goods_location_id equals loc.Id
                           group stock by stock.sku_id into sg
                           select new
                           {
                               SkuId = sg.Key,
                               QtyTotal = sg.Sum(s => s.qty),
                           };

        var dispatchLocked = from dp in dispatchDbSet
                             join pick in dispatchDetailDbSet on dp.Id equals pick.dispatchlist_id
                             where dp.dispatch_status > 0 && dp.dispatch_status < 6
                             group pick by pick.sku_id into dg
                             select new
                             {
                                 SkuId = dg.Key,
                                 QtyLocked = (decimal)dg.Sum(p => (decimal)p.allocated_qty)
                             };

        var query = from sku in skuDbSet
                    join spu in spuDbSet on sku.spu_id equals spu.Id

                    join sg in stockGrouped on sku.Id equals sg.SkuId into sgLeft
                    from sg in sgLeft.DefaultIfEmpty()

                        // Left Join Dispatch
                    join dl in dispatchLocked on sku.Id equals dl.SkuId into dlLeft
                    from dl in dlLeft.DefaultIfEmpty()
                    select new SkuAvailableSummaryViewModel
                    {
                        IsFound = true,
                        SkuId = sku.Id,
                        SkuCode = sku.sku_code,
                        SkuName = sku.sku_name,
                        SpuCode = spu.spu_code,
                        SpuName = spu.spu_name,
                        Unit = sku.unit,
                        QtyTotal = (decimal?)sg.QtyTotal ?? 0,
                        QtyFrozen = 0,
                        QtyLockedDispatch = (decimal?)dl.QtyLocked ?? 0,
                        QtyLockedProcess = 0,
                        QtyLockedMove = 0,
                        QtyLocked = (decimal?)dl.QtyLocked ?? 0,
                        QtyAvailable = ((decimal?)sg.QtyTotal ?? 0) - ((decimal?)dl.QtyLocked ?? 0)
                    };

        query = query.Where(x => x.QtyAvailable > 0);


        query = query.Where(queries.AsExpression<SkuAvailableSummaryViewModel>());

        int totals = await query.CountAsync();
        var list = await query
            .OrderBy(x => x.SkuCode)
            .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
            .Take(pageSearch.pageSize)
            .ToListAsync();

        return (list, totals);
    }

    /// <summary>
    /// Gets available stock grouped by Pallet and Location for a specific SKU.
    /// Used for manual outbound selection with FEFO (First Expiry First Out) ordering.
    /// </summary>
    /// <param name="skuId">SKU ID to query</param>
    /// <param name="requiredQty">Required quantity for auto-suggest</param>
    /// <param name="currentUser">Current user context</param>
    /// <returns>List of available stock options for user selection</returns>
    public async Task<List<AvailableStockSelectionViewModel>> GetAvailableStockForOutboundAsync(
        int skuId,
        decimal requiredQty,
        CurrentUser currentUser)
    {
        var tenantId = currentUser.tenant_id;

        var stockDbSet = _dbContext.GetDbSet<StockEntity>().AsNoTracking();
        var palletDbSet = _dbContext.GetDbSet<PalletEntity>().AsNoTracking();
        var locationDbSet = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
        var goodsOwnerDbSet = _dbContext.GetDbSet<GoodsownerEntity>().AsNoTracking();
        var dispatchDbSet = _dbContext.GetDbSet<DispatchlistEntity>().AsNoTracking();
        var dispatchDetailDbSet = _dbContext.GetDbSet<DispatchListDetailEntity>().AsNoTracking();
        var dispatchPickDbSet = _dbContext.GetDbSet<DispatchpicklistEntity>().AsNoTracking();

        var stockQuery = from stock in stockDbSet
                         join pallet in palletDbSet on stock.Palletcode equals pallet.PalletCode into palletLeft
                         from pallet in palletLeft.DefaultIfEmpty()
                         join gw in goodsOwnerDbSet on stock.goods_owner_id equals gw.Id into gwLeft
                         from gw in gwLeft.DefaultIfEmpty()
                         where stock.sku_id == skuId
                         group new { stock, pallet, gw } by new
                         {
                             PalletId = pallet != null ? pallet.Id : 0,
                             PalletCode = stock.Palletcode ?? string.Empty,
                             stock.goods_location_id,
                             stock.goods_owner_id,
                             stock.series_number,
                             stock.expiry_date,
                             stock.price,
                             PutAwayDate = stock.PutAwayDate.Date
                         } into sg
                         select new
                         {
                             sg.Key.PalletId,
                             sg.Key.PalletCode,
                             sg.Key.goods_location_id,
                             sg.Key.goods_owner_id,
                             GoodsOwnerName = sg.Select(x => x.gw.goods_owner_name).FirstOrDefault() ?? string.Empty,
                             sg.Key.series_number,
                             sg.Key.expiry_date,
                             sg.Key.price,
                             sg.Key.PutAwayDate,
                             QtyTotal = (decimal)sg.Sum(x => x.stock.qty),
                             QtyFrozen = (decimal)sg.Where(x => x.stock.is_freeze).Sum(x => x.stock.qty)
                         };

        var lockedQuery = from dp in dispatchDbSet
                          join detail in dispatchDetailDbSet on dp.Id equals detail.dispatchlist_id
                          join pick in dispatchPickDbSet on detail.Id equals pick.dispatch_detail_id
                          where dp.dispatch_status > 0 && dp.dispatch_status < 6
                                && detail.sku_id == skuId

                          group pick by new
                          {
                              pick.goods_location_id,
                              pick.goods_owner_id,
                              pick.series_number,
                              pick.expiry_date,
                              pick.price,
                              PutAwayDate = pick.putaway_date.Date
                          } into dg
                          select new
                          {
                              dg.Key.goods_location_id,
                              dg.Key.goods_owner_id,
                              dg.Key.series_number,
                              dg.Key.expiry_date,
                              dg.Key.price,
                              dg.Key.PutAwayDate,
                              QtyLocked = (decimal?)dg.Sum(x => x.pick_qty) ?? 0
                          };

        var stockList = await (from s in stockQuery
                               join loc in locationDbSet on s.goods_location_id equals loc.Id
                               where loc.WarehouseAreaProperty != 5
                               select new
                               {
                                   s.PalletId,
                                   s.PalletCode,
                                   LocationId = s.goods_location_id,
                                   LocationName = loc.LocationName,
                                   WarehouseName = loc.WarehouseName,
                                   s.goods_owner_id,
                                   s.GoodsOwnerName,
                                   s.series_number,
                                   s.expiry_date,
                                   s.price,
                                   s.PutAwayDate,
                                   s.QtyTotal,
                                   s.QtyFrozen
                               }).ToListAsync();

        var lockedList = await lockedQuery.ToListAsync();

        var result = (from s in stockList
                      join l in lockedList on new
                      {
                          id = s.LocationId,
                          owner = s.goods_owner_id,
                          series = s.series_number ?? string.Empty,
                          exp = s.expiry_date.GetValueOrDefault(),
                          price = s.price,
                          date = s.PutAwayDate
                      } equals new
                      {
                          id = l.goods_location_id,
                          owner = l.goods_owner_id,
                          series = l.series_number ?? string.Empty,
                          exp = l.expiry_date,
                          price = l.price,
                          date = l.PutAwayDate
                      } into lockedGroup
                      from l in lockedGroup.DefaultIfEmpty()

                      let qtyLockedDispatch = l?.QtyLocked ?? 0
                      let qtyAvailable = s.QtyTotal - s.QtyFrozen - qtyLockedDispatch

                      where qtyAvailable > 0

                      select new AvailableStockSelectionViewModel
                      {
                          PalletId = s.PalletId,
                          PalletCode = s.PalletCode,
                          LocationId = s.LocationId,
                          LocationName = s.LocationName,
                          WarehouseName = s.WarehouseName,
                          GoodsOwnerId = s.goods_owner_id,
                          GoodsOwnerName = s.GoodsOwnerName,
                          ExpiryDate = s.expiry_date,
                          Price = s.price,
                          PutAwayDate = s.PutAwayDate,
                          QtyTotal = s.QtyTotal,
                          QtyFrozen = s.QtyFrozen,
                          QtyLockedDispatch = qtyLockedDispatch,
                          QtyLocked = qtyLockedDispatch,
                          QtyAvailable = qtyAvailable
                      })
                      .OrderBy(x => x.ExpiryDate)
                      .ThenByDescending(x => x.QtyAvailable)
                      .ThenBy(x => x.PutAwayDate)
                      .ToList();


        decimal remainingQty = requiredQty;
        foreach (var item in result)
        {
            if (remainingQty <= 0)
            {
                item.SuggestedQty = 0;
                continue;
            }

            var suggestQty = Math.Min(remainingQty, item.QtyAvailable);
            item.SuggestedQty = suggestQty;
            remainingQty -= suggestQty;
        }

        return result;
    }

    /// <summary>
    /// Get Locations
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<LocationWithPalletViewModel>> GetLocationStockBySkuAsync(GetLocationStockBySkuRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;

        var locations = await _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
            .Where(gl => gl.WarehouseId == request.WarehouseId
                         && gl.IsValid)
            .OrderBy(gl => gl.CoordinateZ)
            .ToListAsync(cancellationToken);

        if (locations.Count == 0)
        {
            return [];
        }

        var locationIds = locations.Select(l => l.Id).Distinct().ToList();
        var locationLookup = locations.ToDictionary(l => l.Id);
        var stocks = await _dbContext.GetDbSet<StockEntity>(tenantId)
                                     .Where(s => s.sku_id == request.SkuId
                                                && locationIds.Contains(s.goods_location_id))
                                     .ToListAsync(cancellationToken);

        if (stocks.Count == 0)
        {
            return [];
        }

        var sku = await _dbContext.GetDbSet<SkuEntity>()
                                  .AsNoTracking()
                                  .Where(s => s.Id == request.SkuId)
                                  .Select(s => new { s.Id, s.sku_name })
                                  .FirstOrDefaultAsync(cancellationToken);

        if (sku is null)
        {
            return [];
        }

        var palletCodes = stocks
          .Select(s => s.Palletcode ?? "")
          .Where(code => code != "")
          .Distinct()
          .ToList();

        var palletLookup = palletCodes.Count > 0
            ? await _dbContext.GetDbSet<PalletEntity>(tenantId)
                .AsNoTracking()
                .Where(p => palletCodes.Contains(p.PalletCode))
                .Select(p => new { p.PalletCode, p.Id })
                .ToDictionaryAsync(p => p.PalletCode, p => p.Id, cancellationToken)
            : [];


        var dispatchLockedByLocation = await (from dp in _dbContext.GetDbSet<DispatchlistEntity>(tenantId)
                                              join dpp in _dbContext.GetDbSet<DispatchpicklistEntity>().AsNoTracking()
                                                  on dp.Id equals dpp.dispatchlist_id
                                              where dp.dispatch_status > 1 && dp.dispatch_status < 6
                                                    && dpp.sku_id == request.SkuId
                                                    && locationIds.Contains(dpp.goods_location_id)
                                              group dpp by dpp.goods_location_id into g
                                              select new
                                              {
                                                  GoodsLocationId = g.Key,
                                                  QtyLocked = g.Sum(x => x.pick_qty)
                                              }
                                     ).ToListAsync(cancellationToken);



        var lockedByLocation = dispatchLockedByLocation.ToDictionary(x => x.GoodsLocationId, x => x.QtyLocked);

        var receiptLocked = await (from rd in _dbContext.GetDbSet<OutBoundReceiptDetailEntity>().AsNoTracking()
                                   join r in _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId).AsNoTracking()
                                        on rd.ReceiptId equals r.Id
                                   where (r.Status == ReceiptStatus.DRAFT || r.Status == ReceiptStatus.NEW)
                                         && rd.SkuId == request.SkuId
                                         && rd.LocationId != null
                                         && locationIds.Contains(rd.LocationId.Value)
                                   group rd by rd.LocationId into g
                                   select new
                                   {
                                       LocationId = g.Key,
                                       Qty = g.Sum(x => (int)x.Quantity)
                                   })
                                   .ToDictionaryAsync(x => x.LocationId!.Value, x => x.Qty);



        var stockByLocationAndPallet = stocks
            .GroupBy(s => new { s.goods_location_id, PalletCode = s.Palletcode ?? "" })
            .ToDictionary(g => g.Key, g => g.ToList());

        var results = new List<LocationWithPalletViewModel>();

        foreach (var group in stockByLocationAndPallet)
        {
            if (!locationLookup.TryGetValue(group.Key.goods_location_id, out var location))
            {
                continue;
            }


            var totalQty = group.Value.Sum(s => s.qty);
            var frozenQty = group.Value.Where(s => s.is_freeze).Sum(s => s.qty);

            lockedByLocation.TryGetValue(group.Key.goods_location_id, out var lockedQty);
            receiptLocked.TryGetValue(group.Key.goods_location_id, out var receiptLockedQty);
            palletLookup.TryGetValue(group.Key.PalletCode, out var palletId);

            var availableQty = totalQty - frozenQty - lockedQty - receiptLockedQty;
            if (availableQty <= 0)
            {
                continue;
            }

            results.Add(new LocationWithPalletViewModel
            {
                GoodLocationId = location.Id,
                GoodLocationName = location.LocationName,
                PalletId = palletId,
                PalletCode = group.Key.PalletCode,
                Items =
                [
                    new ItemInPalletViewModel
                    {
                        SkuId = sku.Id,
                        SkuName = sku.sku_name,
                        Qty = availableQty
                    }
                ]
            });
        }

        return [.. results
        .OrderBy(x => x.GoodLocationName)
        .ThenBy(x => x.PalletCode)];
    }

    /// <summary>
    /// Filter Sku Locations By Stock
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<GoodSkuLocationInfo>> FilterSkuLocationByStock(GetLocationStockBySkuRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var queryStock = _dbContext.GetDbSet<StockEntity>(tenantId);
        var locationList = _dbContext.GetDbSet<GoodslocationEntity>(tenantId).Where(x => x.IsValid);
        var querySupplier = _dbContext.GetDbSet<SupplierEntity>(tenantId);
        var querySku = _dbContext.GetDbSet<SkuEntity>(tenantId);

        var queryLock = from rd in _dbContext.GetDbSet<OutBoundReceiptDetailEntity>().AsNoTracking()
                        join r in _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId)
                             on rd.ReceiptId equals r.Id
                        where (r.Status == ReceiptStatus.DRAFT || r.Status == ReceiptStatus.NEW)
                              && rd.SkuId == request.SkuId
                              && rd.LocationId != null
                        group rd by new { rd.LocationId, rd.SkuId } into g
                        select new
                        {
                            g.Key.LocationId,
                            g.Key.SkuId,
                            Qty = g.Sum(x => (int)x.Quantity)
                        };

        if (request.WarehouseId > 0)
        {
            queryStock = from stock in queryStock
                         join loc in locationList on stock.goods_location_id equals loc.Id
                         where loc.WarehouseId == request.WarehouseId
                         select stock;
        }

        if (request.SkuId > 0)
        {
            queryStock = queryStock.Where(s => s.sku_id == request.SkuId);
        }

        var data = await queryStock
            .Select(x => new
            {
                GoodLocationId = x.goods_location_id,
                PalletCode = x.Palletcode ?? string.Empty,
                Quantity = x.qty,
                SkuId = x.sku_id,
                ExpiryDate = x.expiry_date,
                x.SupplierId
            })
            .ToListAsync(cancellationToken);

        var results = new List<GoodSkuLocationInfo>();
        foreach (var item in data)
        {
            var location = await locationList
                .FirstOrDefaultAsync(x => x.Id == item.GoodLocationId, cancellationToken);
            var supplier = await querySupplier.FirstOrDefaultAsync(s => s.Id == item.SupplierId, cancellationToken);
            var qtyLocked = await queryLock
                .Where(l => l.LocationId == item.GoodLocationId && l.SkuId == item.SkuId)
                .SumAsync(l => (int?)l.Qty, cancellationToken) ?? 0;

            var sku = await querySku.FirstOrDefaultAsync(x => x.Id == item.SkuId, cancellationToken);

            results.Add(new GoodSkuLocationInfo
            {
                WarehouseName = location?.WarehouseName ?? "",
                GoodLocationId = item.GoodLocationId,
                GoodLocationName = location != null ? location.LocationName : string.Empty,
                PalletCode = item.PalletCode,
                CurrentQuantity = item.Quantity,
                AvailableQuantity = item.Quantity - qtyLocked,
                SkuId = item.SkuId,
                SkuCode = sku?.sku_code ?? "",
                SkuName = sku?.sku_name ?? "",
                ExpiryDate = item.ExpiryDate,
                SupplierId = item.SupplierId,
                VirtualLocation = location?.GoodsLocationType == GoodsLocationTypeEnum.VirtualLocation,
                SupplierName = supplier != null ? supplier.supplier_name : string.Empty
            });
        }

        return [.. results.Where(x => x.CurrentQuantity > 0).OrderBy(x => x.WarehouseName)
            .ThenBy(x => x.GoodLocationName)
            .ThenBy(x => x.PalletCode)];
    }

    /// <summary>
    /// Get location and pallet with item in this
    /// </summary>
    /// <param name="warehouseID"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<LocationStockInfoDTO>?> GetLocationStockInfoAsync(int warehouseID, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;

        // ignore virtual location
        var locationList = await _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
                                     .Where(x => x.WarehouseId == warehouseID && x.IsValid && x.GoodsLocationType != GoodsLocationTypeEnum.VirtualLocation)
                                     .Select(x => new
                                     {
                                         x.Id,
                                         x.LocationName
                                     }).ToListAsync(cancellationToken);


        if (locationList.Count == 0)
        {
            return [];
        }


        var locationLookup = locationList.ToDictionary(x => x.Id, x => x.LocationName);
        var locationIds = locationList.Select(x => x.Id).ToList();

        var stocks = await _dbContext.GetDbSet<StockEntity>(tenantId)
                                     .Where(s => locationIds.Contains(s.goods_location_id) && s.qty > 0)
                                     .Select(s => new
                                     {
                                         s.Id,
                                         s.sku_id,
                                         s.qty,
                                         s.Palletcode,
                                         s.goods_location_id,
                                         s.PutAwayDate,
                                         s.expiry_date,
                                         s.SupplierId
                                     })
                                    .ToListAsync(cancellationToken);

        if (stocks.Count == 0)
        {
            return [];
        }

        var skuIds = stocks.Select(s => s.sku_id).Distinct().ToList();
        var supplierIds = stocks.Where(s => s.SupplierId.HasValue)
                                .Select(s => s.SupplierId!.Value)
                                .Distinct()
                                .ToList();

        var skuLookup = await _dbContext.GetDbSet<SkuEntity>(tenantId)
                                        .Where(s => skuIds.Contains(s.Id))
                                        .Select(s => new { s.Id, s.sku_code, s.sku_name })
                                        .ToDictionaryAsync(s => s.Id, s => new
                                        { s.sku_code, s.sku_name }, cancellationToken);

        var supplierLookup = await _dbContext.GetDbSet<SupplierEntity>(tenantId)
                                               .AsNoTracking()
                                               .Where(s => supplierIds.Contains(s.Id))
                                               .Select(s => new { s.Id, s.supplier_name })
                                               .ToDictionaryAsync(s => s.Id, s => s.supplier_name, cancellationToken);


        var result = stocks.GroupBy(s => new
        {
            s.goods_location_id,
            PalletCode = s.Palletcode ?? string.Empty,
            PutawayDate = s.PutAwayDate.Date,
        }).Select(group =>
            {
                if (!locationLookup.TryGetValue(group.Key.goods_location_id, out var locationName))
                {
                    return null;
                }

                var items = group.GroupBy(x => new
                {
                    x.sku_id,
                    x.expiry_date,
                    x.SupplierId,
                }).Select(itemGroup =>
                {
                    skuLookup.TryGetValue(itemGroup.Key.sku_id, out var skuInfo);

                    string? supplierName = null;
                    if (itemGroup.Key.SupplierId.HasValue)
                    {
                        supplierLookup.TryGetValue(itemGroup.Key.SupplierId.Value, out supplierName);
                    }

                    return new LocationStockInfoItemDTO
                    {
                        SkuId = itemGroup.Key.sku_id,
                        SkuCode = skuInfo?.sku_code ?? string.Empty,
                        SkuName = skuInfo?.sku_name ?? string.Empty,
                        Quantity = itemGroup.Sum(i => i.qty),
                        ExpiryDate = itemGroup.Key.expiry_date,
                        SupplierId = itemGroup.Key.SupplierId,
                        SupplierName = supplierName
                    };
                }).Where(i => i.Quantity > 0).ToList();

                if (items.Count == 0)
                {
                    return null;
                }

                return new LocationStockInfoDTO
                {
                    PalletCode = group.Key.PalletCode,
                    LocationId = group.Key.goods_location_id,
                    LocationName = locationName,
                    PutawayDate = group.Key.PutawayDate,
                    Items = items
                };
            })
            .Where(x => x is not null)
            .Select(x => x!)
            .OrderBy(x => x.LocationName)
            .ToList();

        return result;

    }
    /// <summary>
    /// update conflict when sync location between WMS and WCS, if WMS has pallet but WCS not, or WCS has pallet but WMS not, or different wcs status, then create a conflict to review and resolve
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    public async Task<(int inserted, int updated)> UpsertLocationSyncConflictsAsync(
      List<UpsertLocationSyncConflictRequest> requests,
      CurrentUser currentUser,
      CancellationToken cancellationToken)
    {
        if (requests == null || requests.Count == 0)
        {
            return (0, 0);
        }

        var tenantId = currentUser.tenant_id;
        var dbSet = _dbContext.GetDbSet<LocationSyncConflictEntity>();

        int inserted = 0;
        int updated = 0;

        foreach (var req in requests)
        {
            if (string.IsNullOrWhiteSpace(req.LocationName) || string.IsNullOrWhiteSpace(req.Reason))
            {
                continue;
            }

            var locationName = req.LocationName.Trim();
            var reason = req.Reason.Trim().ToUpperInvariant();

            var existing = await dbSet.FirstOrDefaultAsync(x =>
                x.TenantId == tenantId &&
                x.WarehouseId == req.WarehouseId &&
                x.LocationName == locationName &&
                x.Reason == reason &&
                x.WmsHasPallet == req.WmsHasPallet &&
                x.Status != 3 && // 3 = Resolved (quy ước)
                x.TraceId == req.TraceId,
                cancellationToken);

            if (existing is null)
            {
                await dbSet.AddAsync(new LocationSyncConflictEntity
                {
                    TenantId = tenantId,
                    WarehouseId = req.WarehouseId,
                    LocationId = req.LocationId,
                    LocationName = locationName,
                    WcsStatus = req.WcsStatus,
                    WmsHasPallet = req.WmsHasPallet,
                    Reason = reason,
                    Status = 0, // New
                    CreatedTime = DateTime.UtcNow,
                    TraceId = req.TraceId

                }, cancellationToken);

                inserted++;
            }
            else
            {
                existing.WcsStatus = req.WcsStatus;
                existing.WmsHasPallet = req.WmsHasPallet;
                existing.Status = 1; // InProgress
                existing.ResolutionNote = null;
                existing.ResolvedBy = null;
                existing.ResolvedTime = null;
                existing.TraceId = req.TraceId;
                updated++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (inserted, updated);
    }

    /// <summary>
    /// Insert stock and handle logic for Resolve logic for conflict
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool success, string message)> ResolveWcsOnlyInboundAsync(ResolveWcsOnlyInboundRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var now = DateTime.UtcNow;
        var normalizedLocation = request.WcsLocation.Trim();

        var locationDbSet = _dbContext.GetDbSet<GoodslocationEntity>();
        var stockDbSet = _dbContext.GetDbSet<StockEntity>();
        var conflictDbSet = _dbContext.GetDbSet<LocationSyncConflictEntity>();
        var transactionDbSet = _dbContext.GetDbSet<StockTransactionEntity>();
        var supplierDbSet = _dbContext.GetDbSet<SupplierEntity>();

        var location = await locationDbSet
            .FirstOrDefaultAsync(x =>
                x.TenantId == tenantId &&
                x.WarehouseId == request.WarehouseId &&
                x.LocationName == normalizedLocation,
                cancellationToken);

        if (location is null)
        {
            return (false, $"Location '{normalizedLocation}' not found.");
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var groupedItems = request.Items
                .GroupBy(x => new
                {
                    x.SkuId,
                    ExpiryDate = x.ExpiryDate?.Date,
                    x.SupplierId
                })
                .Select(g => new
                {
                    g.Key.SkuId,
                    g.Key.ExpiryDate,
                    g.Key.SupplierId,
                    Qty = g.Sum(i => i.Qty),
                    Price = g.Select(i => i.Price).FirstOrDefault() ?? 0m
                })
                .Where(x => x.Qty > 0)
                .ToList();

            var skuIds = groupedItems.Select(x => x.SkuId).Distinct().ToList();
            var supplierIds = groupedItems
                            .Where(x => x.SupplierId.HasValue)
                            .Select(x => x.SupplierId!.Value)
                            .Distinct()
                            .ToList();
            var supplierLookup = supplierIds.Count == 0
           ? []
           : await supplierDbSet
               .Where(s => supplierIds.Contains(s.Id))
               .ToDictionaryAsync(s => s.Id, s => (string?)s.supplier_name, cancellationToken);

            var existingStocks = await stockDbSet
                 .Where(s =>
                     s.TenantId == tenantId &&
                     s.goods_location_id == location.Id &&
                     skuIds.Contains(s.sku_id) &&
                     (!s.SupplierId.HasValue || supplierIds.Contains(s.SupplierId.Value)))
                 .ToListAsync(cancellationToken);

            // var transactionLogs = new List<StockTransactionEntity>();
            var pendingLogs = new List<(StockEntity stock, decimal qty, int? supplierId, string? supplierName)>();
            foreach (var item in groupedItems)
            {
                var matched = existingStocks.FirstOrDefault(s =>
                  s.sku_id == item.SkuId &&
                  s.expiry_date == item.ExpiryDate &&
                  s.SupplierId == item.SupplierId);

                supplierLookup.TryGetValue(item.SupplierId ?? 0, out var supplierName);

                if (matched is null)
                {
                    var newStock = new StockEntity
                    {
                        sku_id = item.SkuId,
                        goods_location_id = location.Id,
                        qty = item.Qty,
                        actual_qty = item.Qty,
                        goods_owner_id = 0, // default value
                        is_freeze = false,
                        last_update_time = now,
                        TenantId = tenantId,
                        expiry_date = item.ExpiryDate,
                        PutAwayDate = now,
                        price = item.Price,
                        Palletcode = string.IsNullOrWhiteSpace(request.PalletCode) ? null : request.PalletCode.Trim()
                    };

                    await stockDbSet.AddAsync(newStock, cancellationToken);
                    existingStocks.Add(newStock);

                    //transactionLogs.Add(new StockTransactionEntity
                    //{
                    //    StockId = newStock.Id,
                    //    Quantity = item.Qty,
                    //    SkuId = newStock.sku_id,
                    //    TransactionType = StockTransactionType.Inbound,
                    //    SupplierId = item.SupplierId,
                    //    SupplierName = supplierName,
                    //    TenantId = tenantId,
                    //    RefReceipt = string.Empty,
                    //    TransactionDate = now
                    //});

                    pendingLogs.Add((newStock, item.Qty, item.SupplierId, supplierName));
                }
                else
                {
                    matched.qty += item.Qty;
                    matched.actual_qty += item.Qty;
                    matched.last_update_time = now;
                    matched.Palletcode = string.IsNullOrWhiteSpace(request.PalletCode)
                        ? matched.Palletcode
                        : request.PalletCode.Trim();

                    //transactionLogs.Add(new StockTransactionEntity
                    //{
                    //    StockId = matched.Id,
                    //    Quantity = item.Qty,
                    //    SkuId = matched.sku_id,
                    //    TransactionType = StockTransactionType.Inbound,
                    //    SupplierId = item.SupplierId,
                    //    SupplierName = supplierName,
                    //    TenantId = tenantId,
                    //    RefReceipt = string.Empty,
                    //    TransactionDate = now
                    //});

                    pendingLogs.Add((matched, item.Qty, item.SupplierId, supplierName));
                }
            }

            //if (transactionLogs.Count > 0)
            //{
            //    await transactionDbSet.AddRangeAsync(transactionLogs, cancellationToken);
            //}

            if (location.LocationStatus == (byte)GoodLocationStatusEnum.AVAILABLE)
            {
                location.LocationStatus = (byte)GoodLocationStatusEnum.OCCUPIED;
                location.LastUpdateTime = now;
            }

            var openConflicts = await conflictDbSet
                .Where(c =>
                    c.TenantId == tenantId &&
                    c.WarehouseId == request.WarehouseId &&
                    c.LocationName == normalizedLocation &&
                    c.Reason == "WCS_ONLY" &&
                    c.Status != 3)
                .ToListAsync(cancellationToken);

            foreach (var conflict in openConflicts)
            {
                conflict.Status = 3;
                conflict.ResolvedBy = currentUser.user_name;
                conflict.ResolvedTime = now;
                conflict.ResolutionNote = string.IsNullOrWhiteSpace(request.Note)
                    ? "Resolved by WCS_ONLY inbound flow."
                    : request.Note!.Trim();
                conflict.WmsHasPallet = true;
                conflict.WcsStatus = 1;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (pendingLogs.Count > 0)
            {
                // Waiting id from new Stock EFcore 
                var trans = pendingLogs.Select(x => new StockTransactionEntity
                {
                    StockId = x.stock.Id,
                    Quantity = x.qty,
                    SkuId = x.stock.sku_id,
                    TransactionType = StockTransactionType.Inbound,
                    SupplierId = x.supplierId,
                    SupplierName = x.supplierName,
                    TenantId = tenantId,
                    RefReceipt = string.Empty,
                    TransactionDate = now
                }).ToList();

                await transactionDbSet.AddRangeAsync(trans, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return (true, "Resolve WCS_ONLY inbound success.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "ResolveWcsOnlyInboundAsync failed for warehouseId={WarehouseId}, location={Location}",
                request.WarehouseId, request.WcsLocation);
            return (false, "Resolve WCS_ONLY inbound failed.");
        }
    }

    /// <summary>
    /// Resolve pallet merge conflict when sync location between WMS and WCS, if both WMS and WCS have pallet in the same location but different pallet code, or same pallet code but different status, then create a conflict to review and resolve. when resolve, need to merge stock into one pallet and update location status, also need to update wcs status to be consistent
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool success, string message)> ResolvePalletMergeSameLocationAsync(ResolvePalletMergeSameLocationRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (request.Items is null || request.Items.Count == 0)
            return (false, "No selected items to merge.");


        var tenantId = currentUser.tenant_id;
        var now = DateTime.UtcNow;

        var locationDbSet = _dbContext.GetDbSet<GoodslocationEntity>();
        var stockDbSet = _dbContext.GetDbSet<StockEntity>();
        var conflictDbSet = _dbContext.GetDbSet<LocationSyncConflictEntity>();
        var transactionDbSet = _dbContext.GetDbSet<StockTransactionEntity>();
        var supplierDbSet = _dbContext.GetDbSet<SupplierEntity>();

        var location = await locationDbSet.FirstOrDefaultAsync(x =>
                                           x.TenantId == tenantId &&
                                           x.WarehouseId == request.WarehouseId &&
                                           x.LocationName == request.LocationName,
                                           cancellationToken);

        if (location is null)
        {
            return (false, $"Location '{request.LocationName}' not found.");
        }


        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var sourceQuery = stockDbSet.Where(s => s.TenantId == tenantId &&
                                 s.goods_location_id == location.Id);

            if (!string.IsNullOrEmpty(request.WmsPalletCode))
            {
                sourceQuery = sourceQuery.Where(s => s.Palletcode == request.WmsPalletCode);
            }

            else
            {
                sourceQuery = sourceQuery.Where(s => s.Palletcode != request.TargetPalletCode);
            }

            var sourceStocks = await sourceQuery.ToListAsync(cancellationToken);
            var selectedStocks = new List<StockEntity>();

            foreach (var item in request.Items)
            {
                if (item.SkuId == null)
                    continue;

                var matches = sourceStocks.Where(s => s.sku_id == item.SkuId.Value &&
                                                 s.SupplierId == item.SupplierId &&
                                                 s.expiry_date == item.ExpiryDate);

                if (item.Qty.HasValue && item.Qty.Value > 0)
                {
                    matches = matches.Where(s => s.qty == item.Qty.Value);
                }

                foreach (var stock in matches)
                {
                    if (!selectedStocks.Any(x => x.Id == stock.Id))
                    {
                        selectedStocks.Add(stock);
                    }
                }
            }

            if (selectedStocks.Count == 0)
                return (false, "No matching stock found from selected items.");

            var selectedIds = selectedStocks.Select(x => x.Id).ToList();

            var targetStocks = await stockDbSet.Where(s =>
               s.TenantId == tenantId &&
               s.goods_location_id == location.Id &&
               s.Palletcode == request.TargetPalletCode &&
               !selectedIds.Contains(s.Id))
           .ToListAsync(cancellationToken);

            var supplierIds = selectedStocks
                              .Where(x => x.SupplierId.HasValue)
                              .Select(x => x.SupplierId!.Value)
                              .Distinct()
                              .ToList();

            var supplierLookup = await supplierDbSet
                                  .Where(x => supplierIds.Contains(x.Id))
                                  .ToDictionaryAsync(x => x.Id, x => x.supplier_name, cancellationToken);

            var transactionLog = new List<StockTransactionEntity>();
            foreach (var moving in selectedStocks)
            {
                if (moving.Palletcode == request.TargetPalletCode)
                {
                    continue;
                }

                var movedQty = (decimal)moving.qty;
                if (movedQty <= 0)
                {
                    continue;
                }

                supplierLookup.TryGetValue(moving.SupplierId ?? 0, out var supplierName);

                var target = targetStocks.FirstOrDefault(t =>
                                             t.sku_id == moving.sku_id &&
                                             t.SupplierId == moving.SupplierId &&
                                             t.expiry_date == moving.expiry_date);

                if (target is null)
                {
                    transactionLog.Add(new StockTransactionEntity
                    {
                        StockId = moving.Id,
                        Quantity = -movedQty,
                        SkuId = moving.sku_id,
                        TransactionType = StockTransactionType.Outbound,
                        SupplierId = moving.SupplierId,
                        SupplierName = supplierName,
                        TenantId = tenantId,
                        RefReceipt = string.Empty, // not Ref
                        TransactionDate = now
                    });

                    moving.Palletcode = request.TargetPalletCode;
                    moving.last_update_time = now;
                    targetStocks.Add(moving);

                    transactionLog.Add(new StockTransactionEntity
                    {
                        StockId = moving.Id,
                        Quantity = movedQty,
                        SkuId = moving.sku_id,
                        TransactionType = StockTransactionType.Inbound,
                        SupplierId = moving.SupplierId,
                        SupplierName = supplierName,
                        TenantId = tenantId,
                        RefReceipt = string.Empty,
                        TransactionDate = now
                    });
                }

                else
                {
                    transactionLog.Add(new StockTransactionEntity
                    {
                        StockId = moving.Id,
                        Quantity = -movedQty,
                        SkuId = moving.sku_id,
                        TransactionType = StockTransactionType.Outbound,
                        SupplierId = moving.SupplierId,
                        SupplierName = supplierName,
                        TenantId = tenantId,
                        RefReceipt = string.Empty,
                        TransactionDate = now
                    });

                    // inbound to target
                    target.qty += moving.qty;
                    target.actual_qty += moving.actual_qty;
                    target.last_update_time = now;


                    transactionLog.Add(new StockTransactionEntity
                    {
                        StockId = target.Id,
                        Quantity = movedQty,
                        SkuId = target.sku_id,
                        TransactionType = StockTransactionType.Inbound,
                        SupplierId = target.SupplierId,
                        SupplierName = supplierName,
                        TenantId = tenantId,
                        RefReceipt = string.Empty,
                        TransactionDate = now,
                    });

                    stockDbSet.Remove(moving);
                }
            }

            if (transactionLog.Count > 0)
            {
                await transactionDbSet.AddRangeAsync(transactionLog, cancellationToken);
            }

            if (location.LocationStatus == (byte)GoodLocationStatusEnum.AVAILABLE)
            {
                location.LocationStatus = (byte)GoodLocationStatusEnum.OCCUPIED;
                location.LastUpdateTime = now;
            }

            var openConflicts = await conflictDbSet.Where(c =>
                            c.TenantId == tenantId &&
                            c.WarehouseId == request.WarehouseId &&
                            c.LocationName == request.LocationName &&
                            c.Reason == "PALLET_CODE_MERGE_CANDIDATE" &&
                            c.Status != 3)
                        .ToListAsync(cancellationToken);

            foreach (var conflict in openConflicts)
            {
                conflict.Status = 3;
                conflict.ResolvedBy = currentUser.user_name;
                conflict.ResolvedTime = now;
                conflict.WmsHasPallet = true;
                conflict.WcsStatus = 1;
                conflict.ResolutionNote = string.IsNullOrEmpty(request.Note)
                    ? $"Merged selected stocks to pallet '{request.TargetPalletCode}'."
                    : request.Note;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return (true, "Resolve pallet merge same location success.");
        }
        catch (Exception ex)
        {

            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "ResolvePalletMergeSameLocationAsync failed. WarehouseId={WarehouseId}, Location={LocationName}",
                request.WarehouseId, request.LocationName);

            return (false, "Resolve pallet merge same location failed.");
        }
    }

    /// <summary>
    /// Resolve WMS only clear location conflict
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool success, string message)> ResolveWmsOnlyClearLocationAsync(ResolveWmsOnlyClearLocationRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (request.WarehouseId <= 0 || string.IsNullOrWhiteSpace(request.LocationName))
        {
            return (false, "Invalid request.");
        }

        var tenantId = currentUser.tenant_id;
        var now = DateTime.UtcNow;

        var locationDbSet = _dbContext.GetDbSet<GoodslocationEntity>(tenantId, true);
        var stockDbSet = _dbContext.GetDbSet<StockEntity>(tenantId);
        var conflictDbSet = _dbContext.GetDbSet<LocationSyncConflictEntity>(tenantId, true);
        var supplierDbSet = _dbContext.GetDbSet<SupplierEntity>(tenantId);

        var location = await locationDbSet.FirstOrDefaultAsync(x =>
            x.WarehouseId == request.WarehouseId &&
            x.LocationName == request.LocationName, cancellationToken);

        if (location is null)
            return (false, $"Location '{request.LocationName}' not found.");

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var stocksAtLocation = await stockDbSet
                .Where(s => s.goods_location_id == location.Id)
                .ToListAsync(cancellationToken);

            var supplierIds = stocksAtLocation
                    .Where(s => s.SupplierId.HasValue)
                    .Select(s => s.SupplierId!.Value)
                    .Distinct()
                    .ToList();

            var supplierLookup = supplierIds.Count == 0
                ? new Dictionary<int, string?>()
                : await supplierDbSet
                    .Where(x => supplierIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => (string?)x.supplier_name, cancellationToken);

            var outboundLogs = stocksAtLocation
                .Where(s => s.qty > 0)
                .Select(s => new StockTransactionEntity
                {
                    StockId = s.Id,
                    Quantity = -(decimal)s.qty,
                    SkuId = s.sku_id,
                    TransactionType = StockTransactionType.Outbound,
                    SupplierId = s.SupplierId,
                    SupplierName = s.SupplierId.HasValue && supplierLookup.ContainsKey(s.SupplierId.Value)
                    ? supplierLookup[s.SupplierId.Value] ?? string.Empty
                    : string.Empty,
                    TenantId = tenantId,
                    RefReceipt = string.Empty,
                    TransactionDate = now,
                })
                .ToList();

            if (outboundLogs.Count > 0)
                await _dbContext.GetDbSet<StockTransactionEntity>().AddRangeAsync(outboundLogs, cancellationToken);

            if (stocksAtLocation.Count > 0)
            {
                _dbContext.GetDbSet<StockEntity>().RemoveRange(stocksAtLocation);
            }

            location.LocationStatus = (byte)GoodLocationStatusEnum.AVAILABLE;
            location.LastUpdateTime = now;

            var openConflicts = await conflictDbSet
                .Where(c => c.TenantId == tenantId
                         && c.WarehouseId == request.WarehouseId
                         && c.LocationName == request.LocationName
                         && c.Reason == "WMS_ONLY"
                         && c.Status != 3)
                .ToListAsync(cancellationToken);

            foreach (var conflict in openConflicts)
            {
                conflict.Status = 3;
                conflict.ResolvedBy = currentUser.user_name;
                conflict.ResolvedTime = now;
                conflict.WmsHasPallet = false;
                conflict.WcsStatus = 0;
                conflict.ResolutionNote = string.IsNullOrWhiteSpace(request.Note)
                    ? "Resolved by clearing all WMS stock at location."
                    : request.Note;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return (true, "Cleared all stock at location successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "ResolveWmsOnlyClearLocationAsync failed. WarehouseId={WarehouseId}, Location={Location}",
                request.WarehouseId, request.LocationName);
            return (false, "Failed to clear stock at location.");
        }
    }

    /// <summary>
    /// Handle for resolve location mismatch
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool success, string message)> ResolveLocationMismatchAsync(ResolveLocationMismatchRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (request.WarehouseId <= 0 ||
          string.IsNullOrWhiteSpace(request.PalletCode) ||
          string.IsNullOrWhiteSpace(request.WmsLocation) ||
          string.IsNullOrWhiteSpace(request.WcsLocation))
        {
            return (false, "Invalid request.");
        }

        var tenantId = currentUser.tenant_id;
        var now = DateTime.UtcNow;
        var fromName = request.WmsLocation;
        var toName = request.WcsLocation;

        var locationQuery = _dbContext.GetDbSet<GoodslocationEntity>(tenantId, true);
        var stockQuery = _dbContext.GetDbSet<StockEntity>(tenantId, true);
        var conflictQuery = _dbContext.GetDbSet<LocationSyncConflictEntity>(tenantId, true);

        var fromLocation = await locationQuery.FirstOrDefaultAsync(
            x => x.WarehouseId == request.WarehouseId && x.LocationName == fromName && x.IsValid, cancellationToken);


        var toLocation = await locationQuery.FirstOrDefaultAsync(
            x => x.WarehouseId == request.WarehouseId && x.LocationName == toName && x.IsValid, cancellationToken);

        if (fromLocation is null || toLocation is null)
        {
            return (false, "Source or target location not found.");
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var movingStocks = await stockQuery
                                      .Where(s => s.goods_location_id == fromLocation.Id && s.Palletcode == request.PalletCode)
                                      .ToListAsync(cancellationToken);


            if (movingStocks.Count == 0)
            {
                return (false, $"No stock found for pallet '{request.PalletCode}' at location '{fromName}'.");
            }

            var supplierDbSet = _dbContext.GetDbSet<SupplierEntity>(tenantId);
            var transactionDbSet = _dbContext.GetDbSet<StockTransactionEntity>();

            var supplierIds = movingStocks
                            .Where(s => s.SupplierId.HasValue)
                            .Select(s => s.SupplierId!.Value)
                            .Distinct()
                            .ToList();

            var supplierLookup = supplierIds.Count == 0
                ? new Dictionary<int, string?>()
                : await supplierDbSet
                    .Where(x => supplierIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => (string?)x.supplier_name, cancellationToken);

            var txLogs = new List<StockTransactionEntity>();

            foreach (var stock in movingStocks)
            {
                var qty = (decimal)stock.qty;
                if (qty <= 0)
                {
                    continue;
                }

                supplierLookup.TryGetValue(stock.SupplierId ?? 0, out var supplierName);

                // log for outbound from source location
                txLogs.Add(new StockTransactionEntity
                {
                    StockId = stock.Id,
                    SkuId = stock.sku_id,
                    Quantity = -qty,
                    TransactionType = StockTransactionType.Outbound,
                    SupplierId = stock.SupplierId,
                    SupplierName = supplierName,
                    TenantId = tenantId,
                    RefReceipt = string.Empty,
                    TransactionDate = now
                });

                stock.goods_location_id = toLocation.Id; // set from -> to
                stock.last_update_time = now;

                // log for inbound to target location
                txLogs.Add(new StockTransactionEntity
                {
                    StockId = stock.Id,
                    SkuId = stock.sku_id,
                    Quantity = qty,
                    TransactionType = StockTransactionType.Inbound,
                    SupplierId = stock.SupplierId,
                    SupplierName = supplierName,
                    TenantId = tenantId,
                    RefReceipt = string.Empty,
                    TransactionDate = now
                });
            }

            if (txLogs.Count > 0)
            {
                await transactionDbSet.AddRangeAsync(txLogs, cancellationToken);
            }

            var movingIds = movingStocks.Select(x => x.Id).ToList();
            var sourceStillHasStock = await stockQuery.AnyAsync(s => s.goods_location_id == fromLocation.Id
                                                                && !movingIds.Contains(s.Id), cancellationToken);

            fromLocation.LocationStatus = sourceStillHasStock
                                         ? (byte)GoodLocationStatusEnum.OCCUPIED
                                         : (byte)GoodLocationStatusEnum.AVAILABLE;

            fromLocation.LastUpdateTime = now;

            toLocation.LocationStatus = (byte)GoodLocationStatusEnum.OCCUPIED;
            toLocation.LastUpdateTime = now;

            var openConflicts = await conflictQuery
                                 .Where(c => c.WarehouseId == request.WarehouseId
                                             && c.LocationName == fromName
                                             && c.Reason == "LOCATION_MISMATCH"
                                             && c.Status != 3)
                                 .ToListAsync(cancellationToken);

            foreach (var conflict in openConflicts)
            {
                conflict.Status = 3;
                conflict.ResolvedBy = currentUser.user_name;
                conflict.ResolvedTime = now;
                conflict.ResolutionNote = string.IsNullOrWhiteSpace(request.Note)
                    ? $"Moved pallet '{request.PalletCode}' from '{fromName}' to '{toName}'."
                    : request.Note;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return (true, "Resolve LOCATION_MISMATCH success.");


        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "ResolveLocationMismatchAsync failed. WarehouseId={WarehouseId}, Pallet={PalletCode}",
                request.WarehouseId, request.PalletCode);
            return (false, "Resolve LOCATION_MISMATCH failed.");
        }
    }

    /// <summary>
    /// List log of location sync between WMS and WCS, include both success and failed log, also include the conflict log with detail info, for review and analysis. support filter by warehouse and date range, also support pagination. for conflict log, also need to show the related stock and location info for analysis. also need to support export log to excel for further analysis.
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<LocationSyncLogItemDto>> GetLocationSyncLogsAsync(int warehouseId, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (warehouseId <= 0)
        {
            return [];
        }

        var tenantId = currentUser.tenant_id;

        var logs = await _dbContext.GetDbSet<LocationSyncConflictLogEntity>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                     && x.WarehouseId == warehouseId
                     && x.Status == ConflictStatus.Pending)
            .OrderByDescending(x => x.ActionTime ?? x.ReceivedTime)
            .Select(x => new LocationSyncLogItemDto
            {
                TraceId = x.TraceId,
                ActionTime = x.ActionTime,
                ReceivedTime = x.ReceivedTime,
                ConflictInserted = x.ConflictInserted,
                ConflictUpdated = x.ConflictUpdated,
                Status = x.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return logs;
    }

    /// <summary>
    /// List conflict log of location sync between WMS and WCS, include the conflict reason, conflict status, also include the related stock and location info for analysis. support filter by warehouse and date range, also support pagination. also need to support export log to excel for further analysis.
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<LocationSyncConflictKeyDto>> GetLocationSyncConflictsByTraceIdAsync(string traceId, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(traceId))
        {
            return [];
        }

        var tenantId = currentUser.tenant_id;
        var normalizedTraceId = traceId.Trim();

        var conflicts = await _dbContext.GetDbSet<LocationSyncConflictEntity>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                     && x.TraceId == normalizedTraceId
                     && x.Status != 3)
            .OrderBy(x => x.LocationName)
            .Select(x => new LocationSyncConflictKeyDto
            {
                TraceId = x.TraceId,
                LocationName = x.LocationName,
                Reason = x.Reason ?? string.Empty,
                WmsHasPallet = x.WmsHasPallet,
                WcsStatus = x.WcsStatus
            })
            .ToListAsync(cancellationToken);

        return conflicts;
    }

    /// <summary>
    /// Cancel conflict
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <param name="currentTraceId"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(int canceledLogs, int deletedConflicts)> CancelPreviousLogsAndDeleteConflictsAsync(int warehouseId, string currentTraceId, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (warehouseId <= 0 || string.IsNullOrWhiteSpace(currentTraceId))
            return (0, 0);

        var tenantId = currentUser.tenant_id;
        var now = DateTime.UtcNow;

        var logDbSet = _dbContext.GetDbSet<LocationSyncConflictLogEntity>();
        var conflictDbSet = _dbContext.GetDbSet<LocationSyncConflictEntity>();

        var currentLog = await logDbSet.FirstOrDefaultAsync(x =>
            x.TenantId == tenantId &&
            x.WarehouseId == warehouseId &&
            x.TraceId == currentTraceId,
            cancellationToken);

        if (currentLog is null)
            return (0, 0);

        var pivotTime = currentLog.ActionTime;

        var prevLogs = await logDbSet
            .Where(x => x.TenantId == tenantId
                     && x.WarehouseId == warehouseId
                     && x.TraceId != currentTraceId
                     && x.Status != ConflictStatus.Cancelled
                     && x.Status != ConflictStatus.Failed
                     && (x.ActionTime ?? x.ReceivedTime) < pivotTime)
            .Select(x => new { x.Id, x.TraceId })
            .ToListAsync(cancellationToken);

        if (prevLogs.Count == 0)
            return (0, 0);

        var prevTraceIds = prevLogs.Select(x => x.TraceId).Distinct().ToList();

        var deletedConflicts = await conflictDbSet
            .Where(x => x.TenantId == tenantId && prevTraceIds.Contains(x.TraceId))
            .ExecuteDeleteAsync(cancellationToken);

        var canceledLogs = await logDbSet
            .Where(x => x.TenantId == tenantId
                     && x.WarehouseId == warehouseId
                     && prevTraceIds.Contains(x.TraceId))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, ConflictStatus.Cancelled)
                .SetProperty(x => x.ErrorMessage, $"Cancelled by editing traceId={currentTraceId}")
                .SetProperty(x => x.CompletedTime, now),
                cancellationToken);

        return (canceledLogs, deletedConflicts);
    }
}


#endregion
