using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Reports;
using Wms.Theme.Web.Services.Reports;

namespace Wms.Theme.Web.Pages.Reports;

public class InOutStockModel(IReportService service) : PageModel
{
    private readonly IReportService _service = service;
    public IEnumerable<WarehouseInventoryReport> Inventories { get; set; } = [];
    public async Task OnGet()
    {
        var fromDate = DateTime.UtcNow.AddMonths(-1);
        var toDate = DateTime.UtcNow;
        var request = new InventoryReportRequest
        {
            FromDate = fromDate,
            ToDate = toDate,
            WarehouseId = 1
        };

        Inventories = await _service.GetInventories(request);
    }
}

