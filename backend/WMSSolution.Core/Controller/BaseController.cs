
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Utility;
using WMSSolution.Shared.RBAC;

namespace WMSSolution.Core.Controller;

/// <summary>
/// base controller
/// </summary>
[Authorize]
[Produces("application/json")]
public class BaseController : ControllerBase
{
    /// <summary>
    /// current user
    /// </summary>
    public CurrentUser CurrentUser
    {
        get
        {
            if (User != null && User.Claims.ToList().Count > 0)
            {
                var Claim = User.Claims.First(claim => claim.Type == ClaimValueTypes.Json);
                return Claim == null ? new CurrentUser() : JsonHelper.DeserializeObject<CurrentUser>(Claim.Value);
            }
            else
            {
                return new CurrentUser();
            }
        }
    }

    /// <summary>
    /// My Client Environment
    /// </summary>
    public ClientEnvironment MyClientEnvironment
    {
        get
        {
            return new ClientEnvironment
            {
                UserName = CurrentUser.user_name,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                //ScreenWidth = Request.Headers["ScreenWidth"].FirstOrDefault() ?? string.Empty,
                //ScreenHeight = Request.Headers["ScreenHeight"].FirstOrDefault() ?? string.Empty,
                BrowserInfo = Request.Headers["BrowserInfo"].FirstOrDefault() ?? string.Empty,
                BrowserVersion = Request.Headers["BrowserVersion"].FirstOrDefault() ?? string.Empty,
                OSName = Request.Headers["OSName"].FirstOrDefault() ?? string.Empty,
                ActionName = Request.Path
            };
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public BaseController()
    {
    }
}
