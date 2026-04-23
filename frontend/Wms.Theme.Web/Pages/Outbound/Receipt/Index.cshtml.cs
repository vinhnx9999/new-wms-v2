using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.Receipt;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Outbound.Receipt
{
    public class IndexModel(IStringLocalizer<SharedResource> localizer, IOutboundReceiptService outboundReceiptService) : PageModel
    {
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;
        private readonly IOutboundReceiptService _outboundReceiptService = outboundReceiptService;

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
            var pageSearchRequest = new PageSearchRequest
            {
                pageIndex = pageIndex,
                pageSize = pageSize,
                sqlTitle = "",
                searchObjects = new List<SearchObject>(),
            };

            if (!string.IsNullOrEmpty(sortCol))
            {
                pageSearchRequest.searchObjects.Add(new SearchObject
                {
                    Name = sortCol,
                    Sort = sortDir?.ToLower() == "desc" ? 2 : 1
                });
            }

            if (!string.IsNullOrEmpty(text))
            {
                var groupId = Guid.NewGuid().ToString();

                pageSearchRequest.searchObjects.Add(new SearchObject
                {
                    Name = "ReceiptNumber",
                    Operator = Operators.Contains,
                    Text = text,
                    Value = text,
                    Group = groupId,
                });

                pageSearchRequest.searchObjects.Add(new SearchObject
                {
                    Name = "CustomerName",
                    Operator = Operators.Contains,
                    Text = text,
                    Value = text,
                    Group = groupId,
                });

                pageSearchRequest.searchObjects.Add(new SearchObject
                {
                    Name = "WarehouseName",
                    Operator = Operators.Contains,
                    Text = text,
                    Value = text,
                    Group = groupId,
                });

                pageSearchRequest.searchObjects.Add(new SearchObject
                {
                    Name = "Type",
                    Operator = Operators.Contains,
                    Text = text,
                    Value = text,
                    Group = groupId,
                });

                pageSearchRequest.searchObjects.Add(new SearchObject
                {
                    Name = "OutBoundGatewayName",
                    Operator = Operators.Contains,
                    Text = text,
                    Value = text,
                    Group = groupId,
                });
            }

            var result = await _outboundReceiptService.PageSearchReceipt(pageSearchRequest);
            return new JsonResult(result);

        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _outboundReceiptService.CancelReceipt(id);
            if (result)
            {
                return new JsonResult(new { success = true, message = _localizer["DeleteSuccess"].Value });
            }
            else
            {
                return new JsonResult(new { success = false, message = _localizer["DeleteFailed"].Value });
            }
        }

        public async Task<IActionResult> OnPostCloneDataAsync(int id)
        {
            var result = await _outboundReceiptService.CloneReceipt(id);
            if (result)
            {
                return new JsonResult(new { success = true, message = _localizer["Success"].Value });
            }
            else
            {
                return new JsonResult(new { success = false, message = _localizer["RevertFailed"].Value });
            }
        }

        public async Task<IActionResult> OnPostRevertDataAsync(int id)
        {
            var result = await _outboundReceiptService.RevertReceipt(id);
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
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "TemplateOutbound.xlsx");

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "TemplateOutbound.xlsx");
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
                var inputOrders = await fileUtil.ReadSheetOutboundOrderAsync(ExcelFile, startRow);
                if (inputOrders != null && inputOrders.Count > 0)
                {
                    (int? data, string? message) = await _outboundReceiptService.ImportExcelData(inputOrders);
                    return new JsonResult(data.HasValue && data.Value > 0
                       ? new { success = true, id = data.Value }
                       : new { success = false, message = message ?? "Failed to import excel" });
                }
            }

            return new JsonResult(new { success = false, message = "Invalid request data" });
        }
    }
}
