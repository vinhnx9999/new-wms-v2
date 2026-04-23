using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Sku;
using Wms.Theme.Web.Services.Sku;

namespace Wms.Theme.Web.Pages.Setting.Sku;

public class CreateModel(ISkuService skuService) : PageModel
{
    private readonly ISkuService _skuService = skuService;

    [BindProperty(SupportsGet = true)]
    public SkuCreateRequest Sku { get; set; } = default!;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _skuService.CreateSkuAsync(Sku);
        
        return Page();
    }

}
