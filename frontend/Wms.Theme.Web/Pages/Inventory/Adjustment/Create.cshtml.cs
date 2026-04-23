using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Entities.ViewModels;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.StockProcess;
using Wms.Theme.Web.Services.StockAdjust;
using Wms.Theme.Web.Services.StockProcess;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Inventory.Adjustment;

public class CreateModel(IStockAdjustServices stockAdjustService, 
    IStockProcessService stockProcessService) : PageModel
{
    private readonly IStockAdjustServices _stockAdjustService = stockAdjustService;
    private readonly IStockProcessService _stockProcessService = stockProcessService;

    public void OnGet()
    {

    }

    public async Task<JsonResult> OnGetStockData([FromQuery] string searchKey, [FromQuery] int pageIndex = 1)
    {
        int pageSize = SystemConfig.PAGE_SIZE;
        var searchObject = new List<SearchObject>();

        if (searchKey != null)
        {
            searchObject.Add(new SearchObject
            {
                Sort = 1,
                Label = "sku_name",
                Name = "sku_name",
                Type = "string",
                Operator = Operators.Contains,
                Text = searchKey,
                Value = searchKey,
            });
        }

        var request = new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            sqlTitle = "",
            searchObjects = searchObject
        };

        var response = await _stockAdjustService.GetSkuForAdjustmentSelectionAsync(request);
        
        if (response?.Data == null)
        {
            return new JsonResult(new { success = false, data = new List<SkuAdjustmentSelectionViewModel>(), totals = 0 });
        }

        var data = response.Data?.Rows ?? [];
        var totals = response.Data?.Totals ?? 0;

        return response.IsSuccess ? 
            new JsonResult(new { success = true, data, totals }) :
            new JsonResult(new { success = false, data, totals });
    }

    public async Task<JsonResult> OnPostAsync([FromBody] StockprocessViewModel request)
    {
        var result = await _stockProcessService.AddAsync(request);

        if (result != null && result.StartsWith("ERROR:"))
        {
            return new JsonResult(new { success = false, message = result.Replace("ERROR: ", "") });
        }

        return new JsonResult(new { success = true, message = "Tạo phiếu thành công! ID: " + result });
    }
}

