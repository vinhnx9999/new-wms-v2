
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Core.Utility;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.IServices;

namespace WMSSolution.WMS.Services
{
    /// <summary>
    ///  Rolemenu Service
    /// </summary>
    public class RolemenuService : BaseService<RolemenuEntity>, IRolemenuService
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
        ///Rolemenu  constructor
        /// </summary>
        /// <param name="dBContext">The DBContext</param>
        /// <param name="stringLocalizer">Localizer</param>
        public RolemenuService(
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
        /// Get all records
        /// </summary>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        public async Task<List<RolemenuListViewModel>> GetAllAsync(CurrentUser currentUser)
        {
            var Rolemenus = _dBContext.GetDbSet<RolemenuEntity>();
            var Userroles = _dBContext.GetDbSet<UserroleEntity>();
            var queryMenusGroup = Rolemenus.AsNoTracking()
               .Where(t => t.tenant_id == currentUser.tenant_id)
               .GroupBy(g => new { g.userrole_id })
               .Select(g => new
               {
                   userrole_id = g.Key.userrole_id,
                   create_time = g.Min(t => t.create_time),
                   last_update_time = g.Max(t => t.last_update_time)
               });
            var data = await (from g in queryMenusGroup
                              join r in Userroles.AsNoTracking().Where(t => t.tenant_id == currentUser.tenant_id)
                              on g.userrole_id equals r.Id
                              select new RolemenuListViewModel
                              {
                                  userrole_id = g.userrole_id,
                                  role_name = r.role_name,
                                  is_valid = r.is_valid,
                                  create_time = g.create_time,
                                  last_update_time = g.last_update_time
                              }).ToListAsync();
            return data;
        }

        /// <summary>
        /// Get a record by id
        /// </summary>
        /// <param name="userrole_id">userrole id</param>
        /// <returns></returns>
        public async Task<RolemenuBothViewModel> GetAsync(int userrole_id)
        {
            var Rolemenus = _dBContext.GetDbSet<RolemenuEntity>();
            var Userroles = _dBContext.GetDbSet<UserroleEntity>();
            var Menus = _dBContext.GetDbSet<MenuEntity>();
            var entities = await (from rm in Rolemenus.AsNoTracking()
                                  join m in Menus.AsNoTracking() on rm.menu_id equals m.Id
                                  join r in Userroles.AsNoTracking() on rm.userrole_id equals r.Id
                                  where rm.userrole_id == userrole_id
                                  orderby r.role_name, m.sort, m.menu_name
                                  select new
                                  {
                                      rm.Id,
                                      rm.userrole_id,
                                      r.role_name,
                                      r.is_valid,
                                      rm.menu_id,
                                      m.menu_name,
                                      rm.authority,
                                      rm.menu_actions_authority,
                                      rm.create_time,
                                      rm.last_update_time
                                  }).ToListAsync();
            if (entities.Any())
            {
                var data = new RolemenuBothViewModel
                {
                    userrole_id = entities.First().userrole_id,
                    role_name = entities.First().role_name,
                    is_valid = entities.First().is_valid,
                    detailList = entities.Select(t => new RolemenuViewModel
                    {
                        id = t.Id,
                        menu_id = t.menu_id,
                        menu_name = t.menu_name,
                        authority = t.authority,
                        menu_actions_authority = JsonHelper.DeserializeObject<List<string>>(t.menu_actions_authority)
                    }).ToList()
                };
                return data;
            }
            else
            {
                return new RolemenuBothViewModel();
            }
        }
        /// <summary>
        /// Get all menus
        /// </summary>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        public async Task<List<MenuViewModel>> GetAllMenusAsync(CurrentUser currentUser)
        {
            var Menus = _dBContext.GetDbSet<MenuEntity>();
            var data = await Menus.AsNoTracking()
                .Where(t => t.tenant_id == currentUser.tenant_id)
                .Select(m => new
                {
                    id = m.Id,
                    menu_name = m.menu_name,
                    module = m.module,
                    vue_path = m.vue_path,
                    vue_path_detail = m.vue_path_detail,
                    vue_directory = m.vue_directory,
                    sort = m.sort,
                    menu_actions = m.menu_actions
                }).ToListAsync();

            var result = data.Select(m => new MenuViewModel
            {
                id = m.id,
                menu_name = m.menu_name,
                module = m.module,
                vue_path = m.vue_path,
                vue_path_detail = m.vue_path_detail,
                vue_directory = m.vue_directory,
                sort = m.sort,
                menu_actions = JsonHelper.DeserializeObject<List<string>>(m.menu_actions)
            }).ToList();
            return result;
        }
        /// <summary>
        /// Get menu's authority by user role id
        /// </summary>
        /// <param name="userrole_id">user role id</param>
        /// <returns></returns>
        public async Task<List<MenuViewModel>> GetMenusByRoleId(int userrole_id)
        {
            var Rolemenus = _dBContext.GetDbSet<RolemenuEntity>();
            var Menus = _dBContext.GetDbSet<MenuEntity>();
            var data = await (from rm in Rolemenus.AsNoTracking()
                              join m in Menus.AsNoTracking() on rm.menu_id equals m.Id
                              where rm.userrole_id == userrole_id
                              orderby m.sort, m.menu_name
                              select new
                              {
                                  id = m.Id,
                                  menu_name = m.menu_name,
                                  module = m.module,
                                  vue_path = m.vue_path,
                                  vue_path_detail = m.vue_path_detail,
                                  vue_directory = m.vue_directory,
                                  sort = m.sort,
                                  rm.menu_actions_authority
                              }).ToListAsync();
            if (data.Any())
            {
                var result = data.Select(m => new MenuViewModel
                {
                    id = m.id,
                    menu_name = m.menu_name,
                    module = m.module,
                    vue_path = m.vue_path,
                    vue_path_detail = m.vue_path_detail,
                    vue_directory = m.vue_directory,
                    sort = m.sort,
                    menu_actions = JsonHelper.DeserializeObject<List<string>>(m.menu_actions_authority)
                }).ToList();
                return result;
            }
            return new List<MenuViewModel>();
        }
        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        public async Task<(int id, string msg)> AddAsync(RolemenuBothViewModel viewModel, CurrentUser currentUser)
        {
            var Rolemenus = _dBContext.GetDbSet<RolemenuEntity>();
            if (await Rolemenus.AnyAsync(t => t.userrole_id.Equals(viewModel.userrole_id)))
            {
                return (0, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["role_name"], viewModel.role_name));
            }
            var entities = viewModel.detailList.Select(t => new RolemenuEntity
            {
                Id = 0,
                userrole_id = viewModel.userrole_id,
                menu_id = t.menu_id,
                authority = t.authority,
                menu_actions_authority = JsonHelper.SerializeObject(t.menu_actions_authority),
                create_time = DateTime.UtcNow,
                last_update_time = DateTime.UtcNow,
                tenant_id = currentUser.tenant_id
            }).ToList();

            await Rolemenus.AddRangeAsync(entities);
            var qty = await _dBContext.SaveChangesAsync();
            if (qty > 0)
            {
                return (viewModel.userrole_id, _stringLocalizer["save_success"]);
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
        public async Task<(bool flag, string msg)> UpdateAsync(RolemenuBothViewModel viewModel, CurrentUser currentUser)
        {
            var Rolemenus = _dBContext.GetDbSet<RolemenuEntity>();
            if (!(await Rolemenus.AnyAsync(t => t.userrole_id.Equals(viewModel.userrole_id))))
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }
            var dbEntities = await Rolemenus.AsNoTracking().Where(t => t.userrole_id == viewModel.userrole_id).ToListAsync();

            var entities = (from vm in viewModel.detailList
                            join db in dbEntities on new { id = Math.Abs(vm.id), menu_id = vm.menu_id } equals new { id = db.Id, menu_id = db.menu_id } into dbJoin
                            from db in dbJoin.DefaultIfEmpty()
                            select new RolemenuEntity
                            {
                                Id = vm.id,
                                userrole_id = viewModel.userrole_id,
                                menu_id = vm.menu_id,
                                authority = vm.authority,
                                menu_actions_authority = JsonHelper.SerializeObject(vm.menu_actions_authority),
                                create_time = db == null ? DateTime.UtcNow : db.create_time,
                                last_update_time = DateTime.UtcNow,
                                tenant_id = currentUser.tenant_id
                            }).ToList();

            if (Enumerable.Any<RolemenuEntity>(entities, (Func<RolemenuEntity, bool>)(t => t.Id > 0)))
            {
                Rolemenus.UpdateRange(Enumerable.Where<RolemenuEntity>(entities, (Func<RolemenuEntity, bool>)(t => t.Id > 0)).ToList());
            }
            if (Enumerable.Any<RolemenuEntity>(entities, (Func<RolemenuEntity, bool>)(t => t.Id == 0)))
            {
                Rolemenus.AddRange(Enumerable.Where<RolemenuEntity>(entities, (Func<RolemenuEntity, bool>)(t => t.Id == 0)).ToList());
            }
            if (Enumerable.Any<RolemenuEntity>(entities, (Func<RolemenuEntity, bool>)(t => t.Id < 0)))
            {
                var dels = Enumerable.Where<RolemenuEntity>(entities, (Func<RolemenuEntity, bool>)(t => t.Id < 0)).ToList();
                dels.ForEach(t => t.Id *= -1);
                Rolemenus.RemoveRange(dels);
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
        /// delete a record
        /// </summary>
        /// <param name="userrole_id">userrole id</param>
        /// <returns></returns>
        public async Task<(bool flag, string msg)> DeleteAsync(int userrole_id)
        {
            var qty = await _dBContext.GetDbSet<RolemenuEntity>().Where(t => t.userrole_id.Equals(userrole_id)).ExecuteDeleteAsync();
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

