using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using WMSSolution.Core;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Shared.Enums;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.Models.Stock;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Stockprocess;
using WMSSolution.WMS.IServices;

namespace WMSSolution.WMS.Services.Stockprocess;

/// <summary>
///  Stockprocess Service
/// </summary>
/// <remarks>
///Stockprocess  constructor
/// </remarks>
/// <param name="dbContext">The DBContext</param>
/// <param name="stringLocalizer">Localizer</param>
/// <param name="functionHelper">Function Helper</param>
/// <param name="logger">Logger</param>
public class StockprocessService(SqlDBContext dbContext
      , IStringLocalizer<MultiLanguage> stringLocalizer
        , FunctionHelper functionHelper
        , ILogger<StockprocessService> logger) : BaseService<StockprocessEntity>, IStockprocessService
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
    /// Function Helper
    /// </summary>
    private readonly FunctionHelper _functionHelper = functionHelper;

    /// <summary>
    ///   Logger service
    /// </summary>
    private readonly ILogger<StockprocessService> _logger = logger;

    #endregion Args

    #region Api

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <returns></returns>
    public async Task<(List<StockprocessGetViewModel> data, int totals)> PageAsync(PageSearch pageSearch)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count != 0)
        {
            pageSearch.searchObjects.ForEach(queries.Add);
        }
        var stockDbSet = _dbContext.GetDbSet<StockprocessEntity>().AsNoTracking();
        var adjustDBSet = _dbContext.GetDbSet<StockadjustEntity>().AsNoTracking();
        var processDb = _dbContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();

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

        IEnumerable<StockprocessGetViewModel> data = await query.ToListAsync();
        //foreach (var queryItem in queries)
        {
            var queryItem = queries.FirstOrDefault(t => t.Label == "series_number");
            //if (queryItem == null) continue;
            if (!string.IsNullOrEmpty(queryItem?.Text))
                data = data.Where(x =>
                x.series_number.Contains((queryItem.Text ?? ""), StringComparison.InvariantCultureIgnoreCase)
                || x.job_code.Contains((queryItem.Text ?? ""), StringComparison.InvariantCultureIgnoreCase)
                || x.creator.Contains((queryItem.Text ?? ""), StringComparison.InvariantCultureIgnoreCase)
                || x.processor.Contains((queryItem.Text ?? ""), StringComparison.InvariantCultureIgnoreCase)
                );
        }
        //query = query.Where(queries.AsExpression<StockprocessGetViewModel>());
        int totals = data.Count(); // await query.CountAsync();
        var list = data
            .OrderBy(x => x.adjust_status)
            .ThenBy(x => x.process_status)
            .ThenByDescending(t => t.last_update_time)
            .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
            .Take(pageSearch.pageSize)
            .ToList();

        foreach (var item in list)
        {
            var details = processDb
                .Where(x => x.stock_process_id == item.id)
                .ToList();

            if (details.Count > 0)
            {
                item.series_number = string.Join(";", details.Select(x => x.series_number).Distinct());
                item.qty = details.Sum(x => x.qty);
            }
        }

        return (list, totals);
    }

    /// <summary>
    /// Get all records
    /// </summary>
    /// <returns></returns>
    public async Task<List<StockprocessGetViewModel>> GetAllAsync(CurrentUser currentUser)
    {
        var data = await _dbContext.GetDbSet<StockprocessEntity>()
            .AsNoTracking()
            .Where(x => x.tenant_id == currentUser.tenant_id)
            .ToListAsync();
        return data.Adapt<List<StockprocessGetViewModel>>();
    }

    /// <summary>
    /// Get a record by id
    /// </summary>
    /// <returns></returns>
    public async Task<StockprocessWithDetailViewModel> GetAsync(int id)
    {
        var dbSet = _dbContext.GetDbSet<StockprocessEntity>().AsNoTracking();
        var processDetailDb = _dbContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();
        var skuDb = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var spuDb = _dbContext.GetDbSet<SpuEntity>().AsNoTracking();
        var locationDb = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();

        try
        {
            var entity = await dbSet.FirstOrDefaultAsync(t => t.Id.Equals(id));
            var details = await (from spd in processDetailDb.Where(t => t.stock_process_id == id)
                                 join sku in skuDb on spd.sku_id equals sku.Id
                                 join spu in spuDb on sku.spu_id equals spu.Id
                                 join gl in locationDb on spd.goods_location_id equals gl.Id into gl_left
                                 from gl in gl_left.DefaultIfEmpty()
                                 select new StockprocessdetailViewModel
                                 {
                                     id = spd.Id,
                                     stock_process_id = spd.stock_process_id,
                                     sku_id = spd.sku_id,
                                     goods_owner_id = spd.goods_owner_id,
                                     goods_location_id = spd.goods_location_id,
                                     qty = spd.qty,
                                     last_update_time = spd.last_update_time,
                                     tenant_id = spd.TenantId,
                                     is_source = spd.is_source,
                                     sku_code = sku.sku_code,
                                     spu_code = spu.spu_code,
                                     spu_name = spu.spu_name,
                                     unit = sku.unit,
                                     LocationName = gl.LocationName == null ? "" : gl.LocationName,
                                     location_name = gl.LocationName == null ? "" : gl.LocationName,
                                     series_number = spd.series_number,
                                     expiry_date = spd.expiry_date,
                                     price = spd.price,
                                     putaway_date = spd.putaway_date,
                                 }).ToListAsync();

            if (entity == null)
            {
                _logger.LogWarning("Stockprocess stockEntity with id {Id} not found.", id);
                return new StockprocessWithDetailViewModel();
            }

            var res = entity.Adapt<StockprocessWithDetailViewModel>();
            var adjustDb = _dbContext.GetDbSet<StockadjustEntity>().AsNoTracking();

            var adjusted = await (from a in adjustDb
                                  join d in processDetailDb on a.source_table_id equals d.Id
                                  where a.job_type == 2 && d.stock_process_id == id
                                  select a
                           ).AnyAsync();

            res.adjust_status = entity.process_status && adjusted;
            res.source_detail_list = [.. details.Where(t => t.is_source)];
            res.target_detail_list = [.. details.Where(t => !t.is_source)];
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception when get Stockprocess With Detail {id}", id);
            throw;
        }
    }

    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<(int id, string msg)> AddAsync(StockprocessViewModel viewModel, CurrentUser currentUser)
    {
        var dbSet = _dbContext.GetDbSet<StockprocessEntity>();
        var entity = viewModel.Adapt<StockprocessEntity>();
        var stockDBSet = _dbContext.GetDbSet<StockEntity>().AsNoTracking();

        var parameterExpression = Expression.Parameter(typeof(StockEntity), "m");
        Expression? exp = null;
        for (int i = 0; i < entity.detailList.Count; i++)
        {
            var t_constan_location = Expression.Constant(entity.detailList[i].goods_location_id);
            var t_prop_location = typeof(StockEntity).GetProperty("goods_location_id");
            var t_location_exp = Expression.Property(parameterExpression, t_prop_location);
            var t_location_full_exp = Expression.Equal(t_location_exp, t_constan_location);
            var t_constan_sku = Expression.Constant(entity.detailList[i].sku_id);
            var t_prop_sku = typeof(StockEntity).GetProperty("sku_id");
            var t_sku_exp = Expression.Property(parameterExpression, t_prop_sku);
            var t_sku_full_exp = Expression.Equal(t_sku_exp, t_constan_sku);
            var t_constan_owner = Expression.Constant(entity.detailList[i].goods_owner_id);
            var t_prop_owner = typeof(StockEntity).GetProperty("goods_owner_id");
            var t_owner_exp = Expression.Property(parameterExpression, t_prop_owner);
            var t_owner_full_exp = Expression.Equal(t_owner_exp, t_constan_owner);
            var t_constan_sn = Expression.Constant(entity.detailList[i].series_number);
            var t_prop_sn = typeof(StockEntity).GetProperty("series_number");
            var t_sn_exp = Expression.Property(parameterExpression, t_prop_sn);
            var t_sn_full_exp = Expression.Equal(t_sn_exp, t_constan_sn);
            var t_constan_expiry = Expression.Constant(entity.detailList[i].expiry_date);
            var t_prop_expiry = typeof(StockEntity).GetProperty("expiry_date");
            var t_expiry_exp = Expression.Property(parameterExpression, t_prop_sn);
            var t_expiry_full_exp = Expression.Equal(t_sn_exp, t_constan_sn);
            var t_constan_price = Expression.Constant(entity.detailList[i].price);
            var t_prop_price = typeof(StockEntity).GetProperty("price");
            var t_price_exp = Expression.Property(parameterExpression, t_prop_sn);
            var t_price_full_exp = Expression.Equal(t_sn_exp, t_constan_sn);
            var t_constan_putaway = Expression.Constant(entity.detailList[i].putaway_date);
            var t_prop_putaway = typeof(StockEntity).GetProperty("putaway_date");
            // MemberExpression t_putaway_exp = Expression.Property(parameterExpression, t_prop_sn);
            //BinaryExpression t_putaway_full_exp = Expression.Equal(t_sn_exp, t_constan_sn);
            var t_exp = Expression.And(t_location_full_exp, t_sku_full_exp);
            t_exp = Expression.And(t_exp, t_owner_full_exp);
            if (exp != null)
                exp = Expression.Or(exp, t_exp);
            else
                exp = t_exp;
        }
        var predicate_res = Expression.Lambda<Func<StockEntity, bool>>(exp, [parameterExpression]);

        var query = stockDBSet.Where(predicate_res);

        var stocks = await query.ToListAsync();

        var goods_location_id_list = viewModel.detailList
            .Where(t => t.is_source == true)
            .Select(t => t.goods_location_id)
            .ToList();

        var sku_id_list = viewModel.detailList.Where(t => t.is_source == true)
            .Select(t => t.sku_id)
            .ToList();

        var processDetailDb = _dbContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();

        var lockeds = await (from d in processDetailDb
                             where d.is_update_stock == false && goods_location_id_list.Contains(d.goods_location_id)
                             && sku_id_list.Contains(d.sku_id)
                             group d by new { d.goods_location_id, d.sku_id, d.goods_owner_id, d.series_number, d.expiry_date, d.price, d.putaway_date } into lg
                             select new
                             {
                                 sku_id = lg.Key.sku_id,
                                 goods_location_id = lg.Key.goods_location_id,
                                 goods_owner_id = lg.Key.goods_owner_id,
                                 series_number = lg.Key.series_number,
                                 lg.Key.expiry_date,
                                 lg.Key.price,
                                 lg.Key.putaway_date,
                                 qty_locked = lg.Sum(e => e.qty)
                             }).ToListAsync();


        entity.Id = 0;
        entity.create_time = DateTime.UtcNow;
        entity.creator = currentUser.user_name;
        entity.last_update_time = DateTime.UtcNow;
        entity.tenant_id = currentUser.tenant_id;
        entity.job_code = await _functionHelper.GetFormNoAsync("Stockprocess");
        await dbSet.AddAsync(entity);

        foreach (var d in entity.detailList)
        {
            d.TenantId = currentUser.tenant_id;
            d.last_update_time = DateTime.UtcNow;
            d.Id = 0;
            // var s = stocks.FirstOrDefault(t => t.sku_id == d.sku_id && t.goods_location_id == d.goods_location_id && t.goods_owner_id == d.goods_owner_id && t.series_number == d.series_number && t.expiry_date == d.expiry_date && t.price == d.price && t.PutAwayDate.Date == d.putaway_date.Date);

            var s = stocks.FirstOrDefault(t => t.sku_id == d.sku_id && t.goods_location_id == d.goods_location_id && t.expiry_date == d.expiry_date);
            if (d.is_source == true)
            {
                //if (s == null)
                //{
                //    return (0, _stringLocalizer["data_changed"]);
                //}
                //var locked = lockeds.FirstOrDefault(t => t.sku_id == d.sku_id && t.goods_location_id == d.goods_location_id && t.goods_owner_id == d.goods_owner_id && t.series_number == d.series_number && t.expiry_date == d.expiry_date && t.price == d.price && t.putaway_date == d.putaway_date);
                //if ((s.qty - (locked == null ? 0 : locked.qty_locked)) < d.qty)
                //{
                //    return (0, _stringLocalizer["data_changed"]);
                //}
                //if (s.is_freeze == true)
                //{
                //    return (0, _stringLocalizer["stock_frozen"]);
                //}
            }
        }

        await _dbContext.SaveChangesAsync();

        return entity.Id > 0 ?
            (entity.Id, _stringLocalizer["save_success"]) :
            (0, _stringLocalizer["save_failed"]);
    }

    /// <summary>
    /// update a record
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> UpdateAsync(StockprocessViewModel viewModel)
    {
        var dbSet = _dbContext.GetDbSet<StockprocessEntity>();
        var entity = await dbSet.FirstOrDefaultAsync(t => t.Id.Equals(viewModel.id));
        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }

        entity.Id = viewModel.id;
        entity.job_code = viewModel.job_code;
        entity.job_type = viewModel.job_type;
        entity.process_status = viewModel.process_status;
        entity.processor = viewModel.processor;
        entity.process_time = viewModel.process_time;
        entity.last_update_time = DateTime.UtcNow;

        var hasSave = await _dbContext.SaveChangesAsync();

        return hasSave > 0 ? (true, _stringLocalizer["save_success"]) : (false, _stringLocalizer["save_failed"]);
    }

    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> DeleteAsync(int id)
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

    /// <summary>
    /// confirm adjustment
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ConfirmAdjustment(int id, CurrentUser currentUser)
    {
        var dbSet = _dbContext.GetDbSet<StockprocessEntity>();
        var processDetailDb = _dbContext.GetDbSet<StockprocessdetailEntity>();
        var adjustDbset = _dbContext.GetDbSet<StockadjustEntity>();
        var stockDBSet = _dbContext.GetDbSet<StockEntity>();
        var stockTransactionDbSet = _dbContext.GetDbSet<StockTransactionEntity>();

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
                                  join d in processDetailDb.AsNoTracking() on a.source_table_id equals d.Id
                                  where a.job_type == (int)StockJobTypeEnum.StockAdjust && d.stock_process_id == id
                                  select a).AnyAsync();

            if (stockEntity.process_status && adjusted)
            {
                return (false, _stringLocalizer["status_changed"]);
            }

            var details = await processDetailDb.Where(t => t.stock_process_id == id).ToListAsync();
            var code = await GetAdjustOrderCode(currentUser);

            var adjusts = details.Select(d => new StockadjustEntity
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
                job_code = code
            }).ToList();

            stockEntity.last_update_time = nowTime;

            var stocks = await stockDBSet
                .Where(s => processDetailDb
                    .Where(t => t.stock_process_id == id)
                    .Any(t => t.goods_location_id == s.goods_location_id
                           && t.sku_id == s.sku_id
                           && t.goods_owner_id == s.goods_owner_id
                           && t.series_number == s.series_number
                           && t.expiry_date == s.expiry_date
                           && t.price == s.price
                           && t.putaway_date.Date == s.PutAwayDate.Date))
                .ToListAsync();

            var stockTransactions = new List<StockTransactionEntity>();
            var pendingInboundForNewStocks = new List<(StockEntity stock, StockprocessdetailEntity detail)>();

            foreach (var d in details)
            {
                var stock = stocks.FirstOrDefault(t =>
                    t.goods_location_id == d.goods_location_id
                    && t.sku_id == d.sku_id
                    && t.goods_owner_id == d.goods_owner_id
                    && t.series_number == d.series_number
                    && t.expiry_date == d.expiry_date
                    && t.price == d.price
                    && t.PutAwayDate.Date == d.putaway_date.Date);

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

                    stockTransactions.Add(new StockTransactionEntity
                    {
                        StockId = stock.Id,
                        Quantity = -d.qty,
                        SkuId = d.sku_id,
                        TransactionType = StockTransactionType.Outbound,
                        TenantId = currentUser.tenant_id,
                        RefReceipt = code,
                        TransactionDate = nowTime
                    });
                }
                else
                {
                    d.putaway_date = DateTime.UtcNow;

                    if (stock == null)
                    {
                        var newStock = new StockEntity
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
                        };

                        await stockDBSet.AddAsync(newStock);
                        pendingInboundForNewStocks.Add((newStock, d));
                    }
                    else
                    {
                        if (stock.qty < 0)
                        {
                            _logger.LogWarning("Stock qty less than zero for Stockprocess id {Id}", id);
                        }

                        stock.qty += d.qty;
                        stock.last_update_time = nowTime;

                        stockTransactions.Add(new StockTransactionEntity
                        {
                            StockId = stock.Id,
                            Quantity = d.qty,
                            SkuId = d.sku_id,
                            TransactionType = StockTransactionType.Inbound,
                            TenantId = currentUser.tenant_id,
                            RefReceipt = code,
                            TransactionDate = nowTime
                        });
                    }
                }
            }

            await adjustDbset.AddRangeAsync(adjusts);


            await _dbContext.SaveChangesAsync();


            foreach (var (stock, detail) in pendingInboundForNewStocks)
            {
                stockTransactions.Add(new StockTransactionEntity
                {
                    StockId = stock.Id,
                    Quantity = detail.qty,
                    SkuId = detail.sku_id,
                    TransactionType = StockTransactionType.Inbound,
                    TenantId = currentUser.tenant_id,
                    RefReceipt = code,
                    TransactionDate = nowTime
                });
            }

            if (stockTransactions.Count > 0)
            {
                await stockTransactionDbSet.AddRangeAsync(stockTransactions);
                await _dbContext.SaveChangesAsync();
            }

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
    /// confirm processing
    /// </summary>
    /// <param name="id">id</param>
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
        dbSet.Update(entity);

        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ?
            (true, _stringLocalizer["operation_success"]) :
            (false, _stringLocalizer["operation_failed"]);
    }

    /// <summary>
    /// get next order code number
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetOrderCode(CurrentUser currentUser)
    {
        string code;
        string date = DateTime.UtcNow.ToString("yyyy" + "MM" + "dd");
        string maxNo = await _dbContext.GetDbSet<StockprocessEntity>()
            .AsNoTracking()
            .Where(t => t.tenant_id == currentUser.tenant_id)
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
    /// get next order code number
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetAdjustOrderCode(CurrentUser currentUser)
    {
        string code;
        string date = DateTime.UtcNow.ToString("yyyy" + "MM" + "dd");
        string maxNo = await _dbContext.GetDbSet<StockadjustEntity>()
            .AsNoTracking()
            .Where(t => t.TenantId == currentUser.tenant_id)
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
    /// Get dashboard statistics
    /// </summary>
    /// <returns></returns>
    public async Task<StockProcessDashboardStatsViewModel> GetDashboardStatsAsync()
    {
        var stockProcessDbSet = _dbContext.GetDbSet<StockprocessEntity>().AsNoTracking();
        var stockProcessDetailDbSet = _dbContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        // Pending Count: process_status == false
        var pendingCount = await stockProcessDbSet
            .Where(x => x.process_status == false)
            .CountAsync();

        // Approved Today Count: process_status == true && process_time is today
        var approvedTodayCount = await stockProcessDbSet
            .Where(x => x.process_status == true &&
                        x.process_time >= today &&
                        x.process_time < tomorrow)
            .CountAsync();

        // Total Loss Quantity: Sum of result where is_source == true
        var totalLossQuantity = await stockProcessDetailDbSet
            .Where(x => x.is_source == true)
            .SumAsync(x => x.qty);

        return new StockProcessDashboardStatsViewModel
        {
            PendingCount = pendingCount,
            ApprovedTodayCount = approvedTodayCount,
            TotalLossQuantity = (int)totalLossQuantity
        };
    }

    #endregion Api

}