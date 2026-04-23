using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wms.Theme.Web.Pages;

public class SetLanguageModel : PageModel
{
    public IActionResult OnGet(string culture, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = "vi-VN";
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            returnUrl = Url.Content("~/");
        }

        return LocalRedirect(returnUrl);
    }
}

