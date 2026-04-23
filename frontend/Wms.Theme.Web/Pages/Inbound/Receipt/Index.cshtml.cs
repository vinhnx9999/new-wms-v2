using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.Receipt;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Inbound.Receipt;

public class IndexModel(IReceiptService receiptService, IStringLocalizer<SharedResource> localizer) : PageModel
{
    private readonly IReceiptService _receiptService = receiptService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetDataTableAsync(
        int pageIndex = 1,
        int pageSize = SystemConfig.PAGE_SIZE,
        string text = "",
        string sortCol = "",
        string sortDir = "")
    {
        var searches = new List<SearchObject>();
        if (!string.IsNullOrEmpty(sortCol))
        {
            searches.Add(new SearchObject
            {
                Name = sortCol,
                Sort = sortDir.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? 2 : 1

            });
        }
        if (!string.IsNullOrEmpty(text))
        {
            var groupId = Guid.NewGuid().ToString();

            searches.Add(new SearchObject
            {
                Name = "ReceiptNo",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupId,
            });

            searches.Add(new SearchObject
            {
                Name = "SupplierName",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupId,
            });

            searches.Add(new SearchObject
            {
                Name = "WarehouseName",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupId,
            });

            searches.Add(new SearchObject
            {
                Name = "ReceiptType",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupId,
            });
        }

        PageSearchRequest pageSearch = SearchUtil.GetPageSearch(searches, pageIndex, pageSize);
        var result = await _receiptService.PageSearchReceipt(pageSearch);
        return new JsonResult(result);

    }
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _receiptService.CancelReceipt(id);
        if (result)
        {
            return new JsonResult(new { success = true, message = _localizer["DeleteSuccess"].Value });
        }
        else
        {
            return new JsonResult(new { success = false, message = _localizer["DeleteFailed"].Value });
        }
    }
    public async Task<IActionResult> OnPostRevertDataAsync(int id)
    {
        var result = await _receiptService.RevertReceipt(id);
        if (result)
        {
            return new JsonResult(new { success = true, message = _localizer["Success"].Value });
        }
        else
        {
            return new JsonResult(new { success = false, message = _localizer["RevertFailed"].Value });
        }
    }

    public async Task<IActionResult> OnPostCloneDataAsync(int id)
    {
        var result = await _receiptService.CloneReceipt(id);
        if (result)
        {
            return new JsonResult(new { success = true, message = _localizer["Success"].Value });
        }
        else
        {
            return new JsonResult(new { success = false, message = _localizer["RevertFailed"].Value });
        }
    }

    public IActionResult OnGetDownloadTemplate()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "TemplateInbound.xlsx");

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return File(stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "TemplateInbound.xlsx");
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
            var inputOrders = await fileUtil.ReadSheetInboundOrderAsync(ExcelFile, startRow);
            if (inputOrders != null && inputOrders.Count > 0)
            {
                (int? data, string? message) = await _receiptService.ImportExcelData(inputOrders);
                return new JsonResult(data.HasValue && data.Value > 0
                   ? new { success = true, id = data.Value }
                   : new { success = false, message = message ?? "Failed to import excel" });
            }
        }

        return new JsonResult(new { success = false, message = "Invalid request data" });
    }

}
