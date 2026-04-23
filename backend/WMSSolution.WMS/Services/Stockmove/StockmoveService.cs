
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WMSSolution.Core;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Core.Utility;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.IServices;

namespace WMSSolution.WMS.Services
{
    /// <summary>
    ///  Stockmove Service
    /// </summary>
    public class StockmoveService : BaseService<StockmoveEntity>, IStockmoveService
    {
        #region Args

        /// <summary>
        /// The DBContext
        /// </summary>
        private readonly SqlDBContext _dBContext;

        /// <summary>
        /// Localizer Service
        /// </summary>
        private readonly IStringLocalizer<MultiLanguage> _stringLocalizer;

        /// <summary>
        /// Function Helper
        /// </summary>
        private readonly FunctionHelper _functionHelper;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<StockmoveService> _logger;

        #endregion Args

        #region constructor

        /// <summary>
        ///Stockmove  constructor
        /// </summary>
        /// <param name="dBContext">The DBContext</param>
        /// <param name="stringLocalizer">Localizer</param>
        /// <param name="functionHelper">Function Helper</param>
        /// <param name="logger">Logger</param>
        public StockmoveService(
            SqlDBContext dBContext
          , IStringLocalizer<MultiLanguage> stringLocalizer
            , FunctionHelper functionHelper
            , ILogger<StockmoveService> logger)
        {
            _dBContext = dBContext;
            _stringLocalizer = stringLocalizer;
            _functionHelper = functionHelper;
            _logger = logger;
        }

        #endregion constructor

        #region Api

        /// <summary>
        /// page search
        /// </summary>
        /// <param name="pageSearch">args</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        public async Task<(List<StockmoveViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser)
        {
            QueryCollection queries = new QueryCollection();
            if (pageSearch.searchObjects.Any())
            {
                pageSearch.searchObjects.ForEach(s =>
                {
                    queries.Add(s);
                });
            }
            var DbSet = _dBContext.GetDbSet<StockmoveEntity>();
            var location_DBSet = _dBContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
            var query = from m in DbSet.AsNoTracking()
                        join sku in _dBContext.GetDbSet<SkuEntity>().AsNoTracking() on m.sku_id equals sku.Id
                        join spu in _dBContext.GetDbSet<SpuEntity>().AsNoTracking() on sku.spu_id equals spu.Id
                        join orig_location in location_DBSet on m.orig_goods_location_id equals orig_location.Id
                        join dest_location in location_DBSet on m.dest_googs_location_id equals dest_location.Id
                        select new StockmoveViewModel
                        {
                            id = m.Id,
                            job_code = m.job_code,
                            move_status = m.move_status,
                            sku_id = m.sku_id,
                            orig_goods_location_id = m.orig_goods_location_id,
                            dest_googs_location_id = m.dest_googs_location_id,
                            qty = m.qty,
                            goods_owner_id = m.goods_owner_id,
                            handler = m.handler,
                            handle_time = m.handle_time,
                            creator = m.creator,
                            create_time = m.create_time,
                            last_update_time = m.last_update_time,
                            tenant_id = m.TenantId,
                            sku_code = sku.sku_code,
                            sku_name = sku.sku_name,
                            spu_code = spu.spu_code,
                            spu_name = spu.spu_name,
                            dest_googs_LocationName = dest_location.LocationName,
                            dest_googs_warehouse = dest_location.WarehouseName,
                            orig_goods_LocationName = orig_location.LocationName,
                            orig_goods_warehouse = orig_location.WarehouseName,
                            series_number = m.series_number,
                            expiry_date = m.expiry_date,
                            price = m.price,
                            putaway_date = m.putaway_date,
                        };
            query = query.Where(t => t.tenant_id.Equals(currentUser.tenant_id))
                .Where(queries.AsExpression<StockmoveViewModel>());
            int totals = await query.CountAsync();
            var list = await query.OrderByDescending(t => t.last_update_time)
                       .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                       .Take(pageSearch.pageSize)
                       .ToListAsync();
            return (list, totals);
        }

        /// <summary>
        /// Get all records
        /// </summary>
        /// <returns></returns>
        public async Task<List<StockmoveViewModel>> GetAllAsync(CurrentUser currentUser)
        {
            var DbSet = _dBContext.GetDbSet<StockmoveEntity>();
            var location_DBSet = _dBContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
            var data = await (from m in DbSet.AsNoTracking().Where(t => t.TenantId.Equals(currentUser.tenant_id))
                              join sku in _dBContext.GetDbSet<SkuEntity>().AsNoTracking() on m.sku_id equals sku.Id
                              join spu in _dBContext.GetDbSet<SpuEntity>().AsNoTracking() on sku.spu_id equals spu.Id
                              join orig_location in location_DBSet on m.orig_goods_location_id equals orig_location.Id
                              join dest_location in location_DBSet on m.dest_googs_location_id equals dest_location.Id
                              select new StockmoveViewModel
                              {
                                  id = m.Id,
                                  job_code = m.job_code,
                                  move_status = m.move_status,
                                  sku_id = m.sku_id,
                                  orig_goods_location_id = m.orig_goods_location_id,
                                  dest_googs_location_id = m.dest_googs_location_id,
                                  qty = m.qty,
                                  goods_owner_id = m.goods_owner_id,
                                  handler = m.handler,
                                  handle_time = m.handle_time,
                                  creator = m.creator,
                                  create_time = m.create_time,
                                  last_update_time = m.last_update_time,
                                  tenant_id = m.TenantId,
                                  sku_code = sku.sku_code,
                                  sku_name = sku.sku_name,
                                  spu_code = spu.spu_code,
                                  spu_name = spu.spu_name,
                                  dest_googs_LocationName = dest_location.LocationName,
                                  dest_googs_warehouse = dest_location.WarehouseName,
                                  orig_goods_LocationName = orig_location.LocationName,
                                  orig_goods_warehouse = orig_location.WarehouseName,
                                  series_number = m.series_number,
                                  expiry_date = m.expiry_date,
                                  price = m.price,
                                  putaway_date = m.putaway_date,
                              }
            ).OrderByDescending(t => t.create_time)
            .ToListAsync();
            return data.Adapt<List<StockmoveViewModel>>();
        }

        /// <summary>
        /// Get a record by id
        /// </summary>
        /// <returns></returns>
        public async Task<StockmoveViewModel> GetAsync(int id)
        {
            var DbSet = _dBContext.GetDbSet<StockmoveEntity>();
            var location_DBSet = _dBContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
            var data = await (from m in DbSet.AsNoTracking()
                              join sku in _dBContext.GetDbSet<SkuEntity>().AsNoTracking() on m.sku_id equals sku.Id
                              join spu in _dBContext.GetDbSet<SpuEntity>().AsNoTracking() on sku.spu_id equals spu.Id
                              join orig_location in location_DBSet on m.orig_goods_location_id equals orig_location.Id
                              join dest_location in location_DBSet on m.dest_googs_location_id equals dest_location.Id
                              where m.Id == id
                              select new StockmoveViewModel
                              {
                                  id = m.Id,
                                  job_code = m.job_code,
                                  move_status = m.move_status,
                                  sku_id = m.sku_id,
                                  orig_goods_location_id = m.orig_goods_location_id,
                                  dest_googs_location_id = m.dest_googs_location_id,
                                  qty = m.qty,
                                  goods_owner_id = m.goods_owner_id,
                                  handler = m.handler,
                                  handle_time = m.handle_time,
                                  creator = m.creator,
                                  create_time = m.create_time,
                                  last_update_time = m.last_update_time,
                                  tenant_id = m.TenantId,
                                  sku_code = sku.sku_code,
                                  sku_name = sku.sku_name,
                                  spu_code = spu.spu_code,
                                  spu_name = spu.spu_name,
                                  dest_googs_LocationName = dest_location.LocationName,
                                  dest_googs_warehouse = dest_location.WarehouseName,
                                  orig_goods_LocationName = orig_location.LocationName,
                                  orig_goods_warehouse = orig_location.WarehouseName,
                                  series_number = m.series_number,
                                  expiry_date = m.expiry_date,
                                  price = m.price,
                                  putaway_date = m.putaway_date,
                              }).FirstOrDefaultAsync();

            return data;
        }

        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        public async Task<(int id, string msg)> AddAsync(StockmoveViewModel viewModel, CurrentUser currentUser)
        {
            try
            {
                var stockMoveDbSet = _dBContext.GetDbSet<StockmoveEntity>();
                var stockDbSet = _dBContext.GetDbSet<StockEntity>();
                var stockMoveEntity = viewModel.Adapt<StockmoveEntity>();
                var processDetailDbSet = _dBContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();
                var dispatchPickDbSet = _dBContext.GetDbSet<DispatchpicklistEntity>();
                var dispatchDbSet = _dBContext.GetDbSet<DispatchlistEntity>()
                                              .Where(t => t.TenantId.Equals(currentUser.tenant_id));

                var dispatchGroupDatas = from dp in dispatchDbSet.AsNoTracking()
                                         join dpp in dispatchPickDbSet.AsNoTracking() on dp.Id equals dpp.dispatchlist_id
                                         where dp.dispatch_status > 1 && dp.dispatch_status < 6
                                         && dpp.goods_owner_id == stockMoveEntity.goods_owner_id && dpp.series_number == stockMoveEntity.series_number && dpp.goods_location_id == stockMoveEntity.orig_goods_location_id && dpp.sku_id == stockMoveEntity.sku_id && dpp.expiry_date == stockMoveEntity.expiry_date && dpp.price == stockMoveEntity.price && dpp.putaway_date == stockMoveEntity.putaway_date
                                         group dpp by new { dpp.sku_id, dpp.goods_location_id } into dg
                                         select new
                                         {
                                             sku_id = dg.Key.sku_id,
                                             goods_location_id = dg.Key.goods_location_id,
                                             qty_locked = dg.Sum(t => t.pick_qty)
                                         };

                var processLockedGroupDatas = from pd in processDetailDbSet
                                              where pd.is_update_stock == false
                                              && pd.sku_id == stockMoveEntity.sku_id && pd.goods_location_id == stockMoveEntity.orig_goods_location_id
                                              && pd.goods_owner_id == stockMoveEntity.goods_owner_id && pd.series_number == stockMoveEntity.series_number && pd.expiry_date == stockMoveEntity.expiry_date && pd.price == stockMoveEntity.price && pd.putaway_date == stockMoveEntity.putaway_date
                                              group pd by new { pd.sku_id, pd.goods_location_id } into pdg
                                              select new
                                              {
                                                  sku_id = pdg.Key.sku_id,
                                                  goods_location_id = pdg.Key.goods_location_id,
                                                  qty_locked = pdg.Sum(t => t.qty)
                                              };

                var moveLockedGroupDatas = from sm in stockMoveDbSet.AsNoTracking()
                                           where sm.move_status == 0 && sm.sku_id == stockMoveEntity.sku_id && sm.orig_goods_location_id == stockMoveEntity.orig_goods_location_id
                                           && sm.goods_owner_id == stockMoveEntity.goods_owner_id && sm.series_number == stockMoveEntity.series_number && sm.expiry_date == stockMoveEntity.expiry_date && sm.price == stockMoveEntity.price && sm.putaway_date == stockMoveEntity.putaway_date
                                           group sm by new { sm.sku_id, goods_location_id = sm.orig_goods_location_id } into smg
                                           select new
                                           {
                                               smg.Key.sku_id,
                                               smg.Key.goods_location_id,
                                               qty_locked = smg.Sum(t => t.qty)
                                           };

                var origStock = await
                    (from sg in stockDbSet.AsNoTracking()
                     join dp in dispatchGroupDatas on new { sg.sku_id, sg.goods_location_id }
                     equals new { dp.sku_id, dp.goods_location_id } into dp_left
                     from dp in dp_left.DefaultIfEmpty()
                     join pl in processLockedGroupDatas on new { sg.sku_id, sg.goods_location_id }
                     equals new { pl.sku_id, pl.goods_location_id } into pl_left
                     from pl in pl_left.DefaultIfEmpty()
                     join sm in moveLockedGroupDatas on new { sg.sku_id, sg.goods_location_id }
                     equals new { sm.sku_id, goods_location_id = sm.goods_location_id } into sm_left
                     from sm in sm_left.DefaultIfEmpty()
                     where sg.sku_id == stockMoveEntity.sku_id
                     && sg.goods_location_id == stockMoveEntity.orig_goods_location_id
                     && sg.goods_owner_id == stockMoveEntity.goods_owner_id
                     && sg.series_number == stockMoveEntity.series_number
                     && sg.expiry_date == stockMoveEntity.expiry_date
                     && sg.price == stockMoveEntity.price
                     && sg.PutAwayDate == stockMoveEntity.putaway_date
                     select new
                     {
                         id = sg.Id,
                         qty_available = sg.is_freeze
                                                ? 0 : (sg.qty - (dp.qty_locked == null
                                                ? 0 : dp.qty_locked) - (pl.qty_locked == null
                                                ? 0 : pl.qty_locked) - (sm.qty_locked == null
                                                ? 0 : sm.qty_locked)),
                     }
                    ).FirstOrDefaultAsync();

                var destinationStock = await stockDbSet.
                                    FirstOrDefaultAsync(t => t.goods_owner_id == stockMoveEntity.goods_owner_id
                                                        && t.series_number == stockMoveEntity.series_number
                                                        && t.goods_location_id == stockMoveEntity.dest_googs_location_id
                                                        && t.sku_id == stockMoveEntity.sku_id
                                                        && t.expiry_date == stockMoveEntity.expiry_date
                                                        && t.price == stockMoveEntity.price
                                                        && t.PutAwayDate == stockMoveEntity.putaway_date);

                if (origStock == null || origStock.qty_available < stockMoveEntity.qty)
                {
                    return (0, _stringLocalizer["qty_not_available"]);
                }
                if (destinationStock != null && destinationStock.is_freeze == true)
                {
                    return (0, _stringLocalizer["dest_stock_freeze"]);
                }
                stockMoveEntity.Id = 0;
                stockMoveEntity.move_status = 0;
                stockMoveEntity.create_time = DateTime.UtcNow;
                stockMoveEntity.creator = currentUser.user_name;
                stockMoveEntity.last_update_time = DateTime.UtcNow;
                stockMoveEntity.TenantId = currentUser.tenant_id;
                stockMoveEntity.job_code = await _functionHelper.GetFormNoAsync("Stockmove");
                await stockMoveDbSet.AddAsync(stockMoveEntity);
                await _dBContext.SaveChangesAsync();
                if (stockMoveEntity.Id > 0)
                {
                    return (stockMoveEntity.Id, _stringLocalizer["save_success"]);
                }
                else
                {
                    return (0, _stringLocalizer["save_failed"]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in AddAsync");
                return (0, _stringLocalizer["save_failed"]);
            }
        }

        /// <summary>
        /// confirm move
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        public async Task<(bool flag, string msg)> Confirm(int id, CurrentUser currentUser)
        {
            var DbSet = _dBContext.GetDbSet<StockmoveEntity>();
            var stockDbSet = _dBContext.GetDbSet<StockEntity>();
            var entity = await DbSet.FirstOrDefaultAsync(t => t.Id.Equals(id));
            if (entity == null)
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }
            var now_time = DateTime.UtcNow;
            entity.handler = currentUser.user_name;
            entity.handle_time = now_time;
            entity.move_status = 1;
            entity.last_update_time = now_time;
            var orig_stock = await stockDbSet
                                   .FirstOrDefaultAsync(t => t.goods_owner_id == entity.goods_owner_id
                                                && t.series_number == entity.series_number
                                                && t.goods_location_id == entity.orig_goods_location_id
                                                && t.sku_id == entity.sku_id
                                                && t.expiry_date == entity.expiry_date
                                                && t.price == entity.price
                                                && t.PutAwayDate == entity.putaway_date);

            var dest_stock = await stockDbSet.
                                    FirstOrDefaultAsync(t => t.goods_owner_id == entity.goods_owner_id
                                                && t.series_number == entity.series_number
                                                && t.goods_location_id == entity.dest_googs_location_id
                                                && t.sku_id != entity.sku_id
                                                && t.expiry_date == entity.expiry_date
                                                && t.price == entity.price
                                                && t.PutAwayDate == entity.putaway_date);
            if (orig_stock != null)
            {
                orig_stock.qty -= entity.qty;
                if (orig_stock.qty < 0)
                {
                    orig_stock.qty = 0;
                }
                orig_stock.last_update_time = now_time;
            }
            if (dest_stock == null)
            {
                dest_stock = new StockEntity
                {
                    goods_location_id = entity.dest_googs_location_id,
                    sku_id = entity.sku_id,
                    goods_owner_id = entity.goods_owner_id,
                    is_freeze = false,
                    last_update_time = now_time,
                    qty = entity.qty,
                    TenantId = entity.TenantId,
                    series_number = entity.series_number,
                    expiry_date = entity.expiry_date,
                    price = entity.price,
                    PutAwayDate = entity.putaway_date,
                };
                await stockDbSet.AddAsync(dest_stock);
            }
            else
            {
                dest_stock.qty += entity.qty;
                dest_stock.last_update_time = now_time;
            }
            var saved = false;
            int res = 0;
            while (!saved)
            {
                try
                {
                    res = await _dBContext.SaveChangesAsync();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is StockEntity)
                        {
                            var proposedValues = entry.CurrentValues;
                            var databaseValues = entry.GetDatabaseValues();
                            if (UtilConvert.ObjToInt(proposedValues["id"]) == orig_stock.Id)
                            {
                                if (UtilConvert.ObjToInt(databaseValues["qty"]) - entity.qty < 0)
                                {
                                    throw new NotSupportedException(_stringLocalizer["try_agin"]);
                                }
                                else if (UtilConvert.ObjToInt(databaseValues["qty"]) - entity.qty == 0)
                                {
                                    entry.State = EntityState.Modified;
                                    proposedValues["qty"] = Math.Max(0,
                                        UtilConvert.ObjToInt(databaseValues["qty"]) - entity.qty);
                                }
                                else
                                {
                                    entry.State = EntityState.Modified;
                                    proposedValues["qty"] = UtilConvert.ObjToInt(databaseValues["qty"]) - entity.qty;
                                }
                            }
                            else if (UtilConvert.ObjToInt(proposedValues["id"]) == dest_stock.Id)
                            {
                                proposedValues["qty"] = UtilConvert.ObjToInt(databaseValues["qty"]) + entity.qty;
                            }
                            // Refresh original values to bypass next concurrency check
                            entry.OriginalValues.SetValues(databaseValues);
                        }
                        else
                        {
                            throw new NotSupportedException(_stringLocalizer["try_agin"]);
                        }
                    }
                }
            }
            if (res > 0)
            {
                return (true, _stringLocalizer["operation_success"]);
            }
            else
            {
                return (false, _stringLocalizer["operation_failed"]);
            }
        }

        /// <summary>
        /// delete a record
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public async Task<(bool flag, string msg)> DeleteAsync(int id)
        {
            var qty = await _dBContext.GetDbSet<StockmoveEntity>().Where(t => t.Id.Equals(id) && t.move_status == 0).ExecuteDeleteAsync();
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
        /// get next order code number
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetOrderCode(CurrentUser currentUser)
        {
            string code;
            string date = DateTime.UtcNow.ToString("yyyy" + "MM" + "dd");
            string maxNo = await _dBContext.GetDbSet<StockmoveEntity>().AsNoTracking().Where(t => t.TenantId == currentUser.tenant_id).MaxAsync(t => t.job_code);
            if (maxNo == null)
            {
                code = date + "-0001";
            }
            else
            {
                string maxDate = maxNo.Substring(0, 8);
                string maxDateNo = maxNo.Substring(9, 4);
                if (date == maxDate)
                {
                    int.TryParse(maxDateNo, out int dd);
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

        public async Task<StockMoveDashboardStats> GetDashboardStatsAsync(CurrentUser currentUser)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var query = _dBContext.GetDbSet<StockmoveEntity>().AsNoTracking()
                .Where(t => t.TenantId == currentUser.tenant_id);

            var pending = await query.CountAsync(t => t.move_status == 0 && string.IsNullOrEmpty(t.handler));

            var inProgress = await query.CountAsync(t => t.move_status == 0 && !string.IsNullOrEmpty(t.handler));

            var completedToday = await query.CountAsync(t => t.move_status == 1 && t.last_update_time >= today && t.last_update_time < tomorrow);

            return new StockMoveDashboardStats
            {
                Pending = pending,
                InProgress = inProgress,
                CompletedToday = completedToday
            };
        }

        #endregion Api
    }
}