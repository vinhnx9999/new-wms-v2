using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.InboundReceipt;
using Wms.Theme.Web.Model.Warehouse;
using Wms.Theme.Web.Services.Stock;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Inventory.Beginning;

public class IndexModel(IBeginMerchandiseService service) : PageModel
{
    private readonly IBeginMerchandiseService _service = service;

    public IEnumerable<BeginMerchandiseDto> BeginMerchandises { get; set; } = [];
    public async Task<IActionResult> OnGetAsync()
    {
        BeginMerchandises = await _service.GetBeginMerchandises();
        return Page();
    }

    public IActionResult OnGetDownloadTemplate()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "TemplateBeginMerchandise.xlsx");

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return File(stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "TemplateBeginMerchandise.xlsx");
    }

    /// <summary>
    /// Excel File
    /// </summary>
    [BindProperty]
    public IFormFile ExcelFile { get; set; } = default!;

    public async Task<JsonResult> OnPostImportExcelAsync()
    {
        if (ExcelFile != null && ExcelFile.Length > 0)
        {
            ExcelFileUtil fileUtil = new();
            int startRow = 2;
            var inputOrders = await fileUtil.ReadSheetBeginMerchandiseAsync(ExcelFile, startRow);
            if (inputOrders != null && inputOrders.Count > 0)
            {
                (int? data, string? message) = await _service.ImportExcelData(inputOrders);
                return new JsonResult(data.HasValue && data.Value > 0
                   ? new { success = true, id = data.Value }
                   : new { success = false, message = message ?? "Failed to import excel" });
            }
        }

        return new JsonResult(new { success = false, message = "Invalid request data" });
    }

    public async Task<JsonResult> OnPostDeleteBeginning([FromBody] BasePostActionRequest request)
    {
        if (request.Id <= 0)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }

        (int? data, string? message) = await _service.DeleteBeginning(request.Id);
        return new JsonResult(data.HasValue && data.Value > 0
           ? new { success = true, id = data.Value }
           : new { success = false, message = message ?? "Failed to delete Beginning" });
    }

    public async Task<JsonResult> OnPostSaveBeginning()
    {
        (int? data, string? message) = await _service.SaveBeginning();
        return new JsonResult(data.HasValue && data.Value > 0
           ? new { success = true, id = data.Value }
           : new { success = false, message = message ?? "Failed to Save Beginning" });
    }
}
