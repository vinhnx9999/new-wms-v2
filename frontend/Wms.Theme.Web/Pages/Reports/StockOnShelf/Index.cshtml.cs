using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Reports;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.Reports;
using Wms.Theme.Web.Services.Warehouse;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Reports.StockOnShelf;

public class IndexModel(IReportService service,
    IWarehouseService warehouseService) : PageModel
{
    private readonly IReportService _service = service;
    private readonly IWarehouseService _warehouseService = warehouseService;
    public IEnumerable<StockOnShelfDto> StockOnShelf { get; set; } = [];
    public async Task OnGetAsync()
    {
        var fromDate = DateTime.UtcNow.AddMonths(-1);
        var toDate = DateTime.UtcNow;
        var request = new InventoryReportRequest
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        StockOnShelf = await _service.SearchStockOnShelf(request);
    }

    public async Task<IActionResult> OnPostSearchStockOnShelf([FromBody] InventoryReportRequest request)
    {
        var items = await _service.SearchStockOnShelf(request);
        StockOnShelf = items;
        return new JsonResult(new { items });
    }
    
    public async Task<JsonResult> OnGetSearchWareHouse(string? keyWord, int pageIndex = SystemConfig.DEFAULT_INDEX)
    {
        var searches = new List<SearchObject>
                {
                    new() {
                        Name = "WarehouseName",
                        Value = keyWord ?? "",
                        Text = keyWord ?? "",
                        Operator = Operators.Contains,
                        Label = "WarehouseName"
                    }
                };

        PageSearchRequest pageSearch = SearchUtil.GetPageSearch(searches, pageIndex);
        var data = await _warehouseService.PageSearchWarehouse(pageSearch);
        return new JsonResult(data);
    }

}
