using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Sku;
using Wms.Theme.Web.Services.Warehouse;

namespace Wms.Theme.Web.Pages.Warehouse.Rules;

public class SafetyStockConfigModel(IWarehouseService service) : PageModel
{
    private readonly IWarehouseService _service = service;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public IEnumerable<SkuSafetyStockDto> SkuSafetyStocks { get; set; } = [];

    public async Task OnGet()
    {
        
    }


}
