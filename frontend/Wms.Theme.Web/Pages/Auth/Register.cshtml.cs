using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Wms.Theme.Web.Services.Authen;
using WMSSolution.Shared.RBAC;

namespace Wms.Theme.Web.Pages.Auth;

public class RegisterModel(IAuthenService service, ILogger<RegisterModel> logger) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "UsernameRequired")]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;
    [BindProperty]
    [Required(ErrorMessage = "DisplayNameRequired")]
    [Display(Name = "DisplayName")]
    public string DisplayName { get; set; } = "";
    [BindProperty]
    [Required(ErrorMessage = "CompanyNameRequired")]
    [Display(Name = "CompanyName")]
    public string CompanyName { get; set; } = "";
    [BindProperty]
    [Required(ErrorMessage = "ContactNumberRequired")]
    [Display(Name = "ContactNumber")]
    public string ContactNumber { get; set; } = "";    

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

    private readonly IAuthenService _authenService = service;
    private readonly ILogger<RegisterModel> _logger = logger;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? HttpContext.Connection.RemoteIpAddress?.ToString();

            ClientEnvironment environment = new()
            {
                UserName = UserName,
                IPAddress = ipAddress ?? "",
                BrowserInfo = BrowserInfo ?? "",
                BrowserVersion = BrowserVersion ?? "",
                OSName = OSName ?? "",
                ScreenHeight = ScreenHeight ?? "",
                ScreenWidth = ScreenWidth ?? "",
                ActionName = "Register"
            };

            RegisterRequest request = new()
            {
                UserName = UserName, 
                DisplayName = DisplayName,
                ContactNumber = ContactNumber,
                CompanyName = CompanyName,
                Environment = environment
            };

            bool isRegisted = await _authenService.RegisterAsync(request);
            return isRegisted ? RedirectToPage("/Auth/Index") : Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error register user: {username}", UserName);
            return Page();
        }
    }
}
