using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Model.Supplier;
using Wms.Theme.Web.Models;
using Wms.Theme.Web.Services.Supplier;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Setting.Supplier;

public class IndexModel(ISupplierService service) : PageModel
{
    private readonly ISupplierService _service = service;

    /// <summary>
    /// Excel File
    /// </summary>
    [BindProperty]
    public IFormFile ExcelFile { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public List<SupplierDTO> Suppliers { get; set; } = [];
    public async Task OnGetAsync()
    {
        Suppliers = await _service.GetAllSuppliers();
    }

    public async Task<JsonResult> OnPostImportExcelAsync()
    {
        if (ExcelFile != null && ExcelFile.Length > 0)
        {
            ExcelFileUtil fileUtil = new();
            int startRow = 2;
            var inputSuppliers = await fileUtil.ReadSheetInputSupplierAsync(ExcelFile, startRow);
            if (inputSuppliers != null && inputSuppliers.Count > 0)
            {
                (int? data, string? message) = await _service.ImportExcelData(inputSuppliers);
                return new JsonResult(data.HasValue && data.Value > 0
                   ? new { success = true, id = data.Value }
                   : new { success = false, message = message ?? "Failed to import excel" });
            }
        }

        return new JsonResult(new { success = false, message = "Invalid request data" });
    }

    public async Task<JsonResult> OnPostDeleteSupplier([FromBody] DeleteSupplyChainRequest request)
    {
        (int? data, string? message) = await _service.DeleteSupplier(request.ChainId);
        return new JsonResult(data.HasValue && data.Value > 0
                   ? new { success = true, id = data.Value }
                   : new { success = false, message = message ?? "Failed to Delete Supplier" });
    }

    public IActionResult OnGetDownloadTemplate()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "TemplateSupplier.xlsx");

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return File(stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "TemplateSupplier.xlsx");
    }

    public async Task<JsonResult> OnPostUpdateSuplier(int id, [FromBody] UpdateSupplierRequest request)
    {
        var result = await _service.UpdateSupplierAsync(id, request);
        if (result.isSuccess)
        {
            return new JsonResult(new { success = true });
        }
        else
        {
            return new JsonResult(new { success = false, message = result.message });
        }
    }
}

