using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Model.PurchaseOrder;
using Wms.Theme.Web.Services.AsnMaster;
using Wms.Theme.Web.Services.GoodsOwner;
using Wms.Theme.Web.Services.PurchaseOrder;
using Wms.Theme.Web.Services.Stock;
using Wms.Theme.Web.Services.Supplier;

namespace Wms.Theme.Web.Pages.Inbound;

public class AsnCreateModel(IAsnMasterService asnMasterService, IGoodOwnerService goodOwnerService,
                    ISupplierService supplierService, IStockService stockService,
                    Services.PurchaseOrder.IPurchaseOrderService poService, ILogger<AsnCreateModel> logger) : PageModel
{
    private readonly ILogger<AsnCreateModel> _logger = logger;
    private readonly IGoodOwnerService _goodOwnerService = goodOwnerService;
    private readonly ISupplierService _supplierService = supplierService;
    private readonly IStockService _stockService = stockService;
    private readonly IAsnMasterService _asnMasterService = asnMasterService;
    private readonly IPurchaseOrderService _poService = poService;

    [BindProperty]
    public AsnMasterCustomDetailedDTO CreateRequest { get; set; } = new AsnMasterCustomDetailedDTO();

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnGetLoadSupllier()
    {
        List<SupplierDTO>? result = await _supplierService.GetAllSuppliers();
        return new JsonResult(new { data = result });
    }

    public async Task<IActionResult> OnGetLoadGoodsOwner()
    {
        List<GoodOwnerDTO>? result = await _goodOwnerService.GetAllGoodOwner();
        return new JsonResult(new { data = result });
    }

    public async Task<IActionResult> OnGetLoadSkuSelect()
    {
        List<SkuSelectDTO>? result = await _stockService.GetSkuSelect(new());
        return new JsonResult(new { data = result });
    }

    public async Task<IActionResult> OnGetLoadOpenPos()
    {
        Model.ShareModel.ApiResult<List<CreateNewOrderRequest>>? result = await _poService.GetOpenPosAsync();
        return new JsonResult(new { data = result.Data });
    }

    public async Task<IActionResult> OnGetLoadPoDetail(int id)
    {
        Model.ShareModel.ApiResult<PoDetailDto>? result = await _poService.GetDetailAsync(id);
        _logger.LogInformation($"Data {id} : {result?.Data}");
        return new JsonResult(new { data = result?.Data });
    }

    public async Task<IActionResult> OnPostAsync([FromBody] AsnMasterCustomDetailedDTO request)
    {
        var response = await _asnMasterService.CreateAsnMasterAsync(request);

        if (response?.IsSuccess == true)
        {
            string redirectUrl = Url.Page("Detail", new { id = response.CreatedId }) ?? "/Inbound/ASN";

            return new JsonResult(new
            {
                success = true,
                message = "ASN created successfully!",
                redirectUrl
            });
        }
        else
        {
            return new JsonResult(new
            {
                success = false,
                message = response?.ErrorMessage ?? "Failed to create ASN."
            });
        }
    }

}
