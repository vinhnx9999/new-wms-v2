using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.OutboundReceipt;
using Wms.Theme.Web.Services.Receipt;

namespace Wms.Theme.Web.Pages.Outbound.Receipt
{
    public class DetailModel(IOutboundReceiptService outboundReceiptService,
        IConfiguration configuration) : PageModel
    {
        private readonly IOutboundReceiptService _outboundReceiptService = outboundReceiptService;
        private readonly IConfiguration _configuration = configuration;

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }
        public OutboundReceiptDetailedDto Receipt { get; set; } = new();
        public CompanyInfo CompanyInfo { get; set; } = new();
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var dto = await _outboundReceiptService.GetReceiptDetailAsync(id);
            if (dto == null || dto.Id <= 0)
            {
                return NotFound();
            }
            Id = id;
            dto.SharingUrl = BuildSharingUrl(dto.SharingUrl);
            Receipt = dto;
            return Page();
        }

        public async Task<IActionResult> OnGetShareOutbound()
        {
            var relativePath = await _outboundReceiptService.GetShareOutbound(Id);
            if (string.IsNullOrEmpty(relativePath))
                return new JsonResult(new { success = false, message = "Can't Share this item" });

            var fullUri = BuildSharingUrl(relativePath);
            return new JsonResult(new { success = true, linkShare = $"{fullUri}" });
        }

        private string BuildSharingUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return "";

            Uri baseUri = new($"{_configuration["Ownership:SharingUrl"]}");
            var fullUri = new Uri(baseUri, $"SalesOrders/{relativePath}");

            return $"{fullUri}";
        }
    }
}
