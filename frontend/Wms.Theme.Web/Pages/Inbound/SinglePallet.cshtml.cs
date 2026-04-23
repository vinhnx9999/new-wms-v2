using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.InboundReceipt;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.GoodLocations;
using Wms.Theme.Web.Services.InboundPallet;
using Wms.Theme.Web.Services.Sku;
using Wms.Theme.Web.Services.Supplier;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Inbound;

public class SinglePalletModel(
    ISkuService skuService,
    IInboundPalletService inboundPalletService,
    ISupplierService supplierService,
    IGoodLocationService goodLocationService) : PageModel
{
    private readonly ISkuService _skuService = skuService;
    private readonly IInboundPalletService _inboundPalletService = inboundPalletService;
    private readonly ISupplierService _supplierService = supplierService;
    private readonly IGoodLocationService _goodLocationService = goodLocationService;
    private const int PAGE_INDEX = 1;
    private const int PAGE_SIZE = 100;

    public void OnGet()
    {
    }

    public async Task<JsonResult> OnPostCreateInboundPalletAsync([FromBody] CreateInboundPalletRequest request, CancellationToken cancellationToken)
    {
        var result = await _inboundPalletService.CreateAsync(request, cancellationToken);
        return new JsonResult(new
        {
            success = result.id > 0,
            id = result.id,
            message = result.message
        });
    }

    public async Task<JsonResult> OnGetSearchSku(int? supplierId, string? keyWord, int pageIndex = PAGE_INDEX, int pageSize = PAGE_SIZE)
    {
        var searches = new List<SearchObject>
        {
            new() { Name = "SkuName", Value = keyWord ?? "", Text = keyWord ?? "", Operator = Operators.Contains, Label = "SkuName", Group = "Search" },
            new() { Name = "SkuCode", Value = keyWord ?? "", Text = keyWord ?? "", Operator = Operators.Contains, Label = "SkuCode", Group = "Search" },
            new() { Name = "SupplierName", Value = keyWord ?? "", Text = keyWord ?? "", Operator = Operators.Contains, Label = "SupplierName", Group = "Search" },
            new() { Name = "UnitName", Value = keyWord ?? "", Text = keyWord ?? "", Operator = Operators.Contains, Label = "UnitName", Group = "Search" }
        };

        var pageSearch = SearchUtil.GetPageSearch(searches, pageIndex);


        var data = await _skuService.PageSearch(pageSearch, null);

        return new JsonResult(data);
    }

    public async Task<JsonResult> OnGetSearchSupplier(string? keyword, int pageIndex = PAGE_INDEX)
    {
        var searches = new List<SearchObject>
        {
            new() { Name = "SupplierName", Value = keyword ?? "", Text = keyword ?? "", Operator = Operators.Contains, Label = "SupplierName", Group = "Search" }
        };

        var pageSearch = SearchUtil.GetPageSearch(searches, pageIndex);
        var data = await _supplierService.PageSearchAsync(pageSearch);
        return new JsonResult(data);
    }

    public async Task<JsonResult> OnGetSearchLocation(string? keyWord, int pageIndex = PAGE_INDEX)
    {
        var request = new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = PAGE_SIZE,
            sqlTitle = "",
            searchObjects = new List<SearchObject>
            {
                new()
                {
                    Name = "LocationName",
                    Value = keyWord ?? "",
                    Text = keyWord ?? "",
                    Operator = Operators.Contains,
                    Label = "LocationName",

                }
            }
        };

        var data = await _goodLocationService.GetGoodsLocationPageAsync(request);
        return new JsonResult(data);
    }
}


