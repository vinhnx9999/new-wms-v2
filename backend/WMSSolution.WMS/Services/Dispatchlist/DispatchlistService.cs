using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WMSSolution.Core;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Models.IntegrationWCS;
using WMSSolution.Core.Services;
using WMSSolution.Core.Utility;
using WMSSolution.Shared.Enums;
using WMSSolution.Shared.Enums.Location;
using WMSSolution.Shared.Enums.Outbound;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Dispatchlist;
using WMSSolution.WMS.Entities.Models.Pallet;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Dispatchlist.Duy_Phat_Solution;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Outbound;
using WMSSolution.WMS.IServices;
using WMSSolution.WMS.IServices.IntegrationWCS;

namespace WMSSolution.WMS.Services.Dispatchlist;

/// <summary>
/// Dispatchlist Service
/// </summary>
/// <param name="dBContext">The DBContext</param>
/// <param name="stringLocalizer">Localizer</param>
/// <param name="functionHelper"></param>
/// <param name="logger"></param>
/// <param name="serviceProvider"></param>
public class DispatchlistService(
    SqlDBContext dBContext
      , IStringLocalizer<MultiLanguage> stringLocalizer
       , FunctionHelper functionHelper, ILogger<DispatchlistService> logger, IServiceProvider serviceProvider
        ) : BaseService<DispatchlistEntity>, IDispatchlistService
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
    /// functions
    /// </summary>
    private readonly FunctionHelper _functionHelper = functionHelper;

    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger<DispatchlistService> _logger = logger;

    private readonly IServiceProvider _serviceProvider = serviceProvider;

    #endregion Args

    #region Api

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(List<DispatchlistViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser)
    {
        QueryCollection queries = new QueryCollection();
        if (pageSearch.searchObjects.Any())
        {
            pageSearch.searchObjects.ForEach(s =>
            {
                queries.Add(s);
            });
        }
        var DbSet = _dBContext.GetDbSet<DispatchlistEntity>().AsNoTracking();
        if (pageSearch.sqlTitle.Contains("dispatch_status"))
        {
            var dispatch_status = Convert.ToByte(pageSearch.sqlTitle.Trim().ToLower().Replace("dispatch_status", "").Replace("：", "").Replace(":", "").Replace("=", ""));
            DbSet = DbSet.Where(t => t.dispatch_status.Equals(dispatch_status));
        }
        else if (pageSearch.sqlTitle.Equals("package"))
        {
            DbSet = DbSet.Where(t => t.picked_qty == t.qty && (t.dispatch_status.Equals(3)
                        || (t.package_qty < t.picked_qty && t.dispatch_status.Equals(5))
                        || t.dispatch_status.Equals(4)));
        }
        else if (pageSearch.sqlTitle.Equals("weight"))
        {
            DbSet = DbSet.Where(t => t.picked_qty == t.qty && (t.dispatch_status.Equals(3)
                            || (t.weighing_qty < t.picked_qty && t.dispatch_status.Equals(4))
                            || t.dispatch_status.Equals(5)));
        }
        else if (pageSearch.sqlTitle.Equals("delivery"))
        {
            DbSet = DbSet.Where(t => t.picked_qty == t.qty && (t.dispatch_status.Equals(3)
                               || t.dispatch_status.Equals(4) || t.dispatch_status.Equals(5)
                               || t.dispatch_status.Equals(6)));
        }

        var query = from d in DbSet.AsNoTracking()
                    join sku in _dBContext.GetDbSet<SkuEntity>().AsNoTracking() on d.sku_id equals sku.Id
                    join spu in _dBContext.GetDbSet<SpuEntity>().AsNoTracking() on sku.spu_id equals spu.Id
                    select new DispatchlistViewModel
                    {
                        id = d.Id,
                        dispatch_no = d.dispatch_no,
                        dispatch_status = d.dispatch_status,
                        customer_id = d.customer_id,
                        customer_name = d.customer_name,
                        sku_id = d.sku_id,
                        qty = d.qty,
                        weight = d.weight,
                        volume = d.volume,
                        creator = d.creator,
                        create_time = d.create_time,
                        damage_qty = d.damage_qty,
                        lock_qty = d.lock_qty,
                        picked_qty = d.picked_qty,
                        intrasit_qty = d.intrasit_qty,
                        package_qty = d.package_qty,
                        unpackage_qty = d.picked_qty - d.package_qty,
                        weighing_qty = d.weighing_qty,
                        unweighing_qty = d.picked_qty - d.weighing_qty,
                        actual_qty = d.actual_qty,
                        sign_qty = d.sign_qty,
                        package_no = d.package_no,
                        package_person = d.package_person,
                        package_time = d.package_time,
                        weighing_no = d.weighing_no,
                        weighing_person = d.weighing_person,
                        weighing_weight = d.weighing_weight,
                        waybill_no = d.waybill_no,
                        carrier = d.carrier,
                        freightfee = d.freightfee,
                        last_update_time = d.last_update_time,
                        tenant_id = d.TenantId,
                        sku_code = sku.sku_code,
                        spu_code = spu.spu_code,
                        spu_description = spu.spu_description,
                        spu_name = spu.spu_name,
                        bar_code = sku.bar_code,
                        unpicked_qty = d.qty - d.picked_qty,
                        length_unit = spu.length_unit.GetValueOrDefault(),
                        volume_unit = spu.volume_unit.GetValueOrDefault(),
                        weight_unit = spu.weight_unit.GetValueOrDefault(),
                        pick_checker = d.pick_checker,
                        pick_checker_id = d.pick_checker_id,
                        is_todo = pageSearch.sqlTitle.Contains("dispatch_status") || (pageSearch.sqlTitle.Equals("package") && d.dispatch_status.Equals(4))
                                        || (pageSearch.sqlTitle.Equals("weight") && d.dispatch_status.Equals(5))
                                        || (pageSearch.sqlTitle.Equals("delivery") && d.dispatch_status.Equals(6)) ? false : true,
                    };
        query = query.Where(t => t.tenant_id.Equals(currentUser.tenant_id))
             .Where(queries.AsExpression<DispatchlistViewModel>());

        int totals = await query.CountAsync();
        var list = await query.OrderBy(t => t.is_todo == true ? 0 : 1).ThenByDescending(t => t.last_update_time)
                   .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                   .Take(pageSearch.pageSize)
                   .ToListAsync();

        return (list, totals);
    }

    /// <summary>
    /// get dispatchlist by dispatch_no
    /// </summary>
    /// <param name="dispatch_no"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<List<DispatchlistViewModel>> GetByDispatchlistNo(string dispatch_no, CurrentUser currentUser)
    {
        //  var DbSet = _dBContext.GetDbSet<DispatchlistEntity>();
        var datas = await (from d in _dBContext.GetDbSet<DispatchlistEntity>().AsNoTracking()
                           join sku in _dBContext.GetDbSet<SkuEntity>().AsNoTracking() on d.sku_id equals sku.Id
                           join spu in _dBContext.GetDbSet<SpuEntity>().AsNoTracking() on sku.spu_id equals spu.Id
                           where d.dispatch_no == dispatch_no && d.TenantId == currentUser.tenant_id
                           select new DispatchlistViewModel
                           {
                               id = d.Id,
                               dispatch_no = d.dispatch_no,
                               dispatch_status = d.dispatch_status,
                               customer_id = d.customer_id,
                               customer_name = d.customer_name,
                               sku_id = d.sku_id,
                               qty = d.qty,
                               weight = d.weight,
                               volume = d.volume,
                               creator = d.creator,
                               create_time = d.create_time,
                               damage_qty = d.damage_qty,
                               lock_qty = d.lock_qty,
                               picked_qty = d.picked_qty,
                               intrasit_qty = d.intrasit_qty,
                               package_qty = d.package_qty,
                               unpackage_qty = d.picked_qty - d.package_qty,
                               weighing_qty = d.weighing_qty,
                               unweighing_qty = d.picked_qty - d.weighing_qty,
                               actual_qty = d.actual_qty,
                               sign_qty = d.sign_qty,
                               package_no = d.package_no,
                               package_person = d.package_person,
                               package_time = d.package_time,
                               weighing_no = d.weighing_no,
                               weighing_person = d.weighing_person,
                               weighing_weight = d.weighing_weight,
                               waybill_no = d.waybill_no,
                               carrier = d.carrier,
                               freightfee = d.freightfee,
                               last_update_time = d.last_update_time,
                               tenant_id = d.TenantId,
                               sku_code = sku.sku_code,
                               spu_code = spu.spu_code,
                               spu_description = spu.spu_description,
                               spu_name = spu.spu_name,
                               bar_code = sku.bar_code,
                               unpicked_qty = d.qty - d.picked_qty,
                               sku_name = sku.sku_name,
                               unit = sku.unit,
                               pick_checker = d.pick_checker,
                               pick_checker_id = d.pick_checker_id,
                           }).ToListAsync();
        return datas;
    }

    /// <summary>
    /// update dispatchlist with same dispatch_no
    /// </summary>
    /// <param name="viewModels"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> UpdateAsycn(List<DispatchlistViewModel> viewModels, CurrentUser currentUser)
    {
        if (viewModels == null || viewModels.Count == 0)
        {
            return (false, "[201]" + _stringLocalizer["no_data_update"]);
        }
        var DBSet = _dBContext.GetDbSet<DispatchlistEntity>();
        var dispatch_no = viewModels?.FirstOrDefault()?.dispatch_no;
        var dispatch_status = viewModels?.FirstOrDefault()?.dispatch_status;
        if (dispatch_status == null || dispatch_no == null)
        {
            return (false, "[201]" + _stringLocalizer["no_data_update"]);
        }
        var entities = await (DBSet.Where(t => t.dispatch_no == dispatch_no && t.TenantId == currentUser.tenant_id)).ToListAsync();
        var delete_id_list = new List<int>();
        var sku_id_list = viewModels?.Select(t => t.sku_id).ToList();
        if (sku_id_list == null || sku_id_list.Count == 0)
        {
            return (false, "[201]" + _stringLocalizer["no_data_update"]);
        }
        var skus = await (_dBContext.GetDbSet<SkuEntity>().AsNoTracking().Where(t => sku_id_list.Contains(t.Id))).ToListAsync();
        var now_time = DateTime.UtcNow;
        if (entities.Any(t => t.dispatch_status != 1 && t.dispatch_status != 0))
        {
            return (false, "[202]" + _stringLocalizer["data_changed"]);
        }
        foreach (var vm in viewModels)
        {
            if (vm.id < 0)
            {
                var entity = entities.FirstOrDefault(t => t.Id == -vm.id);
                if (entity == null)
                {
                    return (false, "[202]" + _stringLocalizer["data_changed"]);
                }
                DBSet.Remove(entity);
                delete_id_list.Add(entity.Id);
            }
            else if (vm.id > 0)
            {
                var entity = entities.FirstOrDefault(t => t.Id == vm.id);
                if (entity == null)
                {
                    return (false, "[202]" + _stringLocalizer["data_changed"]);
                }
                entity.sku_id = vm.sku_id;
                entity.qty = vm.qty;
                entity.last_update_time = now_time;
                var sku = skus.FirstOrDefault(t => t.Id == entity.sku_id);
                if (sku != null)
                {
                    entity.volume = sku.volume.GetValueOrDefault() * entity.qty;
                    entity.weight = sku.weight.GetValueOrDefault() * entity.qty;
                }
            }
            else if (vm.id == 0)
            {

                var entity = new DispatchlistEntity
                {
                    Id = 0,
                    dispatch_no = dispatch_no,
                    creator = currentUser.user_name,
                    create_time = now_time,
                    last_update_time = now_time,
                    dispatch_status = (byte)dispatch_status,
                    sku_id = vm.sku_id,
                    qty = vm.qty
                };
                var sku = skus.FirstOrDefault(t => t.Id == entity.sku_id);
                if (sku != null)
                {
                    entity.volume = sku.volume.GetValueOrDefault() * entity.qty;
                    entity.weight = sku.weight.GetValueOrDefault() * entity.qty;
                }
                entities.Add(entity);
                DBSet.Add(entity);
            }
        }
        var repeat_skus_id_list = entities.Where(t => !delete_id_list.Contains(t.Id)).GroupBy(t => t.sku_id).Select(t => new { t.Key, cnt = t.Count() }).Where(t => t.cnt > 1).Select(t => t.Key).ToList();
        if (repeat_skus_id_list.Count > 0)
        {
            var repeat_skus = (skus.Where(t => repeat_skus_id_list.Contains(t.Id)).Select(t => t.sku_code).ToList());
            var msg = "";
            foreach (var sku in repeat_skus)
            {
                msg += string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["sku_code"], sku);
            }
            return (false, msg);
        }
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
    /// get pick list by dispatch_id
    /// </summary>
    /// <param name="dispatch_id">dispatch_id</param>
    /// <returns></returns>
    public async Task<List<DispatchpicklistViewModel>> GetPickListByDispatchID(int dispatch_id)
    {
        var datas = await (from dpl in _dBContext.GetDbSet<DispatchpicklistEntity>().AsNoTracking()
                           join sku in _dBContext.GetDbSet<SkuEntity>().AsNoTracking() on dpl.sku_id equals sku.Id
                           join spu in _dBContext.GetDbSet<SpuEntity>().AsNoTracking() on sku.spu_id equals spu.Id
                           join owner in _dBContext.GetDbSet<GoodsownerEntity>().AsNoTracking() on dpl.goods_owner_id equals owner.Id into o_left
                           from owner in o_left.DefaultIfEmpty()
                           join location in _dBContext.GetDbSet<GoodslocationEntity>().AsNoTracking() on dpl.goods_location_id equals location.Id
                           where dpl.dispatchlist_id == dispatch_id
                           select new DispatchpicklistViewModel
                           {
                               id = dpl.Id,
                               dispatchlist_id = dpl.dispatchlist_id,
                               goods_owner_id = dpl.goods_owner_id,
                               goods_location_id = dpl.goods_location_id,
                               sku_id = dpl.sku_id,
                               pick_qty = dpl.pick_qty,
                               picked_qty = dpl.picked_qty,
                               goods_owner_name = owner.goods_owner_name == null ? "" : owner.goods_owner_name,
                               sku_code = sku.sku_code,
                               spu_code = spu.spu_code,
                               spu_description = spu.spu_description,
                               spu_name = spu.spu_name,
                               bar_code = sku.bar_code,
                               LocationName = location.LocationName,
                               warehouse_area_name = location.WarehouseAreaName,
                               WarehouseAreaProperty = location.WarehouseAreaProperty,
                               WarehouseName = location.WarehouseName,
                               series_number = dpl.series_number,
                               expiry_date = dpl.expiry_date,
                               price = dpl.price,
                               picker = dpl.picker,
                               picker_id = dpl.picker_id,
                               putaway_date = dpl.putaway_date,
                           }).ToListAsync();
        return datas;
    }

    /// <summary>
    /// advanced dispatch order page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(List<PreDispatchlistViewModel> data, int totals)> AdvancedDispatchlistPageAsync(PageSearch pageSearch, CurrentUser currentUser)
    {
        try
        {


            QueryCollection queries = new QueryCollection();
            if (pageSearch.searchObjects.Any())
            {
                pageSearch.searchObjects.ForEach(s =>
                {
                    queries.Add(s);
                });
            }
            var DbSet = _dBContext.GetDbSet<DispatchlistEntity>();
            var query = from d in DbSet.AsNoTracking().Where(t => t.TenantId.Equals(currentUser.tenant_id))
                        //join sku in _dBContext.GetDbSet<SkuEntity>().AsNoTracking() on d.sku_id equals sku.id
                        //join spu in _dBContext.GetDbSet<SpuEntity>().AsNoTracking() on sku.spu_id equals spu.id
                        group new { d/*, spu*/ } by new { d.dispatch_no, d.dispatch_status, d.customer_id, d.customer_name, d.creator }
                        into dg
                        select new PreDispatchlistViewModel
                        {
                            dispatch_no = dg.Key.dispatch_no,
                            dispatch_status = dg.Key.dispatch_status,
                            customer_id = dg.Key.customer_id,
                            customer_name = dg.Key.customer_name,
                            qty = dg.Sum(t => t.d.qty),
                            creator = dg.Key.creator,
                            /*dispatch_no = dg.Key.dispatch_no,
                            dispatch_status = dg.Key.dispatch_status,
                            customer_id = dg.Key.customer_id,
                            customer_name = dg.Key.customer_name,
                            qty = dg.Sum(t => t.d.qty),
                            volume = dg.Sum(t =>t.spu.volume_unit==1?  t.d.volume:(t.spu.volume_unit==0?t.d.volume/1000:t.d.volume*1000)),
                            weight = dg.Sum(t =>t.spu.weight_unit==0?t.d.weight/1000000:(t.spu.weight_unit==1? t.d.weight/1000:t.d.weight)),
                            creator = dg.Key.creator,*/
                        };
            query = query.Where(queries.AsExpression<PreDispatchlistViewModel>());
            if (pageSearch.sqlTitle.Contains("dispatch_status"))
            {
                var dispatch_status = Convert.ToByte(pageSearch.sqlTitle.Trim().ToLower().Replace("dispatch_status", "").Replace("：", "").Replace(":", "").Replace("=", ""));
                query = query.Where(t => t.dispatch_status.Equals(dispatch_status));
            }
            else if (pageSearch.sqlTitle.Equals("todo"))
            {
                query = query.Where(t => t.dispatch_status >= 2 && t.dispatch_status <= 5);
            }
            int totals = await query.CountAsync();
            var list = await query.OrderByDescending(t => t.dispatch_no)
                       .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                       .Take(pageSearch.pageSize)
                       .ToListAsync();

            //#region sqlite cannot sum data of decimal type

            // var dispatch_no_list = list.Select(t => t.dispatch_no).Distinct().ToList();
            // var d_datas = await (from d in DbSet.AsNoTracking()
            //                      join sku in _dBContext.GetDbSet<SkuEntity>().AsNoTracking() on d.sku_id equals sku.id
            //                      join spu in _dBContext.GetDbSet<SpuEntity>().AsNoTracking() on sku.spu_id equals spu.id
            //                      where d.tenant_id == currentUser.tenant_id && dispatch_no_list.Contains(d.dispatch_no)
            //                      select new
            //                      {
            //                          d.dispatch_no,
            //                          volume = spu.volume_unit == 1 ? d.volume : (spu.volume_unit == 0 ? d.volume / 1000 : d.volume * 1000),
            //                          weight = spu.weight_unit == 0 ? d.weight / 1000000 : (spu.weight_unit == 1 ? d.weight / 1000 : d.weight)
            //                      }).ToListAsync();
            // list.ForEach(t =>
            // {
            //     t.volume = d_datas.Where(d => d.dispatch_no == t.dispatch_no).Sum(t => t.volume);
            //     t.weight = d_datas.Where(d => d.dispatch_no == t.dispatch_no).Sum(t => t.weight);
            // });
            #region sqlite cannot sum data of decimal type

            // Tách nhỏ truy vấn d_datas để debug lỗi syntax
            var dispatch_no_list = list.Select(t => t.dispatch_no).Distinct().ToList();

            // 1. Lấy DbSet
            var dbSetDispatch = DbSet.AsNoTracking();
            var dbSetSku = _dBContext.GetDbSet<SkuEntity>().AsNoTracking();
            var dbSetSpu = _dBContext.GetDbSet<SpuEntity>().AsNoTracking();

            // 2. Join dispatch với sku
            var joinDispatchSku = from d in dbSetDispatch
                                  join sku in dbSetSku on d.sku_id equals sku.Id
                                  select new { d, sku };

            // 3. Join tiếp với spu
            var joinDispatchSkuSpu = from ds in joinDispatchSku
                                     join spu in dbSetSpu on ds.sku.spu_id equals spu.Id
                                     select new { ds.d, ds.sku, spu };

            // 4. Where điều kiện
            var filtered = dispatch_no_list.Any()
                            ? joinDispatchSkuSpu
                                .Where(x => x.d.TenantId == currentUser.tenant_id && dispatch_no_list.Contains(x.d.dispatch_no))
                                .Select(x => new { d = x.d, sku = x.sku, spu = x.spu })
                            : joinDispatchSkuSpu
                                .Where(x => x.d.TenantId == currentUser.tenant_id)
                                .Select(x => new { d = x.d, sku = x.sku, spu = x.spu });
            // var sql = filtered.Select(x => new { x.d, x.sku, x.spu }).ToQueryString();
            // 5. Select kết quả
            var d_datas = await filtered.Select(x => new
            {
                x.d.dispatch_no,
                volume = x.spu.volume_unit == 1 ? x.d.volume : (x.spu.volume_unit == 0 ? x.d.volume / 1000 : x.d.volume * 1000),
                weight = x.spu.weight_unit == 0 ? x.d.weight / 1000000 : (x.spu.weight_unit == 1 ? x.d.weight / 1000 : x.d.weight)
            }).ToListAsync();

            // Nếu muốn xem SQL sinh ra (EF Core >= 5.0)
            // var sql = filtered.Select(x => new { ... }).ToQueryString();
            // Console.WriteLine(sql);

            list.ForEach(t =>
            {
                t.volume = d_datas.Where(d => d.dispatch_no == t.dispatch_no).Sum(t => t.volume);
                t.weight = d_datas.Where(d => d.dispatch_no == t.dispatch_no).Sum(t => t.weight);
            });

            #endregion sqlite cannot sum data of decimal type

            return (list, totals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdvancedDispatchlistPageAsync");
            return (null, 0);
        }
    }

    /// <summary>
    /// Get dispatchlist by dispatch_no
    /// </summary>
    /// <returns></returns>
    public async Task<List<DispatchlistDetailViewModel>> GetAllAsync(string dispatch_no, CurrentUser currentUser)
    {
        var DbSet = _dBContext.GetDbSet<DispatchlistEntity>();
        var Spus = _dBContext.GetDbSet<SpuEntity>();
        var Skus = _dBContext.GetDbSet<SkuEntity>();
        var data = await (from dl in DbSet.AsNoTracking()
                          join sku in Skus.AsNoTracking() on dl.sku_id equals sku.Id
                          join spu in Spus.AsNoTracking() on sku.spu_id equals spu.Id
                          where dl.dispatch_no == dispatch_no && dl.TenantId == currentUser.tenant_id
                          select new DispatchlistDetailViewModel
                          {
                              id = dl.Id,
                              dispatch_no = dl.dispatch_no,
                              sku_code = sku.sku_code,
                              spu_code = spu.spu_code,
                              spu_description = spu.spu_description,
                              spu_name = spu.spu_name,
                              bar_code = sku.bar_code,
                              dispatch_status = dl.dispatch_status,
                              customer_id = dl.customer_id,
                              customer_name = dl.customer_name,
                              sku_id = dl.sku_id,
                              qty = dl.qty,
                              weight = dl.weight,
                              volume = dl.volume,
                              creator = dl.creator,
                              create_time = dl.create_time,
                              damage_qty = dl.damage_qty,
                              lock_qty = dl.lock_qty,
                              picked_qty = dl.picked_qty,
                              intrasit_qty = dl.intrasit_qty,
                              package_qty = dl.package_qty,
                              weighing_qty = dl.weighing_qty,
                              actual_qty = dl.actual_qty,
                              sign_qty = dl.sign_qty,
                              package_no = dl.package_no,
                              package_person = dl.package_person,
                              package_time = dl.package_time,
                              weighing_no = dl.weighing_no,
                              weighing_person = dl.weighing_person,
                              weighing_weight = dl.weighing_weight,
                              waybill_no = dl.waybill_no,
                              carrier = dl.carrier,
                              freightfee = dl.freightfee,
                              pick_checker = dl.pick_checker,
                              pick_checker_id = dl.pick_checker_id,
                          }
                          ).ToListAsync();
        return data.Adapt<List<DispatchlistDetailViewModel>>();
    }

    /// <summary>
    /// add a new Dispatchlist
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> AddAsync(List<DispatchlistAddViewModel> viewModel, CurrentUser currentUser)
    {
        try
        {
            var dispatchListDbSet = _dBContext.GetDbSet<DispatchlistEntity>();
            var entities = viewModel.Adapt<List<DispatchlistEntity>>();
            var skuIds = entities.Select(t => t.sku_id).Distinct().ToList();

            var skus = await _dBContext.GetDbSet<SkuEntity>()
                                       .Where(t => skuIds.Contains(t.Id))
                                       .ToListAsync();

            var missingSkuIds = skuIds.Except(skus.Select(s => s.Id)).ToList();
            if (missingSkuIds.Any())
            {
                var missingSkuCodes = string.Join(", ", missingSkuIds);
                _logger.LogWarning("The following SKU IDs do not exist: {MissingSkuCodes}", missingSkuCodes);
                return (false, _stringLocalizer["sku_not_exist"]);
            }

            var dispatch_no = await _functionHelper.GetFormNoAsync("dispatchlist", "Outbound");
            var now_time = DateTime.UtcNow;
            foreach (var entity in entities)
            {
                var sku = skus.FirstOrDefault(t => t.Id == entity.sku_id);
                entity.Id = 0;
                entity.create_time = now_time;
                entity.creator = currentUser.user_name;
                entity.last_update_time = now_time;
                entity.TenantId = currentUser.tenant_id;
                if (sku != null)
                {
                    entity.volume = entity.qty * sku.volume.GetValueOrDefault();
                    entity.weight = entity.qty * sku.weight.GetValueOrDefault();
                }
                entity.dispatch_no = dispatch_no;
            }
            await dispatchListDbSet.AddRangeAsync(entities);
            var qty = await _dBContext.SaveChangesAsync();
            if (qty > 0)
            {
                return (true, _stringLocalizer["save_success"]);
            }
            else
            {
                _logger.LogWarning("Failed to save new Dispatchlist records for dispatch_no: {DispatchNo}", dispatch_no);
                return (false, _stringLocalizer["save_failed"]);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AddAsync: {Message}", ex.Message);
            return (false, _stringLocalizer["save_failed"]);
        }
    }

    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="dispatch_no">dispatch_no</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> DeleteAsync(string dispatch_no, CurrentUser currentUser)
    {
        var entities = await _dBContext.GetDbSet<DispatchlistEntity>().Where(t => t.dispatch_no.Equals(dispatch_no) && t.TenantId == currentUser.tenant_id).ToListAsync();
        if (entities.Any(t => t.dispatch_status > 1))
        {
            return (false, _stringLocalizer["status_not_delete"]);
        }
        _dBContext.GetDbSet<DispatchlistEntity>().RemoveRange(entities);
        var qty = await _dBContext.SaveChangesAsync();
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
    /// Dispatchlist details with available stock
    /// </summary>
    /// <param name="dispatch_no">dispatch_no</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<List<DispatchlistConfirmDetailViewModel>> ConfirmOrderCheck(string dispatch_no, CurrentUser currentUser)
    {
        var DbSet = _dBContext.GetDbSet<DispatchlistEntity>();
        var dispatchpick_DBSet = _dBContext.GetDbSet<DispatchpicklistEntity>();
        var stock_DbSet = _dBContext.GetDbSet<StockEntity>();
        var asn_DBSet = _dBContext.GetDbSet<AsnEntity>();
        var sku_DBSet = _dBContext.GetDbSet<SkuEntity>().AsNoTracking();
        var spu_DBSet = _dBContext.GetDbSet<SpuEntity>().AsNoTracking();
        var processdetail_DBSet = _dBContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();
        var move_DBSet = _dBContext.GetDbSet<StockmoveEntity>();
        var owner_DBSet = _dBContext.GetDbSet<GoodsownerEntity>();
        var location_DBSet = _dBContext.GetDbSet<GoodslocationEntity>();

        var stock_group_datas = from stock in stock_DbSet.AsNoTracking()
                                join gl in _dBContext.GetDbSet<GoodslocationEntity>().AsNoTracking() on stock.goods_location_id equals gl.Id
                                where stock.TenantId == currentUser.tenant_id
                                group stock by new
                                {
                                    stock.sku_id,
                                    stock.goods_location_id,
                                    stock.goods_owner_id,
                                    stock.series_number // <--- GIỮ LẠI SERIES NUMBER
                                } into sg
                                select new
                                {
                                    stock_id = 0,
                                    goods_owner_id = sg.Key.goods_owner_id,
                                    sku_id = sg.Key.sku_id,
                                    goods_location_id = sg.Key.goods_location_id,
                                    series_number = sg.Key.series_number, // <--- CÓ DỮ LIỆU

                                    // Các thông tin khác lấy đại diện (Vì cùng series thường sẽ cùng date/price)
                                    expiry_date = sg.Min(x => x.expiry_date),
                                    price = sg.Max(x => x.price),
                                    PutAwayDate = sg.Min(x => x.PutAwayDate),

                                    qty_frozen = sg.Where(t => t.is_freeze == true).Sum(e => e.qty),
                                    qty = sg.Sum(t => t.qty)
                                };

        var dispatch_group_datas = from dp in DbSet.AsNoTracking()
                                   join dpp in dispatchpick_DBSet.AsNoTracking() on dp.Id equals dpp.dispatchlist_id
                                   where dp.dispatch_status > 1 && dp.dispatch_status < 6
                                   group dpp by new
                                   {
                                       dpp.sku_id,
                                       dpp.goods_location_id,
                                       dpp.goods_owner_id,
                                       dpp.series_number // <--- Thêm vào
                                   } into dg
                                   select new
                                   {
                                       goods_owner_id = dg.Key.goods_owner_id,
                                       sku_id = dg.Key.sku_id,
                                       goods_location_id = dg.Key.goods_location_id,
                                       series_number = dg.Key.series_number,
                                       qty_locked = dg.Sum(t => t.pick_qty)
                                   };


        var process_locked_group_datas = from pd in processdetail_DBSet
                                         where pd.is_update_stock == false && pd.is_source == true
                                         group pd by new
                                         {
                                             pd.sku_id,
                                             pd.goods_location_id,
                                             pd.goods_owner_id,
                                             pd.series_number
                                         } into pdg
                                         select new
                                         {
                                             goods_owner_id = pdg.Key.goods_owner_id,
                                             sku_id = pdg.Key.sku_id,
                                             goods_location_id = pdg.Key.goods_location_id,
                                             series_number = pdg.Key.series_number,
                                             qty_locked = pdg.Sum(t => t.qty)
                                         };


        var move_locked_group_datas = from m in move_DBSet.AsNoTracking()
                                      where m.move_status == 0
                                      group m by new
                                      {
                                          m.sku_id,
                                          m.orig_goods_location_id,
                                          m.goods_owner_id,
                                          m.series_number
                                      } into mg
                                      select new
                                      {
                                          goods_owner_id = mg.Key.goods_owner_id,
                                          sku_id = mg.Key.sku_id,
                                          goods_location_id = mg.Key.orig_goods_location_id,
                                          series_number = mg.Key.series_number,
                                          qty_locked = mg.Sum(t => t.qty)
                                      };

        var datas = await (from dl in DbSet

                           // Join Stock Available
                           join sg in stock_group_datas on dl.sku_id equals sg.sku_id into sg_left
                           from sg in sg_left.DefaultIfEmpty()

                               // Join Dispatch Locked
                           join dp in dispatch_group_datas on
                           new
                           {
                               sku_id = (int?)sg.sku_id,
                               goods_location_id = (int?)sg.goods_location_id,
                               goods_owner_id = (int?)sg.goods_owner_id,
                               series_number = sg.series_number
                           }
                           equals
                           new
                           {
                               sku_id = (int?)dp.sku_id,
                               goods_location_id = (int?)dp.goods_location_id,
                               goods_owner_id = (int?)dp.goods_owner_id,
                               series_number = dp.series_number
                           } into dp_left
                           from dp in dp_left.DefaultIfEmpty()

                               // Join Process Locked
                           join pl in process_locked_group_datas on
                           new
                           {
                               sku_id = (int?)sg.sku_id,
                               goods_location_id = (int?)sg.goods_location_id,
                               goods_owner_id = (int?)sg.goods_owner_id,
                               series_number = sg.series_number
                           }
                           equals
                           new
                           {
                               sku_id = (int?)pl.sku_id,
                               goods_location_id = (int?)pl.goods_location_id,
                               goods_owner_id = (int?)pl.goods_owner_id,
                               series_number = pl.series_number
                           } into pl_left
                           from pl in pl_left.DefaultIfEmpty()

                               // Join Move Locked
                           join m in move_locked_group_datas on
                           new
                           {
                               sku_id = (int?)sg.sku_id,
                               goods_location_id = (int?)sg.goods_location_id,
                               goods_owner_id = (int?)sg.goods_owner_id,
                               series_number = sg.series_number
                           }
                           equals
                           new
                           {
                               sku_id = (int?)m.sku_id,
                               goods_location_id = (int?)m.goods_location_id,
                               goods_owner_id = (int?)m.goods_owner_id,
                               series_number = m.series_number
                           } into m_left
                           from m in m_left.DefaultIfEmpty()

                               // Join Master Data
                           join sku in sku_DBSet on dl.sku_id equals sku.Id
                           join spu in spu_DBSet on sku.spu_id equals spu.Id
                           join owner in owner_DBSet.AsNoTracking() on sg.goods_owner_id equals owner.Id into owner_left
                           from owner in owner_left.DefaultIfEmpty()
                           join gl in location_DBSet.Where(t => t.WarehouseAreaProperty != 5).AsNoTracking() on sg.goods_location_id equals gl.Id into gl_left
                           from gl in gl_left.DefaultIfEmpty()

                           where dl.TenantId == currentUser.tenant_id && dl.dispatch_no == dispatch_no && (dl.dispatch_status == 0 || dl.dispatch_status == 1)

                           select new
                           {

                               stock_id = 0,

                               goods_owner_name = owner.goods_owner_name ?? "",
                               goods_location_id = (int?)sg.goods_location_id ?? 0,
                               goods_owner_id = (int?)sg.goods_owner_id ?? 0,
                               spu_name = spu.spu_name,
                               spu_code = spu.spu_code,
                               sku_code = sku.sku_code,
                               sku_name = sku.sku_name,
                               qty_available = ((int?)sg.qty ?? 0)
                                               - ((int?)sg.qty_frozen ?? 0)
                                               - ((int?)dp.qty_locked ?? 0)
                                               - ((int?)pl.qty_locked ?? 0)
                                               - ((int?)m.qty_locked ?? 0),

                               qty = dl.qty,
                               sku_id = dl.sku_id,
                               id = dl.Id,
                               spu_description = spu.spu_description,
                               dispatch_status = dl.dispatch_status,
                               bar_code = sku.bar_code,
                               customer_id = dl.customer_id,
                               customer_name = dl.customer_name,
                               dispatch_no = dl.dispatch_no,
                               location_name = gl.LocationName ?? "",
                               warehouse_area_name = gl.WarehouseAreaName ?? "",
                               WarehouseName = gl.WarehouseName ?? "",

                               // Vẫn giữ được Series Number
                               series_number = sg.series_number ?? "",
                               expiry_date = (DateTime?)sg.expiry_date ?? DateTime.UtcNow,
                               price = (decimal?)sg.price ?? 0,
                               putaway_date = (DateTime?)sg.PutAwayDate ?? DateTime.UtcNow,
                           }).ToListAsync();

        var res = (from d in datas
                   group d by new
                   {
                       d.spu_name,
                       d.spu_code,
                       d.sku_code,
                       d.sku_name,
                       d.qty,
                       d.sku_id,
                       d.id,
                       d.spu_description,
                       d.dispatch_status,
                       d.bar_code,
                       d.customer_id,
                       d.customer_name,
                       d.dispatch_no,
                   }
               into dg
                   select new DispatchlistConfirmDetailViewModel
                   {
                       dispatchlist_id = dg.Key.id,
                       sku_id = dg.Key.sku_id,
                       dispatch_no = dg.Key.dispatch_no,
                       sku_code = dg.Key.sku_code,
                       spu_code = dg.Key.spu_code,
                       dispatch_status = dg.Key.dispatch_status,
                       spu_description = dg.Key.spu_description,
                       spu_name = dg.Key.spu_name,
                       sku_name = dg.Key.sku_name,
                       bar_code = dg.Key.bar_code,
                       customer_id = dg.Key.customer_id,
                       customer_name = dg.Key.customer_name,
                       qty = dg.Key.qty, // Số lượng khách đặt
                       qty_available = dg.Sum(t => t.qty_available), // Tổng số lượng có thể đáp ứng
                       confirm = dg.Key.qty <= dg.Sum(t => t.qty_available)
                   }).ToList();
        foreach (var r in res)
        {
            var picklist = (from d in datas.Where(t => t.sku_id == r.sku_id && t.goods_location_id > 0)
                            // Ưu tiên: Lấy nơi có nhiều hàng nhất trước (để giảm số lần đi lấy hàng)
                            // Nếu muốn ưu tiên hạn dùng: .OrderBy(o => o.expiry_date)
                            .OrderByDescending(o => o.qty_available)
                            select new DispatchlistConfirmPickDetailViewModel
                            {
                                stock_id = d.stock_id, // Lưu ý: Giá trị này là 0
                                dispatchlist_id = r.dispatchlist_id,
                                goods_location_id = d.goods_location_id,
                                qty_available = d.qty_available,
                                goods_owner_id = d.goods_owner_id,
                                goods_owner_name = d.goods_owner_name,
                                location_name = d.location_name,
                                warehouse_area_name = d.warehouse_area_name,
                                sku_code = d.sku_code,
                                sku_name = d.sku_name,
                                WarehouseName = d.WarehouseName,
                                pick_qty = 0,
                                series_number = d.series_number, // Có series number
                                expiry_date = d.expiry_date,
                                price = d.price,
                                putaway_date = d.putaway_date,

                            }
                          ).ToList();

            // Thuật toán tham lam (Greedy) để chia số lượng
            int pick_qty = 0;
            foreach (var pick in picklist)
            {
                if (pick_qty >= r.qty) break; // Đã lấy đủ
                int needed = r.qty - pick_qty;
                pick.pick_qty = (needed <= pick.qty_available) ? needed : pick.qty_available;

                pick_qty += pick.pick_qty;
            }

            // Chỉ lấy những dòng có số lượng pick > 0
            //  r.pick_list = picklist.Where(t => t.pick_qty > 0).ToList();
            r.pick_list = picklist.ToList();
        }

        return res;

    }

    /// <summary>
    ///  Confirm orders and create  dispatchpicklist
    /// </summary>
    /// <param name="viewModels">viewModels</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ConfirmOrder(List<DispatchlistConfirmDetailViewModel> viewModels, CurrentUser currentUser)
    {
        try
        {
            var DBSet = _dBContext.GetDbSet<DispatchlistEntity>();
            var dispatchlist_id_list = viewModels.Select(t => t.dispatchlist_id).ToList();
            var dispatchlist_datas = await DBSet.Where(t => dispatchlist_id_list.Contains(t.Id)).ToListAsync();
            var pick_DBSet = _dBContext.GetDbSet<DispatchpicklistEntity>();
            var stock_DBSet = _dBContext.GetDbSet<StockEntity>();
            var pick_datas = new List<DispatchpicklistEntity>();
            var stock_id_list = new List<int>();
            var processdetail_DBSet = _dBContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();
            var move_DBSet = _dBContext.GetDbSet<StockmoveEntity>();
            var new_dispatchlists = new List<DispatchlistEntity>();
            var topick_viewmodels = new List<StockViewModel>();
            var sku_id_list = viewModels.Select(t => t.sku_id).ToList();
            var now_time = DateTime.UtcNow;
            foreach (var vm in viewModels.Where(t => t.confirm == true).ToList())
            {
                stock_id_list.AddRange(vm.pick_list.Where(t => t.pick_qty > 0).Select(t => t.stock_id).ToList());
            }

            foreach (var vm in viewModels)
            {
                var d = dispatchlist_datas.Where(t => t.Id == vm.dispatchlist_id).FirstOrDefault();
                if (d == null)
                {
                    return (false, _stringLocalizer["data_changed"]);
                }
                if (vm.confirm == true)
                {
                    d.dispatch_status = 2;
                    d.last_update_time = now_time;
                    d.lock_qty = vm.pick_list.Sum(t => t.pick_qty);
                    foreach (var p in vm.pick_list.Where(t => t.pick_qty > 0).ToList())
                    {
                        pick_datas.Add(new DispatchpicklistEntity
                        {
                            sku_id = vm.sku_id,
                            is_update_stock = false,
                            dispatchlist_id = p.dispatchlist_id,
                            goods_location_id = p.goods_location_id,
                            goods_owner_id = p.goods_owner_id,
                            last_update_time = now_time,
                            series_number = p.series_number,
                            expiry_date = p.expiry_date,
                            price = p.price,
                            pick_qty = p.pick_qty,
                            putaway_date = p.putaway_date,
                        });
                        topick_viewmodels.Add(new StockViewModel { id = p.stock_id, qty = p.pick_qty });
                    }
                    if (d.lock_qty < d.qty)
                    {
                        new_dispatchlists.Add(new DispatchlistEntity
                        {
                            sku_id = vm.sku_id,
                            dispatch_status = 1,
                            qty = d.qty - d.lock_qty,
                            TenantId = currentUser.tenant_id,
                            customer_id = d.customer_id,
                            customer_name = d.customer_name,
                        });
                        d.qty = d.lock_qty;
                    }
                }
                else
                {
                    new_dispatchlists.Add(new DispatchlistEntity
                    {
                        sku_id = vm.sku_id,
                        dispatch_status = 1,
                        qty = vm.qty,
                        TenantId = currentUser.tenant_id,
                        customer_id = d.customer_id,
                        customer_name = d.customer_name,
                    });
                    DBSet.Remove(d);
                }
            }
            var stock_group_datas = from stock in stock_DBSet.AsNoTracking()
                                    where stock_id_list.Contains(stock.Id)
                                    group stock by new { stock.Id, stock.sku_id, stock.goods_location_id, stock.goods_owner_id, stock.series_number, stock.expiry_date, stock.price, stock.PutAwayDate } into sg
                                    select new
                                    {
                                        stock_id = sg.Key.Id,
                                        goods_owner_id = sg.Key.goods_owner_id,
                                        sku_id = sg.Key.sku_id,
                                        goods_location_id = sg.Key.goods_location_id,
                                        series_number = sg.Key.series_number,
                                        sg.Key.expiry_date,
                                        sg.Key.price,
                                        sg.Key.PutAwayDate,
                                        qty_frozen = sg.Where(t => t.is_freeze == true).Sum(e => e.qty),
                                        qty = sg.Sum(t => t.qty)
                                    };
            var dispatch_group_datas = from dp in DBSet.AsNoTracking()
                                       join dpp in pick_DBSet.AsNoTracking() on dp.Id equals dpp.dispatchlist_id
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
                                           dg.Key.putaway_date,
                                           qty_locked = dg.Sum(t => t.pick_qty)
                                       };
            var process_locked_group_datas = from pd in processdetail_DBSet
                                             where pd.is_update_stock == false
                                             group pd by new { pd.sku_id, pd.goods_location_id, pd.goods_owner_id, pd.series_number, pd.expiry_date, pd.price, pd.putaway_date } into pdg
                                             select new
                                             {
                                                 goods_owner_id = pdg.Key.goods_owner_id,
                                                 sku_id = pdg.Key.sku_id,
                                                 goods_location_id = pdg.Key.goods_location_id,
                                                 series_number = pdg.Key.series_number,
                                                 pdg.Key.expiry_date,
                                                 pdg.Key.price,
                                                 pdg.Key.putaway_date,
                                                 qty_locked = pdg.Sum(t => t.qty)
                                             };
            var move_locked_group_datas = from m in move_DBSet.AsNoTracking()
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
                                              mg.Key.putaway_date,
                                              qty_locked = mg.Sum(t => t.qty)
                                          };
            var stock_datas = await (from sg in stock_group_datas
                                     join dp in dispatch_group_datas on
                          new
                          {
                              sku_id = (int?)sg.sku_id,
                              goods_location_id = (int?)sg.goods_location_id,
                              goods_owner_id = (int?)sg.goods_owner_id,
                              series_number = sg.series_number,
                              expiry_date = (DateTime?)sg.expiry_date,
                              price = sg.price,
                              putaway_date = (DateTime?)sg.PutAwayDate
                          } equals
                          new
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
                               putaway_date = (DateTime?)sg.PutAwayDate
                           }
                           equals
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
                               putaway_date = (DateTime?)sg.PutAwayDate
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
                                     select new
                                     {
                                         stock_id = sg.stock_id,
                                         qty_available = ((int?)sg.qty ?? 0)
                                                          - ((int?)sg.qty_frozen ?? 0)
                                                          - ((int?)dp.qty_locked ?? 0)
                                                          - ((int?)pl.qty_locked ?? 0)
                                                          - ((int?)m.qty_locked ?? 0),
                                     }).ToListAsync();
            var if_not_stock = (from tp in topick_viewmodels
                                join s in stock_datas on tp.id equals s.stock_id
                                where tp.qty > s.qty_available
                                select tp).Any();
            if (if_not_stock)
            {
                return (false, _stringLocalizer["data_changed"]);
            }
            await pick_DBSet.AddRangeAsync(pick_datas);
            var dispatch_no = await _functionHelper.GetFormNoAsync("Dispatchlist");
            var sku_datas = await _dBContext.GetDbSet<SkuEntity>().Where(t => sku_id_list.Contains(t.Id)).ToListAsync();
            foreach (var nd in new_dispatchlists)
            {
                nd.dispatch_no = dispatch_no;
                nd.creator = currentUser.user_name;
                nd.create_time = DateTime.UtcNow;
                var sku = sku_datas.FirstOrDefault(e => e.Id == nd.sku_id);
                if (sku != null)
                {
                    nd.weight = nd.qty * sku.weight.GetValueOrDefault();
                    nd.volume = nd.qty * sku.volume.GetValueOrDefault();
                }
            }
           ;
            await DBSet.AddRangeAsync(new_dispatchlists);
            var qty = await _dBContext.SaveChangesAsync();

            return (true, _stringLocalizer["operation_success"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ConfirmOrder");
            return (false, _stringLocalizer["operation_failed"]);
        }
    }
    /// <summary>
    ///  cancel order opration
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> CancelOrderOpration(CancelOrderOprationViewModel viewModel, CurrentUser currentUser)
    {
        var DBSet = _dBContext.GetDbSet<DispatchlistEntity>();
        var pick_DBSet = _dBContext.GetDbSet<DispatchpicklistEntity>();
        var entities = await DBSet.Where(t => t.dispatch_no == viewModel.dispatch_no && t.TenantId == currentUser.tenant_id && t.dispatch_status == viewModel.dispatch_status).ToListAsync();
        if (entities.Count == 0)
        {
            return (false, _stringLocalizer["status_changed"]);
        }
        var now_time = DateTime.UtcNow;
        var dispatch_id_list = entities.Select(t => t.Id).ToList();
        var pick_entities = await pick_DBSet.Where(t => dispatch_id_list.Contains(t.dispatchlist_id)).ToListAsync();
        if (viewModel.dispatch_status == 3)
        {
            foreach (var pick in pick_entities)
            {
                pick.picked_qty = 0;
                pick.last_update_time = now_time;
            }
            foreach (var entity in entities)
            {
                entity.picked_qty = 0;
                entity.last_update_time = now_time;
                entity.dispatch_status = 2;
            }
        }
        else if (viewModel.dispatch_status == 2)
        {
            pick_DBSet.RemoveRange(pick_entities);
            foreach (var entity in entities)
            {
                entity.lock_qty = 0;
                entity.last_update_time = now_time;
                entity.dispatch_status = 1;
            }
        }
        var saved = false;
        int res = 0;
        while (!saved)
        {
            try
            {
                // Attempt to save changes to the database
                res = await _dBContext.SaveChangesAsync();
                saved = true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    if (entry.Entity is DispatchlistEntity)
                    {
                        var proposedValues = entry.CurrentValues;
                        var databaseValues = entry.GetDatabaseValues();
                        if (UtilConvert.ObjToInt(databaseValues["dispatch_status"]) != viewModel.dispatch_status)
                            return (false, "[202]" + _stringLocalizer["data_changed"]);
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
    /// cancel dispatchlist detail opration
    /// </summary>
    /// <param name="id">dispatchlist_id</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> CancelDispatchlistDetailOpration(int id)
    {
        var DBSet = _dBContext.GetDbSet<DispatchlistEntity>();
        var entity = await DBSet.Where(t => t.Id == id).FirstOrDefaultAsync();
        var now_time = DateTime.UtcNow;
        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }
        if (entity.dispatch_status == 4)
        {
            if (entity.weighing_no == "")
            {
                entity.dispatch_status = 3;
            }
            else
            {
                entity.dispatch_status = 5;
            }
            entity.package_no = "";
            entity.package_qty = 0;
            entity.package_time = DateTime.UtcNow;
            entity.package_person = "";
        }
        else if (entity.dispatch_status == 5)
        {
            if (entity.package_no == "")
            {
                entity.dispatch_status = 3;
            }
            else
            {
                entity.dispatch_status = 4;
            }
            entity.weighing_no = "";
            entity.weighing_qty = 0;
            entity.weighing_weight = 0;
            entity.weighing_person = "";
        }
        else
        {
            return (false, _stringLocalizer["status_changed"]);
        }
        entity.last_update_time = now_time;
        var qty = await _dBContext.SaveChangesAsync();
        if (qty > 0)
        {
            return (true, _stringLocalizer["operation_success"]);
        }
        else
        {
            return (false, _stringLocalizer["operation_failed"]);
        }
    }

    /// <summary>
    /// confirm dispatchpicklist picked by dispatch_no
    /// </summary>
    /// <param name="dispatch_no">dispatch_no</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ConfirmPickByDispatchNo(string dispatch_no, CurrentUser currentUser)
    {
        using var transaction = await _dBContext.Database.BeginTransactionAsync();
        try
        {
            var dispatchListDBSet = _dBContext.GetDbSet<DispatchlistEntity>();
            var pick_DBSet = _dBContext.GetDbSet<DispatchpicklistEntity>();
            var entities = await dispatchListDBSet
                            .Where(t => t.dispatch_status == 2 && t.dispatch_no == dispatch_no && t.TenantId == currentUser.tenant_id)
                            .ToListAsync();
            var dispatchlist_id_list = entities.Select(t => t.Id).ToList();
            var pick_datas = await pick_DBSet.Where(t => dispatchlist_id_list.Contains(t.dispatchlist_id)).ToListAsync();
            var now_time = DateTime.UtcNow;
            entities.ForEach(t =>
            {
                t.picked_qty = t.lock_qty;
                t.dispatch_status = 3;
                t.last_update_time = now_time;
                t.pick_checker = currentUser.user_name;
                t.pick_checker_id = currentUser.user_id;
            });
            pick_datas.ForEach(t =>
            {


                t.picked_qty = t.pick_qty;
                t.last_update_time = now_time;
            });
            // new flow : integration
            var integrationService = _serviceProvider.GetRequiredService<IIntegrationService>();
            var outboundTasks = pick_datas.Select(p => new CreateOutboundTaskDTO
            {
                PalletCode = p.series_number ?? string.Empty,
                LocationId = p.goods_location_id,
                PickUpDate = DateTime.UtcNow,
                IsActive = true,
                Priority = 1
            }).ToList();

            if (outboundTasks.Any())
            {
                var task = await integrationService.CreateOutboundEntitiesAsync(outboundTasks, currentUser);
                await integrationService.CreateOutboundIntegrationHistoryAsync(task, currentUser);
            }

            var qty = await _dBContext.SaveChangesAsync();


            await transaction.CommitAsync();
            return (true, _stringLocalizer["operation_success"]);

        }
        catch (Exception ex)
        {
            await transaction.CommitAsync();
            _logger.LogError(ex, "Exception in ConfirmPickByDispatchNo");
            return (false, _stringLocalizer["operation_failed"]);
        }
    }

    /// <summary>
    /// confirm pick detail
    /// </summary>
    /// <param name="picklist_id">dispatch list pick detail id</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ConfirmPickDetail(List<int> picklist_id, CurrentUser currentUser)
    {
        try
        {
            var DBSet = _dBContext.GetDbSet<DispatchlistEntity>();
            var pick_DBSet = _dBContext.GetDbSet<DispatchpicklistEntity>();
            var pick_datas = await pick_DBSet.Where(t => picklist_id.Contains(t.Id)).ToListAsync();
            if (pick_datas.Any(t => t.picker_id > 0) || pick_datas.Any(t => t.picked_qty > 0))
            {
                return (false, _stringLocalizer["data_changed"]);
            }
            pick_datas.ForEach(t =>
            {
                t.picker = currentUser.user_name;
                t.picker_id = currentUser.user_id;
            });
            var qty = await _dBContext.SaveChangesAsync();
            if (qty > 0)
            {
                return (true, _stringLocalizer["operation_success"]);
            }
            else
            {
                return (false, _stringLocalizer["operation_failed"]);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in ConfirmPickDetail");
            return (false, _stringLocalizer["operation_failed"]);
        }
    }

    /// <summary>
    /// cancel confirm pick detail
    /// </summary>
    /// <param name="picklist_id">dispatch list pick detail id</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> CancelConfirmPickDetail(List<int> picklist_id, CurrentUser currentUser)
    {
        var DBSet = _dBContext.GetDbSet<DispatchlistEntity>();
        var pick_DBSet = _dBContext.GetDbSet<DispatchpicklistEntity>();
        var pick_datas = await pick_DBSet.Where(t => picklist_id.Contains(t.Id)).ToListAsync();
        if (pick_datas.Any(t => t.picker_id == 0) || pick_datas.Any(t => t.picked_qty > 0))
        {
            return (false, _stringLocalizer["data_changed"]);
        }
        pick_datas.ForEach(t =>
        {
            t.picker = "";
            t.picker_id = 0;
        });
        var qty = await _dBContext.SaveChangesAsync();
        if (qty > 0)
        {
            return (true, _stringLocalizer["operation_success"]);
        }
        else
        {
            return (false, _stringLocalizer["operation_failed"]);
        }
    }

    /// <summary>
    ///  package
    /// </summary>
    /// <param name="viewModels">viewModels</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public async Task<(bool flag, string msg)> Package(List<DispatchlistPackageViewModel> viewModels, CurrentUser currentUser)
    {
        var DBSet = _dBContext.GetDbSet<DispatchlistEntity>();
        var dispatchlist_id_list = viewModels.Select(t => t.id).ToList();
        var entities = await DBSet.Where(t => dispatchlist_id_list.Contains(t.Id)).ToListAsync();
        var now_time = DateTime.UtcNow;
        var code = GetPackageOrWeightCode();
        foreach (var vm in viewModels)
        {
            var entity = entities.FirstOrDefault(t => t.Id == vm.id && t.dispatch_status == vm.dispatch_status);
            if (entity == null)
            {
                return (false, "[202]" + _stringLocalizer["data_changed"]);
            }
            if ((entity.package_qty + vm.package_qty) > entity.picked_qty)
            {
                return (false, "[202]" + _stringLocalizer["unpackgeqty_lessthen"]);
            }
            entity.last_update_time = now_time;
            entity.package_person = currentUser.user_name;
            entity.package_qty += vm.package_qty;
            entity.package_time = now_time;
            entity.package_no = code;
            entity.dispatch_status = 4;
        }
        var saved = false;
        int res = 0;
        while (!saved)
        {
            try
            {
                // Attempt to save changes to the database
                res = await _dBContext.SaveChangesAsync();
                saved = true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    if (entry.Entity is DispatchlistEntity)
                    {
                        var proposedValues = entry.CurrentValues;
                        var databaseValues = entry.GetDatabaseValues();
                        var t_vm = viewModels.FirstOrDefault(t => t.id == UtilConvert.ObjToInt(databaseValues["id"]));
                        if (t_vm == null)
                        {
                            return (false, "[202]" + _stringLocalizer["data_changed"]);
                        }
                        if (UtilConvert.ObjToInt(databaseValues["package_qty"]) + t_vm.package_qty > t_vm.picked_qty)
                        {
                            return (false, "[202]" + _stringLocalizer["data_changed"]);
                        }
                        else
                        {
                            proposedValues["package_qty"] = UtilConvert.ObjToInt(databaseValues["package_qty"]) + t_vm.package_qty;
                            proposedValues["last_update_time"] = DateTime.UtcNow;
                            if (UtilConvert.ObjToInt(databaseValues["package_qty"]) + t_vm.package_qty == UtilConvert.ObjToInt(databaseValues["picked_qty"]))
                            {
                                proposedValues["dispatch_status"] = 4;
                            }
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
    ///  weight
    /// </summary>
    /// <param name="viewModels">viewModels</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public async Task<(bool flag, string msg)> Weight(List<DispatchlistWeightViewModel> viewModels, CurrentUser currentUser)
    {
        var DBSet = _dBContext.GetDbSet<DispatchlistEntity>();
        var dispatchlist_id_list = viewModels.Select(t => t.id).ToList();
        var entities = await DBSet.Where(t => dispatchlist_id_list.Contains(t.Id)).ToListAsync();
        var now_time = DateTime.UtcNow;
        var code = GetPackageOrWeightCode();
        foreach (var vm in viewModels)
        {
            var entity = entities.FirstOrDefault(t => t.Id == vm.id && t.dispatch_status == vm.dispatch_status);
            if (entity == null)
            {
                return (false, "[202]" + _stringLocalizer["data_changed"]);
            }
            if ((entity.weighing_qty + vm.weighing_qty) > entity.picked_qty)
            {
                return (false, "[202]" + _stringLocalizer["unweightqty_lessthen"]);
            }
            entity.last_update_time = now_time;
            entity.weighing_person = currentUser.user_name;
            entity.weighing_qty += vm.weighing_qty;
            entity.weighing_weight += vm.weighing_weight;
            entity.weighing_no = code;
            entity.dispatch_status = 5;
        }
        var saved = false;
        int res = 0;
        while (!saved)
        {
            try
            {
                // Attempt to save changes to the database
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

                        var t_vm = viewModels.FirstOrDefault(t => t.id == UtilConvert.ObjToInt(databaseValues["id"]));
                        if (t_vm == null)
                        {
                            return (false, "[202]" + _stringLocalizer["data_changed"]);
                        }
                        if (UtilConvert.ObjToInt(databaseValues["weighing_qty"]) + t_vm.weighing_qty > t_vm.picked_qty)
                        {
                            return (false, "[202]" + _stringLocalizer["data_changed"]);
                        }
                        else
                        {
                            proposedValues["weighing_qty"] = UtilConvert.ObjToInt(databaseValues["weighing_qty"]) + t_vm.weighing_qty;
                            proposedValues["weighing_weight"] = UtilConvert.ObjToInt(databaseValues["weighing_weight"]) + t_vm.weighing_weight;
                            if (UtilConvert.ObjToInt(databaseValues["weighing_qty"]) + t_vm.weighing_qty == UtilConvert.ObjToInt(databaseValues["picked_qty"]))
                            {
                                proposedValues["dispatch_status"] = 5;
                            }
                            proposedValues["last_update_time"] = now_time;
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
    /// dispatchpicklist outbound delivery
    /// </summary>
    /// <param name="viewModels">viewModels</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public async Task<(bool flag, string msg)> Delivery(List<DispatchlistDeliveryViewModel> viewModels, CurrentUser currentUser)
    {
        using var transaction = _dBContext.Database.BeginTransaction();
        try
        {
            var dispatchListDbSet = _dBContext.GetDbSet<DispatchlistEntity>();
            var pickDbSet = _dBContext.GetDbSet<DispatchpicklistEntity>();
            var stockDbSet = _dBContext.GetDbSet<StockEntity>();
            var goodLocationDbSet = _dBContext.GetDbSet<GoodslocationEntity>();
            var dispatchListIdList = viewModels.Select(t => t.id).ToList();
            var entities = await dispatchListDbSet.Where(t => dispatchListIdList.Contains(t.Id)).ToListAsync();
            var nowTime = DateTime.UtcNow;
            foreach (var entity in entities)
            {
                if (entity.dispatch_status != 3 && entity.dispatch_status != 4 && entity.dispatch_status != 5)
                {
                    return (false, _stringLocalizer["data_changed"]);
                }
                entity.last_update_time = nowTime;
                entity.dispatch_status = 6;
                entity.lock_qty = 0;
                entity.actual_qty = entity.picked_qty;
                entity.intrasit_qty = entity.picked_qty;
            }
            var pick_sql = pickDbSet.Where(t => dispatchListIdList.Contains(t.dispatchlist_id));
            var pick_datas = await pick_sql.ToListAsync();
            var picks_g = pick_sql.AsNoTracking()
                .GroupBy(e =>
                             new { e.goods_location_id, e.sku_id, e.goods_owner_id, e.series_number, e.expiry_date, e.price, e.putaway_date })
                .Select(c =>
                             new { c.Key.goods_location_id, c.Key.sku_id, c.Key.goods_owner_id, c.Key.series_number, c.Key.expiry_date, c.Key.price, c.Key.putaway_date, picked_qty = c.Sum(t => t.picked_qty) });

            var picks = await picks_g.ToListAsync();

            var stocks = await (from stock in stockDbSet
                                where pick_sql.Any(t => t.goods_location_id == stock.goods_location_id
                                && t.sku_id == stock.sku_id && t.goods_owner_id == stock.goods_owner_id && t.series_number == stock.series_number
                                && t.expiry_date == stock.expiry_date && t.price == stock.price
                                && t.putaway_date.Date == stock.PutAwayDate.Date)
                                select stock).ToListAsync();

            foreach (var pick in picks)
            {
                var s = stocks.FirstOrDefault(t => t.goods_location_id == pick.goods_location_id
                                && t.sku_id == pick.sku_id && t.goods_owner_id == pick.goods_owner_id
                                && t.series_number == pick.series_number && t.expiry_date == pick.expiry_date
                                && t.price == pick.price
                                && t.PutAwayDate.Date == pick.putaway_date.Date);
                if (s == null)
                {
                    return (false, _stringLocalizer["data_changed"]);
                }
                s.qty -= pick.picked_qty;
                s.last_update_time = nowTime;
                if (s.qty <= 0)
                {
                    stockDbSet.Remove(s);
                    var location = await goodLocationDbSet.FirstOrDefaultAsync(x => x.Id == s.goods_location_id);
                    if (location != null)
                    {
                        location.LocationStatus = (byte)GoodLocationStatusEnum.AVAILABLE;
                        location.IsValid = true;
                    }
                }
                else
                {
                    stockDbSet.Update(s);
                }
            }

            foreach (var pick in pick_datas)
            {
                pick.is_update_stock = true;
                pick.last_update_time = nowTime;
            }
            var saved = false;
            int res = 0;
            while (!saved)
            {
                try
                {
                    // Attempt to save changes to the database
                    res = await _dBContext.SaveChangesAsync();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is DispatchlistEntity)
                        {
                            var proposedValues = entry.CurrentValues;
                            var databaseValues = entry.GetDatabaseValues();
                            if (UtilConvert.ObjToInt(databaseValues["dispatch_status"]) != 3 && UtilConvert.ObjToInt(databaseValues["dispatch_status"]) != 4 && UtilConvert.ObjToInt(databaseValues["dispatch_status"]) != 5)
                            {
                                return (false, "[202]" + _stringLocalizer["data_changed"]);
                            }
                            proposedValues["last_update_time"] = nowTime;
                        }
                        else if (entry.Entity is StockEntity)
                        {
                            var proposedValues = entry.CurrentValues;
                            var databaseValues = entry.GetDatabaseValues();
                            var t_p = picks.FirstOrDefault(t => t.goods_location_id == UtilConvert.ObjToInt(databaseValues["goods_location_id"]) && t.sku_id == UtilConvert.ObjToInt(databaseValues["sku_id"]) && t.goods_owner_id == UtilConvert.ObjToInt(databaseValues["goods_owner_id"]));
                            if (t_p == null)
                            {
                                return (false, _stringLocalizer["data_changed"]);
                            }
                            proposedValues["qty"] = UtilConvert.ObjToInt(databaseValues["qty"]) - t_p.picked_qty;
                            proposedValues["last_update_time"] = nowTime;
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
                await transaction.CommitAsync();
                return (true, _stringLocalizer["operation_success"]);
            }
            else
            {
                return (false, _stringLocalizer["operation_failed"]);
            }
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Exception in Delivery");
            return (false, _stringLocalizer["operation_failed"]);
        }
    }

    /// <summary>
    ///  set dispatchlist freightfee
    /// </summary>
    /// <param name="viewModels"></param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> SetFreightfee(List<DispatchlistFreightfeeViewModel> viewModels)
    {
        var DBSet = _dBContext.GetDbSet<DispatchlistEntity>();
        var dispatchlist_id_list = viewModels.Select(t => t.id).ToList();
        var freightfee_id_list = viewModels.Select(t => t.freightfee_id).Distinct().ToList();
        var entities = await DBSet.Where(t => dispatchlist_id_list.Contains(t.Id)).ToListAsync();
        var freightfees = await _dBContext.GetDbSet<FreightfeeEntity>().Where(t => freightfee_id_list.Contains(t.Id)).ToListAsync();
        var now_time = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            var vm = viewModels.FirstOrDefault(t => t.id == entity.Id);
            if (vm != null)
            {
                var freightfee = freightfees.FirstOrDefault(t => t.Id == vm.freightfee_id);
                if (freightfee != null)
                {
                    entity.last_update_time = now_time;
                    entity.carrier = freightfee.carrier;
                    entity.waybill_no = vm.waybill_no;
                    if (entity.weighing_no != "")
                    {
                        entity.freightfee = entity.weighing_weight * freightfee.price_per_weight > freightfee.min_payment ? entity.weighing_weight * freightfee.price_per_weight : freightfee.min_payment;
                    }
                    else
                    {
                        entity.freightfee = Math.Max(Math.Max(entity.weight * freightfee.price_per_weight, entity.volume * freightfee.price_per_volume), freightfee.min_payment);
                    }
                }
            }
        }
        var res = await _dBContext.SaveChangesAsync();
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
    /// sign for arrival
    /// </summary>
    /// <param name="viewModels">viewModels</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> SignForArrival(List<DispatchlistSignViewModel> viewModels)
    {
        var DBSet = _dBContext.GetDbSet<DispatchlistEntity>();
        var dispatchlist_id_list = viewModels.Select(t => t.id).ToList();
        var entities = await DBSet.Where(t => dispatchlist_id_list.Contains(t.Id)).ToListAsync();
        var now_time = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            var vm = viewModels.FirstOrDefault(t => t.id == t.id && t.dispatch_status == entity.dispatch_status);
            if (vm == null)
            {
                return (false, "[202]" + _stringLocalizer["data_changed"]);
            }
            entity.sign_qty = entity.actual_qty - vm.damage_qty;
            entity.damage_qty = vm.damage_qty;
            entity.last_update_time = now_time;
            entity.dispatch_status = 7;
        }
        var res = await _dBContext.SaveChangesAsync();
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
    /// get next order code number
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetOrderCode(CurrentUser currentUser)
    {
        string code;
        string date = DateTime.UtcNow.ToString("yyyy" + "MM" + "dd");
        string maxNo = await _dBContext.GetDbSet<DispatchlistEntity>().Where(t => t.TenantId == currentUser.tenant_id).MaxAsync(t => t.dispatch_no);
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

    /// <summary>
    /// Excel Import
    /// </summary>
    /// <param name="viewModels">viewModels</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> Import(List<DispatchlistImportViewModel> viewModels, CurrentUser currentUser)
    {
        var DbSet = _dBContext.GetDbSet<DispatchlistEntity>();
        var import_sku_code = viewModels.Select(e => e.sku_code).ToList();
        var import_customer_name = viewModels.Select(e => e.customer_name).ToList();
        var sku_list = await (from sku in _dBContext.GetDbSet<SkuEntity>()
                              join spu in _dBContext.GetDbSet<SpuEntity>() on sku.spu_id equals spu.Id
                              where spu.TenantId == currentUser.tenant_id && import_sku_code.Contains(sku.sku_code)
                              select sku).ToListAsync();
        var customer_list = await _dBContext.GetDbSet<CustomerEntity>().Where(t => t.TenantId == currentUser.tenant_id && import_customer_name.Contains(t.customer_name)).ToListAsync();
        var entities = new List<DispatchlistEntity>();
        var groups = viewModels.Select(t => t.import_group).Distinct().ToList();
        var groups_code = await _functionHelper.GetFormNoListAsync("Dispatchlist", groups.Count);
        var group_code_dic = new Dictionary<int, string>();
        var now_time = DateTime.UtcNow;
        for (int i = 0; i < groups.Count(); i++)
        {
            group_code_dic.Add(groups[i], groups_code[i]);
        }
        foreach (var vm in viewModels)
        {
            var customer = customer_list.FirstOrDefault(t => t.customer_name == vm.customer_name);
            if (customer == null)
            {
                return (false, _stringLocalizer["customer_name"] + ":" + vm.customer_name + " " + _stringLocalizer["not_exists_entity"]);
            }
            var sku = sku_list.FirstOrDefault(t => t.sku_code == vm.sku_code);
            if (sku == null)
            {
                return (false, _stringLocalizer["sku_name"] + ":" + vm.sku_name + "-" + _stringLocalizer["sku_code"] + ":" + vm.sku_code + " " + _stringLocalizer["not_exists_entity"]);
            }
            entities.Add(new DispatchlistEntity
            {
                customer_id = customer.Id,
                customer_name = vm.customer_name,
                sku_id = sku.Id,
                qty = vm.qty,
                creator = currentUser.user_name,
                create_time = now_time,
                last_update_time = now_time,
                TenantId = currentUser.tenant_id,
                dispatch_no = group_code_dic[vm.import_group],
            });
        }
        await DbSet.AddRangeAsync(entities);
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
    /// get next order code number
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> GetOrderCodeList(CurrentUser currentUser, int cnt)
    {
        List<string> code = new List<string>();
        string date = DateTime.UtcNow.ToString("yyyy" + "MM" + "dd");
        string maxNo = await _dBContext.GetDbSet<DispatchlistEntity>().Where(t => t.TenantId == currentUser.tenant_id).MaxAsync(t => t.dispatch_no);
        if (maxNo == null)
        {
            for (int i = 1; i <= cnt; i++)
            {
                code.Add(date + "-" + cnt.ToString("0000"));
            }
        }
        else
        {
            string maxDate = maxNo.Substring(0, 8);
            string maxDateNo = maxNo.Substring(9, 4);
            if (date == maxDate)
            {
                int.TryParse(maxDateNo, out int dd);
                for (int i = 1; i <= cnt; i++)
                {
                    code.Add(date + "-" + (dd + cnt).ToString("0000"));
                }
            }
            else
            {
                for (int i = 1; i <= cnt; i++)
                {
                    code.Add(date + "-" + cnt.ToString("0000"));
                }
            }
        }

        return code;
    }

    /// <summary>
    /// get package or weight  code
    /// </summary>
    /// <returns></returns>
    public string GetPackageOrWeightCode()
    {
        string date = DateTime.UtcNow.ToString("yyyy" + "MM" + "dd");
        DateTime _dtStart = new DateTime(1970, 1, 1, 8, 0, 0);
        long timeStamp = Convert.ToInt32(DateTime.UtcNow.Subtract(_dtStart).TotalSeconds);
        return date + timeStamp.ToString();
    }

    /*public async Task<bool> AllocateInventory(DateTime TimeBegin, DateTime TimeEnd, CurrentUser currentUser)
    {
        var DbSet = _dBContext.GetDbSet<DispatchlistEntity>();
        var dispatchpick_DBSet = _dBContext.GetDbSet<DispatchpicklistEntity>();
        var to_pick_query = DbSet.Where(t => t.tenant_id == currentUser.tenant_id && t.create_time >= TimeBegin && t.create_time <= TimeEnd && t.dispatch_status == 1);
        var to_pick_list = await dispatchpick_DBSet.Where(t => to_pick_query.Any(e => e.id == t.dispatchlist_id)).OrderBy(t => t.id).ToListAsync();
        var to_pick = await to_pick_query.ToListAsync();
        var pick_sku = to_pick.Select(t => t.sku_id).ToList();

        var stock_DbSet = _dBContext.GetDbSet<StockEntity>();
        var asn_DBSet = _dBContext.GetDbSet<AsnEntity>();
        var dispatch_DBSet = _dBContext.GetDbSet<DispatchlistEntity>();
        var sku_DBSet = _dBContext.GetDbSet<SkuEntity>().AsNoTracking();
        var spu_DBSet = _dBContext.GetDbSet<SpuEntity>().AsNoTracking();
        var processdetail_DBSet = _dBContext.GetDbSet<StockprocessdetailEntity>().AsNoTracking();
        var move_DBSet = _dBContext.GetDbSet<StockmoveEntity>();
        var owner_DBSet = _dBContext.GetDbSet<GoodsownerEntity>();
        var location_DBSet = _dBContext.GetDbSet<GoodslocationEntity>();
        var stock_group_datas = stock_DbSet.Where(t => t.tenant_id == currentUser.tenant_id && pick_sku.Any(s => s == t.sku_id));

        var dispatch_group_datas = from dp in DbSet.AsNoTracking()
                                   join dpp in dispatchpick_DBSet.AsNoTracking() on dp.id equals dpp.dispatchlist_id
                                   where dp.dispatch_status > 1 && dp.dispatch_status < 6 && dp.tenant_id == currentUser.tenant_id
                                   group dpp by new { dpp.sku_id, dpp.goods_location_id, dpp.goods_owner_id, dpp.series_number } into dg
                                   select new
                                   {
                                       goods_owner_id = dg.Key.goods_owner_id,
                                       sku_id = dg.Key.sku_id,
                                       goods_location_id = dg.Key.goods_location_id,
                                       series_number = dg.Key.series_number,
                                       qty_locked = dg.Sum(t => t.pick_qty)
                                   };
        var process_locked_group_datas = from pd in processdetail_DBSet
                                         where pd.is_update_stock == false && pd.tenant_id == currentUser.tenant_id
                                         group pd by new { pd.sku_id, pd.goods_location_id, pd.goods_owner_id, pd.series_number } into pdg
                                         select new
                                         {
                                             goods_owner_id = pdg.Key.goods_owner_id,
                                             sku_id = pdg.Key.sku_id,
                                             goods_location_id = pdg.Key.goods_location_id,
                                             series_number = pdg.Key.series_number,
                                             qty_locked = pdg.Sum(t => t.qty)
                                         };
        var move_locked_group_datas = from m in move_DBSet.AsNoTracking()
                                      where m.move_status == 0 && m.tenant_id == currentUser.tenant_id
                                      group m by new { m.sku_id, m.orig_goods_location_id, m.goods_owner_id, m.series_number } into mg
                                      select new
                                      {
                                          goods_owner_id = mg.Key.goods_owner_id,
                                          sku_id = mg.Key.sku_id,
                                          goods_location_id = mg.Key.orig_goods_location_id,
                                          series_number = mg.Key.series_number,
                                          qty_locked = mg.Sum(t => t.qty)
                                      };
        var datas = await (from sg in stock_group_datas
                           join dp in dispatch_group_datas on new { sg.sku_id, sg.goods_location_id, sg.goods_owner_id, sg.series_number } equals new { dp.sku_id, dp.goods_location_id, dp.goods_owner_id, dp.series_number } into dp_left
                           from dp in dp_left.DefaultIfEmpty()
                           join pl in process_locked_group_datas on new { sg.sku_id, sg.goods_location_id, sg.goods_owner_id, sg.series_number } equals new { pl.sku_id, pl.goods_location_id, pl.goods_owner_id, pl.series_number } into pl_left
                           from pl in pl_left.DefaultIfEmpty()
                           join m in move_locked_group_datas on new { sg.sku_id, sg.goods_location_id, sg.goods_owner_id, sg.series_number } equals new { m.sku_id, m.goods_location_id, m.goods_owner_id, m.series_number } into m_left
                           from m in m_left.DefaultIfEmpty()
                           where pick_sku.Contains(sg.sku_id)
                           select new
                           {
                               stock_id = sg.stock_id,
                               goods_location_id = sg.goods_location_id,
                               goods_owner_id = sg.goods_owner_id,
                               qty_available = sg.qty - sg.qty_frozen - (dp.qty_locked == null ? 0 : dp.qty_locked) - (pl.qty_locked == null ? 0 : pl.qty_locked) - (m.qty_locked == null ? 0 : m.qty_locked),
                               sku_id = sg.sku_id,
                               series_number = sg.series_number
                           }).ToListAsync();
        foreach (var p in to_pick_list)
        {
            var picklist = datas.Where(t => t.sku_id == p.sku_id && t.stock_id > 0).OrderBy(o => o.qty_available).OrderByDescending(o => o.qty_available).ToList();
            int pick_qty = 0;
            foreach (var pick in picklist)
            {
                if (pick_qty >= d.qty)
                {
                    break;
                }
                pick.pick_qty = (r.qty <= (pick_qty + pick.qty_available)) ? (r.qty - pick_qty) : pick.qty_available;
                pick_qty += pick.pick_qty;
            }
            r.pick_list = picklist.Where(t => t.qty_available > 0).ToList();
        }
    }*/

    #region Duy phat Solution

    /// <summary>
    /// Create dispatch order with details and picklists
    /// </summary>
    /// <param name="request">Dispatch order request</param>
    /// <returns>Tuple of (dispatchId, message)</returns>
    public async Task<(int dispatchId, string message)> CreateDispatchDraftAsync(CreateDispatchOrderRequest request)
    {
        // 1. Validate input
        if (request == null)
        {
            _logger.LogWarning("CreateDispatchDraftAsync: Request is null");
            return (0, "Request is required");
        }

        if (request.Lines == null || request.Lines.Count == 0)
        {
            _logger.LogWarning("CreateDispatchDraftAsync: No lines in request");
            return (0, "At least one line item is required");
        }

        var skuIds = request.Lines.Select(l => l.SkuId).Distinct().ToList();
        var existingSkuIds = await _dBContext.GetDbSet<SkuEntity>()
            .Where(s => skuIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();

        var missingSkuIds = skuIds.Except(existingSkuIds).ToList();
        if (missingSkuIds.Any())
        {
            _logger.LogWarning("CreateDispatchDraftAsync: SKUs not found: {SkuIds}", string.Join(", ", missingSkuIds));
            return (0, $"SKU(s) not found: {string.Join(", ", missingSkuIds)}");
        }

        using var transaction = await _dBContext.Database.BeginTransactionAsync();
        try
        {
            var nowTime = DateTime.UtcNow;
            var dispatchNo = string.IsNullOrEmpty(request.DispatchNo)
                ? await _functionHelper.GetFormNoAsync("dispatchlist", "Outbound")
                : request.DispatchNo;

            var header = new DispatchlistEntity
            {
                dispatch_no = dispatchNo,
                customer_id = request.CustomerId,
                customer_name = request.CustomerName,
                dispatch_status = 0,
                create_time = request.CreateDate,
                last_update_time = nowTime,
                DispatchDetails = new List<DispatchListDetailEntity>(),
            };

            await _dBContext.GetDbSet<DispatchlistEntity>().AddAsync(header);
            await _dBContext.SaveChangesAsync();


            var details = new List<DispatchListDetailEntity>();
            var picklists = new List<DispatchpicklistEntity>();

            foreach (var line in request.Lines)
            {
                var totalSelectedQty = line.SelectedLocations?.Sum(x => x.PickQty) ?? 0;

                var detail = new DispatchListDetailEntity
                {
                    dispatchlist_id = header.Id,
                    sku_id = line.SkuId,
                    sku_uom_id = line.UomId,
                    req_qty = line.ReqQty,
                    allocated_qty = totalSelectedQty,
                    picked_qty = 0,
                    status = DispatchDetailStatus.Pending,
                    description = line.Description
                };
                details.Add(detail);
            }

            await _dBContext.GetDbSet<DispatchListDetailEntity>().AddRangeAsync(details);
            await _dBContext.SaveChangesAsync();

            for (int i = 0; i < request.Lines.Count; i++)
            {
                var line = request.Lines[i];
                var detail = details[i];

                if (line.SelectedLocations != null)
                {
                    foreach (var loc in line.SelectedLocations)
                    {
                        picklists.Add(new DispatchpicklistEntity
                        {
                            dispatch_detail_id = detail.Id,
                            goods_location_id = loc.LocationId,
                            pallet_Id = loc.PalletId,
                            sku_id = line.SkuId,
                            pick_qty = (int)loc.PickQty,
                            qty = loc.PickQty,
                            picked_qty = 0,
                            status = DispatchPickListStatus.Pending,
                            last_update_time = nowTime
                        });
                    }
                }
            }


            if (picklists.Count > 0)
            {
                await _dBContext.GetDbSet<DispatchpicklistEntity>().AddRangeAsync(picklists);
                await _dBContext.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            _logger.LogInformation(
                "CreateDispatchDraftAsync: Created dispatch {DispatchNo} with {LineCount} lines and {PicklistCount} picklists",
                dispatchNo, details.Count, picklists.Count);

            return (header.Id, $"Dispatch {dispatchNo} created successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "CreateDispatchDraftAsync: Error creating dispatch");
            return (0, $"Error creating dispatch: {ex.Message}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<bool> ExecuteDispatchAsync(int id, CurrentUser currentUser)
    {
        using var transaction = await _dBContext.Database.BeginTransactionAsync();
        try
        {
            var dispatchEntity = await _dBContext.GetDbSet<DispatchlistEntity>()
                .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == currentUser.tenant_id);

            if (dispatchEntity == null)
            {
                _logger.LogWarning("ExecuteDispatchAsync: Dispatch {DispatchId} not found", id);
                return false;
            }

            if (dispatchEntity.dispatch_status != 0)
            {
                _logger.LogWarning("ExecuteDispatchAsync: Dispatch {DispatchId} is not in draft status", id);
                return false;
            }

            var picklistData = await (
                from detail in _dBContext.GetDbSet<DispatchListDetailEntity>().AsNoTracking()
                join pick in _dBContext.GetDbSet<DispatchpicklistEntity>().AsNoTracking()
                on detail.Id equals pick.dispatch_detail_id
                where detail.dispatchlist_id == id
                select new
                {
                    Detail = detail,
                    Picklist = pick
                }
            ).ToListAsync();

            if (picklistData.Count == 0)
            {
                _logger.LogWarning("ExecuteDispatchAsync: No picklists found for dispatch {DispatchId}", id);
                return false;
            }

            var dispatchListDetails = picklistData.Select(x => x.Detail).DistinctBy(d => d.Id).ToList();
            var dispatchPickList = picklistData.Select(x => x.Picklist).ToList();

            var nowTime = DateTime.UtcNow;

            dispatchEntity.dispatch_status = 2; // Confirmed/Processing
            dispatchEntity.last_update_time = nowTime;


            var detailIds = dispatchListDetails.Select(d => d.Id).ToList();
            await _dBContext.GetDbSet<DispatchListDetailEntity>()
                .Where(d => detailIds.Contains(d.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.status, DispatchDetailStatus.Partial));

            var palletIds = dispatchPickList
                .Where(p => p.pallet_Id.HasValue)
                .Select(p => p.pallet_Id!.Value)
                .Distinct()
                .ToList();

            // Query Pallet table
            var pallets = await _dBContext.GetDbSet<PalletEntity>()
                .AsNoTracking()
                .Where(p => palletIds.Contains(p.Id))
                .Select(p => new { p.Id, p.PalletCode })
                .ToListAsync();

            var palletLookup = pallets.ToDictionary(p => p.Id, p => p.PalletCode);

            var uniquePalletLocationPairs = dispatchPickList
               .Where(p => p.pallet_Id.HasValue && palletLookup.ContainsKey(p.pallet_Id.Value))
               .GroupBy(p => new
               {
                   PalletCode = palletLookup[p.pallet_Id!.Value],
                   p.goods_location_id
               })
               .Select(g => new
               {
                   PalletCode = g.Key.PalletCode,
                   LocationId = g.Key.goods_location_id,
                   TotalPickQty = g.Sum(x => x.pick_qty)
               })
               .ToList();

            if (uniquePalletLocationPairs.Count > 0)
            {
                var outboundEntities = new List<OutboundEntity>();
                var historyEntities = new List<IntegrationHistory>();

                foreach (var pair in uniquePalletLocationPairs)
                {
                    var taskCode = GenarationHelper.GenerateTaskCode();

                    var outbound = new OutboundEntity
                    {
                        TenantId = currentUser.tenant_id,
                        PalletCode = pair.PalletCode,
                        TaskCode = taskCode,
                        LocationId = pair.LocationId,
                        CreatedDate = nowTime,
                        PickUpDate = nowTime,
                        Priority = 1,
                        Status = IntegrationStatus.Processing,
                        IsActive = true,
                    };

                    outboundEntities.Add(outbound);

                    var history = new IntegrationHistory
                    {
                        TenantId = currentUser.tenant_id,
                        PalletCode = pair.PalletCode,
                        TaskCode = taskCode,
                        LocationId = pair.LocationId,
                        CreatedDate = nowTime,
                        PickUpDate = nowTime,
                        Priority = 1,
                        Status = IntegrationStatus.Processing,
                        IsActive = true,
                        HistoryType = HistoryType.Outbound,
                    };
                    historyEntities.Add(history);
                }

                await _dBContext.Outbounds.AddRangeAsync(outboundEntities);
                await _dBContext.IntegrationHistories.AddRangeAsync(historyEntities);
            }

            await _dBContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "ExecuteDispatchAsync: Dispatch {DispatchNo} executed with {PicklistCount} picklists",
                dispatchEntity.dispatch_no, dispatchPickList.Count);

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "ExecuteDispatchAsync: Error executing dispatch {DispatchId}", id);
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<(int dispatchId, bool success, string message)> CreateAndExecuteDispatchAsync(
       CreateDispatchOrderRequest request,
       CurrentUser currentUser)
    {
        if (request == null)
        {
            _logger.LogWarning("CreateAndExecuteDispatchAsync: Request is null");
            return (0, false, "Request is required");
        }

        if (request.Lines == null || request.Lines.Count == 0)
        {
            _logger.LogWarning("CreateAndExecuteDispatchAsync: No lines in request");
            return (0, false, "At least one line item is required");
        }

        var skuIds = request.Lines.Select(l => l.SkuId).Distinct().ToList();
        var existingSkuIds = await _dBContext.GetDbSet<SkuEntity>()
            .Where(s => skuIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();

        var missingSkuIds = skuIds.Except(existingSkuIds).ToList();
        if (missingSkuIds.Any())
        {
            _logger.LogWarning("CreateAndExecuteDispatchAsync: SKUs not found: {SkuIds}", string.Join(", ", missingSkuIds));
            return (0, false, $"SKU(s) not found: {string.Join(", ", missingSkuIds)}");
        }

        //  Validate Pallets exist and get PalletCodes
        var palletIds = request.Lines
                        .Where(l => l.SelectedLocations != null)
                        .SelectMany(l => l.SelectedLocations!)
                        .Where(loc => loc.PalletId > 0)
                        .Select(loc => loc.PalletId)
                        .Distinct()
                        .ToList();

        var palletLookup = new Dictionary<int, string>();
        if (palletIds.Count > 0)
        {
            var pallets = await _dBContext.GetDbSet<PalletEntity>()
                .AsNoTracking()
                .Where(p => palletIds.Contains(p.Id))
                .Select(p => new { p.Id, p.PalletCode })
                .ToListAsync();

            palletLookup = pallets.ToDictionary(p => p.Id, p => p.PalletCode);

            var missingPalletIds = palletIds.Except(palletLookup.Keys).ToList();
            if (missingPalletIds.Any())
            {
                _logger.LogWarning("CreateAndExecuteDispatchAsync: Pallets not found: {PalletIds}",
                    string.Join(", ", missingPalletIds));
                return (0, false, $"Pallet(s) not found: {string.Join(", ", missingPalletIds)}");
            }
        }
        using var transaction = await _dBContext.Database.BeginTransactionAsync();
        try
        {
            var nowTime = DateTime.UtcNow;

            var dispatchNo = string.IsNullOrEmpty(request.DispatchNo)
                ? await _functionHelper.GetFormNoAsync("dispatchlist", "Outbound")
                : request.DispatchNo;

            // Create dispatch header with status = 2 (Confirmed/Processing) directly
            var header = new DispatchlistEntity
            {
                dispatch_no = dispatchNo,
                customer_id = request.CustomerId,
                customer_name = request.CustomerName,
                dispatch_status = 2,
                create_time = request.CreateDate,
                last_update_time = nowTime,
                TenantId = currentUser.tenant_id,
                creator = currentUser.user_name,
            };

            await _dBContext.GetDbSet<DispatchlistEntity>().AddAsync(header);
            await _dBContext.SaveChangesAsync();

            //Create details with status = Partial
            var details = new List<DispatchListDetailEntity>();
            foreach (var line in request.Lines)
            {
                var totalSelectedQty = line.SelectedLocations?.Sum(x => x.PickQty) ?? 0;

                var detail = new DispatchListDetailEntity
                {
                    dispatchlist_id = header.Id,
                    sku_id = line.SkuId,
                    sku_uom_id = line.UomId,
                    req_qty = line.ReqQty,
                    allocated_qty = totalSelectedQty,
                    picked_qty = 0,
                    status = DispatchDetailStatus.Partial,
                    description = line.Description
                };
                details.Add(detail);
            }

            await _dBContext.GetDbSet<DispatchListDetailEntity>().AddRangeAsync(details);
            await _dBContext.SaveChangesAsync();

            var picklists = new List<DispatchpicklistEntity>();
            var palletLocationPairs = new List<(string PalletCode, int LocationId)>();

            for (int i = 0; i < request.Lines.Count; i++)
            {
                var line = request.Lines[i];
                var detail = details[i];

                if (line.SelectedLocations != null)
                {
                    foreach (var loc in line.SelectedLocations)
                    {
                        picklists.Add(new DispatchpicklistEntity
                        {
                            dispatch_detail_id = detail.Id,
                            goods_location_id = loc.LocationId,
                            pallet_Id = loc.PalletId,
                            sku_id = line.SkuId,
                            pick_qty = (int)loc.PickQty,
                            qty = loc.PickQty,
                            picked_qty = 0,
                            status = DispatchPickListStatus.Pending,
                            last_update_time = nowTime
                        });

                        if (loc.PalletId > 0 && palletLookup.TryGetValue(loc.PalletId, out var palletCode))
                        {
                            palletLocationPairs.Add((palletCode, loc.LocationId));
                        }
                    }
                }
            }

            if (picklists.Count > 0)
            {
                await _dBContext.GetDbSet<DispatchpicklistEntity>().AddRangeAsync(picklists);
            }

            var uniquePairs = palletLocationPairs
                .GroupBy(p => new { p.PalletCode, p.LocationId })
                .Select(g => new { g.Key.PalletCode, g.Key.LocationId })
                .ToList();

            if (uniquePairs.Count > 0)
            {
                var outboundEntities = new List<OutboundEntity>();
                var historyEntities = new List<IntegrationHistory>();

                foreach (var pair in uniquePairs)
                {
                    var taskCode = GenarationHelper.GenerateTaskCode();

                    outboundEntities.Add(new OutboundEntity
                    {
                        TenantId = currentUser.tenant_id,
                        PalletCode = pair.PalletCode,
                        TaskCode = taskCode,
                        LocationId = pair.LocationId,
                        CreatedDate = nowTime,
                        PickUpDate = nowTime,
                        Priority = 1,
                        Status = IntegrationStatus.Processing,
                        IsActive = true,
                    });

                    historyEntities.Add(new IntegrationHistory
                    {
                        TenantId = currentUser.tenant_id,
                        PalletCode = pair.PalletCode,
                        TaskCode = taskCode,
                        LocationId = pair.LocationId,
                        CreatedDate = nowTime,
                        PickUpDate = nowTime,
                        Priority = 1,
                        Status = IntegrationStatus.Processing,
                        IsActive = true,
                        HistoryType = HistoryType.Outbound,
                    });
                }

                await _dBContext.Outbounds.AddRangeAsync(outboundEntities);
                await _dBContext.IntegrationHistories.AddRangeAsync(historyEntities);
            }

            // Save all changes in single call
            await _dBContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "CreateAndExecuteDispatchAsync: Created and executed dispatch {DispatchNo} with {LineCount} lines, {PicklistCount} picklists, {TaskCount} outbound tasks",
                dispatchNo, details.Count, picklists.Count, uniquePairs.Count);

            return (header.Id, true, $"Dispatch {dispatchNo} created and executed successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "CreateAndExecuteDispatchAsync: Error creating/executing dispatch");
            return (0, false, $"Error: {ex.Message}");
        }

    }


    #endregion


    #endregion
}