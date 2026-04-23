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

public class InventoryCardModel(IReportService service,
    ISkuService skuService,
    IWarehouseService warehouseService) : PageModel
{
    private readonly IReportService _service = service;
    private readonly IWarehouseService _warehouseService = warehouseService;
    private readonly ISkuService _skuService = skuService;

    [BindProperty]
    public IEnumerable<InventoryCardItem> CardItems { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostSearchInventoryCard([FromBody] InventoryReportRequest request)
    {
        if (!request.SkuIds.Any()) return BadRequest();

        var items = await _service.GetInventoryCards(request);
        CardItems = items;
        return new JsonResult(new { items });
    }

    public async Task<JsonResult> OnGetReloadSkuMaster()
    {
        var data = await _skuService.GetMasterData();
        return new JsonResult(data);
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
