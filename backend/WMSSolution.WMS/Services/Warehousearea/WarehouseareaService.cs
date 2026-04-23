using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Warehouse;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.IServices;

namespace WMSSolution.WMS.Services
{
    /// <summary>
    ///  Warehousearea Service
    /// </summary>
    public class WarehouseareaService : BaseService<WarehouseareaEntity>, IWarehouseareaService
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
        ///Warehousearea  constructor
        /// </summary>
        /// <param name="dBContext">The DBContext</param>
        /// <param name="stringLocalizer">Localizer</param>
        public WarehouseareaService(
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
        public async Task<(List<WarehouseareaViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser)
        {
            QueryCollection queries = new QueryCollection();
            if (pageSearch.searchObjects.Any())
            {
                pageSearch.searchObjects.ForEach(s =>
                {
                    queries.Add(s);
                });
            }
            var DbSet = _dBContext.GetDbSet<WarehouseareaEntity>();
            var warehouse_DBSet = _dBContext.GetDbSet<WarehouseEntity>();

            var query = from wa in DbSet.AsNoTracking()
                        join w in warehouse_DBSet.AsNoTracking() on wa.WarehouseId equals w.Id
                        select new WarehouseareaViewModel
                        {
                            id = wa.Id,
                            WarehouseId = wa.WarehouseId,
                            WarehouseName = w.WarehouseName,
                            area_name = wa.area_name,
                            parent_id = wa.parent_id,
                            create_time = wa.create_time,
                            last_update_time = wa.last_update_time,
                            is_valid = wa.is_valid,
                            tenant_id = wa.tenant_id,
                            area_property = wa.area_property,
                        };
            if (pageSearch.sqlTitle == "select")
            {
                query = query.Where(t => t.is_valid == true);
            }
            query = query.Where(t => t.tenant_id.Equals(currentUser.tenant_id)).Where(queries.AsExpression<WarehouseareaViewModel>());
            int totals = await query.CountAsync();
            var list = await query.OrderByDescending(t => t.create_time)
                       .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                       .Take(pageSearch.pageSize)
                       .ToListAsync();
            return (list, totals);
        }
        /// <summary>
        /// get warehouseareas of the warehouse by WarehouseId
        /// </summary>
        /// <param name="WarehouseId">warehouse's id</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        public async Task<List<FormSelectItem>> GetWarehouseareaByWarehouseId(int WarehouseId, CurrentUser currentUser)
        {
            var res = new List<FormSelectItem>();
            var DbSet = _dBContext.GetDbSet<WarehouseareaEntity>();
            res = await (from wa in DbSet.AsNoTracking()
                         where wa.is_valid == true && wa.tenant_id == currentUser.tenant_id && wa.WarehouseId == WarehouseId
                         select new FormSelectItem
                         {
                             code = "warehousearea",
                             comments = "warehouseareas of the warehouse",
                             name = wa.area_name,
                             value = wa.Id.ToString(),
                         }).ToListAsync();
            return res;
        }

        /// <summary>
        /// Get all records
        /// </summary>
        /// <returns></returns>
        public async Task<List<WarehouseareaViewModel>> GetAllAsync(int WarehouseId, CurrentUser currentUser)
        {
            var DbSet = _dBContext.GetDbSet<WarehouseareaEntity>().AsNoTracking();
            if (WarehouseId > 0)
            {
                DbSet = DbSet.Where(t => t.WarehouseId == WarehouseId);
            }
            var data = await DbSet.Where(t => t.is_valid == true && t.tenant_id.Equals(currentUser.tenant_id)).ToListAsync();
            return data.Adapt<List<WarehouseareaViewModel>>();
        }

        /// <summary>
        /// Get a record by id
        /// </summary>
        /// <returns></returns>
        public async Task<WarehouseareaViewModel> GetAsync(int id)
        {
            var DbSet = _dBContext.GetDbSet<WarehouseareaEntity>();
            var entity = await DbSet.AsNoTracking().FirstOrDefaultAsync(t => t.Id.Equals(id));
            if (entity == null)
            {
                return new WarehouseareaViewModel();
            }
            return entity.Adapt<WarehouseareaViewModel>();
        }
        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        public async Task<(int id, string msg)> AddAsync(WarehouseareaViewModel viewModel, CurrentUser currentUser)
        {
            var DbSet = _dBContext.GetDbSet<WarehouseareaEntity>();
            if (await DbSet.AnyAsync(t => t.WarehouseId == viewModel.WarehouseId && t.area_name == viewModel.area_name && t.tenant_id == currentUser.tenant_id))
            {
                return (0, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["area_name"], viewModel.area_name));
            }
            var entity = viewModel.Adapt<WarehouseareaEntity>();
            entity.Id = 0;
            entity.create_time = DateTime.UtcNow;
            entity.last_update_time = DateTime.UtcNow;
            entity.tenant_id = currentUser.tenant_id;
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
        /// update a record
        /// </summary>
        /// <param name="viewModel">args</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        public async Task<(bool flag, string msg)> UpdateAsync(WarehouseareaViewModel viewModel, CurrentUser currentUser)
        {
            var DbSet = _dBContext.GetDbSet<WarehouseareaEntity>();
            var entity = await DbSet.FirstOrDefaultAsync(t => t.Id.Equals(viewModel.id));
            if (await DbSet.AnyAsync(t => t.Id != viewModel.id && t.WarehouseId == viewModel.WarehouseId && t.area_name == viewModel.area_name && t.tenant_id == currentUser.tenant_id))
            {
                return (false, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["area_name"], viewModel.area_name));
            }
            if (entity == null)
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }
            entity.Id = viewModel.id;
            entity.WarehouseId = viewModel.WarehouseId;
            entity.area_name = viewModel.area_name;
            entity.parent_id = viewModel.parent_id;
            entity.is_valid = viewModel.is_valid;
            entity.area_property = viewModel.area_property;
            entity.last_update_time = DateTime.UtcNow;
            var goodslocation_DBSet = _dBContext.GetDbSet<GoodslocationEntity>();
            var gldatas = await goodslocation_DBSet.Where(t => t.WarehouseAreaId == entity.Id).ToListAsync();
            gldatas.ForEach(t =>
            {
                t.WarehouseAreaName = entity.area_name;
                t.WarehouseAreaProperty = entity.area_property;
                t.IsValid = entity.is_valid;
            });
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
            if (await _dBContext.GetDbSet<GoodslocationEntity>().AnyAsync(t => t.WarehouseAreaId == id))
            {
                return (false, _stringLocalizer["exist_location_not_delete"]);
            }
            var qty = await _dBContext.GetDbSet<WarehouseareaEntity>().Where(t => t.Id.Equals(id)).ExecuteDeleteAsync();
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
    }
}

