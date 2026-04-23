using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using Wms.Theme.Web.Services.Authen;
using WMSSolution.Shared.RBAC;

namespace Wms.Theme.Web.Pages.Auth;

public class IndexModel(IAuthenService service, 
    ILogger<IndexModel> logger, IStringLocalizer<SharedResource> localizer) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "UsernameRequired")]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "PasswordRequired")]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }
    
    private readonly IAuthenService _authenService = service;
    private readonly ILogger<IndexModel> _logger = logger;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

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

    public void OnGet()
    {
        // Clear any existing form data
        UserName = string.Empty;
        Password = string.Empty;
        RememberMe = false;
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            //var browserInfo = HttpContext.Request.Headers["User-Agent"].ToString();
            var ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? HttpContext.Connection.RemoteIpAddress?.ToString();

            ViewData["UserName"] = ""; // Clear username display on error
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
            {
                TempData["ErrorMessage"] = _localizer["LoginErrorMissingInfo"].Value;
                return Page();
            }

            ClientEnvironment environment = new()
            {
                UserName = UserName,
                IPAddress = ipAddress ?? "",
                BrowserInfo = BrowserInfo ?? "",
                BrowserVersion = BrowserVersion ?? "",
                OSName = OSName ?? "",
                ScreenHeight = ScreenHeight ?? "",
                ScreenWidth = ScreenWidth ?? "",
                ActionName = "Login"
            };

            var loginResponse = await _authenService.LoginAsync(UserName, Password, environment);

            if (loginResponse != null && loginResponse.Data != null)
            {
                // Save authentication data to cookies
                SaveAuthenticationCookies(loginResponse.Data);                
                TempData["SuccessMessage"] = _localizer["LoginSuccess"].Value;
                
                return RedirectToPage("/Dashboard/Index");
            }
            else
            {
                TempData["ErrorMessage"] = _localizer["LoginErrorInvalidCredentials"].Value;
                _logger.LogWarning("Login failed for user: {Username}. API Error: {error}", UserName, loginResponse?.ErrorMessage);
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", UserName);
            TempData["ErrorMessage"] = _localizer["LoginErrorGeneric"].Value;
            return Page();
        }
    }

    // private methods to handle cookie saving
    private void SaveAuthenticationCookies(Services.Authen.LoginData loginData)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(loginData.Expire)
        };

        // Save essential user data to cookies
        Response.Cookies.Append("access_token", loginData.Access_token, cookieOptions);
        Response.Cookies.Append("refresh_token", loginData.Refresh_token, cookieOptions);
        Response.Cookies.Append("user_id", loginData.User_id.ToString(), cookieOptions);
        Response.Cookies.Append("user_name", loginData.User_name, cookieOptions);
        Response.Cookies.Append("user_num", loginData.User_num, cookieOptions);
        Response.Cookies.Append("user_role", loginData.User_role, cookieOptions);
        Response.Cookies.Append("tenant_id", loginData.Tenant_id.ToString(), cookieOptions);

        TempData["DisplayName"] = loginData.User_num;
        TempData["UserName"] = loginData.User_name;
        TempData["UserRole"] = loginData.User_role;

        TempData.Keep("UserName"); // keep it alive
        TempData.Keep("UserRole"); // keep it alive
        TempData.Keep("DisplayName");

        _logger.LogInformation("Authentication data saved for user: {Username} with role: {Role}",
            loginData.User_name, loginData.User_role);
    }
}
