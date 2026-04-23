using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.Dispatch;

namespace Wms.Theme.Web.Pages.Outbound.SalesOrders
{
    public class IndexModel : PageModel
    {
        private readonly IDispatchService _dispatchService;
        public IndexModel(IDispatchService dispatchService)
        {
            _dispatchService = dispatchService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnGetDataTable(int pageIndex = 1, int pageSize = 10, string dispatchNo = "")
        {
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

            var result = await _dispatchService.GetDispatchAdvancedList(new PageSearchRequest
            {
                pageIndex = pageIndex,
                pageSize = pageSize,
                sqlTitle = "",
                searchObjects = searchObjects
            });

            if (result?.Data == null)
            {
                return new JsonResult(new { status = false, message = "No data found" });
            }

            var rows = result.Data.Rows.Select((item, index) => new
            {
                id = item.DispatchNo,
                dispatchNo = item.DispatchNo,
                customer = item.CustomerName,
                status = item.DispatchStatus,
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
                    total = result.Data.Totals,
                    pageIndex = pageIndex,
                    totalPages = (int)Math.Ceiling((double)result.Data.Totals / pageSize)
                }
            });
        }

        public async Task<IActionResult> OnPostDeleteDispatch(string dispatch_no)
        {
            var success = await _dispatchService.RemoveDispatchList(dispatch_no);
            if (success)
            {
                return new JsonResult(new { success = true, message = "Dispatch deleted successfully." });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Failed to delete dispatch." });
            }
        }
    }
}
