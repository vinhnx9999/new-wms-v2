using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.StockTaking;
using Wms.Theme.Web.Services.Stock;
using Wms.Theme.Web.Services.StockTaking;

namespace Wms.Theme.Web.Pages.Inventory.StockCount
{
    public class CreateModel : PageModel
    {
        private readonly IStockTakingService _stockTakingService;
        private readonly IStockService _stockServices;

        public CreateModel(IStockTakingService stockTakingService, IStockService stockServices)
        {
            _stockTakingService = stockTakingService;
            _stockServices = stockServices;
        }

        [BindProperty(SupportsGet = true)]
        public StocktakingBasicViewModel Input { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostCreateStockTakingService([FromBody] StocktakingBasicViewModel request)
        {
            var result = await _stockTakingService.AddStockTakingAsync(request);
            if (result)
            {
                return new JsonResult(new { success = true, message = "Stocktaking created successfully." });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Failed to create stocktaking." });
            }
        }

        public async Task<IActionResult> OnPostStockData(int pageIndex = 1, string keyWord = "")
        {
            var searchObjects = new List<SearchObject>();

            if (!string.IsNullOrEmpty(keyWord))
            {
                searchObjects.Add(new SearchObject
                {
                    Sort = 1,
                    Label = "sku_name",
                    Name = "sku_name",
                    Type = "string",
                    Operator = Operators.Contains,
                    Text = keyWord,
                    Value = keyWord,
                });
            }

            var result = await _stockServices.GetSelectPageAsync(new PageSearchRequest
            {
                pageIndex = pageIndex,
                pageSize = 10,
                searchObjects = searchObjects,
            });

            try
            {
                var total = result.Data.Totals;
                var data = result.Data.Rows;
                return new JsonResult(new { success = true, data = data, total = total });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error retrieving stock data." });
            }
        }
    }
}

