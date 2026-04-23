using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Units;
using Wms.Theme.Web.Services.Unit;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Setting.Unit;

public class DeleteUnitRequest
{
    public int UnitId { get; set; }
}

public class IndexModel(IUnitService service) : PageModel
{
    private readonly IUnitService _service = service;

    [BindProperty(SupportsGet = true)]
    public List<UnitDTO> Units { get; set; } = [];

    public async Task OnGet()
    {
        Units = await _service.GetAllUnitsAsync();
    }

    [BindProperty]
    public IFormFile ExcelFile { get; set; }

    public async Task<JsonResult> OnPostImportExcelAsync()
    {
        if (ExcelFile != null && ExcelFile.Length > 0)
        {
            ExcelFileUtil fileUtil = new();
            int startRow = 2;
            var inputUnits = await fileUtil.ReadSheetInputUnitAsync(ExcelFile, startRow);
            if (inputUnits != null && inputUnits.Count > 0)
            {
                (int? data, string? message) = await _service.ImportExcelData(inputUnits);
                return new JsonResult(data.HasValue && data.Value > 0
                   ? new { success = true, id = data.Value }
                   : new { success = false, message = message ?? "Failed to import excel" });
            }
        }

        return new JsonResult(new { success = false, message = "Invalid request data" });
    }

    public async Task<JsonResult> OnPostDeleteUnit([FromBody] DeleteUnitRequest request)
    {
        (int? data, string? message) = await _service.DeleteUnit(request.UnitId);
        return new JsonResult(data.HasValue && data.Value > 0
                   ? new { success = true, id = data.Value }
                   : new { success = false, message = message ?? "Failed to Delete Unit" });
    }

    public IActionResult OnGetDownloadTemplate()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "TemplateUnit.xlsx");

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return File(stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "TemplateUnit.xlsx");
    }
}
