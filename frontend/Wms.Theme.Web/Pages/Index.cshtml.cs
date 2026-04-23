using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WMSSolution.Shared.RBAC;

namespace Wms.Theme.Web.Pages;

public class IndexModel(IConfiguration configuration) : PageModel
{
    //private readonly IHttpContextAccessor _httpContext = httpContext;
    private readonly IConfiguration _configuration = configuration;

    public IActionResult OnGet()
    {
        // Simple check for access_token cookie
        if (Request.Cookies.TryGetValue("access_token", out var accessToken) && !string.IsNullOrEmpty(accessToken))
        {
            string userName = $"{TempData.Peek("UserName")}" ?? "UnknownUser";
            string userRole = $"{TempData.Peek("UserRole")}" ?? "Guest"; 
            string displayName = $"{TempData.Peek("DisplayName")}";
            string tenantId = $"{TempData.Peek("TenantId")}";

            if (string.IsNullOrEmpty(userName))
            {
                var cookies = HttpContext.Request.Cookies;
                userName = $"{cookies["user_name"]}";
                userRole = $"{cookies["user_role"]}";
                displayName = $"{cookies["user_num"]}";
                tenantId = $"{cookies["tenant_id"]}";

                TempData["UserName"] = userName;
                TempData["DisplayName"] = displayName;
                TempData["UserRole"] = userRole;
                TempData["TenantId"] = tenantId;

                // keep it alive 
                TempData.Keep("UserName");
                TempData.Keep("UserRole");
                TempData.Keep("DisplayName");                
                TempData.Keep("TenantId");
            }

            string txtShowVendors = "";
            var ownerId = $"{_configuration["Ownership:TenantId"]}";
            if (userRole == UserRoleDef.SystemAdministrator)
            {
                txtShowVendors = "ShowVendors";
            }
            else if (userRole == UserRoleDef.ShowVendors)
            {
                txtShowVendors = ownerId == tenantId ? "ShowVendors" : "";
            }
            else if (userRole == UserRoleDef.Admin)
            {                
                txtShowVendors = ownerId == tenantId ? "ShowVendors" : "";
            }

            TempData["ShowVendors"] = txtShowVendors;
            TempData.Keep("ShowVendors");
            return RedirectToPage("/Dashboard/Index");
        }
        else
        {
            // User is not authenticated, redirect to login
            return RedirectToPage("/Auth/Index");
        }
    }
}

