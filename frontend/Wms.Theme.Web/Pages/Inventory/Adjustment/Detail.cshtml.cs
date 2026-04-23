using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.StockAdjust;
using Wms.Theme.Web.Services.StockAdjust;
using Wms.Theme.Web.Services.StockProcess;

namespace Wms.Theme.Web.Pages.Inventory.Adjustment;

public class DetailModel(IStockAdjustServices service, IStockProcessService processService) : PageModel
{
    private readonly IStockAdjustServices _service = service;
    private readonly IStockProcessService _processService = processService;

    [BindProperty(SupportsGet = true)]
    public StockadjustViewModel StockModel { get; set; } = new StockadjustViewModel();

    public async Task OnGet(int id)
    {
        StockadjustViewModel? detail = await _service.GetDetailAsync(id);
        StockModel = new StockadjustViewModel();
        if (detail != null)
        {
            if (detail.id < 1) detail.id = id;
            var data = _processService.GetDetailAsync(detail.source_table_id);

            if (data.Result != null)
            {
                var processInfo = data.Result;
                detail.source_detail_list = processInfo.source_detail_list ?? [];
                detail.target_detail_list = processInfo.target_detail_list ?? [];
                detail.process_time = processInfo.process_time;
                detail.processor = processInfo.processor;
                detail.process_status = processInfo.process_status;
            }            

            StockModel = detail;
        }
        else
        {
            var data = _processService.GetDetailAsync(id);
            detail = new StockadjustViewModel();
            if (data.Result != null)
            {
                var processInfo = data.Result;
                detail.id = id;
                detail.qty = processInfo.source_detail_list?.Sum(x => x.qty) ?? 0;
                detail.source_detail_list = processInfo.source_detail_list ?? [];
                detail.target_detail_list = processInfo.target_detail_list ?? [];
                detail.process_time = processInfo.process_time;
                detail.processor = processInfo.processor;
                detail.process_status = processInfo.process_status;
                detail.adjust_status = processInfo.adjust_status;
                detail.creator = processInfo.creator;
                detail.processor = processInfo.processor;
                detail.create_time = processInfo.create_time;
                detail.last_update_time = processInfo.last_update_time;
            }

            StockModel = detail;
        }
    }

    public async Task<IActionResult> OnPostUpdateProcess([FromBody] StockadjustViewModel request)
    {
        var result = await _service.UpdateProcessAsync(request);
        return result ? 
            new JsonResult(new { success = true, message = "Process updated successfully." }): 
            new JsonResult(new { success = false, message = "Error in updating process." });
    }

    public async Task<IActionResult> OnPostApproveAdjust(int id)
    {
        var result = await _processService.ConfirmAdjustment(id);
        return result ? 
            new JsonResult(new { success = true, message = "Adjustment approved successfully." }) :
            new JsonResult(new { success = false, message = "Error in approving adjustment." });
    }

    public async Task<IActionResult> OnPostApproveProcess(int id)
    {
        var result = await _processService.ConfirmProcess(id);
        return result ? 
            new JsonResult(new { success = true, message = "Process approved successfully." }) :
            new JsonResult(new { success = false, message = "Error in approving process." });
    }
}