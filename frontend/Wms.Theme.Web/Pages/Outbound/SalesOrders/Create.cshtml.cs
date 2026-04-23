using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Dispatch;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.Customer;
using Wms.Theme.Web.Services.Dispatch;
using Wms.Theme.Web.Services.Stock;

namespace Wms.Theme.Web.Pages.Outbound;

public class SalesOrdersCreateModel : PageModel
{
    private readonly ICustomerService _customerService;
    private readonly IStockService _stockService;
    private readonly IDispatchService _dispatchService;

    public SalesOrdersCreateModel(ICustomerService customerService, IStockService stockService, IDispatchService dispatchService)
    {
        _customerService = customerService;
        _stockService = stockService;
        _dispatchService = dispatchService;
    }

    [BindProperty]
    public List<DispatchListAddViewModel> DispatchAddRequest { get; set; } = new List<DispatchListAddViewModel>();

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnGetCustomerList()
    {
        var result = await _customerService.GetAllCustomersAsync();
        return result != null ? new JsonResult(result) : new JsonResult(new
        {
            message = "No customers found"
        });
    }

    public async Task<IActionResult> OnGetSkuSelected()
    {
        var result = await _stockService.GetSkuSelectAvailable(new PageSearchRequest
        {
            pageIndex = 1,
            pageSize = 100,
            sqlTitle = "",
            searchObjects = new List<SearchObject>(),
        });
        return result != null ? new JsonResult(result) : new JsonResult(new
        {
            message = "No SKUs found"
        });
    }

    public async Task<IActionResult> OnPostAddDispatch()
    {
        var response = await _dispatchService.AddNewDispatchList(DispatchAddRequest);
        if (response)
        {
            TempData["SuccessMessage"] = "Dispatch created successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to create dispatch.";
        }
        return RedirectToPage("/Outbound/SalesOrders/Index");
    }
}
