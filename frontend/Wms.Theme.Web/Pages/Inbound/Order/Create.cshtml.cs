using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.AsnMaster;
using Wms.Theme.Web.Services.GoodLocations;
using Wms.Theme.Web.Services.GoodsOwner;
using Wms.Theme.Web.Services.Pallet;
using Wms.Theme.Web.Services.PurchaseOrder;
using Wms.Theme.Web.Services.Spu;
using Wms.Theme.Web.Services.Stock;
using Wms.Theme.Web.Services.Supplier;
using Wms.Theme.Web.Services.Warehouse;

namespace Wms.Theme.Web.Pages.Inbound.Order;

public class CreateModel(
    IAsnMasterService asnMasterService,
    IGoodOwnerService goodOwnerService,
    ISupplierService supplierService,
    IStockService stockService,
    IPurchaseOrderService poService,
    ISpuService spuService,
    IWarehouseService warehouseService,
    IGoodLocationService goodLocationService,
    IPalletService palletService,
    ILogger<CreateModel> logger) : PageModel
{
    private readonly ILogger<CreateModel> _logger = logger;
    private readonly IGoodOwnerService _goodOwnerService = goodOwnerService;
    private readonly ISupplierService _supplierService = supplierService;
    private readonly IStockService _stockService = stockService;
    private readonly IAsnMasterService _asnMasterService = asnMasterService;
    private readonly IPurchaseOrderService _poService = poService;
    private readonly ISpuService _spuService = spuService;
    private readonly IWarehouseService _warehouseService = warehouseService;
    private readonly IGoodLocationService _goodLocationService = goodLocationService;
    private readonly IPalletService _palletService = palletService;

    [BindProperty]
    public AsnMasterCustomDetailedDTO CreateRequest { get; set; } = new();

    /// <summary>
    /// Username from cookie for Creator field
    /// </summary>
    public string Creator { get; set; } = string.Empty;

    /// <summary>
    /// Current date formatted for ETA default value
    /// </summary>
    public string CurrentDate { get; set; } = string.Empty;

    /// <summary>
    /// ASN No
    /// </summary>
    [BindProperty]
    public string AsnNo { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        // Get username from cookie
        Creator = Request.Cookies["user_name"] ?? User.Identity?.Name ?? "System";

        // Set current date in dd/MM/yyyy HH:mm format
        CurrentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        // Get ASN No
        AsnNo = await _asnMasterService.GetNextAsnNo();
    }

    public async Task<IActionResult> OnGetLoadSupplier()
    {
        var result = await _supplierService.GetAllSuppliers();
        return new JsonResult(new { data = result });
    }

    public async Task<IActionResult> OnGetLoadGoodsOwner()
    {
        var result = await _goodOwnerService.GetAllGoodOwner();
        return new JsonResult(new { data = result });
    }

    public async Task<IActionResult> OnGetLoadSkuSelect()
    {
        var result = await _stockService.GetSkuSelect(new());
        return new JsonResult(new { data = result });
    }

    public async Task<IActionResult> OnGetLoadSkuUom()
    {
        var result = await _spuService.GetSkuUomListAsync();
        return new JsonResult(new { data = result });
    }

    public async Task<IActionResult> OnGetLoadWarehouse()
    {
        var result = await _warehouseService.GetAllAsync();
        return new JsonResult(new { data = result?.Data });
    }

    public async Task<IActionResult> OnGetLoadOpenPos()
    {
        var result = await _poService.GetOpenPosAsync();
        return new JsonResult(new { data = result.Data });
    }

    public async Task<IActionResult> OnGetLoadPoDetail(int id)
    {
        var result = await _poService.GetDetailAsync(id);
        return new JsonResult(new { data = result?.Data });
    }

    public async Task<IActionResult> OnGetLoadPallets()
    {
        var result = await _palletService.GetAllAsync();
        return new JsonResult(new { data = result?.Data });
    }

    public async Task<IActionResult> OnPostDraftAsync([FromBody] AsnMasterCustomDetailedDTO request)
    {
        _logger.LogInformation("Create Draft request: warehouse_id={WareHouseId}, warehouse_name={WareHouseName}",
            request?.WareHouseId, request?.WareHouseName);
        _logger.LogInformation("Create Draft request: detailCount={DetailCount}, first_goods_location_id={FirstGoodsLocationId}",
            request?.DetailList?.Count ?? 0,
            request?.DetailList?.FirstOrDefault()?.GoodsLocationId);

        if (request == null)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Invalid request data."
            });
        }
        var response = await _asnMasterService.CreateDraftAsync(request);

        if (response?.IsSuccess == true)
        {
            return new JsonResult(new
            {
                success = true,
                message = "Draft inbound order saved successfully!"
            });
        }

        return new JsonResult(new
        {
            success = false,
            message = response?.ErrorMessage ?? "Failed to save draft inbound order."
        });
    }

    public async Task<IActionResult> OnPostSubmitAsync([FromBody] AsnMasterCustomDetailedDTO request)
    {
        _logger.LogInformation("Create Submit request: detailCount={DetailCount}, first_goods_location_id={FirstGoodsLocationId}",
            request?.DetailList?.Count ?? 0,
            request?.DetailList?.FirstOrDefault()?.GoodsLocationId);

        var response = await _asnMasterService.SubmitAsync(request);

        if (response?.IsSuccess == true)
        {
            string redirectUrl = Url.Page("Index") ?? "/Inbound/Order";

            return new JsonResult(new
            {
                success = true,
                message = "Inbound order created successfully!",
                redirectUrl
            });
        }

        return new JsonResult(new
        {
            success = false,
            message = response?.ErrorMessage ?? "Failed to create inbound order."
        });
    }

    public async Task<JsonResult> OnGetGoodLocationsAsync(int totalPalletNeed = 1)
    {
        var goodLocations = await _goodLocationService.GetAvailableLocationsForPalletAsync(totalPalletNeed);
        return new JsonResult(goodLocations ?? []);
    }

    public async Task<JsonResult> OnPostGeneratePalletCodeAsync()
    {
        var pallet = await _palletService.GetNextPalletCode();

        if (pallet != null)
        {
            return new JsonResult(new
            {
                success = true,
                data = pallet
            });
        }

        return new JsonResult(new
        {
            success = false,
            message = "Failed to generate pallet code."
        });
    }

    public async Task<JsonResult> OnPostSearchPalletsAsync([FromBody] PageSearchRequest request)
    {
        var result = await _palletService.SearchAsync(request);

        if (result?.Code == 200 && result.Data != null)
        {
            return new JsonResult(new
            {
                success = true,
                data = new
                {
                    rows = result.Data.Rows,
                    total = result.Data.Totals,
                    pageIndex = request.pageIndex,
                    totalPages = (int)Math.Ceiling((double)result.Data.Totals / request.pageSize)
                }
            });
        }

        return new JsonResult(new
        {
            success = false,
            message = result?.ErrorMessage ?? "Failed to search pallets."
        });
    }
}
