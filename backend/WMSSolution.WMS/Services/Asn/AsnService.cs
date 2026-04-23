using Mapster;
using Microsoft.EntityFrameworkCore;
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
using WMSSolution.Shared.Enums.Inbound;
using WMSSolution.Shared.Enums.Location;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Pallet;
using WMSSolution.WMS.Entities.Models.PurchaseOrders;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.Models.Warehouse;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Asn;
using WMSSolution.WMS.Entities.ViewModels.Asn.Asnmaster;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Inbound;
using WMSSolution.WMS.IServices;
using WMSSolution.WMS.IServices.IntegrationWCS;

namespace WMSSolution.WMS.Services.Asn;

/// <summary>
///  Asn Service
/// </summary>
/// <remarks>
///Asn  constructor
/// </remarks>
/// <param name="dBContext">The DBContext</param>
/// <param name="stringLocalizer">Localizer</param>
/// <param name="functionHelper">Function Helper</param>
/// <param name="logger">Logger</param>
/// <param name="integrationService"></param>
public class AsnService(
    SqlDBContext dBContext
        , IStringLocalizer<MultiLanguage> stringLocalizer
        , FunctionHelper functionHelper
        , ILogger<AsnService> logger, IIntegrationService integrationService
        ) : BaseService<AsnEntity>, IAsnService
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

    /// <summary>
    /// functions
    /// </summary>
    private readonly FunctionHelper _functionHelper = functionHelper;

    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger<AsnService> _logger = logger;

    private readonly IIntegrationService _integrationService = integrationService;

    #endregion Args

    #region Api

    /// <summary>
    /// page search, sqlTitle input asn_status:0 ~ 4
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(List<AsnViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken = default)
    {
        var queries = new QueryCollection();
        if (pageSearch.searchObjects.Count != 0)
        {
            pageSearch.searchObjects.ForEach(queries.Add);
        }
        Byte asn_status = 255;
        var ansEntity = _dbContext.GetDbSet<AsnEntity>().AsNoTracking();
        if (pageSearch.sqlTitle.Contains("asn_status:alltodo", StringComparison.CurrentCultureIgnoreCase))
        {
            _logger.LogInformation("asn_status: alltodo");
            ansEntity = ansEntity.Where(t => (t.asn_status <= (byte)AsnStatusEnum.PREPARE_PUTAWAY));
        }
        else if (pageSearch.sqlTitle.Contains("asn_status:waiting", StringComparison.CurrentCultureIgnoreCase))
        {
            _logger.LogInformation("asn_status: 3 and 4");
            ansEntity = ansEntity
                .Where(t => t.asn_status == (byte)AsnStatusEnum.PREPARE_PUTAWAY || t.asn_status == (byte)AsnStatusEnum.WAITING_ROBOT);
        }
        else if (pageSearch.sqlTitle.Contains("asn_status", StringComparison.CurrentCultureIgnoreCase))
        {
            _logger.LogInformation("asn_status: {asn_status}", pageSearch.sqlTitle);
            asn_status = Convert.ToByte(pageSearch.sqlTitle.Trim().ToLower().Replace("asn_status", "").Replace("：", "").Replace(":", "").Replace("=", ""));
            ansEntity = ansEntity.Where(t => t.asn_status == asn_status);
        }
        var Spus = _dbContext.GetDbSet<SpuEntity>().AsNoTracking();
        var Skus = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var Asnmasters = _dbContext.GetDbSet<AsnmasterEntity>().AsNoTracking();

        var query = from m in ansEntity
                    join am in Asnmasters on m.asnmaster_id equals am.Id
                    join p in Spus on m.spu_id equals p.Id
                    join k in Skus on m.sku_id equals k.Id
                    where m.TenantId == currentUser.tenant_id
                    select new AsnViewModel
                    {
                        id = m.Id,
                        asnmaster_id = m.asnmaster_id,
                        asn_no = m.asn_no,
                        asn_batch = am.asn_batch,
                        estimated_arrival_time = am.estimated_arrival_time,
                        asn_status = m.asn_status,
                        spu_id = m.spu_id,
                        spu_code = p.spu_code,
                        spu_name = p.spu_name,
                        sku_id = m.sku_id,
                        sku_code = k.sku_code,
                        sku_name = k.sku_name,
                        origin = p.origin,
                        length_unit = p.length_unit.GetValueOrDefault(),
                        volume_unit = p.volume_unit.GetValueOrDefault(),
                        weight_unit = p.weight_unit.GetValueOrDefault(),
                        price = m.price,
                        asn_qty = m.asn_qty,
                        actual_qty = m.actual_qty,
                        arrival_time = m.arrival_time,
                        unload_person = m.unload_person,
                        unload_person_id = m.unload_person_id,
                        unload_time = m.unload_time,
                        sorted_qty = m.sorted_qty,
                        shortage_qty = m.shortage_qty,
                        more_qty = m.more_qty,
                        damage_qty = m.damage_qty,
                        weight = k.weight.GetValueOrDefault() * m.asn_qty,
                        volume = k.volume.GetValueOrDefault() * m.asn_qty,
                        supplier_id = m.supplier_id,
                        supplier_name = m.supplier_name,
                        goods_owner_id = m.goods_owner_id,
                        goods_owner_name = m.goods_owner_name,
                        creator = m.creator,
                        create_time = m.create_time,
                        last_update_time = m.last_update_time,
                        is_valid = m.is_valid,
                        expiry_date = m.expiry_date
                    };
        query = query.Where(queries.AsGroupedExpression<AsnViewModel>());

        int totals = 0;
        if (!queries.Any())
        {
            totals = await ansEntity.CountAsync(cancellationToken);
        }
        else
        {
            totals = await query.CountAsync(cancellationToken);
        }

        var list = await query.OrderByDescending(t => t.create_time)
                   .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                   .Take(pageSearch.pageSize)
                   .ToListAsync(cancellationToken);
        return (list, totals);
    }

    /// <summary>
    /// Get a record by id
    /// </summary>
    /// <returns></returns>
    public async Task<AsnViewModel> GetAsync(int id)
    {
        var Spus = _dbContext.GetDbSet<SpuEntity>();
        var Skus = _dbContext.GetDbSet<SkuEntity>();
        var Asns = _dbContext.GetDbSet<AsnEntity>();
        var Asnmasters = _dbContext.GetDbSet<AsnmasterEntity>();
        var query = from m in Asns.AsNoTracking()
                    join am in Asnmasters.AsNoTracking() on m.asnmaster_id equals am.Id
                    join p in Spus.AsNoTracking() on m.spu_id equals p.Id
                    join k in Skus.AsNoTracking() on m.sku_id equals k.Id
                    select new AsnViewModel
                    {
                        id = m.Id,
                        asnmaster_id = m.asnmaster_id,
                        asn_no = m.asn_no,
                        asn_batch = am.asn_batch,
                        estimated_arrival_time = am.estimated_arrival_time,
                        asn_status = m.asn_status,
                        spu_id = m.spu_id,
                        spu_code = p.spu_code,
                        spu_name = p.spu_name,
                        sku_id = m.sku_id,
                        sku_code = k.sku_code,
                        sku_name = k.sku_name,
                        origin = p.origin,
                        length_unit = p.length_unit.GetValueOrDefault(),
                        volume_unit = p.volume_unit.GetValueOrDefault(),
                        weight_unit = p.weight_unit.GetValueOrDefault(),
                        price = m.price,
                        asn_qty = m.asn_qty,
                        actual_qty = m.actual_qty,
                        arrival_time = m.arrival_time,
                        unload_person = m.unload_person,
                        unload_person_id = m.unload_person_id,
                        unload_time = m.unload_time,
                        sorted_qty = m.sorted_qty,
                        shortage_qty = m.shortage_qty,
                        more_qty = m.more_qty,
                        damage_qty = m.damage_qty,
                        weight = k.weight.GetValueOrDefault() * m.asn_qty,
                        volume = k.volume.GetValueOrDefault() * m.asn_qty,
                        supplier_id = m.supplier_id,
                        supplier_name = m.supplier_name,
                        goods_owner_id = m.goods_owner_id,
                        goods_owner_name = m.goods_owner_name,
                        creator = m.creator,
                        create_time = m.create_time,
                        last_update_time = m.last_update_time,
                        is_valid = m.is_valid,
                        expiry_date = m.expiry_date
                    };
        var data = await query.FirstOrDefaultAsync(t => t.id.Equals(id));
        return data ?? new AsnViewModel();
    }

    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(int id, string msg)> AddAsync(AsnViewModel viewModel, CurrentUser currentUser)
    {
        try
        {
            var dbSet = _dbContext.GetDbSet<AsnEntity>();
            // support random batch for AsnMaster
            viewModel.asn_batch ??= await _functionHelper.GetFormNoAsync("Asn");
            var entity = viewModel.Adapt<AsnEntity>();
            entity.Id = 0;
            entity.asn_no = await _functionHelper.GetFormNoAsync("Asn");
            entity.creator = currentUser.user_name;
            entity.create_time = DateTime.UtcNow;
            entity.last_update_time = DateTime.UtcNow;
            entity.TenantId = currentUser.tenant_id;
            entity.is_valid = viewModel.is_valid;
            await dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id > 0 ?
                (entity.Id, _stringLocalizer["save_success"]) :
                (0, _stringLocalizer["save_failed"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in add async with : {error}", ex.Message);
            return (0, _stringLocalizer["save_failed"]);
        }
    }

    /// <summary>
    /// get next code number
    /// </summary>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<string> GetOrderCode(CurrentUser currentUser)
    {
        var dbSet = _dbContext.GetDbSet<AsnEntity>(currentUser.tenant_id);
        string code = "";
        string date = DateTime.UtcNow.ToString("yyyy" + "MM" + "dd");
        string maxNo = await dbSet.MaxAsync(t => t.asn_no) ?? "";

        if (string.IsNullOrEmpty(maxNo))
        {
            code = date + "-0001";
        }
        else
        {
            try
            {
                string maxDate = maxNo[..8];
                string maxDateNo = maxNo[9..];
                if (date == maxDate)
                {
                    bool isParse = int.TryParse(maxDateNo, out int dd);
                    int newDateNo = dd + 1;
                    code = date + "-" + newDateNo.ToString("0000");
                }
                else
                {
                    code = date + "-0001";
                }
            }
            catch
            {
                code = date + "-0001";
            }
        }

        return code;
    }

    /// <summary>
    /// update a record
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> UpdateAsync(AsnViewModel viewModel)
    {
        var DbSet = _dbContext.GetDbSet<AsnEntity>();
        var entity = await DbSet.FirstOrDefaultAsync(t => t.Id.Equals(viewModel.id));
        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }
        entity.Id = viewModel.id;
        entity.asn_no = viewModel.asn_no;
        entity.spu_id = viewModel.spu_id;
        entity.sku_id = viewModel.sku_id;
        entity.price = viewModel.price;
        entity.asn_qty = viewModel.asn_qty;
        entity.weight = viewModel.weight;
        entity.volume = viewModel.volume;
        entity.supplier_id = viewModel.supplier_id;
        entity.supplier_name = viewModel.supplier_name;
        entity.goods_owner_id = viewModel.goods_owner_id;
        entity.goods_owner_name = viewModel.goods_owner_name;
        entity.is_valid = viewModel.is_valid;
        entity.last_update_time = DateTime.UtcNow;
        var qty = await _dbContext.SaveChangesAsync();
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
        var Asns = _dbContext.GetDbSet<AsnEntity>();
        var entity = await Asns.FirstOrDefaultAsync(t => t.Id.Equals(id));
        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }
        if (entity.asn_status.Equals(0))
        {
            Asns.Remove(entity);
        }
        else if (entity.asn_status.Equals(8))
        {
            return (false, _stringLocalizer["asn_had_putaway"]);
        }
        else
        {
            entity.asn_status = (byte)(entity.asn_status - 1);
        }
        var qty = await _dbContext.SaveChangesAsync();
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
    /// Bulk modify Goodsowner
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> BulkModifyGoodsownerAsync(AsnBulkModifyGoodsOwnerViewModel viewModel)
    {
        var Asns = _dbContext.GetDbSet<AsnEntity>();
        var entities = await Asns.Where(t => viewModel.idList.Contains(t.Id)).ToListAsync();
        // What restrictions are needed?
        entities.ForEach(t =>
        {
            t.goods_owner_id = viewModel.goods_owner_id;
            t.goods_owner_name = viewModel.goods_owner_name;
            t.last_update_time = DateTime.UtcNow;
        });
        var qty = await _dbContext.SaveChangesAsync();
        if (qty > 0)
        {
            return (true, _stringLocalizer["save_success"]);
        }
        else
        {
            return (false, _stringLocalizer["save_failed"]);
        }
    }

    #endregion Api

    #region Flow Api
    /// <summary>
    /// Confirm Delivery
    /// change the asn_status from 0 to 1
    /// </summary>
    /// <param name="viewModels">args</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ConfirmAsync(List<AsnConfirmInputViewModel> viewModels)
    {
        try
        {
            if (viewModels == null || viewModels.Count == 0)
            {
                return (false, _stringLocalizer["invalid_input"]);
            }

            //  Db Set
            var asnMasterDbSet = _dbContext.GetDbSet<AsnmasterEntity>();
            var asnDbSet = _dbContext.GetDbSet<AsnEntity>();


            var idList = viewModels.Where(t => t.id > 0)
                                            .Select(t => t.id)
                                            .ToList();

            var asnEntities = await asnDbSet.Where(t => idList.Contains(t.Id))
                                                      .ToListAsync();
            if (asnEntities.Count == 0)
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }
            else if (asnEntities.Any(t => t.asn_status > (byte)AsnStatusEnum.PREPARE_COMING))
            {
                return (false, _stringLocalizer["ASN_Status_Is_Not_Pre_Delivery"]);
            }

            // [UPDATE] Logic for handling missing receipts
            // If the actual number received is less than the ASN,
            // the system will separate the missing portion into a new line
            // to continue waiting for receipt in the next batch

            var addAsnEntities = new List<AsnEntity>();
            foreach (var currentAsn in asnEntities)
            {
                // Handle with Confirm with QTY < ASN QTY
                if (viewModels.Any(t => t.id == currentAsn.Id))
                {
                    var vm = viewModels.First(t => t.id == currentAsn.Id);
                    if (vm.input_qty < currentAsn.asn_qty)
                    {
                        // mapper for new ROW
                        var remainingAsnEntity = currentAsn.Adapt<AsnEntity>();
                        remainingAsnEntity.Id = 0; // new entity
                        remainingAsnEntity.asn_qty = currentAsn.asn_qty - vm.input_qty;
                        remainingAsnEntity.asn_status = (byte)AsnStatusEnum.PREPARE_COMING;
                        addAsnEntities.Add(remainingAsnEntity);

                        // update current asn qty
                        currentAsn.asn_qty = vm.input_qty;
                    }
                    currentAsn.asn_status = (byte)AsnStatusEnum.PREPARE_UNLOAD;
                    currentAsn.arrival_time = vm.arrival_time;
                    currentAsn.last_update_time = DateTime.UtcNow;
                }
            }

            // check and add new asn entities
            if (addAsnEntities.Count > 0)
            {
                await asnDbSet.AddRangeAsync(addAsnEntities);
            }


            // get asnmaster data
            var masterIds = asnEntities.Select(x => x.asnmaster_id).Distinct().ToList();
            var asnMasters = await asnMasterDbSet.Where(m => masterIds.Contains(m.Id)).ToListAsync();

            if (asnMasters.Count == 0)
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }

            // update asnmaster for any detail in progress
            foreach (var master in asnMasters)
            {
                master.last_update_time = DateTime.UtcNow;
                master.asn_status = (byte)AsnMasterStatusEnum.IN_PROGRESS;
            }


            var qtySuccess = await _dbContext.SaveChangesAsync();
            if (qtySuccess > 0)
            {
                return (true, _stringLocalizer["confirm_success"]);
            }
            else
            {
                return (false, _stringLocalizer["confirm_failed"]);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in confirm async with : {ex.Message}");
            return (false, _stringLocalizer["confirm_failed"]);
        }
    }

    /// <summary>
    /// Cancel confirm, change asn_status 1 to 0
    /// </summary>
    /// <param name="idList">id list</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ConfirmCancelAsync(List<int> idList)
    {
        try
        {
            var asnDbSet = _dbContext.GetDbSet<AsnEntity>();
            var asnMasterDbSet = _dbContext.GetDbSet<AsnmasterEntity>();

            var asnEntities = await asnDbSet.Where(t => idList.Contains(t.Id)).ToListAsync();
            if (asnEntities.Count == 0)
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }
            if (asnEntities.Any(t => t.asn_status != (byte)AsnStatusEnum.PREPARE_UNLOAD))
            {
                return (false, _stringLocalizer["ASN_Status_Is_Not_Pre_Delivery"]);
            }

            // new Flow 
            // Handle for revert to PREPARE_COMING status
            var entitiesToRemove = new List<AsnEntity>();
            foreach (var entity in asnEntities)
            {

                var sibling = await asnDbSet.FirstOrDefaultAsync(x =>
                    x.asnmaster_id == entity.asnmaster_id &&
                    x.asn_no == entity.asn_no &&
                    x.sku_id == entity.sku_id &&
                    x.asn_status == (byte)AsnStatusEnum.PREPARE_COMING &&
                    x.Id != entity.Id);

                if (sibling != null)
                {
                    sibling.asn_qty += entity.asn_qty;
                    sibling.last_update_time = DateTime.UtcNow;

                    // Remove current entity
                    entitiesToRemove.Add(entity);
                }
                else
                {
                    entity.asn_status = (byte)AsnStatusEnum.PREPARE_COMING;
                    entity.arrival_time = default;
                    entity.last_update_time = DateTime.UtcNow;
                }
            }

            if (entitiesToRemove.Count != 0)
            {
                asnDbSet.RemoveRange(entitiesToRemove);
            }


            var masterIds = asnEntities.Select(x => x.asnmaster_id).Distinct().ToList();
            var asnMasterEntities = await asnMasterDbSet.Where(m => masterIds.Contains(m.Id)).ToListAsync();

            foreach (var item in asnMasterEntities)
            {
                bool hasProcessingChild = await asnDbSet.AnyAsync(x =>
                 x.asnmaster_id == item.Id &&
                 x.asn_status > (byte)AsnStatusEnum.PREPARE_COMING &&
                 !idList.Contains(x.Id));

                if (!hasProcessingChild)
                {
                    item.asn_status = (byte)AsnMasterStatusEnum.CREATED;
                }
                item.last_update_time = DateTime.UtcNow;
            }

            var qty = await _dbContext.SaveChangesAsync();
            return qty > 0 ? (true, _stringLocalizer["save_success"]) :
                    (false, _stringLocalizer["save_failed"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in Method ConfirmCancelAsync");
            return (false, _stringLocalizer["save_failed"]);
        }
    }

    /// <summary>
    /// Unload
    /// change the asn_status from 1 to 2
    /// </summary>
    /// <param name="viewModels">args</param>
    /// <param name="user">user</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> UnloadAsync(List<AsnUnloadInputViewModel> viewModels, CurrentUser user)
    {
        try
        {
            if (viewModels == null || !viewModels.Any())
            {
                return (false, _stringLocalizer["invalid_input"]);
            }

            var asnDbSet = _dbContext.GetDbSet<AsnEntity>();
            var idList = viewModels.Where(t => t.id > 0).Select(t => t.id).ToList();

            var entities = await asnDbSet.Where(t => idList.Contains(t.Id)).ToListAsync();

            if (!entities.Any())
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }
            else if (entities.Any(t => t.asn_status > (byte)AsnStatusEnum.PREPARE_UNLOAD))
            {
                return (false, _stringLocalizer["ASN_Status_Is_Not_Pre_Load"]);
            }

            var addAsnEntities = new List<AsnEntity>();
            foreach (var currentEntity in entities)
            {
                var vm = viewModels.FirstOrDefault(v => v.id == currentEntity.Id);

                if (vm != null)
                {

                    if (vm.input_qty < currentEntity.asn_qty)
                    {
                        var remainingEntity = currentEntity.Adapt<AsnEntity>();
                        remainingEntity.Id = 0;
                        remainingEntity.asn_qty = currentEntity.asn_qty - vm.input_qty;
                        remainingEntity.asn_status = (byte)AsnStatusEnum.PREPARE_UNLOAD;

                        addAsnEntities.Add(remainingEntity);

                        currentEntity.asn_qty = vm.input_qty;
                    }


                    currentEntity.asn_status = (byte)AsnStatusEnum.PREPARE_QC;
                    currentEntity.last_update_time = DateTime.UtcNow;
                    currentEntity.unload_time = vm.unload_time;


                    currentEntity.unload_person_id = vm.unload_person_id == 0 ? user.user_id : vm.unload_person_id;
                    currentEntity.unload_person = string.IsNullOrEmpty(vm.unload_person) ? user.user_name : vm.unload_person;
                }
            }

            if (addAsnEntities.Any())
            {
                await asnDbSet.AddRangeAsync(addAsnEntities);
            }

            var masterIds = entities.Select(x => x.asnmaster_id).Distinct().ToList();
            var masterDbSet = _dbContext.GetDbSet<AsnmasterEntity>();
            var masters = await masterDbSet.Where(m => masterIds.Contains(m.Id)).ToListAsync();
            foreach (var m in masters)
            {
                m.last_update_time = DateTime.UtcNow;
            }

            var qty = await _dbContext.SaveChangesAsync();
            return qty > 0 ? (true, _stringLocalizer["confirm_success"]) :
                    (false, _stringLocalizer["confirm_failed"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in Method UnloadAsync");
            return (false, _stringLocalizer["confirm_failed"]);
        }
    }

    /// <summary>
    /// Cancel unload
    /// change the asn_status from 2 to 1
    /// </summary>
    /// <param name="idList">id list</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> UnloadCancelAsync(List<int> idList)
    {
        var asnDbSet = _dbContext.GetDbSet<AsnEntity>();
        var asnMasterDbSet = _dbContext.GetDbSet<AsnmasterEntity>();
        var asnSortDbSet = _dbContext.GetDbSet<AsnsortEntity>();

        var asnEntity = await asnDbSet.Where(t => idList.Contains(t.Id)).ToListAsync();
        if (!asnEntity.Any())
        {
            return (false, "[202]" + _stringLocalizer["not_exists_entity"]);
        }
        if (asnEntity.Any(t => t.asn_status != (byte)AsnStatusEnum.PREPARE_QC))
        {
            return (false, "[202]" + $"{_stringLocalizer["ASN_Status_Is_Not_Pre_Load"]}");
        }

        var entitiesRemove = new List<AsnEntity>();
        foreach (var entity in asnEntity)
        {

            var sibling = await asnDbSet.FirstOrDefaultAsync(x =>
                x.asnmaster_id == entity.asnmaster_id &&
                x.asn_no == entity.asn_no &&
                x.sku_id == entity.sku_id &&
                x.asn_status == (byte)AsnStatusEnum.PREPARE_UNLOAD &&
                x.Id != entity.Id);

            if (sibling != null)
            {
                sibling.asn_qty += entity.asn_qty;
                sibling.last_update_time = DateTime.UtcNow;

                entitiesRemove.Add(entity);
            }
            else
            {

                entity.asn_status = (byte)AsnStatusEnum.PREPARE_UNLOAD;

                entity.unload_time = default;
                entity.unload_person_id = 0;
                entity.unload_person = string.Empty;
                entity.last_update_time = DateTime.UtcNow;
            }
        }

        if (entitiesRemove.Any())
        {
            asnDbSet.RemoveRange(entitiesRemove);
        }

        //  var asnSortEntity = asnSortDbSet.Where(t => asnEntity.Contains((t.Id));

        var masterIds = asnEntity.Select(x => x.asnmaster_id).Distinct().ToList();
        var masters = await asnMasterDbSet.Where(m => masterIds.Contains(m.Id)).ToListAsync();
        foreach (var m in masters)
        {
            m.last_update_time = DateTime.UtcNow;
        }
        var qty = await _dbContext.SaveChangesAsync();
        return qty > 0 ? (true, _stringLocalizer["save_success"]) :
                (false, _stringLocalizer["save_failed"]);
    }

    /// <summary>
    /// sorting， add a new asnsort record and update asn sorted_qty
    /// </summary>
    /// <param name="viewModels">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> SortingAsync(List<AsnsortInputViewModel> viewModels, CurrentUser currentUser)
    {
        try
        {
            var asnDbSet = _dbContext.GetDbSet<AsnEntity>();
            var asnSortDbSet = _dbContext.GetDbSet<AsnsortEntity>();

            var idList = viewModels.Select(t => t.asn_id).ToList().Distinct().ToList();
            var entities = await asnDbSet.Where(t => idList.Contains(t.Id)).ToListAsync();

            if (!entities.Any())
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }
            else if (entities.Any(t => t.asn_status != (byte)AsnStatusEnum.PREPARE_QC))
            {
                return (false, _stringLocalizer["ASN_Status_Is_Not_Pre_Sort"]);
            }
            var models = viewModels.Where(v => entities.Select(e => e.Id).ToList().Contains(v.asn_id)).ToList();

            var sortEntities = new List<AsnsortEntity>();
            foreach (var entity in entities)
            {
                var currentVms = viewModels.Where(v => v.asn_id == entity.Id).ToList();
                int totalNewSortQty = currentVms.Sum(v => v.sorted_qty);
                if (entity.sorted_qty + totalNewSortQty > entity.asn_qty)
                {
                    return (false, $"The Number Has Exceed with value : (Has QC: {entity.sorted_qty}, Add More: {totalNewSortQty}, Total ASN: {entity.asn_qty})");
                }
                entity.sorted_qty += totalNewSortQty;
                entity.last_update_time = DateTime.UtcNow;

                var expiryDate = currentVms.Select(v => v.expiry_date).FirstOrDefault();
                if (expiryDate != default)
                {
                    entity.expiry_date = expiryDate;
                }

                foreach (var v in currentVms)
                {
                    if (v.sorted_qty > 1 && v.is_auto_num)
                    {
                        List<string> snlist = await _functionHelper.GetFormNoListAsync("Asnsort", v.sorted_qty, currentUser.tenant_id, "sn");
                        for (int i = 0; i < v.sorted_qty; i++)
                        {
                            sortEntities.Add(new AsnsortEntity
                            {
                                asn_id = v.asn_id,
                                sorted_qty = 1,
                                series_number = snlist[i],
                                create_time = DateTime.UtcNow,
                                creator = currentUser.user_name,
                                is_valid = true,
                                last_update_time = DateTime.UtcNow,
                                TenantId = currentUser.tenant_id,
                                Id = 0,
                                putaway_qty = 0,
                            });
                        }
                    }
                    else
                    {
                        string sn = await _functionHelper.GetFormNoAsync("Asnsort", "PLN");
                        sortEntities.Add(new AsnsortEntity
                        {
                            asn_id = v.asn_id,
                            sorted_qty = v.sorted_qty,
                            series_number = sn,
                            create_time = DateTime.UtcNow,
                            creator = currentUser.user_name,
                            is_valid = true,
                            last_update_time = DateTime.UtcNow,
                            TenantId = currentUser.tenant_id,
                            Id = 0,
                            putaway_qty = 0,
                        });
                    }
                }
            }

            if (sortEntities.Count != 0)
            {
                await asnSortDbSet.AddRangeAsync(sortEntities);
            }

            var qty = await _dbContext.SaveChangesAsync();
            return qty > 0 ? (true, _stringLocalizer["save_success"]) :
                    (false, _stringLocalizer["save_failed"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in SortingAsync Method");
            return (false, _stringLocalizer["save_failed"]);
        }
    }


    /// <summary>
    /// get asnsorts list by asn_id
    /// </summary>
    /// <param name="asn_id">asn id</param>
    /// <returns></returns>
    public async Task<List<AsnsortViewModel>> GetAsnsortsAsync(int asn_id)
    {
        var Asnsorts = _dbContext.GetDbSet<AsnsortEntity>();
        var asns = _dbContext.Set<AsnEntity>().AsNoTracking();

        var data = await (from m in asns
                          join d in Asnsorts on m.Id equals d.asn_id
                          where m.Id == asn_id
                          select new AsnsortViewModel
                          {
                              id = d.Id,
                              asn_id = asn_id,
                              sorted_qty = d.sorted_qty,
                              series_number = d.series_number,
                              putaway_qty = d.putaway_qty,
                              expiry_date = m.expiry_date,
                              creator = d.creator,
                              create_time = d.create_time,
                              last_update_time = d.last_update_time,
                              is_valid = d.is_valid,
                              tenant_id = d.TenantId
                          }).ToListAsync();

        if (data != null && data?.Count > 0)
        {
            return data;
        }

        return [];
    }

    /// <summary>
    /// update or delete asnsorts data
    /// </summary>
    /// <param name="entities">data</param>
    /// <param name="user">CurrentUser</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ModifyAsnsortsAsync(List<AsnsortEntity> entities, CurrentUser user)
    {
        var Asnsorts = _dbContext.GetDbSet<AsnsortEntity>();
        if (entities.Any(t => t.Id < 0 || t.sorted_qty == 0))
        {
            var delIDList = entities.Where(t => t.Id < 0).Select(t => Math.Abs(t.Id)).ToList();
            await Asnsorts.Where(t => delIDList.Contains(t.Id)).ExecuteDeleteAsync();
        }
        var updateEntities = entities.Where(t => t.Id > 0 && t.sorted_qty > 0).ToList();
        if (updateEntities.Count != 0)
        {
            updateEntities.ForEach(t =>
            {
                t.last_update_time = DateTime.UtcNow;
                t.is_valid = true;
            });
            Asnsorts.UpdateRange(updateEntities);
        }

        var qty = await _dbContext.SaveChangesAsync();
        if (qty >= 0)
        {
            var Asns = _dbContext.GetDbSet<AsnEntity>();
            var asnids = entities.Select(t => t.asn_id).Distinct().ToList();

            var sumQty = await Asnsorts.AsNoTracking()
                .Where(t => asnids.Contains(t.asn_id))
                .GroupBy(t => t.asn_id)
                .Select(g => new
                {
                    asn_id = g.Key,
                    sorted_qty = g.Sum(o => o.sorted_qty)
                }).ToListAsync();
            var asnEntities = await Asns.Where(t => asnids.Contains(t.Id)).ToListAsync();

            if (asnEntities.Count != 0)
            {
                asnEntities.ForEach(e =>
                {
                    var s = sumQty.FirstOrDefault(t => t.asn_id == e.Id);
                    if (s != null)
                    {
                        e.sorted_qty = s.sorted_qty;
                    }
                    else
                    {
                        e.sorted_qty = 0;
                    }
                });
                await _dbContext.SaveChangesAsync();
            }

            return (true, _stringLocalizer["sorted_success"]);
        }
        else
        {
            return (false, _stringLocalizer["sorted_failed"]);
        }
    }

    /// <summary>
    /// Sorted
    /// change the asn_status from 2 to 3
    /// </summary>
    /// <param name="idList">id list</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> SortedAsync(List<int> idList)
    {
        try
        {
            var entities = await _dbContext.GetDbSet<AsnEntity>()
                                    .Where(t => idList.Contains(t.Id))
                                    .ToListAsync();

            if (entities.Count == 0)
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }
            else if (entities.Any(t => t.sorted_qty < 1))
            {
                return (false, $"{_stringLocalizer["ASN_Status_Is_Not_Sorting"]}");
            }

            var asnSort = await _dbContext.GetDbSet<AsnsortEntity>()
                                      .Where(t => entities.Select(s => s.Id)
                                      .Contains(t.asn_id)).ToListAsync();

            var newSplitAsnEntities = new List<AsnEntity>();
            foreach (var entity in entities)
            {
                var actualSortedQty = asnSort.Where(t => t.asn_id == entity.Id)
                                                .Sum(t => t.sorted_qty);

                if (entity.sorted_qty != actualSortedQty)
                {
                    _logger.LogError("The Sorted Quantity is inconsistent for ASN ID: {id}", entity.Id);
                    return (false, $"The Sorted Quantity is inconsistent for ASN ID: {entity.Id}");
                }

                if (actualSortedQty <= 0)
                {
                    _logger.LogError("");
                    return (false, $"The Sorted Quantity is invalid for ASN ID: {entity.Id}");
                }

                if (actualSortedQty > entity.asn_qty)
                {
                    var errMsg = $"Overage Error: Actual ({actualSortedQty}) > ASN Qty ({entity.asn_qty}) for ASN ID: {entity.Id}";
                    _logger.LogError("{errMsg}", errMsg);
                    return (false, errMsg);
                }

                if (actualSortedQty < entity.asn_qty)
                {
                    _logger.LogInformation("");
                    var remainingQty = entity.asn_qty - actualSortedQty;
                    var remaningAsnEntity = entity.Adapt<AsnEntity>();
                    remaningAsnEntity.Id = 0;
                    remaningAsnEntity.asn_qty = remainingQty;
                    remaningAsnEntity.sorted_qty = 0;
                    remaningAsnEntity.asn_status = (byte)AsnStatusEnum.PREPARE_QC; // base on the current 
                    remaningAsnEntity.create_time = DateTime.UtcNow;
                    remaningAsnEntity.shortage_qty = 0;

                    newSplitAsnEntities.Add(remaningAsnEntity);

                    entity.asn_qty = actualSortedQty;
                    entity.sorted_qty = actualSortedQty;
                    entity.shortage_qty = 0;
                }
                else
                {
                    _logger.LogInformation("The Sorted Quantity is valid for ASN ID: {id}", entity.Id);
                    entity.shortage_qty = 0;
                }
                entity.asn_status = (byte)AsnStatusEnum.PREPARE_PUTAWAY;
                entity.last_update_time = DateTime.UtcNow;
            }

            if (newSplitAsnEntities.Count != 0)
            {
                await _dbContext.AddRangeAsync(newSplitAsnEntities);
            }

            //entities.ForEach(e =>
            //{
            //    e.asn_status = (byte)AsnStatusEnum.PREPARE_PUTAWAY;
            //    if (e.sorted_qty > e.asn_qty)
            //    {
            //        e.more_qty = e.sorted_qty - e.asn_qty;
            //    }
            //    else if (e.sorted_qty < e.asn_qty)
            //    {
            //        e.shortage_qty = e.asn_qty - e.sorted_qty;
            //    }
            //    e.last_update_time = DateTime.UtcNow;
            //});

            var qty = await _dbContext.SaveChangesAsync();
            if (qty > 0)
            {
                _logger.LogInformation("Sorted successfully");
                return (true, _stringLocalizer["sorted_success"]);
            }
            else
            {
                _logger.LogError("Sorted failed");
                return (false, _stringLocalizer["sorted_failed"]);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in Method");
            return (false, "Sorted failed");
        }
    }

    /// <summary>
    /// Cancel sorted
    /// change the asn_status from 3 to 2
    /// </summary>
    /// <param name="idList">id list</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> SortedCancelAsync(List<int> idList)
    {
        var Asns = _dbContext.GetDbSet<AsnEntity>();
        var entities = new List<AsnEntity>();
        foreach (var id in idList)
        {
            // If idList only has one element:
            var entity = await Asns.Where(t => t.Id == id).ToListAsync();
            if (entity != null)
            {
                entities.AddRange(entity);
            }
        }

        if (entities.Count == 0)
        {
            return (false, "[202]" + _stringLocalizer["not_exists_entity"]);
        }
        else if (entities.Any(t => t.actual_qty > 0))
        {
            return (false, "[202]" + $"{_stringLocalizer["ASN_Status_Is_Putaway"]}");
        }
        else if (entities.Any(t => t.sorted_qty < 1))
        {
            return (false, "[202]" + $"{_stringLocalizer["ASN_Status_Is_Not_Sorting"]}");
        }
        else if (entities.Any(t => t.asn_status != (byte)AsnStatusEnum.PREPARE_PUTAWAY))
        {
            return (false, "[202]" + $"{_stringLocalizer["ASN_Status_Is_Not_Prepare_Putaway"]}");
        }
        entities.ForEach(e =>
        {
            e.asn_status = (byte)AsnStatusEnum.PREPARE_QC;
            e.sorted_qty = 0;
            e.more_qty = 0;
            e.shortage_qty = 0;
            e.last_update_time = DateTime.UtcNow;
        });
        var qty = await _dbContext.SaveChangesAsync();
        if (qty > 0)
        {
            var Asnsorts = _dbContext.GetDbSet<AsnsortEntity>();
            await Asnsorts.Where(t => idList.Contains(t.asn_id)).ExecuteDeleteAsync();
            return (true, _stringLocalizer["save_success"]);
        }
        else
        {
            return (false, _stringLocalizer["save_failed"]);
        }
    }

    /// <summary>
    /// get pending putaway data by asn_id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<List<AsnPendingPutawayViewModel>> GetPendingPutawayDataAsync(int id)
    {
        var asnDbSets = _dbContext.GetDbSet<AsnEntity>();
        var skuDbSets = _dbContext.GetDbSet<SkuEntity>();
        var asnSortDbSets = _dbContext.GetDbSet<AsnsortEntity>();

        var data = await (from m in asnDbSets.AsNoTracking()
                          join s in asnSortDbSets.AsNoTracking() on m.Id equals s.asn_id
                          join k in skuDbSets.AsNoTracking() on m.sku_id equals k.Id
                          where m.Id == id && s.putaway_qty < s.sorted_qty
                          group new { m, s, k } by new { m.Id, m.goods_owner_id, m.goods_owner_name, s.series_number, m.asn_no, m.expiry_date, k.sku_name }
                          into g
                          select new AsnPendingPutawayViewModel
                          {
                              asn_id = g.Key.Id,
                              goods_owner_id = g.Key.goods_owner_id,
                              goods_owner_name = g.Key.goods_owner_name,
                              series_number = g.Key.series_number,
                              asn_no = g.Key.asn_no,
                              expiry_date = g.Key.expiry_date,
                              sku_name = g.Key.sku_name,
                              sorted_qty = g.Sum(o => o.s.sorted_qty - o.s.putaway_qty)
                          }).ToListAsync();
        return data;
    }

    /// <summary>
    /// PutAway
    /// </summary>
    /// <param name="viewModels">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> PutAwayAsync(List<AsnPutAwayInputViewModel> viewModels, CurrentUser currentUser)
    {
        if (viewModels == null || viewModels.Count == 0)
        {
            return (false, "No items to process");
        }

        if (viewModels.Any(v => v.putaway_qty < 1))
        {
            return (false, "Quantity must be greater than 0");
        }

        if (viewModels.Any(t => t.goods_location_id == 0))
        {
            return (false, "[202]" + string.Format(_stringLocalizer["Required"], _stringLocalizer["LocationName"]));
        }
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        {
            try
            {
                var asnsDbSet = _dbContext.GetDbSet<AsnEntity>();
                var goodslocationsDbSet = _dbContext.GetDbSet<GoodslocationEntity>();
                var stocksDbSet = _dbContext.GetDbSet<StockEntity>();
                var asnSortsDbSet = _dbContext.GetDbSet<AsnsortEntity>();

                var locationIdList = viewModels
                    .Where(v => v.goods_location_id > 0)
                    .Select(v => v.goods_location_id)
                    .Distinct().ToList();

                var locations = await goodslocationsDbSet
                    .Where(t => locationIdList.Contains(t.Id))
                    .ToListAsync();

                if (locations.Count != locationIdList.Count)
                {
                    return (false, "Some locations not found");
                }

                if (locations.Any(l => !l.IsValid || l.LocationStatus != (byte)GoodLocationStatusEnum.AVAILABLE))
                {
                    return (false, "Some locations are not AVAILABLE");
                }

                int sumPutawayQty = viewModels.Sum(v => v.putaway_qty);

                var asnId = viewModels.First().asn_id;
                var entity = await asnsDbSet.FirstOrDefaultAsync(t => t.Id == asnId);

                if (entity == null)
                {
                    return (false, "[202]" + _stringLocalizer["not_exists_entity"]);
                }

                else if (entity.asn_status != (byte)AsnStatusEnum.PREPARE_PUTAWAY)
                {
                    return (false, "[202]" + $"{entity.asn_no}{_stringLocalizer["ASN_Status_Is_Not_Sorted"]}");
                }

                else if (entity.actual_qty + sumPutawayQty > entity.sorted_qty)
                {
                    return (false, "[202]" + $"{entity.asn_no}{_stringLocalizer["ASN_Total_PutAway_Qty_Greater_Than_Sorted_Qty"]}");
                }


                entity.actual_qty += sumPutawayQty;

                if (entity.actual_qty.Equals(entity.sorted_qty))
                {
                    entity.asn_status = (byte)AsnStatusEnum.WAITING_ROBOT;

                    // this section is commented out to avoid changing asnmaster status prematurely
                    //if (entity.asnmaster_id > 0)
                    //{
                    //    bool hasIncompleteSibling = await asnsDbSet.AnyAsync(t =>
                    //        t.asnmaster_id == entity.asnmaster_id
                    //        && t.Id != entity.Id
                    //        && t.asn_status != (byte)AsnStatusEnum.WAITING_ROBOT);

                    //    if (!hasIncompleteSibling)
                    //    {
                    //        var AsnMasters = _dBContext.GetDbSet<AsnmasterEntity>();
                    //        var masterEntity = await AsnMasters.FirstOrDefaultAsync(m => m.Id == entity.asnmaster_id);

                    //        if (masterEntity != null && masterEntity.asn_status != (byte)AsnMasterStatusEnum.COMPLETED)
                    //        {
                    //            masterEntity.asn_status = (byte)AsnMasterStatusEnum.COMPLETED;
                    //            masterEntity.last_update_time = DateTime.UtcNow;
                    //        }
                    //    }
                    //}
                }

                entity.last_update_time = DateTime.UtcNow;

                var expiry_date = entity.expiry_date;

                var sortEntities = await asnSortsDbSet
                    .Where(t => t.asn_id == viewModels.First().asn_id && t.sorted_qty > t.putaway_qty)
                    .ToListAsync();

                foreach (var viewModel in viewModels)
                {
                    var sortList = sortEntities.Where(s => s.series_number == viewModel.series_number).ToList();
                    if (sortList.Count != 0)
                    {
                        int left_putaway_qty = viewModel.putaway_qty;
                        sortList.ForEach(s =>
                        {
                            if (left_putaway_qty > 0)
                            {
                                int canPutawayQty = s.sorted_qty - s.putaway_qty;
                                if (left_putaway_qty > canPutawayQty)
                                {
                                    s.putaway_qty += canPutawayQty;
                                    left_putaway_qty -= canPutawayQty;
                                }
                                else
                                {
                                    s.putaway_qty += left_putaway_qty;
                                    left_putaway_qty = 0;
                                }
                            }
                        });
                    }

                    var Location = locations.FirstOrDefault(t => t.Id == viewModel.goods_location_id);
                    if (Location != null && Location.WarehouseAreaProperty.Equals(5))
                    {
                        entity.damage_qty += viewModel.putaway_qty;
                    }
                    DateTime putaway_date = DateTime.UtcNow;

                    var stockEntity = await stocksDbSet.FirstOrDefaultAsync(t => t.sku_id.Equals(entity.sku_id)
                                                    && t.goods_location_id.Equals(viewModel.goods_location_id)
                                                    && t.goods_owner_id.Equals(viewModel.goods_owner_id)
                                                    && t.series_number.Equals(viewModel.series_number)
                                                    && t.expiry_date.Equals(expiry_date)
                                                    && t.price.Equals(entity.price)
                                                    && t.PutAwayDate.Equals(putaway_date)
                                                    );
                    if (stockEntity == null)
                    {
                        var locationCheck = await goodslocationsDbSet.FindAsync(viewModel.goods_location_id) ?? throw new Exception($"Location not found: {viewModel.goods_location_id}");
                        if (!locationCheck.IsValid && locationCheck.LocationStatus != (byte)GoodLocationStatusEnum.AVAILABLE)
                        {
                            throw new Exception("Location is not avalable for chosing");
                        }
                        stockEntity = new StockEntity
                        {
                            sku_id = entity.sku_id,
                            goods_location_id = viewModel.goods_location_id,
                            goods_owner_id = entity.goods_owner_id,
                            series_number = viewModel.series_number,
                            qty = viewModel.putaway_qty,
                            is_freeze = false,
                            last_update_time = DateTime.UtcNow,
                            TenantId = currentUser.tenant_id,
                            expiry_date = expiry_date,
                            price = entity.price,
                            PutAwayDate = putaway_date,
                            Id = 0,
                            AsnMasterID = entity.asnmaster_id
                        };
                        await stocksDbSet.AddAsync(stockEntity);
                        // marks as this location has occuied by pallet
                        locationCheck.IsValid = false;
                        locationCheck.LocationStatus = (byte)GoodLocationStatusEnum.OCCUPIED;
                    }
                    else
                    {
                        stockEntity.qty += viewModel.putaway_qty;
                        stockEntity.last_update_time = DateTime.UtcNow;
                    }
                }


                //TODO : handle wcs integration here 
                // create inbound record for WCS system
                // wcs for control robot to pick up the pallet and move to the location

                var wcsInboundList = viewModels
                      .GroupBy(x => new { x.series_number, x.goods_location_id })
                      .Select(g => new CreateInboundTaskDTO
                      {
                          PalletCode = g.Key.series_number,
                          LocationId = g.Key.goods_location_id,
                          PickUpDate = DateTime.UtcNow,
                          Priority = 1, // default value 
                          IsActive = true,
                      })
                      .ToList();

                var inboundEntities = await _integrationService.CreateInboundEntitiesAsync(wcsInboundList, currentUser);

                if (inboundEntities.Count == 0)
                {
                    throw new Exception("Failed to create WCS inbound task");
                }

                var historyCreated = await _integrationService.CreateIntegrationHistoryAsync(inboundEntities, currentUser);

                if (!historyCreated)
                {
                    throw new Exception("Failed to create WCS integration history");
                }

                // Save any remaining tracked changes (e.g., stock, location updates)
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return (true, _stringLocalizer["putaway_success"]);
            }

            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Exception occurred while processing ASN");
                return (false, _stringLocalizer["putaway_failed"]);
            }
        }
    }

    #endregion Flow Api

    #region Arrival list

    /// <summary>
    /// Arrival list
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(List<AsnmasterBothViewModel> data, int totals)> PageAsnMasterAsync(PageSearch pageSearch, CurrentUser currentUser)
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

            Byte asn_status = 255;

            if (pageSearch.sqlTitle.Contains("asn_status", StringComparison.CurrentCultureIgnoreCase))
            {
                asn_status = Convert.ToByte(pageSearch.sqlTitle.Trim().ToLower().Replace("asn_status", "").Replace("：", "").Replace(":", "").Replace("=", ""));
                asn_status = asn_status.Equals(4) ? (Byte)255 : asn_status;
            }
            var spuDbSet = _dbContext.GetDbSet<SpuEntity>();
            var skuDbSet = _dbContext.GetDbSet<SkuEntity>();
            var asnDbSet = _dbContext.GetDbSet<AsnEntity>();
            var asnMasterDbSet = _dbContext.GetDbSet<AsnmasterEntity>();
            var warehouseDbSet = _dbContext.GetDbSet<WarehouseEntity>();
            var asnSortDbSet = _dbContext.GetDbSet<AsnsortEntity>();
            // Page Asn master Async query
            var query = from m in asnMasterDbSet.AsNoTracking()
                        join w in warehouseDbSet.AsNoTracking() on m.warehouse_id equals w.Id into warehouseJoin
                        from w in warehouseJoin.DefaultIfEmpty()
                        where (asn_status == 255 || m.asn_status == asn_status)
                        select new AsnmasterBothViewModel
                        {
                            id = m.Id,
                            asn_no = m.asn_no,
                            asn_batch = m.asn_batch,
                            estimated_arrival_time = m.estimated_arrival_time,
                            asn_status = m.asn_status,
                            weight = m.weight,
                            volume = m.volume,
                            goods_owner_id = m.goods_owner_id,
                            goods_owner_name = m.goods_owner_name,
                            warehouse_id = m.warehouse_id,
                            warehouse_name = w != null ? w.WarehouseName : string.Empty,
                            creator = m.creator,
                            create_time = m.create_time,
                            last_update_time = m.last_update_time,
                            has_rejected_items = (from a in asnDbSet
                                                  join s in asnSortDbSet on a.Id equals s.asn_id
                                                  where a.asnmaster_id == m.Id
                                                     && s.is_valid == true
                                                     && s.status == AsnSortStatusEnum.Pending
                                                     && s.good_location_id == 0
                                                  select s.Id).Any(),
                            detailList = (from a in asnDbSet.AsNoTracking()
                                          join p in spuDbSet.AsNoTracking() on a.spu_id equals p.Id
                                          join k in skuDbSet.AsNoTracking() on a.sku_id equals k.Id
                                          where a.asnmaster_id == m.Id
                                          select new AsnmasterDetailViewModel
                                          {
                                              id = a.Id,
                                              asnmaster_id = a.asnmaster_id,
                                              asn_status = a.asn_status,
                                              spu_id = a.spu_id,
                                              spu_code = p.spu_code,
                                              spu_name = p.spu_name,
                                              sku_id = a.sku_id,
                                              sku_code = k.sku_code,
                                              sku_name = k.sku_name,
                                              origin = p.origin,
                                              length_unit = p.length_unit.GetValueOrDefault(),
                                              volume_unit = p.volume_unit.GetValueOrDefault(),
                                              weight_unit = p.weight_unit.GetValueOrDefault(),
                                              asn_qty = a.asn_qty,
                                              actual_qty = a.actual_qty,
                                              weight = a.weight,
                                              volume = a.volume,
                                              supplier_id = a.supplier_id,
                                              supplier_name = a.supplier_name,
                                              is_valid = a.is_valid,
                                              expiry_date = a.expiry_date,
                                              price = a.price,
                                              sorted_qty = a.sorted_qty,
                                          }).ToList()
                        };

            query = query.Where(queries.AsGroupedExpression<AsnmasterBothViewModel>());
            int totals = await query.CountAsync();

            var list = await query.OrderBy(x => x.asn_status)
                .ThenBy(t => t.estimated_arrival_time)
                .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                .Take(pageSearch.pageSize)
                .ToListAsync();

            return (list, totals);
        }
        catch (Exception ex)
        {
            // logger stackTrace
            _logger.LogError(ex, "Error in PageAsnmasterAsync");
            return (new List<AsnmasterBothViewModel>(), 0);
        }
    }

    /// <summary>
    /// get Arrival list
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<AsnmasterBothViewModel> GetAsnmasterAsync(int id, CurrentUser currentUser)
    {
        var Spus = _dbContext.GetDbSet<SpuEntity>().AsNoTracking();
        var Skus = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var Asns = _dbContext.GetDbSet<AsnEntity>().AsNoTracking();
        var ansSort = _dbContext.GetDbSet<AsnsortEntity>()
                                .Where(t => t.is_valid && t.status != AsnSortStatusEnum.Cancel)
                                .AsNoTracking();

        var Asnmasters = _dbContext.GetDbSet<AsnmasterEntity>().AsNoTracking();
        var Warehouses = _dbContext.GetDbSet<WarehouseEntity>().AsNoTracking();
        var GoodsLocations = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
        var Pallets = _dbContext.GetDbSet<PalletEntity>(currentUser.tenant_id);

        var query = from m in Asnmasters
                    join w in Warehouses on m.warehouse_id equals w.Id into warehouseJoin
                    from w in warehouseJoin.DefaultIfEmpty()
                    where m.Id == id
                    select new AsnmasterBothViewModel
                    {
                        id = m.Id,
                        asn_no = m.asn_no,
                        asn_batch = m.asn_batch,
                        estimated_arrival_time = m.estimated_arrival_time,
                        asn_status = m.asn_status,
                        weight = m.weight,
                        volume = m.volume,
                        goods_owner_id = m.goods_owner_id,
                        goods_owner_name = m.goods_owner_name,
                        warehouse_id = m.warehouse_id,
                        warehouse_name = w != null ? w.WarehouseName : string.Empty,
                        creator = m.creator,
                        create_time = m.create_time,
                        last_update_time = m.last_update_time,
                        detailList = (from a in Asns
                                      where a.asnmaster_id == m.Id
                                      join p in Spus on a.spu_id equals p.Id into spuGroup
                                      from p in spuGroup.DefaultIfEmpty()

                                      join k in Skus on a.sku_id equals k.Id into skuGroup
                                      from k in skuGroup.DefaultIfEmpty()

                                      join s in ansSort on a.Id equals s.asn_id into sortGroup
                                      from s in sortGroup.DefaultIfEmpty()
                                      join l in GoodsLocations on (s == null ? 0 : s.good_location_id) equals l.Id into locGroup
                                      from l in locGroup.DefaultIfEmpty()


                                      join pl in Pallets on (s == null ? 0 : s.pallet_id) equals pl.Id into plGroup
                                      from pl in plGroup.DefaultIfEmpty()

                                      select new AsnmasterDetailViewModel
                                      {
                                          id = a.Id,
                                          asnmaster_id = a.asnmaster_id,
                                          asn_status = a.asn_status,
                                          spu_id = a.spu_id,
                                          spu_code = p != null ? p.spu_code : "",
                                          spu_name = p != null ? p.spu_name : "",
                                          sku_id = a.sku_id,
                                          sku_code = k != null ? k.sku_code : "",
                                          sku_name = k != null ? k.sku_name : "",
                                          origin = p.origin,
                                          length_unit = p.length_unit.GetValueOrDefault(),
                                          volume_unit = p.volume_unit.GetValueOrDefault(),
                                          weight_unit = p.weight_unit.GetValueOrDefault(),
                                          asn_qty = a.asn_qty,
                                          actual_qty = a.actual_qty,
                                          weight = a.weight,
                                          volume = a.volume,
                                          supplier_id = a.supplier_id,
                                          supplier_name = a.supplier_name,
                                          is_valid = a.is_valid,
                                          sorted_qty = s != null ? s.sorted_qty : 0,
                                          goods_location_id = s != null ? s.good_location_id : 0,
                                          goods_location_name = l != null ? l.LocationName : "",
                                          pallet_id = pl != null ? pl.Id : 0,
                                          pallet_code = pl != null ? pl.PalletCode : (s != null ? s.series_number : ""),


                                          uom_id = a.uom_id,
                                          asn_qty_decimal = a.asn_qty_decimal,
                                          actual_qty_decimal = a.actual_qty_decimal,
                                          batch_number = a.batch_number,
                                          description = a.description,
                                          expiry_date = a.expiry_date,
                                          price = a.price
                                      }).ToList()
                    };
        var data = await query.FirstOrDefaultAsync();
        return data ?? new AsnmasterBothViewModel();
    }

    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(int id, string msg)> AddAsnmasterAsync(AsnmasterBothViewModel viewModel, CurrentUser currentUser)
    {
        if (viewModel == null || viewModel.detailList == null || viewModel.detailList.Count == 0)
        {
            _logger.LogError("Asnmaster detail list is empty");
            return (0, _stringLocalizer["save_failed"]);
        }

        var asnDbSet = _dbContext.GetDbSet<AsnEntity>();
        var asnMasterDbSet = _dbContext.GetDbSet<AsnmasterEntity>();
        var poDbSet = _dbContext.GetDbSet<PurchaseOrderEntity>();
        var poDetailDbSet = _dbContext.GetDbSet<PurchaseOrderDetailsEntity>();
        var skuDbSet = _dbContext.GetDbSet<SkuEntity>();
        var skuUomDbSet = _dbContext.GetDbSet<SkuUomEntity>();

        try
        {
            if (viewModel.po_id != null && viewModel.po_id > 0)
            {
                var poEntity = await poDbSet.FirstOrDefaultAsync(t => t.Id == viewModel.po_id) ?? throw new Exception("Can't not get the value from the PurchaseOrder");
                if (poEntity.PoStatus == PoStatusEnum.COMPLETED)
                {
                    return (0, "PO has been Completed");
                }
                if (poEntity.PoStatus == PoStatusEnum.CANCELED)
                {
                    return (0, "PO has been Canceled");
                }
                var poDetails = await poDetailDbSet.AsNoTracking()
                                           .Where(x => x.PoId == viewModel.po_id)
                                           .ToListAsync();

                var existingAsnDetails = await asnDbSet.AsNoTracking()
                                                .Include(x => x.Asnmaster)
                                                .Where(x => x.Asnmaster.po_id == viewModel.po_id
                                                         && x.asn_status != (byte)AsnStatusEnum.CANCELED)
                                                .ToListAsync();

                var inputGroups = viewModel.detailList
                           .GroupBy(x => x.sku_id)
                           .Select(g => new
                           {
                               SkuId = g.Key,
                               TotalInputQty = g.Sum(x => x.asn_qty),
                               SkuName = g.FirstOrDefault()?.sku_name
                           })
                           .ToList();

                foreach (var inputGroup in inputGroups)
                {

                    var poItem = poDetails.FirstOrDefault(x => x.SkuId == inputGroup.SkuId);

                    if (poItem == null)
                    {
                        return (0, $"Product {inputGroup.SkuName} (ID: {inputGroup.SkuId}) is not in PO .");
                    }


                    var historyReceivedQty = existingAsnDetails.Where(x => x.sku_id == inputGroup.SkuId).Sum(x => x.asn_qty);


                    var remainingQty = poItem.QtyOrdered - historyReceivedQty;


                    if (inputGroup.TotalInputQty > remainingQty)
                    {
                        return (0, "Quantity too much");
                    }
                }

                if (poEntity.PoStatus == PoStatusEnum.CREATED)
                {
                    poEntity.PoStatus = PoStatusEnum.IN_PROGRESS;
                    poEntity.LastUpdateTime = DateTime.UtcNow;
                }
            }

            var skuUomLinkDbSet = _dbContext.GetDbSet<SkuUomLinkEntity>();

            // Get all SKU IDs from detail list
            var skuIds = viewModel.detailList.Select(d => d.sku_id).Distinct().ToList();

            // Query UOMs through the N:N junction table (sku_uom_link)
            var allUoms = await skuUomLinkDbSet
                .Where(link => skuIds.Contains(link.SkuId))
                .Select(link => link.SkuUom!)
                .Distinct()
                .ToListAsync();

            var uomLookup = allUoms.ToDictionary(u => u.Id);

            string asn_no = await _functionHelper.GetFormNoAsync("Asnmaster", "ASN");
            var asnMasterEntity = new AsnmasterEntity
            {
                Id = 0,
                asn_no = asn_no,
                asn_batch = viewModel.asn_batch,
                estimated_arrival_time = viewModel.estimated_arrival_time,
                asn_status = (byte)AsnMasterStatusEnum.CREATED,
                weight = viewModel.weight,
                volume = viewModel.volume,
                goods_owner_id = viewModel.goods_owner_id,
                goods_owner_name = viewModel.goods_owner_name,
                warehouse_id = viewModel.warehouse_id,
                creator = currentUser.user_name,
                create_time = DateTime.UtcNow,
                last_update_time = DateTime.UtcNow,
                TenantId = currentUser.tenant_id,
                po_id = viewModel.po_id,
                detailList = [.. viewModel.detailList.Select(details =>
                {
                    return new AsnEntity
                    {
                        Id = 0,
                        asnmaster_id = 0,
                        asn_no = asn_no,
                        asn_status = (byte)AsnStatusEnum.PREPARE_PUTAWAY,
                        spu_id = details.spu_id,
                        sku_id = details.sku_id,
                        asn_qty = 0,
                        actual_qty = 0,
                        uom_id = details.uom_id,
                        batch_number = details.batch_number,
                        weight = details.weight,
                        volume = details.volume,
                        supplier_id = details.supplier_id,
                        supplier_name = details.supplier_name,
                        goods_owner_id = viewModel.goods_owner_id,
                        goods_owner_name = viewModel.goods_owner_name,
                        creator = currentUser.user_name,
                        create_time = DateTime.UtcNow,
                        last_update_time = DateTime.UtcNow,
                        is_valid = true,
                        TenantId = currentUser.tenant_id,
                        price = details.price,
                        description = details.description,
                        // new value
                        asn_qty_decimal = details.asn_qty_decimal,
                        actual_qty_decimal = details.actual_qty_decimal,
                        goods_location_id = details.goods_location_id,
                    };
                })]
            };

            await asnMasterDbSet.AddAsync(asnMasterEntity);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0 ? (asnMasterEntity.Id, _stringLocalizer["save_success"]) :
                (0, _stringLocalizer["save_failed"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception add AsnMaster Async with {msg}", ex.Message);
            return (0, _stringLocalizer["save_failed"]);
        }
    }

    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> UpdateAsnmasterAsync(AsnmasterBothViewModel viewModel, CurrentUser currentUser)
    {
        try
        {
            var asnDbSet = _dbContext.GetDbSet<AsnEntity>();
            var asnMasterDbSet = _dbContext.GetDbSet<AsnmasterEntity>();
            var asnMasterEntity = await asnMasterDbSet.Include(t => t.detailList)
                .FirstOrDefaultAsync(t => t.Id.Equals(viewModel.id));
            if (asnMasterEntity == null)
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }
            var goodLocationDbSet = await _dbContext.GetDbSet<GoodslocationEntity>(currentUser.tenant_id, true)
                                    .Where(t => asnMasterEntity.detailList.Select(d => d.goods_location_id).Contains(t.Id))
                                    .ToListAsync();


            asnMasterEntity.asn_batch = viewModel.asn_batch;
            asnMasterEntity.estimated_arrival_time = viewModel.estimated_arrival_time;
            asnMasterEntity.weight = viewModel.weight;
            asnMasterEntity.volume = viewModel.volume;
            asnMasterEntity.goods_owner_id = viewModel.goods_owner_id;
            asnMasterEntity.goods_owner_name = viewModel.goods_owner_name;
            asnMasterEntity.warehouse_id = viewModel.warehouse_id;
            asnMasterEntity.last_update_time = DateTime.UtcNow;
            asnMasterEntity.warehouse_id = viewModel.warehouse_id;
            if (viewModel.detailList.Any(t => t.id > 0))
            {
                asnMasterEntity.detailList.ForEach(asn =>
                {
                    var vm = viewModel.detailList.Where(t => t.id > 0)
                    .FirstOrDefault(t => t.id == asn.Id);
                    if (vm != null)
                    {
                        asn.spu_id = vm.spu_id;
                        asn.sku_id = vm.sku_id;
                        asn.asn_qty = vm.asn_qty;
                        asn.actual_qty = vm.actual_qty;
                        asn.asn_qty_decimal = vm.asn_qty_decimal;
                        asn.actual_qty_decimal = vm.actual_qty_decimal;
                        asn.weight = vm.weight;
                        asn.volume = vm.volume;
                        asn.supplier_id = vm.supplier_id;
                        asn.supplier_name = vm.supplier_name;
                        asn.goods_owner_id = viewModel.goods_owner_id;
                        asn.goods_owner_name = viewModel.goods_owner_name;
                        asn.uom_id = vm.uom_id;
                        asn.batch_number = vm.batch_number;
                        asn.description = vm.description;
                        asn.goods_location_id = vm.goods_location_id;
                        asn.expiry_date = vm.expiry_date;
                        asn.is_valid = vm.is_valid;
                        asn.last_update_time = DateTime.UtcNow;
                        asn.price = vm.price;
                        asn.goods_location_id = vm.goods_location_id;
                    }
                });
            }
            if (viewModel.detailList.Any(d => d.id == 0))
            {
                var adds = viewModel.detailList.Where(d => d.id == 0)
                    .Select(d => new AsnEntity
                    {
                        Id = 0,
                        asnmaster_id = asnMasterEntity.Id,
                        asn_no = viewModel.asn_no,
                        asn_status = 0,
                        spu_id = d.spu_id,
                        sku_id = d.sku_id,
                        asn_qty = d.asn_qty,
                        actual_qty = d.actual_qty,
                        asn_qty_decimal = d.asn_qty_decimal,
                        actual_qty_decimal = d.actual_qty_decimal,
                        weight = d.weight,
                        volume = d.volume,
                        supplier_id = d.supplier_id,
                        supplier_name = d.supplier_name,
                        goods_owner_id = viewModel.goods_owner_id,
                        goods_owner_name = viewModel.goods_owner_name,
                        uom_id = d.uom_id,
                        batch_number = d.batch_number,
                        description = d.description,
                        goods_location_id = d.goods_location_id,
                        expiry_date = d.expiry_date,
                        creator = currentUser.user_name,
                        create_time = DateTime.UtcNow,
                        last_update_time = DateTime.UtcNow,
                        is_valid = d.is_valid,
                        TenantId = currentUser.tenant_id,
                        price = d.price
                    }).ToList();
                asnMasterEntity.detailList.AddRange(adds);
            }
            if (viewModel.detailList.Any(t => t.id < 0))
            {
                var delIds = viewModel.detailList.Where(t => t.id < 0).Select(t => t.id * -1).ToList();
                asnMasterEntity.detailList.RemoveAll(entity => delIds.Contains(entity.Id));
            }
            await _dbContext.SaveChangesAsync();

            return (true, _stringLocalizer["save_success"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in Update Asn master Async with {error}", ex.Message);
            return (false, _stringLocalizer["save_failed"]);
        }
    }

    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> DeleteAsnmasterAsync(int id)
    {
        var asnMaster = await _dbContext.GetDbSet<AsnmasterEntity>()
                                  .FirstOrDefaultAsync(t => t.Id == id);

        if (asnMaster == null)
        {
            return (false, _stringLocalizer["not_found"]);
        }

        if (asnMaster.asn_status != (byte)AsnMasterStatusEnum.CREATED)
        {
            return (false, _stringLocalizer["delete_failed_status_invalid"]);
        }

        // Transaction begin
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var countDetail = await _dbContext.GetDbSet<AsnEntity>()
          .Where(t => t.asnmaster_id == id)
          .ExecuteUpdateAsync(s => s.SetProperty(p => p.asn_status, (byte)AsnStatusEnum.CANCELED));

            var countMaster = await _dbContext.GetDbSet<AsnmasterEntity>()
                .Where(t => t.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.asn_status, (byte)AsnMasterStatusEnum.CANCELED));

            if (asnMaster.po_id != null)
            {
                var poEntity = await _dbContext.GetDbSet<PurchaseOrderEntity>()
                    .FirstOrDefaultAsync(t => t.Id == asnMaster.po_id)
                    ?? throw new Exception("Can't not get the value from the PurchaseOrder");

                poEntity.PoStatus = PoStatusEnum.CREATED;
                poEntity.LastUpdateTime = DateTime.UtcNow;
            }

            // rollback location for available
            // Query location ids from ASN details (do not rely on asnMaster.detailList navigation)
            var locationIds = await _dbContext.GetDbSet<AsnEntity>()
                .Where(a => a.asnmaster_id == id && a.goods_location_id > 0)
                .Select(a => a.goods_location_id)
                .Distinct()
                .ToListAsync();

            var goodLocationDbSet = await _dbContext.GetDbSet<GoodslocationEntity>(asnMaster.TenantId, true)
                .Where(g => locationIds.Contains(g.Id))
                .ToListAsync();

            if (goodLocationDbSet.Count <= 0)
            {
                _logger.LogInformation("Good location not found for asn master {id}", id);
                return (false, _stringLocalizer["good_location_not_found"]);
            }
            foreach (var item in goodLocationDbSet)
            {
                item.LocationStatus = (int)GoodLocationStatusEnum.AVAILABLE;
                item.LastUpdateTime = DateTime.UtcNow;
                item.IsValid = true;
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Delete Asn master Async success, countDetail {countDetail}, countMaster {countMaster}", countDetail, countMaster);
            return (true, _stringLocalizer["delete_success"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in Delete Asn master Async with {msg}", ex.Message);
            await transaction.RollbackAsync();
            return (false, _stringLocalizer["delete_failed"]);
        }
    }

    #endregion Arrival list

    #region print series number
    /// <summary>
    /// print series number
    /// </summary>
    /// <param name="input">selected asn id</param>
    /// <returns></returns>
    public async Task<List<AsnPrintSeriesNumberViewModel>> GetAsnPrintSeriesNumberAsync(List<int> input)
    {
        var Spus = _dbContext.GetDbSet<SpuEntity>().AsNoTracking();
        var Skus = _dbContext.GetDbSet<SkuEntity>().AsNoTracking();
        var Asns = _dbContext.GetDbSet<AsnEntity>().AsNoTracking();
        var Asnmasters = _dbContext.GetDbSet<AsnmasterEntity>().AsNoTracking();
        var sorts = _dbContext.GetDbSet<AsnsortEntity>().AsNoTracking();
        var stocks = _dbContext.GetDbSet<StockEntity>().AsNoTracking();
        var goodLocations = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();

        var query = from m in Asnmasters
                    join a in Asns on m.Id equals a.asnmaster_id
                    join p in Spus.AsNoTracking() on a.spu_id equals p.Id
                    join k in Skus.AsNoTracking() on a.sku_id equals k.Id
                    join s in sorts on a.Id equals s.asn_id
                    where input.Contains(a.Id)
                    select new AsnPrintSeriesNumberViewModel
                    {
                        asn_id = a.Id,
                        asnmaster_id = m.Id,
                        asn_no = m.asn_no,
                        sku_id = a.sku_id,
                        sku_code = k.sku_code,
                        sku_name = k.sku_name,
                        spu_code = p.spu_code,
                        spu_name = p.spu_name,
                        series_number = s.series_number,
                        LocationName = (from st in stocks
                                        join g in goodLocations on st.goods_location_id equals g.Id
                                        where st.sku_id == a.sku_id
                                        select g.LocationName).FirstOrDefault() ?? ""
                    };
        var data = await query.OrderBy(t => t.asn_id).ToListAsync();
        data ??= new List<AsnPrintSeriesNumberViewModel>();
        return data;
    }

    #endregion

    #region Flow count status
    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<Dictionary<int, int>> GetAsnStatusTotalRecordAsync(CurrentUser currentUser)
    {
        var query = _dbContext.GetDbSet<AsnEntity>()
                        .AsNoTracking()
                        .Where(m => m.TenantId == currentUser.tenant_id);

        var groupedResult = await query
            .GroupBy(x => x.asn_status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        var result = new Dictionary<int, int>
                {
                    {0 , 0 },
                    {1 , 0 },
                    {2 , 0 },
                    {3 , 0 }
                };

        foreach (var item in groupedResult)
        {
            int statusKey = (int)item.Status;
            if (statusKey == 3 || statusKey == 4)
            {
                if (result.ContainsKey(3))
                {
                    result[3] += item.Count;
                }
            }

            else if (result.ContainsKey(statusKey))
            {
                result[statusKey] = item.Count;
            }
        }

        return result;

    }

    /// <summary>
    /// Confirm Robot has Success To putaway
    /// the status has changes from 4 to 5
    /// This method is temporary for bypass the robot status 
    /// this method will be update soon in Intergration with WCS phase 
    /// </summary>
    /// <param name="asnIdList"></param>
    /// <param name="tenantId">Current user</param>
    /// <returns></returns>
    public async Task<bool> ConfirmRobotHasSuccessAsync(List<int> asnIdList, long tenantId = 1)
    {
        if (asnIdList == null || asnIdList.Count == 0)
        {
            _logger.LogError("Invalidate with asnIdList null");
            return (false);
        }

        try
        {
            var asnDbSet = _dbContext.GetDbSet<AsnEntity>(tenantId, true);
            var asnMasterDbSet = _dbContext.GetDbSet<AsnmasterEntity>(tenantId, true);

            var asnToUpdate = await asnDbSet
             .Where(t => asnIdList.Contains(t.Id) && t.asn_status == (byte)AsnStatusEnum.WAITING_ROBOT)
             .ToListAsync();

            if (asnToUpdate == null || asnToUpdate.Count == 0)
            {
                _logger.LogError("No asn need to update");
                return false;
            }
            foreach (var asn in asnToUpdate)
            {
                asn.asn_status = (byte)AsnStatusEnum.COMPLETE;
                asn.last_update_time = DateTime.UtcNow;
            }

            var affectedMasterIds = new List<int>();

            var asnMasterIds = asnToUpdate.Select(a => a.asnmaster_id).Distinct().ToList();
            foreach (var asn in asnToUpdate)
            {
                asn.asn_status = (byte)AsnStatusEnum.COMPLETE;
                asn.last_update_time = DateTime.UtcNow;

                if (asn.asnmaster_id > 0)
                {
                    affectedMasterIds.Add(asn.asnmaster_id);
                }
            }
            if (affectedMasterIds.Count != 0)
            {
                var incompleteMasterIds = await asnDbSet
                    .Where(t => affectedMasterIds.Contains(t.asnmaster_id)
                                && !asnIdList.Contains(t.Id)
                                && t.asn_status != (byte)AsnStatusEnum.COMPLETE)
                    .Select(t => t.asnmaster_id)
                    .Distinct()
                    .ToListAsync();

                var completedMasterIds = affectedMasterIds.Except(incompleteMasterIds).ToList();

                if (completedMasterIds.Any())
                {
                    var mastersToUpdate = await asnMasterDbSet
                        .Where(m => completedMasterIds.Contains(m.Id) && m.asn_status != (byte)AsnMasterStatusEnum.COMPLETED)
                        .ToListAsync();

                    foreach (var master in mastersToUpdate)
                    {
                        master.asn_status = (byte)AsnMasterStatusEnum.COMPLETED;
                        master.last_update_time = DateTime.UtcNow;
                    }
                }
            }
            var result = await _dbContext.SaveChangesAsync();
            _logger.LogError("Save :" + result);
            if (result > 0)
            {
                _logger.LogInformation("Successfully confirmed robot success for ASN IDs: {asnIdList}", asnIdList);
                return true;
            }
            else
            {
                _logger.LogError("Failed to confirm robot success for ASN IDs: {asnIdList}", asnIdList);
                return (false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while confirming robot success");
            return (false);
        }
    }


    #region Duy phat solution 

    /// <summary>
    /// Created AsnNo created bt asn no generator
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetNextAsnNo()
    {
        return await _functionHelper.GetFormNoAsync("Asnmaster", "ASN");
    }

    // extension method to build entities only (no persistence)
    private async Task<AsnmasterEntity> CreateAsnCoreAsync(
        AsnmasterBothViewModel viewModel,
        CurrentUser currentUser,
        AsnMasterStatusEnum targetStatus)
    {

        if (viewModel == null || viewModel.detailList == null || viewModel.detailList.Count == 0)
        {
            throw new ArgumentException(_stringLocalizer["save_failed_empty_data"]);
        }

        var poDbSet = _dbContext.GetDbSet<PurchaseOrderEntity>();
        var poDetailDbSet = _dbContext.GetDbSet<PurchaseOrderDetailsEntity>();
        var asnDbSet = _dbContext.GetDbSet<AsnEntity>();
        var skuUomDbSet = _dbContext.GetDbSet<SkuUomEntity>();


        if (viewModel.po_id != null && viewModel.po_id > 0)
        {
            var poEntity = await poDbSet.FirstOrDefaultAsync(t => t.Id == viewModel.po_id)
                           ?? throw new Exception("Cannot get PurchaseOrder");

            if (poEntity.PoStatus == PoStatusEnum.COMPLETED) throw new Exception("PO has been Completed");
            if (poEntity.PoStatus == PoStatusEnum.CANCELED) throw new Exception("PO has been Canceled");


            var poDetails = await poDetailDbSet.AsNoTracking().Where(x => x.PoId == viewModel.po_id).ToListAsync();
            var existingAsnDetails = await asnDbSet.AsNoTracking()
                .Include(x => x.Asnmaster)
                .Where(x => x.Asnmaster.po_id == viewModel.po_id && x.asn_status != (byte)AsnStatusEnum.CANCELED)
                .ToListAsync();

            var inputGroups = viewModel.detailList.GroupBy(x => x.sku_id)
                .Select(g => new { SkuId = g.Key, TotalInputQty = g.Sum(x => x.asn_qty_decimal), SkuName = g.FirstOrDefault()?.sku_name })
                .ToList();

            foreach (var inputGroup in inputGroups)
            {
                var poItem = poDetails.FirstOrDefault(x => x.SkuId == inputGroup.SkuId);
                if (poItem == null) throw new Exception($"Product {inputGroup.SkuName} is not in PO.");

                var historyReceivedQty = existingAsnDetails.Where(x => x.sku_id == inputGroup.SkuId).Sum(x => x.asn_qty_decimal); // Note: Should match decimal
                var remainingQty = poItem.QtyOrdered - historyReceivedQty;

                if (inputGroup.TotalInputQty > remainingQty) throw new Exception("Quantity too much");
            }


            if (poEntity.PoStatus == PoStatusEnum.CREATED)
            {
                poEntity.PoStatus = PoStatusEnum.IN_PROGRESS;
                poEntity.LastUpdateTime = DateTime.UtcNow;
            }
        }


        var skuUomLinkDbSet = _dbContext.GetDbSet<SkuUomLinkEntity>();

        // Get all SKU IDs from detail list
        var skuIds = viewModel.detailList.Select(d => d.sku_id).Distinct().ToList();

        // Query UOMs through the N:N junction table (sku_uom_link)
        var allUoms = await skuUomLinkDbSet
            .Where(link => skuIds.Contains(link.SkuId))
            .Select(link => link.SkuUom!)
            .Distinct()
            .ToListAsync();

        var uomLookup = allUoms.ToDictionary(u => u.Id);


        if (viewModel.asn_no == null)
        {
            viewModel.asn_no = await _functionHelper.GetFormNoAsync("Asnmaster", "ASN");
        }

        var asnMasterEntity = new AsnmasterEntity
        {
            asn_no = viewModel.asn_no,
            asn_status = (byte)targetStatus,
            asn_batch = viewModel.asn_batch,
            estimated_arrival_time = viewModel.estimated_arrival_time,
            goods_owner_id = viewModel.goods_owner_id,
            goods_owner_name = viewModel.goods_owner_name,
            warehouse_id = viewModel.warehouse_id,
            creator = currentUser.user_name,
            create_time = DateTime.UtcNow,
            last_update_time = DateTime.UtcNow,
            TenantId = currentUser.tenant_id,
            po_id = viewModel.po_id,
            detailList = [.. viewModel.detailList.Select(details =>
        {
            // dont need to change the skuoum
            return new AsnEntity
            {
                asn_no = viewModel.asn_no,
                asn_status = (byte)AsnStatusEnum.PREPARE_PUTAWAY,
                spu_id = details.spu_id,
                sku_id = details.sku_id,
                uom_id = details.uom_id,
                asn_qty_decimal = details.asn_qty_decimal,
                actual_qty_decimal = details.actual_qty_decimal,
                goods_location_id = details.goods_location_id,

                is_valid = true,
                create_time = DateTime.UtcNow,
                last_update_time = DateTime.UtcNow,
                creator = currentUser.user_name,
                TenantId = currentUser.tenant_id
            };
        })]
        };
        return asnMasterEntity;
    }

    /// <summary>
    /// Save draft order
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<(int id, string msg)> SaveDraftAsync(AsnmasterBothViewModel viewModel, CurrentUser currentUser)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var asnSortDbSet = _dbContext.GetDbSet<AsnsortEntity>();


            // Validate: draft requires location for each detail
            if (viewModel?.detailList == null || viewModel.detailList.Count == 0)
            {
                throw new ArgumentException(_stringLocalizer["save_failed_empty_data"]);
            }
            if (viewModel.detailList.Any(d => d.goods_location_id <= 0))
            {
                throw new ArgumentException("Goods location is required for each detail item.");
            }

            // Build master + details (details are created inside CreateAsnCoreAsync)
            var masterEntity = await CreateAsnCoreAsync(viewModel, currentUser, AsnMasterStatusEnum.CREATED);

            // Sync location into ASN details and ensure draft actual qty is 0
            if (masterEntity.detailList == null || masterEntity.detailList.Count != viewModel.detailList.Count)
            {
                throw new Exception("Invalid ASN detail mapping.");
            }

            // Persist master + all ASN details first to get ASN detail Ids (FK target for asnsort.asn_id)
            await _dbContext.GetDbSet<AsnmasterEntity>().AddAsync(masterEntity);
            await _dbContext.SaveChangesAsync();

            // Create asnsort records for each detail, using ASN detail Id
            var asnSortEntities = new List<AsnsortEntity>(viewModel.detailList.Count);
            for (int i = 0; i < viewModel.detailList.Count; i++)
            {
                var itemVM = viewModel.detailList[i];
                var asnDetail = masterEntity.detailList[i];

                asnSortEntities.Add(new AsnsortEntity
                {
                    asn_id = asnDetail.Id,
                    series_number = "",
                    pallet_id = itemVM.pallet_id,
                    good_location_id = itemVM.goods_location_id,
                    sorted_qty = 0,
                    putaway_qty = 0,
                    creator = currentUser.user_name,
                    create_time = DateTime.UtcNow,
                    last_update_time = DateTime.UtcNow,
                    TenantId = currentUser.tenant_id,
                    is_valid = true,
                });
            }

            await asnSortDbSet.AddRangeAsync(asnSortEntities);

            // block the locations used in this draft
            var goodLocationDbSet = _dbContext.GetDbSet<GoodslocationEntity>(currentUser.tenant_id, true)
                                    .Where(t => viewModel.detailList.Select(g => g.goods_location_id)
                                    .Contains(t.Id));
            await goodLocationDbSet.ExecuteUpdateAsync(s => s.SetProperty(p => p.LocationStatus, (byte)GoodLocationStatusEnum.OCCUPIED));

            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return (masterEntity.Id, _stringLocalizer["save_draft_success"]);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving draft");
            return (0, _stringLocalizer["save_failed"]);
        }
    }


    /// <summary>
    /// Submit order and create new receipt for handle 
    ///
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<(int id, string msg)> SubmitOrderAsync(AsnmasterBothViewModel viewModel, CurrentUser currentUser)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // 0) Validate
            if (viewModel?.detailList == null || viewModel.detailList.Count == 0)
            {
                _logger.LogError("Detail list is required.");
                return (0, "Detail list is required.");
            }
            if (viewModel.detailList.Any(d => d.goods_location_id <= 0))
            {
                _logger.LogError("Goods location is required for each detail item.");
                return (0, "Goods location is required for each detail item.");
            }
            if (viewModel.detailList.Any(d => d.pallet_id <= 0))
            {
                _logger.LogError("Pallet is required for each detail item.");
                return (0, "Pallet is required for each detail item.");
            }

            var now = DateTime.UtcNow;
            var asnSortDbSet = _dbContext.GetDbSet<AsnsortEntity>();
            var inboundDbSet = _dbContext.GetDbSet<InboundEntity>();
            var historyDbSet = _dbContext.GetDbSet<IntegrationHistory>();


            var masterEntity = await CreateAsnCoreAsync(viewModel, currentUser, AsnMasterStatusEnum.IN_PROGRESS);
            await _dbContext.GetDbSet<AsnmasterEntity>().AddAsync(masterEntity);
            await _dbContext.SaveChangesAsync(); // get ids for ASN details

            if (masterEntity.detailList == null || masterEntity.detailList.Count != viewModel.detailList.Count)
            {
                _logger.LogError("Invalid ASN detail mapping.");
                return (0, "Invalid ASN detail mapping.");
            }

            var asnSortEntities = new List<AsnsortEntity>(viewModel.detailList.Count);
            for (int i = 0; i < viewModel.detailList.Count; i++)
            {
                var itemVM = viewModel.detailList[i];
                var asnDetail = masterEntity.detailList[i];

                asnSortEntities.Add(new AsnsortEntity
                {
                    asn_id = asnDetail.Id,
                    series_number = "",
                    pallet_id = itemVM.pallet_id,
                    good_location_id = itemVM.goods_location_id,
                    sorted_qty = 0,
                    putaway_qty = 0,
                    creator = currentUser.user_name,
                    create_time = now,
                    last_update_time = now,
                    TenantId = currentUser.tenant_id,
                    is_valid = true,
                    status = AsnSortStatusEnum.Moving
                });
            }

            await asnSortDbSet.AddRangeAsync(asnSortEntities);

            // Block the locations used in this submit
            var goodLocationDbSet = _dbContext.GetDbSet<GoodslocationEntity>(currentUser.tenant_id, true)
                .Where(t => viewModel.detailList.Select(g => g.goods_location_id).Contains(t.Id));
            await goodLocationDbSet.ExecuteUpdateAsync(s =>
                s.SetProperty(p => p.LocationStatus, (byte)GoodLocationStatusEnum.OCCUPIED));

            var palletIds = asnSortEntities
                .Select(x => x.pallet_id)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            var palletCodeById = await _dbContext.GetDbSet<PalletEntity>(currentUser.tenant_id)
                .AsNoTracking()
                .Where(p => palletIds.Contains(p.Id))
                .Select(p => new { p.Id, p.PalletCode })
                .ToDictionaryAsync(x => x.Id, x => x.PalletCode);

            var inboundList = new List<InboundEntity>(asnSortEntities.Count);
            var historyList = new List<IntegrationHistory>(asnSortEntities.Count);
            var taskCodeByKey = new Dictionary<(string PalletCode, int LocationId), string>();

            foreach (var item in asnSortEntities)
            {
                if (!(item.pallet_id > 0) ||
                    !palletCodeById.TryGetValue(item.pallet_id, out var palletCode) ||
                    string.IsNullOrWhiteSpace(palletCode))
                {
                    _logger.LogError("Pallet code not found for pallet_id={PalletId}", item.pallet_id);
                    return (0, $"Pallet code not found for pallet_id={item.pallet_id}.");
                }

                if (!(item.good_location_id > 0))
                {
                    _logger.LogError("Invalid target location for pallet={PalletCode} (asn_id={AsnId})", palletCode, item.asn_id);
                    return (0, $"Invalid target location for pallet={palletCode} (asn_id={item.asn_id}).");
                }

                var locationId = item.good_location_id;
                var key = (PalletCode: palletCode, LocationId: locationId);
                if (!taskCodeByKey.TryGetValue(key, out var taskCode))
                {
                    taskCode = GenarationHelper.GenerateTaskCode();
                    taskCodeByKey[key] = taskCode;
                }

                inboundList.Add(new InboundEntity
                {
                    TenantId = currentUser.tenant_id,
                    TaskCode = taskCode,
                    PalletCode = palletCode,
                    PickUpDate = now,
                    LocationId = locationId,
                    Priority = 1,
                    Status = IntegrationStatus.Processing,
                    IsActive = true,
                    CreatedDate = now,
                    FinishedDate = null
                });

                historyList.Add(new IntegrationHistory
                {
                    TenantId = currentUser.tenant_id,
                    TaskCode = taskCode,
                    PalletCode = palletCode,
                    PickUpDate = now,
                    LocationId = locationId,
                    Priority = 1,
                    Status = IntegrationStatus.Processing,
                    IsActive = true,
                    CreatedDate = now,
                    FinishedDate = null,
                    HistoryType = HistoryType.Inbound
                });
            }

            await inboundDbSet.AddRangeAsync(inboundList);
            await historyDbSet.AddRangeAsync(historyList);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return (masterEntity.Id, _stringLocalizer["submit_success"]);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error submitting order");
            return (0, ex.Message);
        }
    }

    /// <summary>
    /// using for update order to waiting and create inbound task for WCS
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<(bool, string)> UpdateOrderToCompletedAsync(int id, CurrentUser currentUser)
    {

        var inboundWCSEntity = _dbContext.GetDbSet<InboundEntity>();
        var inboundHistoryWCSEntity = _dbContext.GetDbSet<IntegrationHistory>();

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var asnMasterEntity = await _dbContext.GetDbSet<AsnmasterEntity>(currentUser.tenant_id, true)
                                                  .FirstOrDefaultAsync(x => x.Id == id);
            if (asnMasterEntity == null)
            {
                _logger.LogError("Order not found at {id}", id);
                return (false, "Order not found");
            }

            if (asnMasterEntity.asn_status != (byte)AsnMasterStatusEnum.CREATED)
            {
                return (true, "Order status can't be updated");
            }

            asnMasterEntity.asn_status = (byte)AsnMasterStatusEnum.IN_PROGRESS;
            asnMasterEntity.last_update_time = DateTime.UtcNow;

            var asnEntity = await _dbContext.GetDbSet<AsnEntity>(currentUser.tenant_id)
                .Where(t => t.asnmaster_id == id)
                .ToListAsync();

            var asnIds = asnEntity.Select(x => x.Id).ToList();
            var asnSortEntity = await _dbContext.GetDbSet<AsnsortEntity>(currentUser.tenant_id)
                .Where(t => asnIds.Contains(t.asn_id))
                .ToListAsync();

            if (asnEntity.Count == 0 || asnSortEntity.Count == 0)
            {
                _logger.LogError("Related ASN or ASN Sort not found at {id}", id);
                return (false, "Related ASN or ASN Sort not found");
            }

            var palletIds = asnSortEntity
                .Select(x => x.pallet_id)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            var palletCodeById = await _dbContext.GetDbSet<PalletEntity>(currentUser.tenant_id)
                                                .AsNoTracking()
                                                .Where(p => palletIds.Contains(p.Id))
                                                .Select(p => new { p.Id, p.PalletCode })
                                                .ToDictionaryAsync(x => x.Id, x => x.PalletCode);

            var now = DateTime.UtcNow;
            var inboundList = new List<InboundEntity>();
            var historyList = new List<IntegrationHistory>();
            var taskCodeByKey = new Dictionary<(string PalletCode, int LocationId), string>();

            foreach (var item in asnSortEntity)
            {
                if (!(item.pallet_id > 0) || !palletCodeById.TryGetValue(item.pallet_id, out var palletCode) || string.IsNullOrWhiteSpace(palletCode))
                {
                    _logger.LogError("Missing pallet code for pallet_id {PalletId} (asn_id {AsnId})", item.pallet_id, item.asn_id);
                    return (false, $"Pallet code not found for pallet_id={item.pallet_id}.");
                }

                if (!(item.good_location_id > 0))
                {
                    _logger.LogError("Missing target location for pallet {PalletCode} (asn_id {AsnId})", palletCode, item.asn_id);
                    return (false, $"Pallet {palletCode} has no valid target location.");
                }

                var locationId = item.good_location_id;
                var key = (PalletCode: palletCode, LocationId: locationId);
                if (!taskCodeByKey.TryGetValue(key, out var taskCode))
                {
                    taskCode = GenarationHelper.GenerateTaskCode();
                    taskCodeByKey[key] = taskCode;
                }

                inboundList.Add(new InboundEntity
                {
                    TenantId = currentUser.tenant_id,
                    TaskCode = taskCode,
                    PalletCode = palletCode,
                    PickUpDate = now,
                    LocationId = locationId,
                    Priority = 1,
                    Status = IntegrationStatus.Processing,
                    IsActive = true,
                    CreatedDate = now,
                    FinishedDate = null
                });

                historyList.Add(new IntegrationHistory
                {
                    TenantId = currentUser.tenant_id,
                    TaskCode = taskCode,
                    PalletCode = palletCode,
                    PickUpDate = now,
                    LocationId = locationId,
                    Priority = 1,
                    Status = IntegrationStatus.Processing,
                    IsActive = true,
                    CreatedDate = now,
                    FinishedDate = null,
                    HistoryType = HistoryType.Inbound
                });

                item.status = AsnSortStatusEnum.Moving;
            }

            if (inboundList.Count == 0)
            {
                return (false, "No valid pallet data to create WCS tasks.");
            }

            _dbContext.GetDbSet<AsnsortEntity>().UpdateRange(asnSortEntity);
            await inboundWCSEntity.AddRangeAsync(inboundList);
            await inboundHistoryWCSEntity.AddRangeAsync(historyList);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Order marked completed and WCS tasks created successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating order to completed");
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Retry Inbound with new location
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<(bool, string)> SubmitInboundRetryAsync(RetryInboundItemRequest request, CurrentUser currentUser)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {

            var asnItem = await _dbContext.GetDbSet<AsnEntity>(currentUser.tenant_id)
                .FirstOrDefaultAsync(x => x.Id == request.AsnId);

            if (asnItem == null)
            {
                _logger.LogError("Asn Item not found for AsnId {AsnId}", request.AsnId);
                return (false, "Asn Item is not found.");
            }

            var newLocation = await _dbContext.GetDbSet<GoodslocationEntity>()
                .FirstOrDefaultAsync(x => x.Id == request.NewLocationId);

            if (newLocation?.LocationStatus != (byte)GoodLocationStatusEnum.AVAILABLE)
            {
                _logger.LogError("New location is not valid or occupied for LocationId {LocationId}", request.NewLocationId);
                return (false, "New location is not valid or occupied.");
            }

            var targetSortItem = await _dbContext.GetDbSet<AsnsortEntity>(currentUser.tenant_id)
              .Where(x => x.asn_id == request.AsnId
                       && (x.status == AsnSortStatusEnum.Pending)
                       && x.is_valid)
              .FirstOrDefaultAsync();

            if (targetSortItem == null)
            {
                _logger.LogError("Asn sort is not found with id {id}", request.AsnId);
                return (false, "Asn sort is not found.");
            }

            var itemsToUpdate = new List<AsnsortEntity>();

            if (targetSortItem.pallet_id > 0)
            {
                itemsToUpdate = await _dbContext.GetDbSet<AsnsortEntity>(currentUser.tenant_id)
                    .Where(x => x.pallet_id == targetSortItem.pallet_id
                             && x.is_valid
                             && (x.status == AsnSortStatusEnum.Pending))
                    .ToListAsync();
            }
            else
            {
                itemsToUpdate.Add(targetSortItem);
            }

            foreach (var item in itemsToUpdate)
            {
                item.good_location_id = newLocation.Id;
                item.status = AsnSortStatusEnum.Moving;
                item.last_update_time = DateTime.UtcNow;
            }

            _dbContext.GetDbSet<AsnsortEntity>().UpdateRange(itemsToUpdate);

            newLocation.LocationStatus = (byte)GoodLocationStatusEnum.OCCUPIED;
            _dbContext.GetDbSet<GoodslocationEntity>().Update(newLocation);


            var newTaskCode = GenarationHelper.GenerateTaskCode();

            var inboundTask = new InboundEntity
            {
                TenantId = currentUser.tenant_id,
                TaskCode = newTaskCode,
                PalletCode = request.PalletCode,
                LocationId = request.NewLocationId,
                Status = IntegrationStatus.Processing,
                Priority = 1,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _dbContext.Inbounds.AddAsync(inboundTask);

            var history = new IntegrationHistory
            {
                HistoryType = HistoryType.Inbound,
                TaskCode = newTaskCode,
                PalletCode = request.PalletCode,
                Status = IntegrationStatus.Processing,
                Description = $"User {currentUser.user_name} retried with new location: {newLocation.LocationName}",
                CreatedDate = DateTime.UtcNow,
                TenantId = currentUser.tenant_id
            };
            await _dbContext.IntegrationHistories.AddAsync(history);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Retry successful. WCS Task created.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "SubmitInboundRetryAsync Failed");
            return (false, "System Error: " + ex.Message);
        }
    }

    #endregion

    #endregion

}