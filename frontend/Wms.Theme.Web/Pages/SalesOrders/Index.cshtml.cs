using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.OutboundReceipt;
using Wms.Theme.Web.Services.Receipt;

namespace Wms.Theme.Web.Pages.SalesOrders;

public class IndexModel(IOutboundReceiptService outboundReceiptService) : PageModel
{
    private readonly IOutboundReceiptService _receiptService = outboundReceiptService;

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";
    public OutboundReceiptDetailedDto Receipt { get; set; } = new();
    public async Task<IActionResult> OnGetAsync(string id)
    {
        var dto = await _receiptService.GetReceiptSharingUrl(id);
        if (dto == null || dto.Id <= 0)
        {
            return NotFound();
        }

        Receipt = dto;
        Id = id;
        return Page();
    }
}
