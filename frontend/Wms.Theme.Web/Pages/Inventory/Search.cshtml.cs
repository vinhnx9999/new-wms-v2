using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.Category;
using Wms.Theme.Web.Services.Stock;
using Wms.Theme.Web.Services.Warehouse;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Inventory;

public class SearchModel(IStockService stockService,
    IWarehouseService wareHouseService,
    ICategoryService categoryService) : PageModel
{
    private readonly IStockService _stockService = stockService;
    private readonly IWarehouseService _wareHouseService = wareHouseService;
    private readonly ICategoryService _categoryService = categoryService;

    public void OnGet()
    {
    }

    public async Task<JsonResult> OnGetTableDataAsync(
       string? pStatus,
       string? pExpiry,
       string? pCategory,
       string? pKeyword,
       string pWarehouse,
       int pageIndex = 1,
       int pageSize = SystemConfig.PAGE_SIZE)
    {
        var searchObjects = new List<SearchObject>();

        if (!string.IsNullOrEmpty(pKeyword))
        {
            var group = Guid.NewGuid().ToString();
            searchObjects.Add(
                new SearchObject
                {
                    Label = "sku_name",
                    Name = "sku_name",
                    Value = pKeyword,
                    Operator = Operators.Contains,
                    Type = "text",
                    Text = pKeyword,
                    Group = group
                });

            searchObjects.Add(
                new SearchObject
                {
                    Label = "sku_code",
                    Name = "sku_code",
                    Value = pKeyword,
                    Operator = Operators.Contains,
                    Type = "text",
                    Text = pKeyword,
                    Group = group,
                });
        }

        var pageSearch = new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            searchObjects = searchObjects
        };

        var result = await _stockService.StockPageAsync(pageSearch);

        var dataList = result?.Data;
        var totalRecords = dataList?.Totals ?? 0;


        var rows = (dataList?.Rows ?? []).Select(item => new
        {
            id = item.sku_id,
            item.sku_code,
            item.sku_name,
            item.unit,
            item.category_name,
            item.spu_code,
            item.spu_name,
            item.qty,
            item.qty_available,
            item.qty_locked,
            item.qty_frozen,
            item.qty_asn,
            item.qty_to_sort,
            item.qty_sorted,
            item.warehouse_name,
            item.location_name,
            item.series_number,
            expiry_date = item.expiry_date.Convert2LocalDate()
        }).ToList();


        return new JsonResult(new
        {
            status = true,
            data = rows,
            totals = totalRecords
        });
    }

    public async Task<JsonResult> OnGetBadgeTotal()
    {
        var result = await _stockService.GetInventoryDashboardAsync();
        return new JsonResult(new
        {
            status = result.IsSuccess,
            message = result.ErrorMessage,
            data = result.Data
        });
    }

    public async Task<JsonResult> OnGetWarehouseSelectItems()
    {
        List<Model.Warehouse.FormSelectItem>? warehouseItems = await _wareHouseService.GetSelectItemsAsnyc();
        return new JsonResult(new
        {
            status = true,
            data = warehouseItems
        });
    }

    public async Task<JsonResult> OnGetCategorySelectItems()
    {
        List<Model.Category.CategoryViewModel>? categoryItems = await _categoryService.GetAllCategory();
        return new JsonResult(new
        {
            status = true,
            data = categoryItems
        });
    }

    public async Task<JsonResult> OnGetStockDetailAsync(int skuId, int pageSize = SystemConfig.PAGE_SIZE)
    {
        var searchObjects = new List<SearchObject>
        {
            new() {
                Name = "sku_id",
                Value = $"{skuId}",
                Operator = Operators.Equal,
                Type = "number"
            },
        };

        var pageSearch = new PageSearchRequest
        {
            pageIndex = 1,
            pageSize = pageSize,
            searchObjects = searchObjects
        };

        var result = await _stockService.LocationStockPageAsync(pageSearch);
        var dataList = result?.Data;

        var details = (dataList?.Rows ?? []).Select(item => new
        {
            item.sku_name,
            item.sku_code,
            item.location_name,
            item.warehouse_name,
            item.series_number,
            item.pallet_code,
            item.qty,
            item.qty_available,
            item.expiry_date,
            item.putaway_date,
            is_expired = item.expiry_date.IsExpired(),
            is_soon_expired = item.expiry_date.IsSoonExpired()
        }).ToList() ?? [];

        return new JsonResult(new
        {
            status = true,
            data = details
        });
    }
}

