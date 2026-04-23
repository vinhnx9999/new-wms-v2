using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Reports;
using Wms.Theme.Web.Services.Reports;

namespace Wms.Theme.Web.Pages.Vendors;

public class IndexModel(IReportService service) : PageModel
{
    private readonly IReportService _service = service;
    public IEnumerable<VendorMasterDto> Vendors { get; set; } = [];
    public async Task<IActionResult> OnGet()
    {
        var data = await _service.GetVendors();
        Vendors = (data ?? []).Select(x => new VendorMasterDto(x));
        return Page();
    }

    public async Task<JsonResult> OnPostDeleteVendor([FromBody] DeleteVendorRequest request)
    {
        (int? data, string? message) = await _service.BanVendor(request.Id);
        return new JsonResult(data.HasValue && data.Value > 0
                   ? new { success = true, id = data.Value }
                   : new { success = false, message = message ?? "Failed to Delete Vendor" });
    }
}

public class DeleteVendorRequest
{
    public long Id { get; set; }
}
