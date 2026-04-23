using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Shared.RBAC;

namespace WMSSolution.Core.Controller;
/// <summary>
/// account
/// </summary>
/// <remarks>
/// Structure
/// </remarks>
/// <param name="logger">logger helper</param>
/// <param name="tokenManager">token manger</param>
/// <param name="cacheManager">cache helper</param>
/// <param name="accountService">account service class</param>
/// <param name="stringLocalizer">Localizer</param>
[Route("[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "Base")]
public class AccountController(ILogger<AccountController> logger
        , ITokenManager tokenManager
        , CacheManager cacheManager
        , IAccountService accountService
        , IStringLocalizer stringLocalizer
        ) : BaseController
{
    /// <summary>
    /// token manger
    /// </summary>
    private readonly ITokenManager _tokenManager = tokenManager;
    /// <summary>
    /// Log helper
    /// </summary>
    private readonly ILogger<AccountController> _logger = logger;

    /// <summary>
    /// cache helper
    /// </summary>
    private readonly CacheManager _cacheManager = cacheManager;

    /// <summary>
    /// account service class
    /// </summary>
    private readonly IAccountService _accountService = accountService;

    /// <summary>
    /// Localizer
    /// </summary>
    private readonly IStringLocalizer _stringLocalizer = stringLocalizer;

    #region Login

    /// <summary>
    /// audit-action
    /// </summary>
    /// <param name="clientAction"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("/audit-action")]
    public async Task AuditUser([FromBody] ClientEnvironment clientAction)
    {
        await _accountService.AuditUserAction(clientAction);
    }
    /// <summary>
    /// login
    /// </summary>
    /// <param name="loginAccount">user's account infomation</param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("/login")]
    public async Task<ResultModel<LoginOutputViewModel>> LoginAsync(LoginInputViewModel loginAccount)
    {
        var user = await _accountService.Login(loginAccount);
        if (user != null)
        {
            var (token, expire) = _tokenManager.GenerateToken(
                new CurrentUser
                {
                    user_id = user.user_id,
                    user_name = user.user_name,
                    user_num = user.user_num,
                    user_role = user.user_role,
                    tenant_id = user.tenant_id
                }, user.integration_app);

            string rt = this._tokenManager.GenerateRefreshToken();

            user.access_token = token;
            user.expire = expire;
            user.refresh_token = rt;

            await _cacheManager.TokenSet(user.user_id, "WebRefreshToken", rt, _tokenManager.GetRefreshTokenExpireMinute());

            return ResultModel<LoginOutputViewModel>.Success(user);
        }
        else
        {
            return ResultModel<LoginOutputViewModel>.Error(_stringLocalizer["login_failed"]);
        }
    }

    /// <summary>
    /// Register Async
    /// </summary>
    /// <param name="registerInput"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("/register")]
    public async Task<bool> RegisterAsync([FromBody] RegisterRequest registerInput)
    {
        return await _accountService.RegisterAsync(registerInput);
    }

    /// <summary>
    /// get a new token
    /// </summary>
    /// <param name="inPutViewModel">old access token and refreshtoken key</param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("/refresh-token")]
    public async Task<ResultModel<string>> RefreshToken([FromBody] RefreshTokenInPutViewModel inPutViewModel)
    {
        var currentUser = this._tokenManager.GetCurrentUser(inPutViewModel.AccessToken);

        var flag = _cacheManager.Is_Token_Exist<string>(currentUser.user_id, "WebRefreshToken", _tokenManager.GetRefreshTokenExpireMinute());
        if (!flag)
        {
            return ResultModel<string>.Error("refreshtoken_failure");
        }
        else
        {
            var (token, expire) = _tokenManager.GenerateToken(currentUser);
            return ResultModel<string>.Success(token);
        }
    }

    /// <summary>
    /// Get my info
    /// </summary>
    /// <returns></returns>
    //[AllowAnonymous]
    [HttpGet("/my-info")]
    public async Task<ResultModel<BaseUserInfo>> GetUserInfo()
    {
        var user = await _accountService.GetUserInfo(CurrentUser);
        if (user != null)
        {
            return ResultModel<BaseUserInfo>.Success(user);
        }
        else
        {
            return ResultModel<BaseUserInfo>.Error("");
        }
    }

    #endregion

}
