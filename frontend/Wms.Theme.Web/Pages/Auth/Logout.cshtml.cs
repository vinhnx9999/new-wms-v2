using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UAParser;
using Wms.Theme.Web.Services.Authen;


namespace Wms.Theme.Web.Pages.Auth;

public class LogoutModel(IAuthenService service) : PageModel
{
    private readonly IAuthenService _authenService = service;

    [BindProperty]
    public string? ScreenWidth { get; set; }
    [BindProperty]
    public string? ScreenHeight { get; set; }

    [BindProperty]
    public string? BrowserVersion { get; set; }
    [BindProperty]
    public string? BrowserInfo { get; set; }

    [BindProperty]
    public string? OSName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var ipAddress = GetMyIpAddress();                
            string userName = $"{TempData.Peek("UserName")}" ?? "UnknownUser";
            if (string.IsNullOrEmpty(userName))
            {
                var cookies = HttpContext.Request.Cookies;
                userName = $"{cookies["user_name"]}";
            }

            string browserInfo = BrowserInfo ?? "";
            string osInfo = OSName ?? "";
            string browserVersion = BrowserVersion ?? "";

            if (string.IsNullOrEmpty(browserInfo))
            {
                string userAgent = $"{HttpContext.Request.Headers.UserAgent}";
                var parser = Parser.GetDefault();
                var clientInfo = parser.Parse(userAgent);
                browserInfo = $"{clientInfo.UA.Family} {clientInfo.UA.Major}";
                osInfo = $"{clientInfo.OS.Family} {clientInfo.OS.Major}";
                browserVersion = $"{clientInfo.UA.Major}.{clientInfo.UA.Minor}.{clientInfo.UA.Patch}";
            }

            await _authenService.AuditUserAction(new()
            {
                UserName = userName,
                IPAddress = ipAddress ?? "",
                BrowserInfo = browserInfo ?? "",
                BrowserVersion = browserVersion ?? "",
                OSName = osInfo ?? "",
                ScreenHeight = ScreenHeight ?? "",
                ScreenWidth = ScreenWidth ?? "",
                ActionName = "Logout"
            });
        }
        catch
        {

        }

        // Sign out from the Cookie Authentication Scheme
        await HttpContext.SignOutAsync("MyCookieAuth");

        // Explicitly delete the access_token cookie if it exists
        if (Request.Cookies.ContainsKey("access_token"))
        {
            Response.Cookies.Delete("access_token");
        }

        // Redirect to Login page
        return RedirectToPage("/Auth/Index");
    }

    private string? GetMyIpAddress()
    {
        try
        {
            return HttpContext.Request.Headers["X-Forwarded-For"]
                .FirstOrDefault()
                ?? HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        catch
        {
            return "";
        }
    }
}
