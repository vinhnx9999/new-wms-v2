using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Reports;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.Reports;
using Wms.Theme.Web.Services.Warehouse;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Inventory.LowStockAlert;

public class IndexModel(IWarehouseService warehouseService, IReportService service) : PageModel
{
    private readonly IWarehouseService _warehouseService = warehouseService;
    private readonly IReportService _service = service;
    public IEnumerable<LowStockAlertDto> LowStockAlerts { get; set; } = [];
    public async Task OnGet()
    {
        LowStockAlerts = await _service.GetLowStockAlerts();
    }

    public async Task<JsonResult> OnGetSearchWareHouse(string? keyWord, int pageIndex = 1)
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
