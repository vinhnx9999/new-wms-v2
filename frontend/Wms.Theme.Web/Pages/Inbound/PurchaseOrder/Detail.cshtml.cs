using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.PurchaseOrder;
using Wms.Theme.Web.Services.PurchaseOrder;
using Wms.Theme.Web.Services.Stock;
using Wms.Theme.Web.Services.Supplier;

namespace Wms.Theme.Web.Pages.PurchaseOrder;

public class DetailModel(IPurchaseOrderService poService, ISupplierService supplierService, IStockService stockService) : PageModel
{
    private readonly IPurchaseOrderService _poService = poService;
    private readonly ISupplierService _supplierService = supplierService;
    private readonly IStockService _stockService = stockService;

    [BindProperty]
    public PoDetailDto Model { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _poService.GetDetailAsync(id);
        if (result.IsSuccess && result.Data != null)
        {
            Model = result.Data;
            return Page();
        }
        return RedirectToPage("/PurchaseOrder/Index");
    }

}
