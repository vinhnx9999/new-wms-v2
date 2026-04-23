using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.InboundReceipt;
using Wms.Theme.Web.Model.OutboundReceipt;
using Wms.Theme.Web.Services.Receipt;

namespace Wms.Theme.Web.Pages.Inbound.Receipt;

public class DetailModel(
    IReceiptService receiptService,    
    IConfiguration configuration) : PageModel
{
    private readonly IReceiptService _receiptService = receiptService;
    private readonly IConfiguration _configuration = configuration;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }
    public InboundReceiptDetailedDTO Receipt { get; set; } = new();
    public CompanyInfo CompanyInfo { get; set; } = new();
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var dto = await _receiptService.GetReceiptDetailAsync(id);
        if (dto == null || dto.Id <= 0)
        {
            return NotFound();
        }

        dto.SharingUrl = BuildSharingUrl(dto.SharingUrl);
        Receipt = dto;
        Id = id;
        return Page();
    }

    public async Task<IActionResult> OnGetShareInbound()
    {
        var relativePath = await _receiptService.GetShareInbound(Id);
        if (string.IsNullOrEmpty(relativePath)) 
            return new JsonResult(new { success = false, message = "Can't Share this item" });
        
        var fullUri = BuildSharingUrl(relativePath);
        return new JsonResult(new { success = true, linkShare = $"{fullUri}" });
    }

    private string BuildSharingUrl(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return "";

        Uri baseUri = new($"{_configuration["Ownership:SharingUrl"]}");
        var fullUri = new Uri(baseUri, $"Orders/{relativePath}");

        return $"{fullUri}";
    }
}
