using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Wms.Theme.Web.Services.Authen;

namespace Wms.Theme.Web.Pages.Auth;

public class ChangePasswordModel(IAuthenService authenService,
    ILogger<IndexModel> logger) : PageModel
{
    private readonly IAuthenService _authenService = authenService;
    private readonly ILogger<IndexModel> _logger = logger;

    /// <summary>
    /// Input
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel
    {
        /// <summary>
        /// UserName
        /// </summary>
        public string UserName { get; set; } = "";

        /// <summary>
        /// Password
        /// </summary>
        [Required]
        public string Password { get; set; } = "";

        [Required]
        public string NewPassword { get; set; } = "";

        //[Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.NewPassword != Input.ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match.");
            return Page();
        }

        if(string.IsNullOrEmpty(Input.UserName))
        {
            Input.UserName = $"{TempData.Peek("UserName")}";
        }

        // TODO: Verify reset token, update password in database
        var (id, message) = await _authenService.ChangePassword(Input.UserName, Input.Password, Input.NewPassword);
        if (id == 0)
        {
            ModelState.AddModelError(string.Empty, message); 
            return Page();
        }

        TempData["Message"] = "Password has been reset successfully.";
        // Sign out from the Cookie Authentication Scheme
        await HttpContext.SignOutAsync("MyCookieAuth");

        // Explicitly delete the access_token cookie if it exists
        if (Request.Cookies.ContainsKey("access_token"))
        {
            Response.Cookies.Delete("access_token");
        }

        // Redirect to Login page
        return RedirectToPage("/Auth/Index");

        //// Optionally sign the user out so they log in with the new password
        //await _signInManager.SignOutAsync();

        //// Redirect to Login page
        //return RedirectToPage("/Auth/Login");
    }

}
