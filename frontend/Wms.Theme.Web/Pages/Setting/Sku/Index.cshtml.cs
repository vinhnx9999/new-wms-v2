using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Sku;
using Wms.Theme.Web.Model.Units;
using Wms.Theme.Web.Services.Sku;
using Wms.Theme.Web.Services.Unit;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Setting.Sku;

public class IndexModel(ISkuService skuService,
                        IUnitService unitService) : PageModel
{
    private readonly ISkuService _skuService = skuService;
    private readonly IUnitService _unitService = unitService;
    /// <summary>
    /// Sku Supplier DTO
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public List<SkuSupplierDTO> Sku { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public List<UnitDTO> Units { get; set; } = [];

    public async Task OnGetAsync()
    {
        var pageSearch = new PageSearchRequest
        {
            pageIndex = SystemConfig.GET_ALL,
            pageSize = SystemConfig.PAGE_SIZE
        };

        var skuTask = _skuService.PageSearch(pageSearch, null);
        var unitsTask = _unitService.GetAllUnitsAsync();

        await Task.WhenAll(skuTask, unitsTask);

        Sku = await skuTask;
        Units = await unitsTask;
    }

    public async Task<JsonResult> OnPostDeleteSku([FromBody] DeleteSkuRequest request)
    {
        (int? data, string? message) = await _skuService.DeleteSku(request.SkuId);
        return new JsonResult(data.HasValue && data.Value > 0
                   ? new { success = true, id = data.Value }
                   : new { success = false, message = message ?? "Failed to Delete SKU" });
    }

    /// <summary>
    /// Excel File
    /// </summary>
    [BindProperty]
    public IFormFile ExcelFile { get; set; } = default!;

    public async Task<JsonResult> OnPostAsync()
    {
        if (ExcelFile != null && ExcelFile.Length > 0)
        {
            ExcelFileUtil fileUtil = new();
            int startRow = 2;
            var inputSkus = await fileUtil.ReadSheetInputSkuAsync(ExcelFile, startRow);
            if (inputSkus != null && inputSkus.Count > 0)
            {
                (int? data, string? message) = await _skuService.ImportExcelData(inputSkus);
                return new JsonResult(data.HasValue && data.Value > 0
                   ? new { success = true, id = data.Value }
                   : new { success = false, message = message ?? "Failed to import excel" });
            }
        }

        return new JsonResult(new { success = false, message = "Invalid request data" });
    }

    public IActionResult OnGetDownloadTemplate()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "TemplateSKU.xlsx");

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return File(stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "TemplateSKU.xlsx");
    }

    public async Task<JsonResult> OnPostUpdateSkuAsync(int id, [FromBody] UpdateSkuRequest request)
    {
        var result = await _skuService.UpdateSkuAsync(id, request);
        if (result.isSucess)
        {
            return new JsonResult(new { success = true });
        }
        else
        {
            return new JsonResult(new { success = false, message = result.message });
        }
    }

}

public class DeleteSkuRequest
{
    public int SkuId { get; set; } = 0;
}
