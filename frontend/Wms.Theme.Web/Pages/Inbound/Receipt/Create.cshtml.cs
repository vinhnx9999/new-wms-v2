using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.GoodLocation;
using Wms.Theme.Web.Model.InboundReceipt;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Supplier;
using Wms.Theme.Web.Model.Warehouse;
using Wms.Theme.Web.Services.GoodLocations;
using Wms.Theme.Web.Services.Pallet;
using Wms.Theme.Web.Services.PurchaseOrder;
using Wms.Theme.Web.Services.Receipt;
using Wms.Theme.Web.Services.Sku;
using Wms.Theme.Web.Services.Supplier;
using Wms.Theme.Web.Services.Warehouse;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Inbound.Receipt;

public class CreateModel(IWarehouseService warehouseService,
                         ISkuService skuService,
                         IReceiptService receiptService,
                         IGoodLocationService goodLocationService,
                         IPalletService palletService,
                         ISupplierService supplierService,
                         IPurchaseOrderService purchaseOrderService) : PageModel
{
    private readonly IWarehouseService _warehouseService = warehouseService;
    private readonly ISkuService _skuService = skuService;
    private readonly IReceiptService _receiptService = receiptService;
    private readonly IGoodLocationService _goodLocationService = goodLocationService;
    private readonly IPalletService _palletService = palletService;
    //private readonly IStringLocalizer<SharedResource> _stringLocalizer = stringLocalizer;
    private readonly ISupplierService _supplierService = supplierService;
    private readonly IPurchaseOrderService _purchaseOrderService = purchaseOrderService;


    /// <summary>
    /// constant for page index
    /// </summary>
    private const int PAGE_INDEX = 1;
    /// <summary>
    ///  constant for page size
    /// </summary>
    private const int PAGE_SIZE = 10;
    private const int MaxRows = 5;

    public void OnGet()
    {
    }

    public async Task<JsonResult> OnGetSearchWareHouse(string? keyWord, int pageIndex = PAGE_INDEX)
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

    public async Task<JsonResult> OnGetSearchSku(int supplierId, string? keyWord, int pageIndex = PAGE_INDEX, int pageSize = PAGE_SIZE)
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

        PageSearchRequest pageSearch = SearchUtil.GetPageSearch(searches, pageIndex);
        var data = await _skuService.PageSearchSkuSupplier(supplierId, pageSearch);
        return new JsonResult(data);
    }

    public async Task<JsonResult> OnPost([FromBody] CreateReceiptRequest request)
    {
        if (request is null)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }

        var (id, message) = await _receiptService.CreateNewReceiptAsync(request);
        if (id == 0)
        {
            return new JsonResult(new { success = false, message });
        }
        return new JsonResult(new { success = true, id });
    }

    /// <summary>
    /// Page Search for location with pallet
    /// </summary>
    /// <param name="keyWord"></param>
    /// <returns></returns>
    public async Task<JsonResult> OnGetSearchLocationWithPallet(int warehouseId, int qty = 0)
    {
        if (warehouseId <= 0)
        {
            return new JsonResult(Array.Empty<object>());
        }

        var request = new GetLocationWithPalletRequest
        {
            WarehouseId = warehouseId,
            Qty = qty
        };

        var data = await _goodLocationService.GetGoodLocationWithPalletAsync(request);
        return new JsonResult(data);
    }

    /// <summary>
    /// Page Search for location by SKU
    /// </summary>
    /// <param name="keyWord"></param>
    /// <returns></returns>
    public async Task<JsonResult> OnGetSearchLocationWithSku(int warehouseId,
        int qty = 0, int skuId = 0, int supplierId = 0, bool applyRuleSettings = true)
    {
        if (warehouseId <= 0)
        {
            return new JsonResult(Array.Empty<object>());
        }

        var request = new GetLocationWithSkuIdRequest
        {
            WarehouseId = warehouseId,
            RequestedQuantity = qty,
            SkuId = skuId,
            SupplierId = supplierId,
            Qty = MaxRows,
            ApplyRuleSettings = applyRuleSettings
        };

        var data = await _goodLocationService.SearchLocationWithSkuAsync(request);
        return new JsonResult(data);
    }

    /// <summary>
    /// Generate pallet code
    /// </summary>
    /// <returns>New Pallet</returns>
    public async Task<JsonResult> OnGetGeneratePalletCode()
    {
        var result = await _palletService.GetNextPalletCode();
        return new JsonResult(result);
    }

    public async Task<JsonResult> OnGetSearchPallet(string? keyword, int pageIndex = PAGE_INDEX)
    {
        var searches = new List<SearchObject>
        {
                new() {
                    Name = "PalletCode",
                    Value = keyword ?? "",
                    Text = keyword ?? "",
                    Operator = Operators.Contains,
                    Label = "PalletCode",
                }

        };

        PageSearchRequest pageSearch = SearchUtil.GetPageSearch(searches, pageIndex);
        var data = await _palletService.SearchAsync(pageSearch);
        return new JsonResult(data);
    }

    /// <summary>
    /// Add Warehouse
    /// </summary>
    /// <param name="request">The request object containing warehouse details</param>
    /// <returns>A task representing the asynchronous operation</returns>   
    public async Task<JsonResult> OnPostAddWareHouse([FromBody] AddWareHouseRequest request)
    {
        if (request is null)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }
        (int? data, string? message) = await _warehouseService.AddAsync(request);

        return new JsonResult(data.HasValue && data.Value > 0
           ? new { success = true, id = data.Value }
           : new { success = false, message = message ?? "Failed to add warehouse" });
    }

    public async Task<JsonResult> OnGetGenarationReceiptNo()
    {
        var result = await _receiptService.GetNextReceiptCode();
        return new JsonResult(result);
    }

    public async Task<JsonResult> OnGetSearchSupplier(string? keyword, int pageIndex = PAGE_INDEX, int pageSize = PAGE_SIZE)
    {

        var searches = new List<SearchObject>
                {
                    new() {
                        Name = "SupplierName",
                        Value = keyword ?? "",
                        Text = keyword ?? "",
                        Operator = Operators.Contains,
                        Label = "SupplierName",
                        Group = "Search"
                    },
                    new() {
                        Name = "ContactTel",
                        Value = keyword ?? "",
                        Text = keyword ?? "",
                        Operator = Operators.Contains,
                        Label = "ContactTel",
                        Group = "Search"
                    },
                    new() {
                        Name = "City",
                        Value = keyword ?? "",
                        Text = keyword ?? "",
                        Operator = Operators.Contains,
                        Label = "City",
                        Group = "Search"
                    },
                     new() {
                        Name = "Address",
                        Value = keyword ?? "",
                        Text = keyword ?? "",
                        Operator = Operators.Contains,
                        Label = "Address",
                        Group = "Search"
                    },
                    new() {
                        Name = "Email",
                        Value = keyword ?? "",
                        Text = keyword ?? "",
                        Operator = Operators.Contains,
                        Label = "Email",
                        Group = "Search"
                    }

            };

        PageSearchRequest pageSearch = SearchUtil.GetPageSearch(searches, pageIndex);
        var data = await _supplierService.PageSearchAsync(pageSearch);
        return new JsonResult(data);

    }

    public async Task<JsonResult> OnPostAddSupplier([FromBody] AddSupplierRequest request)
    {
        if (request is null)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }
        var result = await _supplierService.AddSuplierAsync(request);

        return new JsonResult(result > 0
           ? new { success = true, id = result }
           : new { success = false, message = "Failed to add supplier" });
    }

    public async Task<JsonResult> OnGetSearchPurchaseOrder(string? keyword, int pageIndex = PAGE_INDEX, int pageSize = PAGE_SIZE)
    {
        var searchObjects = new List<SearchObject>();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            searchObjects.Add(new SearchObject
            {
                Name = "po_no",
                Operator = Operators.Contains,
                Text = keyword,
                Value = keyword
            });
        }

        var pageResult = await _purchaseOrderService.GetPageAsync(new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            searchObjects = searchObjects
        });

        var rows = pageResult?.Data?.Rows ?? [];
        var data = rows.Select(po => new
        {
            id = po.Id,
            poNo = po.PoNo,
            expectedDate = po.ExpectedDeliveryDate?.ToString("yyyy-MM-dd") ?? "",
            supplierName = "",
            poStatus = po.PoStatus
        }).Where(p => p.poStatus != 3);

        return new JsonResult(new
        {
            data,
            total = pageResult?.Data?.Totals ?? 0
        });
    }

    public async Task<JsonResult> OnGetPurchaseOrderItems(int poId)
    {
        var detailResult = await _purchaseOrderService.GetDetailAsync(poId);
        if (detailResult?.IsSuccess != true || detailResult.Data is null)
        {
            return new JsonResult(new
            {
                poNo = "",
                supplierId = 0,
                supplierName = "",
                items = Array.Empty<object>()
            });
        }

        var po = detailResult.Data;

        var items = (po.Details ?? [])
            .Select(x => new
            {
                skuId = x.SkuId,
                skuUomId = x.SkuUomId,
                itemCode = x.SkuCode ?? "",
                itemName = x.SkuName ?? "",
                supplierName = x.SupplierName,
                supplierId = x.SupplierId,
                uom = x.UnitName,
                qtyOpen = x.QtyOrdered - x.QtyReceived,
                expiryDate = x.ExpiryDate?.ToString("yyyy-MM-dd")
            })
            .ToList();

        return new JsonResult(new
        {
            poNo = po.PoNo,
            supplierId = po.SupplierId ?? 0,
            supplierName = po.SupplierName ?? "",
            items
        });
    }



}
