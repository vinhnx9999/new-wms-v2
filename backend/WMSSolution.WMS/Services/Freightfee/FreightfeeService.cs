
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.IServices;

namespace WMSSolution.WMS.Services
{
    /// <summary>
    ///  Freightfee Service
    /// </summary>
    public class FreightfeeService : BaseService<FreightfeeEntity>, IFreightfeeService
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
        ///Freightfee  constructor
        /// </summary>
        /// <param name="dBContext">The DBContext</param>
        /// <param name="stringLocalizer">Localizer</param>
        public FreightfeeService(
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
        public async Task<(List<FreightfeeViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser)
        {
            QueryCollection queries = new QueryCollection();
            if (pageSearch.searchObjects.Any())
            {
                pageSearch.searchObjects.ForEach(s =>
                {
                    queries.Add(s);
                });
            }
            var DbSet = _dBContext.GetDbSet<FreightfeeEntity>();
            var query = DbSet.AsNoTracking()
                .Where(t => t.tenant_id.Equals(currentUser.tenant_id))
                .Where(queries.AsExpression<FreightfeeEntity>());
            int totals = await query.CountAsync();
            var list = await query.OrderByDescending(t => t.create_time)
                       .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                       .Take(pageSearch.pageSize)
                       .ToListAsync();
            return (list.Adapt<List<FreightfeeViewModel>>(), totals);
        }

        /// <summary>
        /// Get all records
        /// </summary>
        /// <returns></returns>
        public async Task<List<FreightfeeViewModel>> GetAllAsync(CurrentUser currentUser)
        {
            var DbSet = _dBContext.GetDbSet<FreightfeeEntity>();
            var data = await DbSet.AsNoTracking().Where(t => t.tenant_id.Equals(currentUser.tenant_id)).ToListAsync();
            return data.Adapt<List<FreightfeeViewModel>>();
        }

        /// <summary>
        /// Get a record by id
        /// </summary>
        /// <returns></returns>
        public async Task<FreightfeeViewModel> GetAsync(int id)
        {
            var DbSet = _dBContext.GetDbSet<FreightfeeEntity>();
            var entity = await DbSet.AsNoTracking().FirstOrDefaultAsync(t => t.Id.Equals(id));
            if (entity == null)
            {
                return new FreightfeeViewModel();
            }
            return entity.Adapt<FreightfeeViewModel>();
        }
        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        public async Task<(int id, string msg)> AddAsync(FreightfeeViewModel viewModel, CurrentUser currentUser)
        {
            var DbSet = _dBContext.GetDbSet<FreightfeeEntity>();
            var entity = viewModel.Adapt<FreightfeeEntity>();
            entity.Id = 0;
            entity.create_time = DateTime.UtcNow;
            entity.creator = currentUser.user_name;
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
        /// <returns></returns>
        public async Task<(bool flag, string msg)> UpdateAsync(FreightfeeViewModel viewModel)
        {
            var DbSet = _dBContext.GetDbSet<FreightfeeEntity>();
            var entity = await DbSet.FirstOrDefaultAsync(t => t.Id.Equals(viewModel.id));
            if (entity == null)
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }
            entity.Id = viewModel.id;
            entity.carrier = viewModel.carrier;
            entity.departure_city = viewModel.departure_city;
            entity.arrival_city = viewModel.arrival_city;
            entity.price_per_weight = viewModel.price_per_weight;
            entity.price_per_volume = viewModel.price_per_volume;
            entity.min_payment = viewModel.min_payment;
            entity.is_valid = viewModel.is_valid;
            entity.last_update_time = DateTime.UtcNow;
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
            var qty = await _dBContext.GetDbSet<FreightfeeEntity>().Where(t => t.Id.Equals(id)).ExecuteDeleteAsync();
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
        /// import freightfee by excel
        /// </summary>
        /// <param name="datas">excel datas</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        public async Task<(bool flag, string msg)> ExcelAsync(List<FreightfeeExcelmportViewModel> datas, CurrentUser currentUser)
        {
            // StringBuilder sb = new StringBuilder();
            var DbSet = _dBContext.GetDbSet<FreightfeeEntity>();
            /*        var user_num_repeat_excel = datas.GroupBy(t => t.WarehouseName).Select(t => new { WarehouseName = t.Key, cnt = t.Count() }).Where(t => t.cnt > 1).ToList();
                    foreach (var repeat in user_num_repeat_excel)
                    {
                        sb.AppendLine(string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["WarehouseName"], repeat.WarehouseName));
                    }
                    if (user_num_repeat_excel.Count > 1)
                    {
                        return (false, sb.ToString());
                    }

                    var user_num_repeat_exists = await DbSet.Where(t => datas.Select(t => t.WarehouseName).ToList().Contains(t.WarehouseName)).Select(t => t.WarehouseName).ToListAsync();
                    foreach (var repeat in user_num_repeat_exists)
                    {
                        sb.AppendLine(string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["WarehouseName"], repeat));
                    }
                    if (user_num_repeat_exists.Count > 1)
                    {
                        return (false, sb.ToString());
                    }*/

            var entities = datas.Adapt<List<FreightfeeEntity>>();
            entities.ForEach(t =>
            {
                t.creator = currentUser.user_name;
                t.tenant_id = currentUser.tenant_id;
                t.create_time = DateTime.UtcNow;
                t.last_update_time = DateTime.UtcNow;
                t.is_valid = true;
            });
            await DbSet.AddRangeAsync(entities);
            var res = await _dBContext.SaveChangesAsync();
            if (res > 0)
            {
                return (true, _stringLocalizer["save_success"]);
            }
            return (false, _stringLocalizer["save_failed"]);
        }
        #endregion
    }
}

