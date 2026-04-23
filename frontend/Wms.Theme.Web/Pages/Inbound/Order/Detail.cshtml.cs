using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Services.AsnMaster;
using Wms.Theme.Web.Services.GoodLocations;
using Wms.Theme.Web.Services.GoodsOwner;
using Wms.Theme.Web.Services.Pallet;
using Wms.Theme.Web.Services.Spu;
using Wms.Theme.Web.Services.Stock;
using Wms.Theme.Web.Services.Warehouse;

namespace Wms.Theme.Web.Pages.Inbound.Order;

public class DetailModel(
    IAsnMasterService asnMasterService,
    IGoodLocationService goodLocationService,
    IGoodOwnerService goodOwnerService,
    IPalletService palletService,
    IStockService stockService,
    ISpuService spuService,
    IWarehouseService warehouseService,
    IStringLocalizer<SharedResource> localizer,
    ILogger<DetailModel> logger) : PageModel
{
    private readonly ILogger<DetailModel> _logger = logger;
    private readonly IAsnMasterService _asnMasterService = asnMasterService;
    private readonly IGoodLocationService _goodLocationService = goodLocationService;
    private readonly IGoodOwnerService _goodOwnerService = goodOwnerService;
    private readonly IPalletService _palletService = palletService;
    private readonly IStockService _stockService = stockService;
    private readonly ISpuService _spuService = spuService;
    private readonly IWarehouseService _warehouseService = warehouseService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public AsnMasterCustomDetailedDTO Order { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (id <= 0) return NotFound();

        Id = id;
        var dto = await _asnMasterService.GetAnsMasterDetailed(id);
        if (dto == null || dto.Id <= 0)
        {
            return NotFound();
        }

        Order = dto;
        return Page();
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

    public async Task<IActionResult> OnGetLoadPallets()
    {
        var result = await _palletService.GetAllAsync();
        return new JsonResult(new { data = result?.Data });
    }

    public async Task<JsonResult> OnGetGoodLocationsAsync(int totalPalletNeed = 1)
    {
        var goodLocations = await _goodLocationService.GetAvailableLocationsForPalletAsync(totalPalletNeed);
        return new JsonResult(goodLocations ?? []);
    }

    public async Task<IActionResult> OnPostUpdateAsync([FromBody] AsnMasterCustomDetailedDTO request)
    {
        if (request == null || request.Id <= 0)
        {
            return new JsonResult(new { success = false, message = "Invalid request data." });
        }

        var ok = await _asnMasterService.UpdateAnsMaster(request);
        if (ok)
        {
            string redirectUrl = Url.Page("Index") ?? "/Inbound/Order";
            return new JsonResult(new { success = true, message = "Cập nhật thành công!", redirectUrl });
        }

        return new JsonResult(new { success = false, message = "Cập nhật thất bại. Vui lòng thử lại." });
    }

    public async Task<IActionResult> OnPostCompleteAsync([FromBody] int id)
    {
        if (id <= 0)
        {
            return new JsonResult(new { success = false, message = "Invalid id." });
        }

        try
        {
            var result = await _asnMasterService.CompleteAsnMasterAsync(id);
            if (result?.IsSuccess == true)
            {
                var redirectUrl = Url.Page("Index") ?? "/Inbound/Order";
                return new JsonResult(new
                {
                    success = true,
                    message = result.Data ?? "Completed successfully.",
                    redirectUrl
                });
            }

            return new JsonResult(new
            {
                success = false,
                message = result?.ErrorMessage ?? "Complete failed."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing ASN master id={Id}", id);
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostRetryAsync([FromBody] RetryInboundItemRequest request)
    {
        if (request == null || request.AsnId <= 0)
        {
            return new JsonResult(new { success = false, message = "Invalid request data." });
        }

        try
        {
            var result = await _asnMasterService.RetryInboundItemAsync(request);
            if (result?.IsSuccess == true)
            {
                return new JsonResult(new
                {
                    success = true,
                    message = result.Data ?? _localizer["RetrySuccess"].Value
                });
            }

            return new JsonResult(new
            {
                success = false,
                message = result?.ErrorMessage ?? _localizer["RetryFailed"].Value
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying inbound item for ASN id={AsnId}", request?.AsnId);
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }
}
