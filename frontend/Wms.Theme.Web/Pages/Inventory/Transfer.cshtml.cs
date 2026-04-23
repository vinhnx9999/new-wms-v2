using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.StockMove;
using Wms.Theme.Web.Services.StockMove;

namespace Wms.Theme.Web.Pages.Inventory
{
    public class TransferModel : PageModel
    {
        private readonly IStockMoveService _stockMoveService;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public TransferModel(IStockMoveService stockMoveService, IStringLocalizer<SharedResource> sharedLocalizer)
        {
            _stockMoveService = stockMoveService;
            _sharedLocalizer = sharedLocalizer;
        }

        public StockMoveDashboardStats DashboardStats { get; set; }

        public async Task OnGetAsync()
        {
            DashboardStats = await _stockMoveService.GetDashboardStatsAsync();
        }

        public async Task<JsonResult> OnGetTableDataAsync(int pageIndex = 1, string status = "", string source = "", string dest = "", string date = "")
        {
            const int pageSize = 10;
            var searchObject = new List<SearchObject>();

            // Filter logic removed as per user request

            var pageRequest = new PageSearchRequest
            {
                pageIndex = pageIndex,
                pageSize = pageSize,
                searchObjects = searchObject
            };

            var data = await _stockMoveService.GetStockPageAsync(pageRequest);

            if (data == null || !data.IsSuccess)
            {
                return new JsonResult(new
                {
                    status = false,
                    data = new
                    {
                        rows = new List<object>(),
                        pageIndex = 1,
                        total = 0,
                        totalPages = 0
                    }
                });
            }

            return new JsonResult(new
            {
                status = true,
                data = new
                {
                    rows = data.Data.Rows,
                    pageIndex = pageIndex,
                    total = data.Data.Totals,
                    totalPages = (int)Math.Ceiling((double)data.Data.Totals / pageSize)
                }
            });
        }

        public async Task<IActionResult> OnPostRemoveStockMove(int id)
        {
            var result = await _stockMoveService.RemoveStockMoveAsync(id);
            if (result)
            {
                return new JsonResult(new { success = true, message = _sharedLocalizer["DeleteJobSuccess"].Value });
            }
            else
            {
                return new JsonResult(new { success = false, message = _sharedLocalizer["DeleteJobFailed"].Value });
            }
        }
    }
}
