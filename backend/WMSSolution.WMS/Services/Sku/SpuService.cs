
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.Models.Warehouse;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.IServices;

namespace WMSSolution.WMS.Services
{
    /// <summary>
    ///  Spu Service
    /// </summary>
    public class SpuService : BaseService<SpuEntity>, ISpuService
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
        #endregion

        #region constructor
        /// <summary>
        ///Spu  constructor
        /// </summary>
        /// <param name="dBContext">The DBContext</param>
        /// <param name="stringLocalizer">Localizer</param>
        public SpuService(
            SqlDBContext dBContext
          , IStringLocalizer<MultiLanguage> stringLocalizer
            )
        {
            this._dBContext = dBContext;
            this._stringLocalizer = stringLocalizer;
        }
        #endregion

        #region Api
        /// <summary>
        /// page search
        /// </summary>
        /// <param name="pageSearch">args</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        public async Task<(List<SpuBothViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser)
        {
            QueryCollection queries = new QueryCollection();
            if (pageSearch.searchObjects.Any())
            {
                pageSearch.searchObjects.ForEach(s =>
                {
                    queries.Add(s);
                });
            }
            var Categorys = _dBContext.GetDbSet<CategoryEntity>();
            var Spus = _dBContext.GetDbSet<SpuEntity>();
            var Skus = _dBContext.GetDbSet<SkuEntity>();
            var SkuSafetyStocks = _dBContext.GetDbSet<SkuSafetyStockEntity>();
            var Warehouses = _dBContext.GetDbSet<WarehouseEntity>();
            var query = from m in Spus.AsNoTracking()
                        join c in Categorys.AsNoTracking() on m.category_id equals c.Id
                        where m.TenantId == currentUser.tenant_id
                        select new SpuBothViewModel
                        {
                            id = m.Id,
                            spu_code = m.spu_code,
                            spu_name = m.spu_name,
                            category_id = m.category_id.GetValueOrDefault(),
                            category_name = c.category_name,
                            spu_description = m.spu_description,
                            supplier_id = m.supplier_id.GetValueOrDefault(),
                            supplier_name = m.supplier_name,
                            brand = m.brand,
                            origin = m.origin,
                            length_unit = m.length_unit.GetValueOrDefault(),
                            volume_unit = m.volume_unit.GetValueOrDefault(),
                            weight_unit = m.weight_unit.GetValueOrDefault(),
                            creator = m.creator,
                            create_time = m.create_time,
                            last_update_time = m.last_update_time,
                            is_valid = m.is_valid,
                            detailList = Skus.AsNoTracking().Where(t => t.spu_id.Equals(m.Id))
                                         .Select(t => new SkuViewModel
                                         {
                                             id = t.Id,
                                             spu_id = t.spu_id.GetValueOrDefault(),
                                             sku_code = t.sku_code,
                                             sku_name = t.sku_name,
                                             bar_code = t.bar_code ?? "",
                                             weight = t.weight.GetValueOrDefault(),
                                             lenght = t.lenght.GetValueOrDefault(),
                                             width = t.width.GetValueOrDefault(),
                                             height = t.height.GetValueOrDefault(),
                                             volume = t.volume.GetValueOrDefault(),
                                             unit = t.unit ?? "",
                                             cost = t.cost.GetValueOrDefault(),
                                             price = t.price.GetValueOrDefault(),
                                             create_time = t.create_time,
                                             last_update_time = t.last_update_time,
                                             detailList = (from sss in SkuSafetyStocks.AsNoTracking()
                                                           join wh in Warehouses on sss.WarehouseId equals wh.Id
                                                           where sss.sku_id.Equals(t.Id)
                                                           select new SkuSafetyStockViewModel
                                                           {
                                                               id = sss.Id,
                                                               sku_id = sss.sku_id,
                                                               safety_stock_qty = sss.safety_stock_qty,
                                                               WarehouseId = sss.WarehouseId,
                                                               WarehouseName = wh.WarehouseName
                                                           }).ToList()
                                         }).ToList()

                        };
            query = query.Where(queries.AsExpression<SpuBothViewModel>());
            int totals = await query.CountAsync();
            var list = await query.OrderByDescending(t => t.create_time)
                       .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                       .Take(pageSearch.pageSize)
                       .ToListAsync();
            return (list, totals);
        }

        /// <summary>
        /// Get a record by id
        /// </summary>
        /// <returns></returns>
        public async Task<SpuBothViewModel> GetAsync(int id)
        {
            var Categorys = _dBContext.GetDbSet<CategoryEntity>();
            var Spus = _dBContext.GetDbSet<SpuEntity>();
            var Skus = _dBContext.GetDbSet<SkuEntity>();
            var SkuSafetyStocks = _dBContext.GetDbSet<SkuSafetyStockEntity>();
            var Warehouses = _dBContext.GetDbSet<WarehouseEntity>();
            var query = from m in Spus.AsNoTracking()
                        join c in Categorys.AsNoTracking() on m.category_id equals c.Id
                        where m.Id == id
                        select new SpuBothViewModel
                        {
                            id = m.Id,
                            spu_code = m.spu_code,
                            spu_name = m.spu_name,
                            category_id = m.category_id.GetValueOrDefault(),
                            category_name = c.category_name,
                            spu_description = m.spu_description,
                            supplier_id = m.supplier_id.GetValueOrDefault(),
                            supplier_name = m.supplier_name,
                            brand = m.brand,
                            origin = m.origin,
                            length_unit = m.length_unit.GetValueOrDefault(),
                            volume_unit = m.volume_unit.GetValueOrDefault(),
                            weight_unit = m.weight_unit.GetValueOrDefault(),
                            creator = m.creator,
                            create_time = m.create_time,
                            last_update_time = m.last_update_time,
                            is_valid = m.is_valid,
                            detailList = Skus.Where(t => t.spu_id.Equals(m.Id))
                                         .Select(t => new SkuViewModel
                                         {
                                             id = t.Id,
                                             spu_id = t.spu_id.GetValueOrDefault(),
                                             sku_code = t.sku_code,
                                             sku_name = t.sku_name,
                                             bar_code = t.bar_code ?? "",
                                             weight = t.weight.GetValueOrDefault(),
                                             lenght = t.lenght.GetValueOrDefault(),
                                             width = t.width.GetValueOrDefault(),
                                             height = t.height.GetValueOrDefault(),
                                             volume = t.volume.GetValueOrDefault(),
                                             unit = t.unit ?? "",
                                             cost = t.cost.GetValueOrDefault(),
                                             price = t.price.GetValueOrDefault(),
                                             create_time = t.create_time,
                                             last_update_time = t.last_update_time,
                                             detailList = (from sss in SkuSafetyStocks.AsNoTracking()
                                                           join wh in Warehouses on sss.WarehouseId equals wh.Id
                                                           where sss.sku_id.Equals(t.Id)
                                                           select new SkuSafetyStockViewModel
                                                           {
                                                               id = sss.Id,
                                                               sku_id = sss.sku_id,
                                                               safety_stock_qty = sss.safety_stock_qty,
                                                               WarehouseId = sss.WarehouseId,
                                                               WarehouseName = wh.WarehouseName
                                                           }).ToList()
                                         }).ToList()

                        };
            var data = await query.FirstOrDefaultAsync();
            if (data != null)
            {
                return data;
            }
            else
            {
                return new SpuBothViewModel();
            }
        }
        /// <summary>
        /// get sku info by sku_id
        /// </summary>
        /// <param name="sku_id">sku_id</param>
        /// <returns></returns>
        public async Task<SkuDetailViewModel> GetSkuAsync(int sku_id)
        {
            var Categorys = _dBContext.GetDbSet<CategoryEntity>();
            var Spus = _dBContext.GetDbSet<SpuEntity>();
            var Skus = _dBContext.GetDbSet<SkuEntity>();
            var query = from m in Spus.AsNoTracking()
                        join c in Categorys.AsNoTracking() on m.category_id equals c.Id
                        join d in Skus.AsNoTracking() on m.Id equals d.spu_id
                        where d.Id == sku_id
                        select new SkuDetailViewModel
                        {
                            spu_id = m.Id,
                            spu_code = m.spu_code,
                            spu_name = m.spu_name,
                            category_id = m.category_id.GetValueOrDefault(),
                            category_name = c.category_name,
                            spu_description = m.spu_description,
                            supplier_id = m.supplier_id.GetValueOrDefault(),
                            supplier_name = m.supplier_name,
                            brand = m.brand,
                            origin = m.origin,
                            length_unit = m.length_unit.GetValueOrDefault(),
                            volume_unit = m.volume_unit.GetValueOrDefault(),
                            weight_unit = m.weight_unit.GetValueOrDefault(),
                            sku_id = d.Id,
                            sku_code = d.sku_code,
                            sku_name = d.sku_name,
                            bar_code = d.bar_code ?? "",
                            weight = d.weight.GetValueOrDefault(),
                            lenght = d.lenght.GetValueOrDefault(),
                            width = d.width.GetValueOrDefault(),
                            height = d.height.GetValueOrDefault(),
                            volume = d.volume.GetValueOrDefault(),
                            unit = d.unit ?? "",
                            cost = d.cost.GetValueOrDefault(),
                            price = d.price.GetValueOrDefault()
                        };
            var data = await query.FirstOrDefaultAsync();
            if (data != null)
            {
                return data;
            }
            else
            {
                return new SkuDetailViewModel();
            }

        }

        /// <summary>
        /// get sku info by bar_code
        /// </summary>
        /// <param name="bar_code">bar_code</param>
        /// <returns></returns>
        public async Task<SkuDetailViewModel> GetSkuByBarCodeAsync(string bar_code)
        {
            var Categorys = _dBContext.GetDbSet<CategoryEntity>();
            var Spus = _dBContext.GetDbSet<SpuEntity>();
            var Skus = _dBContext.GetDbSet<SkuEntity>();
            var query = from m in Spus.AsNoTracking()
                        join c in Categorys.AsNoTracking() on m.category_id equals c.Id
                        join d in Skus.AsNoTracking() on m.Id equals d.spu_id
                        where d.bar_code == bar_code
                        select new SkuDetailViewModel
                        {
                            spu_id = m.Id,
                            spu_code = m.spu_code,
                            spu_name = m.spu_name,
                            category_id = m.category_id.GetValueOrDefault(),
                            category_name = c.category_name,
                            spu_description = m.spu_description,
                            supplier_id = m.supplier_id.GetValueOrDefault(),
                            supplier_name = m.supplier_name,
                            brand = m.brand,
                            origin = m.origin,
                            length_unit = m.length_unit.GetValueOrDefault(),
                            volume_unit = m.volume_unit.GetValueOrDefault(),
                            weight_unit = m.weight_unit.GetValueOrDefault(),
                            sku_id = d.Id,
                            sku_code = d.sku_code,
                            sku_name = d.sku_name,
                            bar_code = d.bar_code ?? "",
                            weight = d.weight.GetValueOrDefault(),
                            lenght = d.lenght.GetValueOrDefault(),
                            width = d.width.GetValueOrDefault(),
                            height = d.height.GetValueOrDefault(),
                            volume = d.volume.GetValueOrDefault(),
                            unit = d.unit ?? "",
                            cost = d.cost.GetValueOrDefault(),
                            price = d.price.GetValueOrDefault()
                        };
            var data = await query.FirstOrDefaultAsync();
            if (data != null)
            {
                return data;
            }
            else
            {
                return new SkuDetailViewModel();
            }

        }

        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        public async Task<(int id, string msg)> AddAsync(SpuBothViewModel viewModel, CurrentUser currentUser)
        {
            DbSet<SpuEntity>? DbSet = _dBContext.GetDbSet<SpuEntity>();
            if (await DbSet.AsNoTracking().AnyAsync(t => t.TenantId.Equals(currentUser.tenant_id) && t.spu_code.Equals(viewModel.spu_code)))
            {
                return (0, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["spu_code"], viewModel.spu_code));
            }
            var entity = viewModel.Adapt<SpuEntity>();
            entity.Id = 0;
            entity.creator = currentUser.user_name;
            entity.create_time = DateTime.UtcNow;
            entity.last_update_time = DateTime.UtcNow;
            entity.TenantId = currentUser.tenant_id;
            if (viewModel.detailList.Any())
            {
                decimal dec = ChangeLengthUnit(entity.length_unit.GetValueOrDefault(), entity.volume_unit.GetValueOrDefault());
                viewModel.detailList.ForEach(t =>
                {
                    t.id = 0;
                    t.volume = Math.Round(t.lenght * dec * t.width * dec * t.height * dec, 3);
                });
            }
            await DbSet.AddAsync(entity);
            await _dBContext.SaveChangesAsync();
            if (entity.Id > 0)
            {
                return (entity.Id, _stringLocalizer["save_success"]);
            }
            else
            {
                return (0, _stringLocalizer["save_failed"]);
            }
        }
        /// <summary>
        /// change to the volume unit
        /// </summary>
        /// <param name="length_unit">length_unit</param>
        /// <param name="volume_unit">volume_unit</param>
        /// <returns></returns>
        private decimal ChangeLengthUnit(byte length_unit, byte volume_unit)
        {
            if (volume_unit.Equals(0)) // cm3
            {
                if (length_unit.Equals(0)) //mm
                {
                    return 0.1M;
                }
                else if (length_unit.Equals(2)) // dm
                {
                    return 10M;
                }
                else if (length_unit.Equals(3)) // m
                {
                    return 100M;
                }
                else // cm
                {
                    return 1M;
                }
            }
            else if (volume_unit.Equals(1)) // dm3
            {
                if (length_unit.Equals(0))
                {
                    return 0.01M;
                }
                else if (length_unit.Equals(2))
                {
                    return 1M;
                }
                else if (length_unit.Equals(3))
                {
                    return 10M;
                }
                else
                {
                    return 0.1M;
                }
            }
            else if (volume_unit.Equals(2)) // m3
            {
                if (length_unit.Equals(0))
                {
                    return 0.001M;
                }
                else if (length_unit.Equals(2))
                {
                    return 0.1M;
                }
                else if (length_unit.Equals(3))
                {
                    return 1M;
                }
                else
                {
                    return 0.01M;
                }
            }
            else
            {
                return 1M;
            }

        }

        private async Task<SpuEntity?> GetSpuEntityAsync(int id)
        {
            var Spus = _dBContext.GetDbSet<SpuEntity>();
            var Skus = _dBContext.GetDbSet<SkuEntity>();
            var SkuSafetyStocks = _dBContext.GetDbSet<SkuSafetyStockEntity>();
            var entity = await (
                 from m in Spus
                 where m.Id == id
                 select new SpuEntity
                 {
                     Id = m.Id,
                     spu_code = m.spu_code,
                     spu_name = m.spu_name,
                     category_id = m.category_id,
                     spu_description = m.spu_description,
                     supplier_id = m.supplier_id,
                     supplier_name = m.supplier_name,
                     brand = m.brand,
                     origin = m.origin,
                     length_unit = m.length_unit,
                     volume_unit = m.volume_unit,
                     weight_unit = m.weight_unit,
                     creator = m.creator,
                     create_time = m.create_time,
                     last_update_time = m.last_update_time,
                     is_valid = m.is_valid,
                     detailList = Skus.Where(t => t.spu_id.Equals(m.Id))
                                  .Select(t => new SkuEntity
                                  {
                                      Spu = m,
                                      Id = t.Id,
                                      spu_id = t.spu_id,
                                      sku_code = t.sku_code,
                                      sku_name = t.sku_name,
                                      bar_code = t.bar_code,
                                      weight = t.weight,
                                      lenght = t.lenght,
                                      width = t.width,
                                      height = t.height,
                                      volume = t.volume,
                                      unit = t.unit,
                                      cost = t.cost,
                                      price = t.price,
                                      create_time = t.create_time,
                                      last_update_time = t.last_update_time,
                                      detailList = (from sss in SkuSafetyStocks
                                                    where sss.sku_id.Equals(t.Id)
                                                    select new SkuSafetyStockEntity
                                                    {
                                                        Sku = t,
                                                        Id = sss.Id,
                                                        sku_id = sss.sku_id,
                                                        safety_stock_qty = sss.safety_stock_qty,
                                                        WarehouseId = sss.WarehouseId
                                                    }).ToList()
                                  }).ToList()

                 })
                 .FirstOrDefaultAsync();
            return entity;
        }
        /// <summary>
        /// update a record
        /// </summary>
        /// <param name="viewModel">args</param>
        /// <returns></returns>
        public async Task<(bool flag, string msg)> UpdateAsync(SpuBothViewModel viewModel)
        {
            var DbSet = _dBContext.GetDbSet<SpuEntity>();
            var entity = await DbSet.Include(d => d.detailList)
                .FirstOrDefaultAsync(t => t.Id.Equals(viewModel.id));
            if (entity == null)
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }
            if (await DbSet.AsNoTracking().AnyAsync(t => !t.Id.Equals(viewModel.id) && t.TenantId.Equals(entity.TenantId) && t.spu_code.Equals(viewModel.spu_code)))
            {
                return (false, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["spu_code"], viewModel.spu_code));
            }
            entity.spu_code = viewModel.spu_code;
            entity.spu_name = viewModel.spu_name;
            entity.category_id = viewModel.category_id;
            entity.spu_description = viewModel.spu_description;
            entity.supplier_id = viewModel.supplier_id;
            entity.supplier_name = viewModel.supplier_name;
            entity.brand = viewModel.brand;
            entity.origin = viewModel.origin;
            entity.length_unit = viewModel.length_unit;
            entity.volume_unit = viewModel.volume_unit;
            entity.weight_unit = viewModel.weight_unit;
            entity.is_valid = viewModel.is_valid;
            entity.last_update_time = DateTime.UtcNow;


            if (viewModel.detailList.Any(t => t.id > 0))
            {
                entity.detailList.ForEach(d =>
                {
                    var vm = viewModel.detailList.Where(t => t.id > 0).FirstOrDefault(t => t.id == d.Id);
                    if (vm != null)
                    {
                        d.sku_code = vm.sku_code;
                        d.sku_name = vm.sku_name;
                        d.bar_code = vm.bar_code;
                        d.weight = vm.weight;
                        d.lenght = vm.lenght;
                        d.width = vm.width;
                        d.height = vm.height;
                        d.volume = vm.volume;
                        d.unit = vm.unit;
                        d.cost = vm.cost;
                        d.price = vm.price;
                        d.last_update_time = DateTime.UtcNow;
                    }
                });
            }
            if (viewModel.detailList.Any(t => t.id == 0))
            {
                entity.detailList.AddRange(viewModel.detailList.Where(t => t.id == 0).ToList().Adapt<List<SkuEntity>>());
            }
            if (viewModel.detailList.Any(t => t.id < 0))
            {
                var delIds = viewModel.detailList.Where(t => t.id < 0).Select(t => t.id * -1).ToList();
                entity.detailList.RemoveAll(entity => delIds.Contains(entity.Id));
            }
            var qty = await _dBContext.SaveChangesAsync();
            if (qty > 0)
            {
                decimal dec = ChangeLengthUnit(entity.length_unit.GetValueOrDefault(), entity.volume_unit.GetValueOrDefault());
                var skuEntity = await _dBContext.GetDbSet<SkuEntity>()
                    .Where(t => t.spu_id.Equals(entity.Id))
                    .FirstOrDefaultAsync();

                if (skuEntity != null)
                {
                    var length = skuEntity.lenght ?? 0;
                    var width = skuEntity.width ?? 0;
                    var height = skuEntity.height ?? 0;
                    var volume = Math.Round(length * dec * width * dec * height * dec, 3);
                    await _dBContext.GetDbSet<SkuEntity>()
                        .Where(t => t.spu_id.Equals(entity.Id))
                        .ExecuteUpdateAsync(p => p.SetProperty(x => x.volume, x => volume));
                }

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
            var Asns = _dBContext.GetDbSet<AsnEntity>();
            if (await Asns.AsNoTracking().AnyAsync(t => t.spu_id.Equals(id)))
            {
                return (false, _stringLocalizer["delete_referenced"]);
            }
            var qty = await _dBContext.GetDbSet<SkuEntity>().Where(t => t.spu_id.Equals(id)).ExecuteDeleteAsync();
            qty += await _dBContext.GetDbSet<SpuEntity>().Where(t => t.Id.Equals(id)).ExecuteDeleteAsync();
            if (qty > 0)
            {
                return (true, _stringLocalizer["delete_success"]);
            }
            else
            {
                return (false, _stringLocalizer["delete_failed"]);
            }
        }
        #endregion

        #region add or update sku_safety_stock
        /// <summary>
        /// add or update sku_safety_stock
        /// </summary>
        /// <param name="viewModel">args</param>
        /// <returns></returns>
        public async Task<(bool flag, string msg)> InsertOrUpdateSkuSafetyStockAsync(SkuSafetyStockPutViewModel viewModel)
        {
            if (viewModel.detailList.Any())
            {
                var SafetyDB = _dBContext.GetDbSet<SkuSafetyStockEntity>();
                var entities = await SafetyDB.Where(t => t.sku_id == viewModel.sku_id)
                    .ToListAsync();
                viewModel.detailList.ForEach(async t =>
                {
                    if (t.id == 0)
                    {
                        await SafetyDB.AddAsync(new SkuSafetyStockEntity { sku_id = viewModel.sku_id, safety_stock_qty = t.safety_stock_qty, WarehouseId = t.WarehouseId });

                    }
                    else
                    {
                        var ent = entities.FirstOrDefault(e => e.Id == Math.Abs(t.id));
                        if (ent != null)
                        {
                            if (t.id < 0)
                            {
                                SafetyDB.Remove(ent);
                            }
                            else
                            {
                                ent.WarehouseId = t.WarehouseId;
                                ent.safety_stock_qty = t.safety_stock_qty;
                            }
                        }
                    }
                });
                await _dBContext.SaveChangesAsync();
                return (true, _stringLocalizer["save_success"]);
            }
            else
            {
                return (false, _stringLocalizer["save_failed"]);
            }
        }

        #endregion
    }
}

