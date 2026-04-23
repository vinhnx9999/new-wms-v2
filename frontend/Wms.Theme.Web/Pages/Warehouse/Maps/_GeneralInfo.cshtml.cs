using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wms.Theme.Web.Pages.Warehouse.Maps;

public class _GeneralInfoModel : PageModel
{
    //[BindProperty(SupportsGet = true)]
    //public int Id { get; set; }

    public void OnGet()
    {
        Console.WriteLine($"Warehouse Map ");
    }
}
