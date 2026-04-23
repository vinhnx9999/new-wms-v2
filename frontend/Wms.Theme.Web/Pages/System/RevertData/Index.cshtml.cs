
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Wms.Theme.Web.Model.InboundReceipt;
using Wms.Theme.Web.Model.OutboundReceipt;
using Wms.Theme.Web.Services.Receipt;

namespace Wms.Theme.Web.Pages.System.RevertData;

public class IndexModel(IReceiptService inboundService, 
    IOutboundReceiptService outboundService,
    IStringLocalizer<SharedResource> localizer) : PageModel
{
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;
    private readonly IReceiptService _inboundService = inboundService;
    private readonly IOutboundReceiptService _outboundService = outboundService;

    public IEnumerable<InboundReceiptListResponse> InboundRevertList { get; set; } = [];
    public IEnumerable<OutboundReceiptListResponse> OutboundRevertList { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        InboundRevertList = await _inboundService.GetDeletedData(); 
        OutboundRevertList = await _outboundService.GetDeletedData();

        return Page();
    }

    public async Task<IActionResult> OnPostRevertDataInbound(int id)
    {
        var result = await _inboundService.RevertReceipt(id);
        if (result)
        {
            return new JsonResult(new { success = true, message = _localizer["Success"].Value });
        }
        else
        {
            return new JsonResult(new { success = false, message = _localizer["RevertFailed"].Value });
        }
    }

    public async Task<IActionResult> OnPostRevertDataOutbound(int id)
    {
        var result = await _outboundService.RevertReceipt(id);
        if (result)
        {
            return new JsonResult(new { success = true, message = _localizer["Success"].Value });
        }
        else
        {
            return new JsonResult(new { success = false, message = _localizer["RevertFailed"].Value });
        }
    }
}
