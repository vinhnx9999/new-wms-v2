using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.InboundReceipt;
using Wms.Theme.Web.Services.Receipt;

namespace Wms.Theme.Web.Pages.Orders;

public class IndexModel(IReceiptService receiptService) : PageModel
{
    private readonly IReceiptService _receiptService = receiptService;

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";
    public InboundReceiptDetailedDTO Receipt { get; set; } = new();

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
