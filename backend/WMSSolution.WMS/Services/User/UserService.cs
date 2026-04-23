using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using System.Text;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Core.Utility;
using WMSSolution.Shared;
using WMSSolution.Shared.RBAC;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.User;
using WMSSolution.WMS.IServices.ActionLog;
using WMSSolution.WMS.IServices.User;

namespace WMSSolution.WMS.Services.User;

/// <summary>
///  User Service
/// </summary>
/// <remarks>
///User  constructor
/// </remarks>
/// <param name="dBContext">The DBContext</param>
/// <param name="configuration"></param>
/// <param name="actionLogService"></param>
/// <param name="stringLocalizer">Localizer</param>
public class UserService(SqlDBContext dBContext,
    IConfiguration configuration,
    IActionLogService actionLogService,
    IStringLocalizer<MultiLanguage> stringLocalizer) :
    BaseService<userEntity>, IUserService
{
    #region Args

    private readonly IConfiguration _configuration = configuration;
    /// <summary>
    /// The DBContext
    /// </summary>
    private readonly SqlDBContext _dBContext = dBContext;
    private readonly IActionLogService _actionLogService = actionLogService;
    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    #endregion Args

    #region Api

    /// <summary>
    /// get select items
    /// </summary>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<List<FormSelectItem>> GetSelectItemsAsnyc(CurrentUser currentUser)
    {
        var res = new List<FormSelectItem>();
        var userrole_DBSet = _dBContext.GetDbSet<UserroleEntity>();

        res.AddRange(await (from ur in userrole_DBSet.AsNoTracking()
                            where ur.is_valid == true && ur.tenant_id == currentUser.tenant_id
                            select new FormSelectItem
                            {
                                code = "user_role",
                                name = ur.role_name,
                                value = ur.Id.ToString(),
                                comments = "user's role",
                            }).ToListAsync());
        return res;
    }

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(List<UserViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count != 0)
        {
            pageSearch.searchObjects.ForEach(s =>
            {
                queries.Add(s);
            });
        }
        //var DbSet = _dBContext.GetDbSet<userEntity>();
        //var query = DbSet.AsNoTracking()
        //    .Where(t => t.tenant_id.Equals(currentUser.tenant_id) && string.IsNullOrEmpty(t.integration_app))
        //    .Where(queries.AsExpression<userEntity>());

        var query = GetQueryUserByUser(currentUser);
        if (pageSearch.sqlTitle == "select")
        {
            query = query.Where(t => t.is_valid == true);
        }
        int totals = await query.CountAsync();
        var list = await query.OrderByDescending(t => t.create_time)
                   .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                   .Take(pageSearch.pageSize)
                   .ToListAsync();
        return (list.Adapt<List<UserViewModel>>(), totals);
    }

    /// <summary>
    /// Get all records
    /// </summary>
    /// <returns></returns>
    public async Task<List<UserViewModel>> GetAllAsync(CurrentUser currentUser)
    {
        var query = GetQueryUserByUser(currentUser);
        var data = await query.ToListAsync();
        return data.Adapt<List<UserViewModel>>();
    }

    /// <summary>
    /// Get a record by id
    /// </summary>
    /// <returns></returns>
    public async Task<UserViewModel> GetAsync(int id)
    {
        var DbSet = _dBContext.GetDbSet<userEntity>();
        var entity = await DbSet.AsNoTracking().FirstOrDefaultAsync(t => t.Id.Equals(id));
        if (entity == null)
        {
            return new UserViewModel();
        }
        return entity.Adapt<UserViewModel>();
    }

    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<(int id, string msg)> AddAsync(UserViewModel viewModel, CurrentUser currentUser)
    {
        var DbSet = _dBContext.GetDbSet<userEntity>();
        if (await DbSet.AnyAsync(t => t.user_num == viewModel.user_num && t.tenant_id == currentUser.tenant_id))
        {
            return (0, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["user_num"], viewModel.user_num));
        }
        var entity = viewModel.Adapt<userEntity>();
        entity.Id = 0;
        //TODO later change to generate random password and send email to user
        //var new_auth = GenarationHelper.GetRandomPassword();
        entity.auth_string = Md5Helper.Md5Encrypt32(SystemDefine.DefaultPassword);
        entity.create_time = DateTime.UtcNow;
        entity.last_update_time = DateTime.UtcNow;
        entity.tenant_id = currentUser.tenant_id;
        await DbSet.AddAsync(entity);
        await _dBContext.SaveChangesAsync();
        if (entity.Id > 0)
        {
            await _actionLogService.AddLogAsync(
            $"[Add] new user {entity.Id} Username {entity.user_name} just requesting by {currentUser.user_name}",
            "User", currentUser);

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
    public async Task<(bool flag, string msg)> UpdateAsync(UserViewModel viewModel, CurrentUser currentUser)
    {
        var DbSet = _dBContext.GetDbSet<userEntity>();
        if (await DbSet.AnyAsync(t => t.Id != viewModel.id && t.user_num == viewModel.user_num && t.tenant_id == currentUser.tenant_id))
        {
            return (false, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["user_num"], viewModel.user_num));
        }
        var entity = await DbSet.FirstOrDefaultAsync(t => t.Id.Equals(viewModel.id));
        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }
        entity.Id = viewModel.id;
        entity.user_num = viewModel.user_num;
        //entity.user_name = viewModel.user_name;
        entity.contact_tel = viewModel.contact_tel;
        entity.user_role = viewModel.user_role;
        entity.sex = viewModel.sex;
        entity.is_valid = viewModel.is_valid;
        entity.last_update_time = DateTime.UtcNow;
        var qty = await _dBContext.SaveChangesAsync();
        if (qty > 0)
        {
            await _actionLogService.AddLogAsync(
                $"[Update] user {entity.Id} Username {entity.user_name} just requesting by {currentUser.user_name}",
                "User", currentUser);
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
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> DeleteAsync(int id, CurrentUser currentUser)
    {
        var qty = await _dBContext.GetDbSet<userEntity>().Where(t => t.Id.Equals(id)).ExecuteDeleteAsync();
        if (qty > 0)
        {
            await _actionLogService.AddLogAsync(
                $"[Update] user {id} just requesting by {currentUser.user_name}",
                "User", currentUser);

            return (true, _stringLocalizer["delete_success"]);
        }
        else
        {
            return (false, _stringLocalizer["delete_failed"]);
        }
    }

    /// <summary>
    /// import users by excel
    /// </summary>
    /// <param name="datas">excel datas</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ExcelAsync(List<UserExcelImportViewModel> datas, CurrentUser currentUser)
    {
        StringBuilder sb = new();
        var DbSet = _dBContext.GetDbSet<userEntity>();
        var user_num_repeat_excel = datas.GroupBy(t => t.user_num).Select(t => new { user_num = t.Key, cnt = t.Count() }).Where(t => t.cnt > 1).ToList();
        foreach (var repeat in user_num_repeat_excel)
        {
            sb.AppendLine(string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["user_num"], repeat.user_num));
        }
        if (user_num_repeat_excel.Count > 0)
        {
            return (false, sb.ToString());
        }

        var user_num_repeat_exists = await DbSet.Where(t => t.tenant_id == currentUser.tenant_id).Where(t => datas.Select(t => t.user_num).ToList().Contains(t.user_num)).Select(t => t.user_num).ToListAsync();
        foreach (var repeat in user_num_repeat_exists)
        {
            sb.AppendLine(string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["user_num"], repeat));
        }
        if (user_num_repeat_exists.Count > 0)
        {
            return (false, sb.ToString());
        }

        var entities = datas.Adapt<List<userEntity>>();
        entities.ForEach(t =>
        {
            t.creator = currentUser.user_name;
            t.tenant_id = currentUser.tenant_id;
            t.auth_string = Md5Helper.Md5Encrypt32(SystemDefine.DefaultPassword);
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

    /// <summary>
    /// reset password
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <returns></returns>
    public async Task<(bool, string)> ResetPwd(BatchOperationViewModel viewModel)
    {
        var DBSet = _dBContext.GetDbSet<userEntity>();
        var entities = await DBSet.Where(t => viewModel.id_list.Contains(t.Id)).ToListAsync();
        //TODO later  GenarationHelper.GetRandomPassword();
        var newpassword = SystemDefine.DefaultPassword;
        entities.ForEach(t => { t.auth_string = Md5Helper.Md5Encrypt32(newpassword); t.last_update_time = DateTime.UtcNow; });
        var res = await _dBContext.SaveChangesAsync();
        if (res > 0)
        {
            return (true, newpassword);
        }
        return (false, _stringLocalizer["operation_failed"]);
    }

    /// <summary>
    /// change password
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ChangePwd(UserChangePwdViewModel viewModel)
    {
        var DBSet = _dBContext.GetDbSet<userEntity>();
        var entity = await DBSet.FirstOrDefaultAsync(t => t.Id.Equals(viewModel.id));
        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }
        if (!entity.auth_string.Equals(viewModel.old_password))
        {
            return (false, _stringLocalizer["old_password"] + _stringLocalizer["is_incorrect"]);
        }
        entity.auth_string = viewModel.new_password;
        await _dBContext.SaveChangesAsync();
        return (true, _stringLocalizer["save_success"]);
    }

    /// <summary>
    /// register a new tenant
    /// </summary>
    /// <param name="viewModel">viewModel</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> Register(RegisterViewModel viewModel)
    {
        var DbSet = _dBContext.GetDbSet<userEntity>();
        var num_exist = await DbSet.AnyAsync(t => t.user_num == viewModel.user_name);
        if (num_exist)
        {
            return (false, _stringLocalizer["username_existed"]);
        }
        var entity = viewModel.Adapt<userEntity>();
        var time = DateTime.UtcNow;
        entity.user_num = entity.user_name;
        entity.Id = 0;
        entity.auth_string = viewModel.auth_string;
        entity.create_time = time;
        entity.last_update_time = time;
        entity.email = viewModel.email;
        entity.sex = viewModel.sex;
        entity.is_valid = true;
        await DbSet.AddAsync(entity);
        await _dBContext.SaveChangesAsync();
        if (entity.Id > 0)
        {
            var tenant_id = entity.Id;

            #region menus

            var menus = new List<MenuEntity>
            {
                new() {
                    menu_name = "companySetting",
                    module = "baseModule",
                    vue_path = "companySetting",
                    vue_path_detail = "",
                    vue_directory = "base/companySetting",
                    sort = 1,
                    tenant_id = tenant_id
                },
                new() {
                    menu_name = "userRoleSetting",
                    module = "baseModule",
                    vue_path = "userRoleSetting",
                    vue_path_detail = "",
                    vue_directory = "base/userRoleSetting",
                    sort = 2,
                    tenant_id = tenant_id
                },
                new() {
                    menu_name = "roleMenu",
                    module = "baseModule",
                    vue_path = "roleMenu",
                    vue_path_detail = "",
                    vue_directory = "base/roleMenu",
                    sort = 3,
                    tenant_id = tenant_id
                },
                new() {
                    menu_name = "userManagement",
                    module = "baseModule",
                    vue_path = "userManagement",
                    vue_path_detail = "",
                    vue_directory = "base/userManagement",
                    sort = 4,
                    tenant_id = tenant_id
                },
                new() {
                    menu_name = "commodityCategorySetting",
                    module = "baseModule",
                    vue_path = "commodityCategorySetting",
                    vue_path_detail = "",
                    vue_directory = "base/commodityCategorySetting",
                    sort = 5,
                    tenant_id = tenant_id
                },
                new() {
                    menu_name = "commodityManagement",
                    module = "baseModule",
                    vue_path = "commodityManagement",
                    vue_path_detail = "",
                    vue_directory = "base/commodityManagement",
                    sort = 6,
                    tenant_id = tenant_id
                },
                new() {
                    menu_name = "supplier",
                    module = "baseModule",
                    vue_path = "supplier",
                    vue_path_detail = "",
                    vue_directory = "base/supplier",
                    sort = 7,
                    tenant_id = tenant_id
                },
                new() {
                    menu_name = "warehouseSetting",
                    module = "baseModule",
                    vue_path = "warehouseSetting",
                    vue_path_detail = "",
                    vue_directory = "base/warehouseSetting",
                    sort = 8,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "ownerOfCargo",
                    module = "baseModule",
                    vue_path = "ownerOfCargo",
                    vue_path_detail = "",
                    vue_directory = "base/ownerOfCargo",
                    sort = 9,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "freightSetting",
                    module = "baseModule",
                    vue_path = "freightSetting",
                    vue_path_detail = "",
                    vue_directory = "base/freightSetting",
                    sort = 10,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "customer",
                    module = "baseModule",
                    vue_path = "customer",
                    vue_path_detail = "",
                    vue_directory = "base/customer",
                    sort = 11,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "print",
                    module = "baseModule",
                    vue_path = "print",
                    vue_path_detail = "",
                    vue_directory = "base/print",
                    sort = 12,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "stockManagement",
                    module = "statisticAnalysis ",
                    vue_path = "stockManagement",
                    vue_path_detail = "",
                    vue_directory = "wms/stockManagement",
                    sort = 3,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "saftyStock",
                    module = "statisticAnalysis ",
                    vue_path = "saftyStock",
                    vue_path_detail = "",
                    vue_directory = "statisticAnalysis/saftyStock",
                    sort = 4,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "asnStatistic",
                    module = "statisticAnalysis ",
                    vue_path = "asnStatistic",
                    vue_path_detail = "",
                    vue_directory = "statisticAnalysis/asnStatistic",
                    sort = 5,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "deliveryStatistic",
                    module = "statisticAnalysis ",
                    vue_path = "deliveryStatistic",
                    vue_path_detail = "",
                    vue_directory = "statisticAnalysis/deliveryStatistic",
                    sort = 6,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "stockageStatistic",
                    module = "statisticAnalysis ",
                    vue_path = "stockageStatistic",
                    vue_path_detail = "",
                    vue_directory = "statisticAnalysis/stockageStatistic",
                    sort = 7,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "warehouseProcessing",
                    module = "warehouseWorkingModule",
                    vue_path = "warehouseProcessing",
                    vue_path_detail = "",
                    vue_directory = "warehouseWorking/warehouseProcessing",
                    sort = 4,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "warehouseMove",
                    module = "warehouseWorkingModule",
                    vue_path = "warehouseMove",
                    vue_path_detail = "",
                    vue_directory = "warehouseWorking/warehouseMove",
                    sort = 5,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "warehouseFreeze",
                    module = "warehouseWorkingModule",
                    vue_path = "warehouseFreeze",
                    vue_path_detail = "",
                    vue_directory = "warehouseWorking/warehouseFreeze",
                    sort = 6,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "warehouseAdjust",
                    module = "warehouseWorkingModule",
                    vue_path = "warehouseAdjust",
                    vue_path_detail = "",
                    vue_directory = "warehouseWorking/warehouseAdjust",
                    sort = 7,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "warehouseTaking",
                    module = "warehouseWorkingModule",
                    vue_path = "warehouseTaking",
                    vue_path_detail = "",
                    vue_directory = "warehouseWorking/warehouseTaking",
                    sort = 8,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "stockAsn",
                    module = "",
                    vue_path = "stockAsn",
                    vue_path_detail = "",
                    vue_directory = "wms/stockAsn",
                    sort = 2,
                    tenant_id = tenant_id
                },new MenuEntity
                {
                    menu_name = "deliveryManagement",
                    module = "",
                    vue_path = "deliveryManagement",
                    vue_path_detail = "",
                    vue_directory = "deliveryManagement/deliveryManagement",
                    sort = 5,
                    tenant_id = tenant_id
                }
                ,new MenuEntity
                {
                    menu_name = "largeScreen",
                    module = "",
                    vue_path = "largeScreen",
                    vue_path_detail = "",
                    vue_directory = "largeScreen/largeScreen",
                    sort = 6,
                    tenant_id = tenant_id
                }
            };

            #endregion menus

            entity.tenant_id = tenant_id;
            entity.creator = entity.user_name;
            entity.user_role = "admin";
            var adminrole = new UserroleEntity { is_valid = true, last_update_time = time, create_time = time, role_name = "admin", tenant_id = tenant_id };
            await _dBContext.GetDbSet<UserroleEntity>().AddAsync(adminrole);
            await _dBContext.GetDbSet<MenuEntity>().AddRangeAsync(menus);
            await _dBContext.SaveChangesAsync();
            foreach (var menu in menus)
            {
                await _dBContext.GetDbSet<RolemenuEntity>().AddAsync(new RolemenuEntity
                {
                    userrole_id = adminrole.Id,
                    authority = 1,
                    menu_id = menu.Id,
                    tenant_id = tenant_id,
                    last_update_time = time,
                    create_time = time,
                });
            }
            await _dBContext.SaveChangesAsync();
            return (true, _stringLocalizer["operation_success"]);
        }
        else
        {
            return (false, _stringLocalizer["operation_failed"]);
        }
    }

    /// <summary>
    /// Get All user Details
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<UserDetailDTO>> GetAllDetailsAsync(CurrentUser currentUser)
    {
        var query = _dBContext.GetDbSet<userEntity>()
                .AsNoTracking();
        query = query.Where(t => string.IsNullOrEmpty(t.integration_app));

        var curUserRole = currentUser.user_role;
        if (UserRoleDef.IsSystemAdministrator(curUserRole))
        {
            return await query
                .OrderByDescending(x => x.is_valid)
                .ThenBy(x => x.user_name)
                .Select(t => new UserDetailDTO
                {
                    Id = t.Id,
                    Username = t.user_name ?? "",
                    Email = t.email ?? "",
                    Role = t.user_role ?? "",
                    IsActive = t.is_valid,
                    DisplayName = t.user_num ?? "",
                    CellPhone = t.contact_tel ?? "",
                    CanActive = true,
                    CanDelete = true
                }).ToListAsync();
        }

        query = query.Where(x => x.tenant_id == currentUser.tenant_id);

        if (UserRoleDef.IsAdminRole(curUserRole))
        {
            return await query
                .OrderByDescending(x => x.is_valid)
                .ThenBy(x => x.user_name)
                .Select(t => new UserDetailDTO
                {
                    Id = t.Id,
                    Username = t.user_name ?? "",
                    Email = t.email ?? "",
                    Role = t.user_role ?? "",
                    IsActive = t.is_valid,
                    CellPhone = t.contact_tel ?? "",
                    DisplayName = $"{t.user_num}",
                    CanActive = true,
                }).ToListAsync();
        }

        var userName = currentUser.user_name;
        query = query.Where(x => x.user_name == userName || x.creator == userName);

        return await query
            .OrderByDescending(x => x.is_valid)
            .ThenBy(x => x.user_name)
            .Select(t => new UserDetailDTO
            {
                Id = t.Id,
                Username = t.user_name ?? "",
                Email = t.email ?? "",
                Role = t.user_role ?? "",
                IsActive = t.is_valid,
                CellPhone = t.contact_tel ?? "",
                DisplayName = $"{t.user_num}",
                CanActive = t.user_name == userName || t.creator == userName
            })
            .ToListAsync() ?? [];
    }

    /// <summary>
    /// Create User
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<(int id, string msg)> CreateUser(CreateUserViewModel viewModel,
        CurrentUser currentUser)
    {
        var dbSet = _dBContext.GetDbSet<userEntity>();
        //TODO : GenarationHelper.GetRandomPassword()
        var new_auth = SystemDefine.DefaultPassword;

        var entity = new userEntity
        {
            Id = 0,
            user_name = viewModel.UserName,
            user_num = viewModel.DisplayName,
            email = viewModel.Email,
            contact_tel = viewModel.ContactTel,
            creator = currentUser.user_name,
            is_valid = false,
            user_role = UserRoleDef.Guest,
            auth_string = Md5Helper.Md5Encrypt32(new_auth),
            create_time = DateTime.UtcNow,
            last_update_time = DateTime.UtcNow,
            tenant_id = currentUser.tenant_id
        };

        dbSet.Add(entity);
        var rs = await _dBContext.SaveChangesAsync();
        if (rs > 0)
        {
            await _actionLogService.AddLogAsync(
                $"[Create User] user {entity.Id} Username {entity.user_name} just requesting by {currentUser.user_name}",
                "User", currentUser);

            return (entity.Id, new_auth);
        }
        else
        {
            return (0, _stringLocalizer["save_failed"]);
        }
    }

    /// <summary>
    /// Active User
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ActiveUser(int userId, CurrentUser currentUser)
    {
        var query = GetQueryUserByUser(currentUser, true);
        var userInfo = await query.FirstOrDefaultAsync(x => x.Id == userId);
        if (userInfo == null)
        {
            return (false, _stringLocalizer["save_failed"]);
        }

        userInfo.is_valid = true;
        if (userInfo.user_role == UserRoleDef.Guest)
        {
            userInfo.user_role = UserRoleDef.Operator;
        }

        _dBContext.GetDbSet<userEntity>().Update(userInfo);
        var rs = await _dBContext.SaveChangesAsync();
        if (rs > 0)
        {
            await _actionLogService.AddLogAsync(
                $"[Active User] user {userInfo.Id} Username {userInfo.user_name} just actived by {currentUser.user_name}",
                "User", currentUser);
            return (true, _stringLocalizer["save_success"]);
        }

        return (false, _stringLocalizer["save_failed"]);
    }

    /// <summary>
    /// De Active User
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> DeActiveUser(int userId, CurrentUser currentUser)
    {
        var query = GetQueryUserByUser(currentUser, true);
        var userInfo = await query.FirstOrDefaultAsync(x => x.Id == userId);

        if (userInfo == null)
        {
            return (false, _stringLocalizer["save_failed"]);
        }

        userInfo.is_valid = false;
        _dBContext.GetDbSet<userEntity>().Update(userInfo);
        var rs = await _dBContext.SaveChangesAsync();
        if (rs > 0)
        {
            await _actionLogService.AddLogAsync(
                $"[InActive] user {currentUser.user_name} just in-actived Username {userInfo.user_name} ({userInfo.Id})",
                "User", currentUser);
            return (true, _stringLocalizer["save_success"]);
        }

        return (false, _stringLocalizer["save_failed"]);
    }

    /// <summary>
    /// Update User Info
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> UpdateUserInfo(int userId, UserDetailDTO request, CurrentUser currentUser)
    {
        var curUserRole = currentUser.user_role;
        bool canUpdate = UserRoleDef.IsSystemAdministrator(curUserRole);
        var dbSet = _dBContext.GetDbSet<userEntity>();
        var query = dbSet.Where(x => x.Id == userId);
        bool canChangeRole = canUpdate;

        if (!canUpdate)
        {
            canUpdate = UserRoleDef.IsAdminRole(curUserRole);
            canChangeRole = canChangeRole || canUpdate;
            query = _dBContext.GetDbSet<userEntity>()
                .Where(x => x.tenant_id == currentUser.tenant_id);

            if (!canUpdate)
            {
                string userName = currentUser.user_name;
                query = query.Where(x => x.user_name == userName || x.creator == userName);
            }
        }

        var userInfo = await query.FirstOrDefaultAsync(x => x.Id == userId);
        if (userInfo == null)
        {
            return (false, _stringLocalizer["save_failed"]);
        }

        if (canChangeRole && userInfo.user_role != request.Role)
        {
            userInfo.user_role = request.Role;
        }

        userInfo.contact_tel = request.CellPhone;
        userInfo.email = request.Email;
        userInfo.user_num = request.DisplayName;

        dbSet.Update(userInfo);
        var rs = await _dBContext.SaveChangesAsync();
        if (rs > 0)
        {
            await _actionLogService.AddLogAsync(
                $"[Update] user {userId} Username {userInfo.user_name} just Updated by {currentUser.user_name}",
                "User", currentUser);

            return (true, _stringLocalizer["save_success"]);
        }

        return (false, _stringLocalizer["save_failed"]);
    }

    /// <summary>
    /// Reset User Password
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ResetUserPasswordAsync(int userId, CurrentUser currentUser)
    {
        var query = GetQueryUserByUser(currentUser, true);
        var userInfo = await query.FirstOrDefaultAsync(x => x.Id == userId);
        if (userInfo == null)
        {
            return (false, _stringLocalizer["save_failed"]);
        }

        //var new_auth = GenarationHelper.GetRandomPassword();
        userInfo.auth_string = Md5Helper.Md5Encrypt32(SystemDefine.DefaultPassword);
        userInfo.last_update_time = DateTime.UtcNow;

        _dBContext.GetDbSet<userEntity>().Update(userInfo);
        var rs = await _dBContext.SaveChangesAsync();
        if (rs > 0)
        {
            await _actionLogService.AddLogAsync(
                $"[Reset Password] user {userId} Username {userInfo.user_name} just reset password to default by {currentUser.user_name}",
                "User", currentUser);

            return (true, _stringLocalizer["save_success"]);
        }

        return (false, _stringLocalizer["save_failed"]);
    }

    #endregion Api

    private IQueryable<userEntity> GetQueryUserByUser(CurrentUser currentUser, bool isTracking = false)
    {
        var curUserRole = currentUser.user_role;
        bool isSysAdmin = UserRoleDef.IsSystemAdministrator(curUserRole);
        var query = isTracking ?
            _dBContext.GetDbSet<userEntity>() :
            _dBContext.GetDbSet<userEntity>().AsNoTracking();

        query = query.Where(t => string.IsNullOrEmpty(t.integration_app));

        if (isSysAdmin)
        {
            return query;
        }

        query = query.Where(x => x.tenant_id == currentUser.tenant_id);
        bool isAdmin = UserRoleDef.IsAdminRole(curUserRole);
        if (isAdmin)
        {
            return query;
        }

        string userName = currentUser.user_name;

        return query.Where(x => x.user_name == userName || x.creator == userName);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="username"></param>
    /// <param name="curPassword"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    public async Task<(bool flag, string? msg)> ChangePassword(CurrentUser currentUser,
        string username, string curPassword, string newPassword)
    {
        var query = GetQueryUserByUser(currentUser, true);
        var userInfo = await query.FirstOrDefaultAsync(x => x.user_name == username);
        if (userInfo == null)
        {
            return (false, _stringLocalizer["save_failed"]);
        }

        //var new_auth = GenarationHelper.GetRandomPassword();
        bool isUser = userInfo.auth_string == Md5Helper.Md5Encrypt32(curPassword);
        if (isUser)
        {
            userInfo.auth_string = Md5Helper.Md5Encrypt32(newPassword);
            userInfo.last_update_time = DateTime.UtcNow;

            _dBContext.GetDbSet<userEntity>().Update(userInfo);
            var rs = await _dBContext.SaveChangesAsync();
            if (rs > 0)
            {
                await _actionLogService.AddLogAsync(
                    $"[Change Password] user {userInfo.Id} Username {userInfo.user_name} just changed new password by {currentUser.user_name}",
                    "User", currentUser);
                return (true, _stringLocalizer["save_success"]);
            }
        }

        return (false, _stringLocalizer["save_failed"]);
    }

    /// <summary>
    /// Get Integration WCS Async
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<IntegrationWcsInfo> GetIntegrationWCSAsync(CurrentUser currentUser)
    {
        var curUserRole = currentUser.user_role;
        var query = _dBContext.GetDbSet<userEntity>().AsNoTracking();
        bool isAdmin = UserRoleDef.IsAdminRole(curUserRole);
        if (!isAdmin)
        {
            return new IntegrationWcsInfo();
        }

        query = query.Where(x => x.tenant_id == currentUser.tenant_id);
        var entity = await query.FirstOrDefaultAsync(x => x.user_name == "WCS");
        var url = _configuration["WcsSettings:ApiUrl"];
        return new IntegrationWcsInfo
        {
            ApiUrl = url ?? "",
            AppKey = entity != null ? entity.integration_app ?? "" : ""
        };
    }

    /// <summary>
    /// Update Integration WCS
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<(bool isSuccess, string? message)> UpdateIntegrationWCS(CurrentUser currentUser, IntegrationWcsInfo request)
    {
        var curUserRole = currentUser.user_role;
        bool isAdmin = UserRoleDef.IsAdminRole(curUserRole);
        if (!isAdmin)
        {
            return (false, _stringLocalizer["save_failed"]);
        }
        var query = _dBContext.GetDbSet<userEntity>()
            .Where(x => x.tenant_id == currentUser.tenant_id);
        var userInfo = await query.FirstOrDefaultAsync(x => x.user_name == "WCS");
        if (userInfo == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }

        await _actionLogService.AddLogAsync(
            $"[Integration WCS] {currentUser.user_id} Username {currentUser.user_name} just requesting change integration app {userInfo.integration_app} to new API key",
            "User", currentUser);

        userInfo.integration_app = request.AppKey;
        userInfo.last_update_time = DateTime.UtcNow;

        _dBContext.GetDbSet<userEntity>().Update(userInfo);
        var rs = await _dBContext.SaveChangesAsync();
        if (rs > 0)
        {
            TryUpdateConfiguration(request.ApiUrl, "appsettings.json");
            TryUpdateConfiguration(request.ApiUrl, "appsettings.Staging.json");
            TryUpdateConfiguration(request.ApiUrl, "appsettings.Production.json");
            return (true, _stringLocalizer["save_success"]);
        }

        return (false, _stringLocalizer["save_failed"]);
    }

    private void TryUpdateConfiguration(string apiUrl, string filePath)
    {
        try
        {
            var url = _configuration["WcsSettings:ApiUrl"];
            // Đọc nội dung file
            string json = File.ReadAllText(filePath);
            var jObject = JObject.Parse(json);

            // Lấy giá trị hiện tại
            string currentApiUrl = jObject["WcsSettings"]["ApiUrl"].ToString();
            Console.WriteLine("Giá trị hiện tại ApiUrl: " + currentApiUrl);

            // Cập nhật giá trị mới
            string newApiUrl = apiUrl;
            jObject["WcsSettings"]["ApiUrl"] = newApiUrl;
            // Ghi lại file
            File.WriteAllText(filePath, jObject.ToString());
        }
        catch
        {

        }
    }
}