using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Components;
using Wms.Theme.Web.Model.Dispatch;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.Dispatch;

namespace Wms.Theme.Web.Pages.Outbound;

public class PickListsModel : PageModel
{
    private readonly IDispatchService _dispatchService;

    [BindProperty]
    public DataTableViewModel DataTableViewModel { get; set; } = new DataTableViewModel();

    public PickListsModel(IDispatchService dispatchService)
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

        var result_Status_0 = await _dispatchService.GetDispatchAdvancedList(new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            sqlTitle = "dispatch_status=0",
            searchObjects = searchObjects
        });

        var result_status_1 = await _dispatchService.GetDispatchAdvancedList(new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            sqlTitle = "dispatch_status=1",
            searchObjects = searchObjects
        });


        var list_0 = result_Status_0?.Data?.Rows ?? [];
        var list_1 = result_status_1?.Data?.Rows ?? [];
        var mergedRows = list_0.Concat(list_1).ToList();
        var combinedTotal = (result_Status_0?.Data?.Totals ?? 0) + (result_status_1?.Data?.Totals ?? 0);

        // Map to anonymous object for JSON
        var rows = mergedRows.Select((item, index) => new
        {
            id = item.DispatchNo, // Use DispatchNo as ID
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

    public async Task<IActionResult> OnGetDispatchForPickLocation(string dispatchNo)
    {
        List<DispatchlistConfirmDetailViewModel>? result = await _dispatchService.GetDispatchByConfirmCheck(dispatchNo);
        if (result != null)
        {
            return new JsonResult(new { success = true, data = result });
        }
        return new JsonResult(new { success = false });
    }

    public async Task<IActionResult> OnPostConfirmPickDispatch([FromBody] List<DispatchlistConfirmDetailViewModel> pickist)
    {
        var response = await _dispatchService.ConfirmDispatchHasOrdered(pickist);
        if (response)
        {
            return new JsonResult(new { success = true, message = "Pick confirmed successfully." });
        }
        return new JsonResult(new { success = false, message = "Failed to confirm pick." });
    }
}


