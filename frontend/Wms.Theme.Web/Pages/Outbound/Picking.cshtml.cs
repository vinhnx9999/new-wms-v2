using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Components;
using Wms.Theme.Web.Model.Dispatch;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.Dispatch;

namespace Wms.Theme.Web.Pages.Outbound;

public class PickingModel : PageModel
{
    private readonly IDispatchService _dispatchService;

    [BindProperty]
    public DataTableViewModel DataTableViewModel { get; set; } = new DataTableViewModel();

    public PickingModel(IDispatchService dispatchService)
    {
        _dispatchService = dispatchService;
    }
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetDataTable(int pageIndex = 1, string dispatchNo = "")
    {
        const int pageSize = 10;
        var searchObjects = new List<SearchObject>();
        if (!string.IsNullOrEmpty(dispatchNo))
        {
            searchObjects.Add(new SearchObject
            {
                Name = "dispatch_no",
                Operator = Operators.Contains,
                Text = dispatchNo,
                Value = dispatchNo,
            });
        }
        var result_2 = await _dispatchService.GetDispatchList(new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            sqlTitle = "dispatch_status=2",
            searchObjects = searchObjects
        });


        var list_0 = result_2?.Data?.Rows ?? new List<DispatchListDTO>();
        var mergedRows = list_0.ToList();
        var combinedTotal = (result_2?.Data?.Totals ?? 0);

        var rows = mergedRows.Select((item, index) => new
        {
            id = item.DispatchNo,
            dispatchNo = item.DispatchNo,
            customerName = item.CustomerName,
            dispatchStatus = item.DispatchStatus,
            qty = item.Qty,
            weight = item.Weight,
            volume = item.Volume,
            creator = item.Creator
        }).ToList();

        return new JsonResult(new
        {
            status = true,
            data = new
            {
                rows = rows,
                total = combinedTotal,
                pageIndex = pageIndex,
                totalPages = (int)Math.Ceiling((double)combinedTotal / pageSize)
            }
        });
    }

    public async Task<IActionResult> OnPostCancelOrder([FromBody] CancelOrderDTO request)
    {
        var result = await _dispatchService.CancelOrder(request);
        if (result)
        {
            return new JsonResult(new
            {
                success = true,
                message = "Order has been successfully canceled."
            });
        }
        return new JsonResult(new
        {
            success = false,
            message = "Failed to cancel order."
        });
    }

    public async Task<IActionResult> OnPostConfirmDispatchHasPicked([FromBody] string dispatchNo)
            {
        var result = await _dispatchService.ConFirmDispatchHasPicked(dispatchNo);
        if (result)
        {
            return new JsonResult(new
            {
                success = true,
                message = "Dispatch has been successfully confirmed as picked."
            });
        }
        return new JsonResult(new
        {
            success = false,
            message = "Failed to confirm dispatch as picked."
        });
    }

}



