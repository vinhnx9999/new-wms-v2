
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Units;
using Wms.Theme.Web.Services.Unit;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Setting.Specification;

public class IndexModel(ISpecificationService service) : PageModel
{
    private readonly ISpecificationService _service = service;
    [BindProperty(SupportsGet = true)]
    public List<SpecificationDTO> Specifications { get; set; } = [];

    public async Task OnGet()
    {
        Specifications = await _service.GetAllSpecificationsAsync();
    }

    [BindProperty]
    public IFormFile ExcelFile { get; set; }

    public async Task<JsonResult> OnPostImportExcelAsync()
    {
        if (ExcelFile != null && ExcelFile.Length > 0)
        {
            ExcelFileUtil fileUtil = new();
            int startRow = 2;
            var inputSpecifications = await fileUtil.ReadSheetInputSpecificationAsync(ExcelFile, startRow);
            if (inputSpecifications != null && inputSpecifications.Count > 0)
            {
                (int? data, string? message) = await _service.ImportExcelData(inputSpecifications);
                return new JsonResult(data.HasValue && data.Value > 0
                   ? new { success = true, id = data.Value }
                   : new { success = false, message = message ?? "Failed to import excel" });
            }
        }

        return new JsonResult(new { success = false, message = "Invalid request data" });
    }

    public async Task<JsonResult> OnPostDeleteSpecification([FromBody] DeleteSpecificationRequest request)
    {
        (int? data, string? message) = await _service.DeleteSpecification(request.SpecId);
        return new JsonResult(data.HasValue && data.Value > 0
                   ? new { success = true, id = data.Value }
                   : new { success = false, message = message ?? "Failed to Delete Unit" });
    }

    public IActionResult OnGetDownloadTemplate()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "TemplateSpecification.xlsx");

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return File(stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "TemplateSpecification.xlsx");
    }
}

public class DeleteSpecificationRequest
{
    public int SpecId { get; set; }
}