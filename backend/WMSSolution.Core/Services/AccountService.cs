using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Data;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Utility;
using WMSSolution.Shared;
using WMSSolution.Shared.RBAC;

namespace WMSSolution.Core.Services;

/// <summary>
/// AccountService
/// </summary>
/// <remarks>
/// Account Service
/// </remarks>
/// <param name="sqlDBContext"></param>
/// <param name="stringLocalizer"></param>
public class AccountService(SqlDBContext sqlDBContext,
    IStringLocalizer<MultiLanguage> stringLocalizer) : IAccountService
{
    private readonly SqlDBContext _sqlDBContext = sqlDBContext;
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    /// <summary>
    /// Get User Info
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<BaseUserInfo> GetUserInfo(CurrentUser currentUser)
    {
        var query = _sqlDBContext.GetDbSet<userEntity>()
                            .AsNoTracking()
                            .Where(t => t.is_valid && t.Id == currentUser.user_id)
                            .Select(x =>
                             new BaseUserInfo
                             {
                                 Id = x.Id,
                                 Username = x.user_name ?? "",
                                 Role = x.user_role ?? "",
                                 DisplayName = x.user_num ?? ""
                             });

        return await query.FirstOrDefaultAsync() ?? new();

    }

    /// <summary>
    /// login
    /// </summary>
    /// <param name="loginInput"> login params viewmodel</param>
    /// <returns></returns>
    public async Task<LoginOutputViewModel> Login(LoginInputViewModel loginInput)
    {
        var queryUser = _sqlDBContext.GetDbSet<userEntity>()
                            .AsNoTracking()
                            .Where(t => t.is_valid);
        var queryRole = _sqlDBContext.GetDbSet<UserroleEntity>()
                            .AsNoTracking();

        queryUser = queryUser.Where(t => t.user_name == loginInput.user_name
            || t.user_num == loginInput.user_name || t.integration_app == loginInput.user_name);

        var users = await (from user in queryUser
                           join ur in queryRole on user.user_role equals ur.role_name
                           select new
                           {
                               user_id = user.Id,
                               user_num = user.user_num ?? "",
                               user_name = user.user_name ?? "",
                               user_role = user.user_role ?? "",
                               userrole_id = ur.Id,
                               cipher = user.auth_string,
                               user.tenant_id,
                               integration_app = user.integration_app ?? ""
                           }
                       ).ToListAsync();

        var clientEnv = loginInput.Environment;
        var env = new ClientEnvironmentEntity()
        {
            Creator = loginInput.user_name,
            ActionName = "LoginFailed",
            BrowserInfo = clientEnv?.BrowserInfo,
            BrowserVersion = clientEnv?.BrowserVersion,
            IPAddress = clientEnv?.IPAddress,
            OSName = clientEnv?.OSName,
            ScreenHeight = clientEnv?.ScreenHeight.ObjToInt(),
            ScreenWidth = clientEnv?.ScreenWidth.ObjToInt(),
            UserName = loginInput.user_name,
            CreatedTime = DateTime.UtcNow,
        };

        if (!string.IsNullOrEmpty(loginInput.password))
        {
            string md5_password = Md5Helper.Md5Encrypt32(loginInput.password);
            var items = users.Where(t => t.cipher == md5_password || t.cipher == loginInput.password);
            var today = DateTime.UtcNow.Date;
            var tenantList = _sqlDBContext.GetDbSet<TenantEntity>()
                .AsNoTracking()
                .Where(x => x.ValidTo == null || x.ValidTo >= today);

            foreach (var item in items)
            {
                var tenant = await tenantList.FirstOrDefaultAsync(x => x.Id == item.tenant_id);
                if (tenant == null) continue;
                if (string.IsNullOrEmpty(item.integration_app))
                {
                    await TrySaveAuditUser(env, "Login Success");
                    return new LoginOutputViewModel()
                    {
                        user_id = item.user_id,
                        user_name = item.user_name,
                        user_num = item.user_num,
                        user_role = item.user_role,
                        userrole_id = item.userrole_id,
                        tenant_id = item.tenant_id
                    };
                }
            }

            await TrySaveAuditUser(env, "Login Failed");
        }
        else
        {
            var result = users.FirstOrDefault(t => t.integration_app == loginInput.user_name);
            if (result != null)
            {
                return new LoginOutputViewModel()
                {
                    user_id = result.user_id,
                    user_name = result.user_name,
                    user_num = result.user_num,
                    user_role = result.user_role,
                    userrole_id = result.userrole_id,
                    integration_app = result.integration_app,
                    tenant_id = result.tenant_id
                };
            }
        }

        await TrySaveAuditUser(env, "Access Denied");
        return null;
    }

    /// <summary>
    /// Audit User Action
    /// </summary>
    /// <param name="clientAction"></param>
    /// <returns></returns>
    public async Task AuditUserAction(ClientEnvironment clientAction)
    {
        var env = new ClientEnvironmentEntity()
        {
            Creator = clientAction.UserName,
            BrowserInfo = clientAction.BrowserInfo,
            BrowserVersion = clientAction.BrowserVersion,
            IPAddress = clientAction.IPAddress,
            OSName = clientAction.OSName,
            ScreenHeight = clientAction.ScreenHeight.ObjToInt(),
            ScreenWidth = clientAction.ScreenWidth.ObjToInt(),
            UserName = clientAction.UserName,
            CreatedTime = DateTime.UtcNow,
            ActionName = clientAction.ActionName
        };

        await TrySaveAuditUser(env);
    }


    private async Task TrySaveAuditUser(ClientEnvironmentEntity env, string actionName = "")
    {
        try
        {
            if (!string.IsNullOrEmpty(actionName)) env.ActionName = actionName;

            _sqlDBContext.GetDbSet<ClientEnvironmentEntity>().Add(env);
            await _sqlDBContext.SaveChangesAsync();
        }
        catch
        {

        }
    }

    /// <summary>
    /// Register 
    /// </summary>
    /// <param name="registerInput"></param>
    /// <returns></returns>
    public async Task<bool> RegisterAsync(RegisterRequest registerInput)
    {
        if (string.IsNullOrEmpty(registerInput.CompanyName) 
            || string.IsNullOrEmpty(registerInput.UserName)) 
            return false;

        var hasCompany = await _sqlDBContext.GetDbSet<TenantEntity>()
            .AsNoTracking()
            .AnyAsync(x => x.TenantName == registerInput.CompanyName);

        if (hasCompany) return false;

        var clientEnv = registerInput.Environment;
        var env = new ClientEnvironmentEntity()
        {
            Creator = registerInput.UserName,
            ActionName = "Register",
            BrowserInfo = clientEnv?.BrowserInfo,
            BrowserVersion = clientEnv?.BrowserVersion,
            IPAddress = clientEnv?.IPAddress,
            OSName = clientEnv?.OSName,
            ScreenHeight = clientEnv?.ScreenHeight.ObjToInt(),
            ScreenWidth = clientEnv?.ScreenWidth.ObjToInt(),
            UserName = registerInput.UserName,
            CreatedTime = DateTime.UtcNow,
        };

        await TrySaveAuditUser(env, "Register");

        var query = _sqlDBContext.GetDbSet<userEntity>()
                            .AsNoTracking()
                            .Where(t => t.user_name == registerInput.UserName);

        var existing = await query.AnyAsync();

        if (existing) return false;

        var tenant = new TenantEntity
        {
            TenantName = registerInput.CompanyName,
            DisplayName = registerInput.DisplayName,
            ContactNumber = registerInput.ContactNumber,
            CreatedDate = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(7),
        };

        _sqlDBContext.GetDbSet<TenantEntity>().Add(tenant);
        await _sqlDBContext.SaveChangesAsync();

        var auth_string = Md5Helper.Md5Encrypt32(SystemDefine.DefaultPassword);
        _sqlDBContext.GetDbSet<userEntity>().Add(new userEntity
        {
            user_name = registerInput.UserName,
            creator = registerInput.UserName,
            user_num = registerInput.DisplayName,
            is_valid = true,
            tenant_id = tenant.Id,
            user_role = UserRoleDef.Admin,
            auth_string = auth_string,
            create_time = DateTime.UtcNow,
            last_update_time = DateTime.UtcNow
        });

        return await _sqlDBContext.SaveChangesAsync() > 0;
    }
}
