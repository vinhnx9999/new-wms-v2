using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Reports;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Sku;
using Wms.Theme.Web.Services.Reports;
using Wms.Theme.Web.Services.Sku;
using Wms.Theme.Web.Services.Warehouse;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Reports;

public class InOutStatementModel(IReportService service,
    ISkuService skuService,
    IWarehouseService warehouseService) : PageModel
{
    private readonly IReportService _service = service;
    private readonly IWarehouseService _warehouseService = warehouseService;
    private readonly ISkuService _skuService = skuService;

    public IEnumerable<InOutStatementDto> InOutStatements { get; set; } = [];
    public async Task OnGetAsync()
    {
        var fromDate = DateTime.UtcNow.AddMonths(-1);
        var toDate = DateTime.UtcNow;
        var request = new InventoryReportRequest
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        InOutStatements = await _service.GetInOutStatements(request);
    }

    public async Task<IActionResult> OnPostSearchInOutStatement([FromBody] InventoryReportRequest request)
    {
        var items = await _service.GetInOutStatements(request);
        InOutStatements = items;
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

    public async Task<JsonResult> OnGetSearchSku(string? keyWord, int pageIndex = SystemConfig.DEFAULT_INDEX, int? warehouseId = null)
    {
        var searches = new List<SearchObject>
                {
                    new() {
                        Name = "SkuName",
                        Value = keyWord ?? "",
                        Text = keyWord ?? "",
                        Operator = Operators.Contains,
                        Label = "SkuName",
                        Group = "Search"
                    },
                    new() {
                        Name = "SkuCode",
                        Value = keyWord ?? "",
                        Text = keyWord ?? "",
                        Operator = Operators.Contains,
                        Label = "SkuCode",
                        Group = "Search"
                    },
                    new() {
                        Name = "SupplierName",
                        Value = keyWord ?? "",
                        Text = keyWord ?? "",
                        Operator = Operators.Contains,
                        Label = "SupplierName",
                        Group = "Search"
                    },
                    new() {
                        Name = "UnitName",
                        Value = keyWord ?? "",
                        Text = keyWord ?? "",
                        Operator = Operators.Contains,
                        Label = "UnitName",
                        Group = "Search"
                    }
            };

        PageSearchRequest pageSearch = SearchUtil.GetPageSearch(searches, pageIndex, 100);
        var data = await _skuService.PageSearch(pageSearch, warehouseId);
        var items = data.Select(x => new SkuDTO
        {
            SkuCode = x.SkuCode,
            SkuId = x.SkuId,
            SkuName = x.SkuName
        })
            .Distinct();
        return new JsonResult(items);
    }
}
