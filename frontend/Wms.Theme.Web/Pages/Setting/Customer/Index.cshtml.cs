using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Customer;
using Wms.Theme.Web.Models;
using Wms.Theme.Web.Services.Customer;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Setting.Customer;

public class IndexModel(ICustomerService service) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public List<CustomerDTO> Customers { get; set; } = [];

    private readonly ICustomerService _service = service;
    public async Task OnGet()
    {
        Customers = await _service.GetAllCustomersAsync();
    }

    [BindProperty]
    public IFormFile ExcelFile { get; set; } = default!;

    public async Task<JsonResult> OnPostImportExcelAsync()
    {
        if (ExcelFile != null && ExcelFile.Length > 0)
        {
            ExcelFileUtil fileUtil = new();
            int startRow = 2;
            var inputSuppliers = await fileUtil.ReadSheetInputCustomerAsync(ExcelFile, startRow);
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

    public async Task<JsonResult> OnPostDeleteCustomer([FromBody] DeleteSupplyChainRequest request)
    {
        (int? data, string? message) = await _service.DeleteCustomer(request.ChainId);
        return new JsonResult(data.HasValue && data.Value > 0
                   ? new { success = true, id = data.Value }
                   : new { success = false, message = message ?? "Failed to Delete Customer" });
    }

    public IActionResult OnGetDownloadTemplate()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "TemplateCustomer.xlsx");

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return File(stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "TemplateCustomer.xlsx");
    }

    public async Task<JsonResult> OnPostUpdateCustomer(int id, [FromBody] UpdateCustomerRequest request)
    {
        var result = await _service.UpdateCustomerAsync(id, request);
        if (result.isSuccess)
        {
            return new JsonResult(new { success = true });
        }
        else
        {
            return new JsonResult(new { success = false, message = result.message ?? "Failed to update customer" });
        }
    }
}
