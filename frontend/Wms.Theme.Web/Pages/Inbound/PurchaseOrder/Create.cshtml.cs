using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.PurchaseOrder;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.PurchaseOrder;
using Wms.Theme.Web.Services.Sku;
using Wms.Theme.Web.Services.Stock;
using Wms.Theme.Web.Services.Supplier;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.PurchaseOrder;

public class CreateModel(IPurchaseOrderService poService,
                         ISupplierService supplierService,
                         IStockService stockService,
                         ISkuService skuService) : PageModel
{
    private readonly IPurchaseOrderService _poService = poService;
    private readonly ISupplierService _supplierService = supplierService;
    private readonly IStockService _stockService = stockService;
    private readonly ISkuService _skuService = skuService;

    private const int PAGE_INDEX = 1;
    private const int PAGE_SIZE = 20;


    [BindProperty]
    public CreatePoRequest CreateRequest { get; set; } = new();

    public async Task OnGetAsync()
    {
        var poNo = await _poService.GeneratePoNo();
        if (poNo != null)
        {
            CreateRequest.PoNo = poNo;
        }
        CreateRequest.ExpectedDeliveryDate = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(CreateRequest.BuyerName))
        {
            CreateRequest.BuyerName = CompanyConstValue.CompanyName;
        }

        if (string.IsNullOrWhiteSpace(CreateRequest.BuyerAddress))
        {
            CreateRequest.BuyerAddress = CompanyConstValue.CompanyAddress;
        }
    }

    public async Task<IActionResult> OnPostAsync([FromBody] CreatePoRequest request)
    {
        var result = await _poService.CreateAsync(request);
        if (result)
        {
            return new JsonResult(new { success = true, message = "Created Successfully", redirectUrl = "/PurchaseOrder" });
        }
        return new JsonResult(new { success = false, message = "Created Failed" });
    }


    public async Task<JsonResult> OnGetSearchSku(int? supplierId, string? keyWord, int pageIndex = PAGE_INDEX, int pageSize = PAGE_SIZE)
    {
        var pageSearch = new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            sqlTitle = "",
            searchObjects = new List<SearchObject>
            {
                new() { Name = "SkuName", Value = keyWord ?? "", Text = keyWord ?? "", Operator = Operators.Contains, Label = "SkuName", Group = "Search" },
                new() { Name = "SkuCode", Value = keyWord ?? "", Text = keyWord ?? "", Operator = Operators.Contains, Label = "SkuCode", Group = "Search" },
                new() { Name = "SupplierName", Value = keyWord ?? "", Text = keyWord ?? "", Operator = Operators.Contains, Label = "SupplierName", Group = "Search" },
                new() { Name = "UnitName", Value = keyWord ?? "", Text = keyWord ?? "", Operator = Operators.Contains, Label = "UnitName", Group = "Search" }
            }
        };

        var data = supplierId.HasValue && supplierId.Value > 0
            ? await _skuService.PageSearchSkuSupplier(supplierId, pageSearch)
            : await _skuService.PageSearch(pageSearch, null);

        return new JsonResult(data);
    }

    public async Task<JsonResult> OnGetSearchSupplier(string? keyword, int pageIndex = PAGE_INDEX, int pageSize = PAGE_SIZE)
    {
        var request = new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            sqlTitle = "",
            searchObjects = new List<SearchObject>
        {
            new() { Name = "SupplierName", Value = keyword ?? "", Text = keyword ?? "", Operator = Operators.Contains, Label = "SupplierName", Group = "Search" },
            new() { Name = "ContactTel",  Value = keyword ?? "", Text = keyword ?? "", Operator = Operators.Contains, Label = "ContactTel",  Group = "Search" },
            new() { Name = "City",        Value = keyword ?? "", Text = keyword ?? "", Operator = Operators.Contains, Label = "City",        Group = "Search" },
            new() { Name = "Address",     Value = keyword ?? "", Text = keyword ?? "", Operator = Operators.Contains, Label = "Address",     Group = "Search" },
            new() { Name = "Email",       Value = keyword ?? "", Text = keyword ?? "", Operator = Operators.Contains, Label = "Email",       Group = "Search" }
        }
        };

        var data = await _supplierService.PageSearchAsync(request);
        return new JsonResult(data);
    }
}
