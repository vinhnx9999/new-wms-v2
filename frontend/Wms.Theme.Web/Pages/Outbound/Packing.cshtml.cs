using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Wms.Theme.Web.Components;
using Wms.Theme.Web.Model.Dispatch;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.Dispatch;

namespace Wms.Theme.Web.Pages.Outbound;

public class PackingModel : PageModel
{
    private readonly IDispatchService _dispatchService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    [BindProperty]
    public DataTableViewModel DataTableViewModel { get; set; } = new DataTableViewModel();
    public PackingModel(IDispatchService dispatchService, IStringLocalizer<SharedResource> localizer)
    {
        _dispatchService = dispatchService;
        _localizer = localizer;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetDataTable(int pageIndex = 1, string sales_order_no = "", string customer_name = "")
    {
        const int pageSize = 10;
        var searchObjects = new List<SearchObject>();
        if (!string.IsNullOrEmpty(sales_order_no))
        {
            searchObjects.Add(new SearchObject
            {
                Name = "sales_order_no",
                Operator = Operators.Contains,
                Text = sales_order_no,
                Value = sales_order_no,
            });

        }
        if (!string.IsNullOrEmpty(customer_name))
        {
            searchObjects.Add(new SearchObject
            {
                Name = "customer_name",
                Operator = Operators.Contains,
                Text = customer_name,
                Value = customer_name
            });
        }
        var result = await _dispatchService.GetDispatchList(new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            sqlTitle = "package",
            searchObjects = searchObjects
        });


        var rows = result?.Data?.Rows.Select((item, index) => new
        {
            id = item.Id, // Needed for actions
            dispatchNo = item.DispatchNo,
            customerName = item.CustomerName,
            dispatchStatus = item.DispatchStatus,
            qty = item.Qty,
            weight = item.Weight,
            volume = item.Volume,
            creator = item.Creator,
            pickedQty = item.PickedQty,
        }).ToList();

        return new JsonResult(new
        {
            status = true,
            data = new
            {
                rows = rows,
                total = result?.Data?.Totals ?? 0,
                pageIndex = pageIndex,
                totalPages = (int)Math.Ceiling((double)(result?.Data?.Totals ?? 0) / pageSize)
            }
        });
    }

    public async Task<IActionResult> OnPostConfirmPacked([FromBody] List<DispatchListPackageDTO> request)
    {
        var result = await _dispatchService.ConFirmDispatchHasPackage(request);
        if (result)
        {
            return new JsonResult(new { success = true, message = _localizer["PackSuccess"].Value });
        }

        return new JsonResult(new { success = false, message = _localizer["PackFailed"].Value });
    }

    public async Task<IActionResult> OnPostWeightDispatchList([FromBody] List<DispatchListWeightDTO> request)
    {
        var result = await _dispatchService.WeighDispatchList(request);
        if (result)
        {
            return new JsonResult(new
            {
                success = true,
                message = _localizer["WeighSuccess"].Value
            });
        }
        return new JsonResult(new
        {
            success = false,
            message = _localizer["WeighFailed"].Value
        });
    }

}

