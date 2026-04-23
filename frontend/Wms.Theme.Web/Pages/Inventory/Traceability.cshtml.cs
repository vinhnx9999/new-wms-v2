using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Components;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.Stock;

namespace Wms.Theme.Web.Pages.Inventory
{
    public class TraceabilityModel : PageModel
    {
        private readonly IStockService _stockService;

        public TraceabilityModel(IStockService stockService)
        {
            _stockService = stockService;
        }

        public void OnGet()
        {
            // Load dữ liệu ban đầu cho dropdown/filter nếu cần
        }

        // Thay đổi signature để nhận các tham số filter từ Ajax request
        public async Task<IActionResult> OnGetDataTable(
            int pageIndex = 1,
            int pageSize = 20, // Increase default page size for timeline
            string skuId = "", // Keeping param name for compatibility, but mapped to multiple fields
            string seriesNumber = "",
            string fromDate = "",
            string toDate = "")
        {
            // 1. Chuẩn bị Search Objects
            var searchObjects = new List<SearchObject>();

            // - Filter SKU (Search by Id, Code, or Name)
            if (!string.IsNullOrEmpty(skuId))
            {
                // Note: The backend 'sku_id' parameter name in search object might need adjustment
                // depending on how the backend StockService parses it. 
                // Since we added sku_name/sku_code columns to the query, we can try to filter by them.
                // However, the backend assumes 'sku_id' is an integer ID often.
                // Let's try to add a generic search term.
                // If backend logic is strict, we might need to change 'sku_id' to something else or use OR condition.
                
                // For now, assuming the user enters text, we send it as 'sku_code' OR 'sku_name' if the backend supported dynamic OR.
                // Since the current backend implementation iterates and adds WHERE clauses with AND by default (likely),
                // we should be careful. 
                
                // Let's assume the input is SKU Code for now as it's most common.
                 searchObjects.Add(new SearchObject { Name = "sku_code", Value = skuId, Operator = Operators.Contains });
                 // If you want to search by name too, we'd need backend support for OR group.
            }

            // - Filter Series Number
            if (!string.IsNullOrEmpty(seriesNumber))
            {
                searchObjects.Add(new SearchObject { Name = "series_number", Value = seriesNumber, Operator = Operators.Contains });
            }

            // - Filter Date Range
            if (!string.IsNullOrEmpty(fromDate))
            {
                searchObjects.Add(new SearchObject
                {
                    Name = "EventTime",
                    Text = fromDate,
                    Operator = Operators.GreaterThanOrEqual,
                    Type = "DATETIMEPICKER"
                });
            }

            if (!string.IsNullOrEmpty(toDate))
            {
                searchObjects.Add(new SearchObject
                {
                    Name = "EventTime",
                    Text = toDate,
                    Operator = Operators.LessThanOrEqual,
                    Type = "DATETIMEPICKER"
                });
            }

            // 2. Tạo Request
            var pageRequest = new PageSearchRequest
            {
                pageIndex = pageIndex,
                pageSize = pageSize,
                searchObjects = searchObjects
            };

            // 3. Gọi Service
            var result = await _stockService.GetProductLiftPageAsync(pageRequest);

            if (result?.Data == null)
            {
                return Content("<div class='p-4 text-center text-gray-500'>No data available</div>");
            }

            // 4. Return Partial View with the raw data model
            // Make sure the Partial View name matches exactly what you created
            return Partial("_TraceabilityTimeline", result.Data.Rows);
        }
    }
}