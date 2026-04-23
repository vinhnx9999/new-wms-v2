using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.GoodLocation;
using Wms.Theme.Web.Model.Warehouse;
using Wms.Theme.Web.Services.GoodLocations;
using Wms.Theme.Web.Services.Warehouse;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Locations;

public class ListModel(IWarehouseService service, IGoodLocationService locationService) : PageModel
{
    private readonly IWarehouseService _service = service;
    private readonly IGoodLocationService _locationService = locationService;


    [BindProperty(SupportsGet = true)]
    public List<WarehouseViewModel> WarehouseViewModels { get; set; } = [];

    [BindProperty]
    public IFormFile ExcelFile { get; set; } = default!;

    public async Task OnGetAsync()
    {
        var warehouses = await _service.GetAllAsync();
        WarehouseViewModels = warehouses.Data;

    }

    public async Task<JsonResult> OnGetLocationAsync([FromQuery] int warehouseId)
    {
        var locations = await _locationService.GetLocationsByWarehouseAsync(warehouseId);
        return new JsonResult(locations);
    }

    public async Task<JsonResult> OnPostImportExcelAsync()
    {
        if (ExcelFile == null || ExcelFile.Length <= 0)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }

        var fileUtil = new ExcelFileUtil();
        var inputLocations = await fileUtil.ReadSheetInputLocationAsync(ExcelFile, 2);

        if (inputLocations.Count == 0)
        {
            return new JsonResult(new { success = false, message = "No valid rows in excel file" });
        }

        var (data, message) = await _locationService.ImportExcelData(inputLocations);

        return new JsonResult(data.HasValue && data.Value > 0
            ? new { success = true, id = data.Value }
            : new { success = false, message = message ?? "Failed to import excel" });
    }

    public IActionResult OnGetDownloadTemplateLocation()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "TemplateLocation.xlsx");
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return File(stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "TemplateLocation.xlsx");
    }

    public async Task<IActionResult> OnPostCreateAsync([FromBody] AddLocationRequest request)
    {
        if (request is null)
        {
            return new JsonResult(new { success = false, message = "Invalid request." });
        }

        var id = await _locationService.CreateLocationAsync(request);
        if (id > 0)
        {
            return new JsonResult(new { success = true, id, message = "Create location successfully." });
        }

        return new JsonResult(new { success = false, message = "Create location failed. Please check input or duplicate data." });
    }
}

