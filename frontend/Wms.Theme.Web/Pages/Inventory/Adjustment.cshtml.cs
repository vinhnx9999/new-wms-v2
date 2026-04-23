using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.StockAdjust;
using Wms.Theme.Web.Services.StockProcess;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Inventory;

public class AdjustmentModel(IStockAdjustServices service,
    IStockProcessService stockProcessService,
    IStringLocalizer<SharedResource> localizer) : PageModel
{
    public int PendingCount { get; set; } = 5;
    public int ApprovedTodayCount { get; set; } = 12;
    public int TotalLossQuantity { get; set; } = 35;

    private readonly IStockAdjustServices _service = service;
    private readonly IStockProcessService _stockProcessService = stockProcessService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;
    public IStringLocalizer<SharedResource> L => _localizer;

    public async Task OnGet()
    {
        var stats = await _stockProcessService.GetDashboardStatsAsync();
        PendingCount = stats.PendingCount;
        ApprovedTodayCount = stats.ApprovedTodayCount;
        TotalLossQuantity = stats.TotalLossQuantity;
    }

    public async Task<IActionResult> OnGetTableData(string keyword, int pageIndex = 1, 
        int pageSize = SystemConfig.PAGE_SIZE)
    {
        var searchList = new List<SearchObject>();
        keyword = (keyword ?? "").ToLower().Trim();

        if (!string.IsNullOrEmpty(keyword))
        {
            var filterCondition = new List<SearchObject>
            {
                //new() {
                //    Label = "textSearch",
                //    Name = "textSearch",
                //    Value = keyword,
                //    Operator = Operators.Contains,
                //    Text = keyword,
                //},
                new() {
                    Label = "job_code",
                    Name = "job_code",
                    Value = keyword,
                    Operator = Operators.Contains,
                    Text = keyword,
                    Group = "GroupFilter"
                },
                new() {
                    Label = "creator",
                    Name = "creator",
                    Value = keyword,
                    Operator = Operators.Contains,
                    Text = keyword,
                    Group = "GroupFilter"
                },
                new() {
                    Label = "processor",
                    Name = "processor",
                    Value = keyword,
                    Operator = Operators.Contains,
                    Text = keyword,
                    Group = "GroupFilter"
                }
            };

            searchList.AddRange(filterCondition);
        }

        var result = await _service.PageProcessingAsync(new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            searchObjects = searchList
        });

        if ((result.Data?.Rows ?? []).Count  < 1) 
        {
            return new JsonResult(new
            {
                status = true,
                data = new
                {
                    total = 0,
                    pageIndex,
                    totalPages = 0
                }
            });
        }

        var rowData = (result.Data?.Rows ?? []).Select(x => new
        {
            id = x.id,
            job_code = x.job_code,
            job_type = (x.job_type ? L["Adjustment"] : L["Other"]).Value, // boolean
            creator = x.creator,
            create_time = x.create_time.Convert2LocalTime(),
            processor = x.processor,
            qty = x.qty,
            series_number = x.series_number,
            process_time = x.process_time?.Year > 2000 ? x.process_time?.Convert2LocalTime() : null,
            adjust_status = x.adjust_status, // boolean
            process_status = x.process_status
        }).ToList();

        return new JsonResult(new
        {
            status = true,
            data = new
            {
                rows = rowData,
                total = result.Data?.Totals,
                pageIndex,
                totalPages = (int)Math.Ceiling((result.Data?.Totals ?? 0) / (decimal)pageSize)
            }
        });
    }

    public async Task<IActionResult> OnPostRemoveAdjustment(int id)
    {
        var result = await _service.DeleteStockProcessAsync(id);
        if (result.StartsWith("ERROR:"))
        {
            return new JsonResult(new { success = false, message = result.Replace("ERROR: ", "") });
        }
        return new JsonResult(new { success = true, message = L["DeleteSuccess"] });
    }
}