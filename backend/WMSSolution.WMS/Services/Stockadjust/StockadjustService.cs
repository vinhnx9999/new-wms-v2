
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Shared.Enums;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Stockadjust;
using WMSSolution.WMS.IServices.Stockadjust;

namespace WMSSolution.WMS.Services.Stockadjust;

/// <summary>
///  Stockadjust Service
/// </summary>
/// <param name="dBContext">The DBContext</param>
/// <param name="stringLocalizer">Localizer</param>
/// <param name="logger">Logger</param>
public class StockadjustService(SqlDBContext dBContext
      , IStringLocalizer<MultiLanguage> stringLocalizer
      , ILogger<StockadjustService> logger
        ) : BaseService<StockadjustEntity>, IStockadjustService
{
    #region Args

    /// <summary>
    /// The DBContext
    /// </summary>
    private readonly SqlDBContext _dbContext = dBContext;

    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;
    private readonly ILogger<StockadjustService> _logger = logger;

    #endregion Args

    #region Api

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(List<StockadjustViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count != 0)
        {
            pageSearch.searchObjects.ForEach(s =>
            {
                queries.Add(s);
            });
        }

        var adjustQuery = _dbContext.GetDbSet<StockadjustEntity>(currentUser.tenant_id);
        var spuQuery = _dbContext.GetDbSet<SpuEntity>(currentUser.tenant_id);
        var skuQuery = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var ownerQuery = _dbContext.GetDbSet<GoodsownerEntity>().AsNoTracking();
        var locationQuery = _dbContext.GetDbSet<GoodslocationEntity>(currentUser.tenant_id);

        var query = from sj in adjustQuery
                    join sku in skuQuery on sj.sku_id equals sku.Id
                    join spu in spuQuery on sku.spu_id equals spu.Id
                    join gsl in locationQuery on sj.goods_location_id equals gsl.Id
                    join gso in ownerQuery on sj.goods_owner_id equals gso.Id into gsoJoin
                    from gso in gsoJoin.DefaultIfEmpty()
                    select new StockadjustViewModel
                    {
                        id = sj.Id,
                        job_code = sj.job_code,
                        is_update_stock = sj.is_update_stock,
                        job_type = sj.job_type,
                        qty = sj.qty,
                        source_table_id = sj.source_table_id,
                        tenant_id = sj.TenantId,
                        sku_id = sku.Id,
                        sku_code = sku.sku_code,
                        sku_name = sku.sku_name,
                        spu_code = spu.spu_code,
                        spu_name = spu.spu_name,
                        goods_location_id = sj.goods_location_id,
                        WarehouseName = gsl.WarehouseName,
                        LocationName = gsl.LocationName,
                        goods_owner_id = sj.goods_owner_id,
                        goods_owner_name = gso.goods_owner_name == null ? string.Empty : gso.goods_owner_name,
                        creator = sj.creator,
                        create_time = sj.create_time,
                        last_update_time = sj.last_update_time,
                        series_number = sj.series_number,
                        expiry_date = sj.expiry_date,
                        price = sj.price,
                        putaway_date = sj.putaway_date,
                    };

        query = query.Where(queries.AsExpression<StockadjustViewModel>());
        int totals = await query.CountAsync();
        var list = await query.OrderByDescending(t => t.create_time)
                   .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                   .Take(pageSearch.pageSize)
                   .ToListAsync();

        return (list, totals);
    }

    /// <summary>
    /// Get all records
    /// </summary>
    /// <returns></returns>
    public async Task<List<StockadjustViewModel>> GetAllAsync(CurrentUser currentUser)
    {
        var data = await _dbContext.GetDbSet<StockadjustEntity>(currentUser.tenant_id)
            .ToListAsync();

        return data.Adapt<List<StockadjustViewModel>>();
    }

    /// <summary>
    /// Get a record by id
    /// </summary>
    /// <returns></returns>
    public async Task<StockadjustViewModel?> GetAsync(int id)
    {
        var entity = await _dbContext.GetDbSet<StockadjustEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id.Equals(id));

        return entity?.Adapt<StockadjustViewModel>();
    }

    /// <summary>
    /// Get By Processing Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<StockadjustViewModel?> GetByProcessingIdAsync(int id)
    {
        var processDetail = await _dbContext.GetDbSet<StockprocessdetailEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.stock_process_id.Equals(id) && t.is_source == false);

        if (processDetail == null)
        {
            return null;
        }

        var entity = await _dbContext.GetDbSet<StockadjustEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.source_table_id.Equals(processDetail.Id));

        return entity?.Adapt<StockadjustViewModel>();
    }

    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<(int total, string msg)> AddAsync(StockAdjustRequest viewModel, CurrentUser currentUser)
    {
        string shareJobCode = await GenerateJobCode();
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        {
            var listEntities = new List<StockadjustEntity>();
            foreach (var items in viewModel.items)
            {
                var entity = items.Adapt<StockadjustEntity>();
                entity.Id = 0;
                entity.create_time = DateTime.UtcNow;
                entity.creator = currentUser.user_name;
                entity.last_update_time = DateTime.UtcNow;
                entity.TenantId = currentUser.tenant_id;
                entity.job_code = shareJobCode;
                listEntities.Add(entity);
            }

            await _dbContext.GetDbSet<StockadjustEntity>().AddRangeAsync(listEntities);

            var total = await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return total <= 0 ? (0, _stringLocalizer["save_failed"]) :
                (total, _stringLocalizer["save_success"]);
        }
    }

    /// <summary>
    /// update a record
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> UpdateAsync(StockadjustViewModel viewModel)
    {
        var entity = await _dbContext
            .GetDbSet<StockadjustEntity>()
            .FirstOrDefaultAsync(t => t.Id.Equals(viewModel.id));

        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }

        entity.Id = viewModel.id;
        entity.job_code = viewModel.job_code;
        entity.sku_id = viewModel.sku_id;
        entity.goods_owner_id = viewModel.goods_owner_id;
        entity.goods_location_id = viewModel.goods_location_id;
        entity.qty = viewModel.qty;
        entity.is_update_stock = viewModel.is_update_stock;
        entity.job_type = viewModel.job_type;
        entity.source_table_id = viewModel.source_table_id;
        entity.last_update_time = DateTime.UtcNow;
        entity.series_number = viewModel.series_number;
        entity.expiry_date = viewModel.expiry_date;
        entity.price = viewModel.price;
        entity.putaway_date = viewModel.putaway_date;

        var saved = await _dbContext.SaveChangesAsync();
        return saved > 0 ?
            (true, _stringLocalizer["save_success"]) :
            (false, _stringLocalizer["save_failed"]);
    }

    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> DeleteAsync(int id)
    {
        var dbSet = _dbContext.GetDbSet<StockadjustEntity>();
        var entity = await dbSet.Where(t => t.Id == id)
            .FirstOrDefaultAsync();

        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }

        dbSet.Remove(entity);
        var deleted = await _dbContext.SaveChangesAsync();

        return deleted > 0 ?
            (true, _stringLocalizer["delete_success"]) :
            (false, _stringLocalizer["delete_failed"]);
    }

    /// <summary>
    /// confirm adjustment
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ConfirmAdjustment(int id)
    {
        var stockAdjustEntity = await _dbContext.GetDbSet<StockadjustEntity>()
            .FirstOrDefaultAsync(t => t.Id == id);
        if (stockAdjustEntity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }

        if (stockAdjustEntity.job_type == 2)
        {
            var processdetail_DBSet = _dbContext.GetDbSet<StockprocessdetailEntity>();
            var processdetail = await processdetail_DBSet
                .Where(t => t.Id == stockAdjustEntity.source_table_id)
                .FirstOrDefaultAsync();

            if (processdetail != null)
            {
                processdetail.last_update_time = DateTime.UtcNow;
                processdetail.is_update_stock = true;
            }
        }

        var stockDbSet = _dbContext.GetDbSet<StockEntity>();
        var stock = await stockDbSet
            .Where(t => t.goods_owner_id == stockAdjustEntity.goods_owner_id
                && t.series_number == stockAdjustEntity.series_number
                && t.goods_location_id == stockAdjustEntity.goods_location_id
                && t.sku_id == stockAdjustEntity.sku_id
                && t.expiry_date == stockAdjustEntity.expiry_date
                && t.price == stockAdjustEntity.price
                && t.PutAwayDate == stockAdjustEntity.putaway_date)
            .FirstOrDefaultAsync();

        if (stock == null)
        {
            stock = new StockEntity
            {
                Id = stockAdjustEntity.Id,
                sku_id = stockAdjustEntity.sku_id,
                goods_location_id = stockAdjustEntity.goods_location_id,
                qty = stockAdjustEntity.qty,
                goods_owner_id = stockAdjustEntity.goods_owner_id,
                series_number = stockAdjustEntity.series_number,
                expiry_date = stockAdjustEntity.expiry_date,
                price = stockAdjustEntity.price,
                PutAwayDate = stockAdjustEntity.putaway_date,
                is_freeze = false,
                last_update_time = DateTime.UtcNow,
                TenantId = stockAdjustEntity.TenantId,
            };
        }
        else
        {
            stock.qty += stockAdjustEntity.qty;
            stock.goods_owner_id = stockAdjustEntity.goods_owner_id;
            stock.last_update_time = DateTime.UtcNow;
        }

        stockAdjustEntity.is_update_stock = true;
        stockAdjustEntity.last_update_time = DateTime.UtcNow;
        var res = await _dbContext.SaveChangesAsync();
        return res > 0 ?
            (true, _stringLocalizer["operation_success"]) :
            (false, _stringLocalizer["operation_failed"]);
    }

    #endregion Api

    /// <summary>   
    /// private extension methods
    /// </summary>
    /// <returns></returns>s
    private async Task<string> GenerateJobCode()
    {
        string prefix = "SA";
        string datePart = DateTime.UtcNow.ToString("yyMMdd");
        string prefixWithDate = $"{prefix}-{datePart}";
        var lastJob = await _dbContext.GetDbSet<StockadjustEntity>()
            .AsNoTracking()
            .Where(x => x.job_code.StartsWith(prefixWithDate))
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        int nextSequence = 1;

        if (lastJob != null && !string.IsNullOrEmpty(lastJob.job_code))
        {
            var parts = lastJob.job_code.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int currentSequence))
            {
                nextSequence = currentSequence + 1;
            }
        }

        return $"{prefixWithDate}-{nextSequence.ToString("D4")}";
    }

    /// <summary>
    /// Get Stock Sources
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <returns></returns>
    public async Task<(List<StockSourceSelectionViewModel> data, int totals)> GetStockSourcesForChangeRequestAsync(PageSearch pageSearch)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count != 0)
        {
            pageSearch.searchObjects.ForEach(queries.Add);
        }

        var stockDbSet = _dbContext.GetDbSet<StockEntity>().AsNoTracking();
        var locationDbSet = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
        var dispatchPickDbSet = _dbContext.GetDbSet<DispatchpicklistEntity>().AsNoTracking();
        var dispatchDbSet = _dbContext.GetDbSet<DispatchlistEntity>().AsNoTracking();
        var ownerDbSet = _dbContext.GetDbSet<GoodsownerEntity>().AsNoTracking();
        var skuDbSet = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var spuDbSet = _dbContext.GetDbSet<SpuEntity>().AsNoTracking();
        var processDb = _dbContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();

        var dispatch_lock = from dp in dispatchDbSet
                            join dpp in dispatchPickDbSet on dp.Id equals dpp.dispatchlist_id
                            where dp.dispatch_status > 1 && dp.dispatch_status < 6
                            group dpp by new { dpp.sku_id, dpp.goods_location_id } into g
                            select new { key = g.Key, qty_locked = g.Sum(x => x.pick_qty) };

        var process_lock = from pd in processDb
                           where pd.is_update_stock == false && pd.is_source == true
                           group pd by new { pd.sku_id, pd.goods_location_id } into g
                           select new { key = g.Key, qty_locked = g.Sum(x => x.qty) };

        var move_lock = from m in _dbContext.GetDbSet<StockmoveEntity>().AsNoTracking()
                        where m.move_status == 0
                        group m by new { m.sku_id, m.orig_goods_location_id } into g
                        select new { key = g.Key, qty_locked = g.Sum(x => x.qty) };

        var stock_query = from s in stockDbSet
                          join gl in locationDbSet on s.goods_location_id equals gl.Id
                          join sku in skuDbSet on s.sku_id equals sku.Id
                          join spu in spuDbSet on sku.spu_id equals spu.Id
                          join owner in ownerDbSet on s.goods_owner_id equals owner.Id into owner_join
                          from owner in owner_join.DefaultIfEmpty()
                          where s.qty > 0
                                && s.is_freeze == false
                                && gl.WarehouseAreaProperty != 5

                          group s by new
                          {
                              s.sku_id,
                              s.goods_location_id,
                              s.series_number,
                              s.expiry_date,
                              s.PutAwayDate,
                              s.goods_owner_id,
                              s.price,
                              goods_owner_name = owner.goods_owner_name,
                              sku.sku_code,
                              sku.sku_name,
                              sku.unit,
                              spu.spu_code,
                              spu.spu_name,
                              gl.LocationName,
                              gl.WarehouseName
                          } into sg


                          select new StockSourceSelectionViewModel
                          {
                              sku_id = sg.Key.sku_id,
                              sku_code = sg.Key.sku_code,
                              sku_name = sg.Key.sku_name,
                              unit = sg.Key.unit,
                              spu_code = sg.Key.spu_code,
                              spu_name = sg.Key.spu_name,
                              goods_location_id = sg.Key.goods_location_id,
                              LocationName = sg.Key.LocationName,
                              WarehouseName = sg.Key.WarehouseName,
                              series_number = sg.Key.series_number,
                              expiry_date = sg.Key.expiry_date,
                              putaway_date = sg.Key.PutAwayDate,
                              price = sg.Key.price,
                              goods_owner_id = sg.Key.goods_owner_id,
                              goods_owner_name = sg.Key.goods_owner_name,

                              qty_total = sg.Sum(x => x.qty),
                              qty_locked = 0,
                              qty_available = 0
                          };


        stock_query = stock_query.Where(queries.AsExpression<StockSourceSelectionViewModel>());

        int totals = await stock_query.CountAsync();

        var paged_query = stock_query.OrderBy(x => x.sku_code)
                                     .ThenBy(x => x.LocationName)
                                     .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                                     .Take(pageSearch.pageSize);

        var final_query = from s in paged_query

                          join dl in dispatch_lock on new { s.sku_id, s.goods_location_id } equals dl.key into dl_left
                          from dl in Enumerable.DefaultIfEmpty(dl_left)

                          join pl in process_lock on new { s.sku_id, s.goods_location_id } equals pl.key into pl_left
                          from pl in Enumerable.DefaultIfEmpty(pl_left)

                          join ml in move_lock on new { s.sku_id, s.goods_location_id } equals new { ml.key.sku_id, goods_location_id = ml.key.orig_goods_location_id } into ml_left
                          from ml in Enumerable.DefaultIfEmpty(ml_left)

                          select new StockSourceSelectionViewModel
                          {
                              sku_id = s.sku_id,
                              sku_code = s.sku_code,
                              sku_name = s.sku_name,
                              unit = s.unit,
                              spu_code = s.spu_code,
                              spu_name = s.spu_name,
                              goods_location_id = s.goods_location_id,
                              LocationName = s.LocationName,
                              WarehouseName = s.WarehouseName,
                              series_number = s.series_number,
                              expiry_date = s.expiry_date,
                              putaway_date = s.putaway_date,
                              price = s.price,
                              goods_owner_id = s.goods_owner_id,
                              goods_owner_name = s.goods_owner_name,

                              qty_total = s.qty_total,

                              qty_locked = ((int?)dl.qty_locked ?? 0)
                                            + ((int?)pl.qty_locked ?? 0)
                                            + ((int?)ml.qty_locked ?? 0),

                              qty_available = s.qty_total - (
                                                    ((int?)dl.qty_locked ?? 0) +
                                                    ((int?)pl.qty_locked ?? 0) +
                                                    ((int?)ml.qty_locked ?? 0)
                                                           )
                          };

        var result = await final_query.ToListAsync();

        return (result, totals);
    }

    /// <summary>
    /// Get Stock Sources with detailed Lock info for Adjustment Selection
    /// handle selection of SKUs for stock adjustment
    /// </summary>
    public async Task<(List<SkuAdjustmentSelectionViewModel> data, int totals)> GetSkuForAdjustmentSelectionAsync(PageSearch pageSearch)
    {
        try
        {
            QueryCollection queries = [];
            if (pageSearch.searchObjects.Count != 0)
            {
                pageSearch.searchObjects.ForEach(s =>
                {
                    queries.Add(s);
                });
            }

            var stockDBSet = _dbContext.GetDbSet<StockEntity>().AsNoTracking();
            var location_DBSet = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
            var dispatchpick_DBSet = _dbContext.GetDbSet<DispatchpicklistEntity>().AsNoTracking();
            var dispatch_DBSet = _dbContext.GetDbSet<DispatchlistEntity>().AsNoTracking();
            var owner_DBSet = _dbContext.GetDbSet<GoodsownerEntity>().AsNoTracking();
            var sku_DBSet = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
            var spu_DBSet = _dbContext.GetDbSet<SpuEntity>().AsNoTracking();
            var processdetail_DBSet = _dbContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();
            var move_DBSet = _dbContext.GetDbSet<StockmoveEntity>().AsNoTracking();

            // 1. Calculate Dispatch Lock (Outbound)
            var dispatch_lock = from dp in dispatch_DBSet
                                join dpp in dispatchpick_DBSet on dp.Id equals dpp.dispatchlist_id
                                where dp.dispatch_status > 1 && dp.dispatch_status < 6
                                group dpp by new { dpp.sku_id, dpp.goods_location_id, dpp.goods_owner_id, dpp.series_number, dpp.expiry_date, dpp.price, dpp.putaway_date } into g
                                select new { key = g.Key, qty_locked = g.Sum(x => x.pick_qty) };

            // 2. Calculate Process Lock (Pending Adjustments)
            var process_lock = from pd in processdetail_DBSet
                               where pd.is_update_stock == false && pd.is_source == true
                               group pd by new { pd.sku_id, pd.goods_location_id, pd.goods_owner_id, pd.series_number, pd.expiry_date, pd.price, pd.putaway_date } into g
                               select new { key = g.Key, qty_locked = g.Sum(x => x.qty) };

            // 3. Calculate Move Lock (Pending Moves)
            var move_lock = from m in move_DBSet
                            where m.move_status == 0
                            group m by new { m.sku_id, goods_location_id = m.orig_goods_location_id, m.goods_owner_id, m.series_number, m.expiry_date, m.price, m.putaway_date } into g
                            select new { key = g.Key, qty_locked = g.Sum(x => x.qty) };

            // 4. Main Query (Stock + Master Data)
            var stock_query = from s in stockDBSet
                              join gl in location_DBSet on s.goods_location_id equals gl.Id
                              join sku in sku_DBSet on s.sku_id equals sku.Id
                              join spu in spu_DBSet on sku.spu_id equals spu.Id
                              join owner in owner_DBSet on s.goods_owner_id equals owner.Id into owner_join
                              from owner in owner_join.DefaultIfEmpty()

                              where s.qty > 0
                                    && s.is_freeze == false
                                    && gl.WarehouseAreaProperty != 5

                              group s by new
                              {
                                  s.sku_id,
                                  s.goods_location_id,
                                  s.series_number,
                                  s.expiry_date,
                                  PutAwayDate = s.PutAwayDate.Date,
                                  s.goods_owner_id,
                                  s.price,
                                  owner.goods_owner_name,
                                  sku.sku_code,
                                  sku.sku_name,
                                  sku.unit,
                                  spu.spu_code,
                                  spu.spu_name,
                                  gl.LocationName,
                                  gl.WarehouseName
                              } into sg

                              select new SkuAdjustmentSelectionViewModel
                              {
                                  sku_id = sg.Key.sku_id,
                                  sku_code = sg.Key.sku_code,
                                  sku_name = sg.Key.sku_name,
                                  unit = sg.Key.unit,
                                  spu_code = sg.Key.spu_code,
                                  spu_name = sg.Key.spu_name,
                                  goods_location_id = sg.Key.goods_location_id,
                                  LocationName = sg.Key.LocationName,
                                  location_name = sg.Key.LocationName,
                                  WarehouseName = sg.Key.WarehouseName,
                                  warehouse_name = sg.Key.WarehouseName,
                                  series_number = sg.Key.series_number,
                                  expiry_date = sg.Key.expiry_date,
                                  putaway_date = sg.Key.PutAwayDate,
                                  price = sg.Key.price,
                                  goods_owner_id = sg.Key.goods_owner_id,
                                  goods_owner_name = sg.Key.goods_owner_name,
                                  qty_total = sg.Sum(x => x.qty),
                                  qty_locked = 0,
                                  qty_available = 0
                              };

            stock_query = stock_query.Where(queries.AsExpression<SkuAdjustmentSelectionViewModel>());
            int totals = await stock_query.CountAsync();

            var paged_query = stock_query.OrderBy(x => x.sku_code)
                                         .ThenBy(x => x.LocationName)
                                         .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                                         .Take(pageSearch.pageSize);

            // 5. Execute main stock query
            var stockData = await paged_query.ToListAsync();

            if (stockData.Count != 0)
            {
                var skuIds = stockData.Select(x => x.sku_id).Distinct().ToList();

                // 6. Fetch Locks for these SKUs
                var d_locks = await dispatch_lock.Where(x => skuIds.Contains(x.key.sku_id)).ToListAsync();
                var p_locks = await process_lock.Where(x => skuIds.Contains(x.key.sku_id)).ToListAsync();
                var m_locks = await move_lock.Where(x => skuIds.Contains(x.key.sku_id)).ToListAsync();

                // 7. In-Memory Calculation
                foreach (var item in stockData)
                {
                    var d_qty = d_locks.Where(x => x.key.sku_id == item.sku_id
                                                && x.key.goods_location_id == item.goods_location_id
                                                && x.key.goods_owner_id == item.goods_owner_id
                                                && x.key.series_number == item.series_number
                                                && x.key.expiry_date == item.expiry_date
                                                && x.key.price == item.price
                                                && x.key.putaway_date.Date == item.putaway_date)
                                       .Sum(x => x.qty_locked);

                    var p_qty = p_locks.Where(x => x.key.sku_id == item.sku_id
                                                && x.key.goods_location_id == item.goods_location_id
                                                && x.key.goods_owner_id == item.goods_owner_id
                                                && x.key.series_number == item.series_number
                                                && x.key.expiry_date == item.expiry_date
                                                && x.key.price == item.price
                                                && x.key.putaway_date.Date == item.putaway_date)
                                       .Sum(x => x.qty_locked);

                    var m_qty = m_locks.Where(x => x.key.sku_id == item.sku_id
                                                && x.key.goods_location_id == item.goods_location_id
                                                && x.key.goods_owner_id == item.goods_owner_id
                                                && x.key.series_number == item.series_number
                                                && x.key.expiry_date == item.expiry_date
                                                && x.key.price == item.price
                                                && x.key.putaway_date.Date == item.putaway_date)
                                       .Sum(x => x.qty_locked);

                    item.qty_locked = d_qty + p_qty + m_qty;
                    item.qty_available = item.qty_total - item.qty_locked;
                }

                stockData = [.. stockData.Where(x => x.qty_available > 0)];
            }

            return (stockData, totals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SKU for adjustment selection {error}", ex.Message);
            throw;
        }

    }

    /// <summary>
    /// Confirm Process
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ConfirmProcess(int id, CurrentUser currentUser)
    {
        var dbSet = _dbContext.GetDbSet<StockprocessEntity>();
        var entity = await dbSet.FirstOrDefaultAsync(t => t.Id == id);
        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }
        if (entity.process_status == true)
        {
            return (false, _stringLocalizer["status_changed"]);
        }

        entity.process_status = true;
        entity.processor = currentUser.user_name;
        entity.process_time = DateTime.UtcNow;
        entity.last_update_time = DateTime.UtcNow;
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ?
            (true, _stringLocalizer["operation_success"]) :
            (false, _stringLocalizer["operation_failed"]);
    }

    /// <summary>
    /// Confirm Adjustment
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ConfirmAdjustment(int id, CurrentUser currentUser)
    {
        var dbSet = _dbContext.GetDbSet<StockprocessEntity>();
        var processDetailDb = _dbContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();
        var adjustDbset = _dbContext.GetDbSet<StockadjustEntity>();
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var stockEntity = await dbSet.FirstOrDefaultAsync(t => t.Id == id);
            var nowTime = DateTime.UtcNow;
            if (stockEntity == null)
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }
            var adjusted = await (from a in adjustDbset
                                  join d in processDetailDb on a.source_table_id equals d.Id
                                  where a.job_type == (int)StockJobTypeEnum.StockAdjust && d.stock_process_id == id
                                  select a
              ).AnyAsync();
            // validate has proces and adjusted
            if (stockEntity.process_status && adjusted)
            {
                return (false, _stringLocalizer["status_changed"]);
            }

            var details = await processDetailDb.Where(t => t.stock_process_id == id).ToListAsync();
            var adjusts = (from d in details
                           select new StockadjustEntity
                           {
                               sku_id = d.sku_id,
                               source_table_id = d.Id,
                               is_update_stock = true,
                               goods_location_id = d.goods_location_id,
                               job_type = (int)StockJobTypeEnum.StockAdjust,
                               goods_owner_id = d.goods_owner_id,
                               qty = d.is_source ? -d.qty : d.qty,
                               create_time = nowTime,
                               creator = currentUser.user_name,
                               last_update_time = nowTime,
                               TenantId = currentUser.tenant_id,
                               series_number = d.series_number,
                               expiry_date = d.expiry_date,
                               price = d.price,
                               putaway_date = d.putaway_date,
                           }).ToList();

            stockEntity.last_update_time = nowTime;
            var stockDBSet = _dbContext.GetDbSet<StockEntity>();
            if (stockEntity == null)
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }

            var stocks = await stockDBSet
                .Where(s => processDetailDb
                .Where(t => t.stock_process_id == id)
                .Any(t => t.goods_location_id == s.goods_location_id && t.sku_id == s.sku_id && t.goods_owner_id == s.goods_owner_id && t.series_number == s.series_number && t.expiry_date == s.expiry_date && t.price == s.price && t.putaway_date == s.PutAwayDate))
                .ToListAsync();

            foreach (StockprocessdetailEntity d in details)
            {
                var stock = stocks.FirstOrDefault(t => t.goods_location_id == d.goods_location_id && t.sku_id == d.sku_id && t.goods_owner_id == d.goods_owner_id && t.series_number == d.series_number && t.expiry_date == d.expiry_date && t.price == d.price && t.PutAwayDate == d.putaway_date);
                d.is_update_stock = true;
                d.last_update_time = nowTime;
                if (d.is_source)
                {
                    if (stock == null)
                    {
                        return (false, _stringLocalizer["data_changed"]);
                    }
                    if (stock.qty < d.qty)
                    {
                        throw new Exception("Qty has changed");
                    }
                    stock.qty -= d.qty;
                    stock.last_update_time = nowTime;
                }
                else
                {
                    d.putaway_date = DateTime.UtcNow;
                    if (stock == null)
                    {
                        await stockDBSet.AddAsync(new StockEntity
                        {
                            sku_id = d.sku_id,
                            goods_location_id = d.goods_location_id,
                            goods_owner_id = d.goods_owner_id,
                            series_number = d.series_number,
                            expiry_date = d.expiry_date,
                            price = d.price,
                            PutAwayDate = d.putaway_date,
                            is_freeze = false,
                            last_update_time = nowTime,
                            qty = d.qty,
                            TenantId = currentUser.tenant_id
                        });
                    }
                    else
                    {
                        if (stock.qty < 0)
                        {
                            _logger.LogWarning("Stock qty less than zero for Stockprocess id {Id}", id);
                        }
                        stock.qty += d.qty;
                        _logger.LogInformation("Stock qty updated with {qty} for Stockprocess id {Id}", d.qty, id);
                        stock.last_update_time = nowTime;
                    }
                }
            }
            var code = await GetAdjustOrderCode(currentUser);
            adjusts.ForEach(t => t.job_code = code);
            await adjustDbset.AddRangeAsync(adjusts);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, _stringLocalizer["operation_success"]);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Exception in ConfirmAdjustment for Stockprocess id {Id}", id);
            return (false, _stringLocalizer["operation_failed"]);
        }
    }

    /// <summary>
    /// Get Adjust Order Code
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<string> GetAdjustOrderCode(CurrentUser currentUser)
    {
        string code;
        string date = DateTime.UtcNow.ToString("yyyy" + "MM" + "dd");
        string maxNo = await _dbContext
            .GetDbSet<StockadjustEntity>(currentUser.tenant_id)
            .MaxAsync(t => t.job_code);

        if (maxNo == null)
        {
            code = date + "-0001";
        }
        else
        {
            string maxDate = maxNo[..8];
            string maxDateNo = maxNo.Substring(9, 4);
            if (date == maxDate)
            {
                _ = int.TryParse(maxDateNo, out int dd);
                int newDateNo = dd + 1;
                code = date + "-" + newDateNo.ToString("0000");
            }
            else
            {
                code = date + "-0001";
            }
        }

        return code;
    }

    /// <summary>
    /// Page Processing Async
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<(List<StockprocessGetViewModel> data, int totals)> PageProcessingAsync(PageSearch pageSearch)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count != 0)
        {
            pageSearch.searchObjects.ForEach(queries.Add);
        }

        var queryItem = queries.FirstOrDefault(t => t.Label == "textSearch");
        string textSearch = (queryItem?.Text ?? "").ToLowerInvariant().Trim();

        var stockDbSet = _dbContext.GetDbSet<StockprocessEntity>().AsNoTracking();
        var adjustDBSet = _dbContext.GetDbSet<StockadjustEntity>().AsNoTracking();
        var processDb = _dbContext.GetDbSet<StockprocessdetailEntity>()
            .AsNoTracking();

        var query = from m in stockDbSet
                    join a in from a in adjustDBSet
                              join d in processDb on a.source_table_id equals d.Id
                              where a.job_type == 2
                              group d by d.stock_process_id into ag
                              select new
                              {
                                  stockprocess_id = ag.Key
                              } on m.Id equals a.stockprocess_id into a_left
                    from a in a_left.DefaultIfEmpty()
                    select new StockprocessGetViewModel
                    {
                        id = m.Id,
                        job_code = m.job_code,
                        job_type = m.job_type,
                        process_status = m.process_status,
                        processor = m.processor,
                        process_time = m.process_time,
                        creator = m.creator,
                        create_time = m.create_time,
                        last_update_time = m.last_update_time,
                        adjust_status = (m.process_status && (a.stockprocess_id == null ? false : true)),
                    };

        query = query.Where(queries.AsGroupedExpression<StockprocessGetViewModel>());
        int totals = await query.CountAsync();
        var list = await query
            .OrderBy(x => x.adjust_status)
            .ThenBy(x => x.process_status)
            .ThenByDescending(t => t.last_update_time)
            .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
            .Take(pageSearch.pageSize)
            .ToListAsync();

        foreach (var item in list)
        {
            if (!string.IsNullOrWhiteSpace(textSearch))
            {
                processDb = processDb.Where(x => x.series_number.Contains(textSearch.ToLowerInvariant()));
            }

            var details = processDb
                .Where(x => x.stock_process_id == item.id)
                .ToList();

            if (details.Count > 0)
            {
                item.series_number = string.Join(";", details.Select(x => x.series_number).Distinct());
                item.qty = details.Sum(x => (x.is_source ? (-1) : 1) * x.qty);
            }
        }

        return (list, totals);
    }

    /// <summary>
    /// Delete a processing item
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> DeleteProcessingAsync(int id)
    {
        var entity = await _dbContext.GetDbSet<StockprocessEntity>()
            .Where(t => t.Id.Equals(id) && t.process_status == false)
            .Include(e => e.detailList)
            .FirstOrDefaultAsync();

        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }

        _dbContext.GetDbSet<StockprocessEntity>().Remove(entity);
        var result = await _dbContext.SaveChangesAsync();

        return result > 0 ?
            (true, _stringLocalizer["delete_success"]) :
            (false, _stringLocalizer["delete_failed"]);
    }
}