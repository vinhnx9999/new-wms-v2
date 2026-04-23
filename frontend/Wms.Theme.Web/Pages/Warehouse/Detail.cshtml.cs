using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Wms.Theme.Web.Model.InboundReceipt;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Sku;
using Wms.Theme.Web.Model.Warehouse;
using Wms.Theme.Web.Services.Category;
using Wms.Theme.Web.Services.GoodLocations;
using Wms.Theme.Web.Services.Pallet;
using Wms.Theme.Web.Services.Receipt;
using Wms.Theme.Web.Services.Sku;
using Wms.Theme.Web.Services.Stock;
using Wms.Theme.Web.Services.Supplier;
using Wms.Theme.Web.Services.Warehouse;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Warehouse;

public class DetailModel(
    IGoodLocationService locationService,
    IStockService stockService,
    IWarehouseService warehouseService,
    ISupplierService supplierService,
    ICategoryService categoryService,
    IReceiptService receiptService,
    ILogger<DetailModel> logger,
    ISkuService skuService,
    IPalletService palletService) : PageModel
{
    private readonly ILogger<DetailModel> _logger = logger;
    private readonly IGoodLocationService _locationService = locationService;
    private readonly IStockService _stockService = stockService;
    private readonly IWarehouseService _warehouseService = warehouseService;
    private readonly ISupplierService _supplierService = supplierService;
    private readonly ICategoryService _categoryService = categoryService;
    private readonly IReceiptService _receiptService = receiptService;
    private readonly ISkuService _skuService = skuService;
    private readonly IPalletService _palletService = palletService;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }
    [BindProperty(SupportsGet = true)]
    public WarehouseGeneralInfo GeneralInfo { get; set; } = new();
    [BindProperty(SupportsGet = true)]
    public IEnumerable<WcsLocationDto> WcsLocations { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public IEnumerable<WcsBlockLocationDto> WcsBlocks { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public WarehouseLocationInfo WHLocations { get; set; } = new();

    //[BindProperty(SupportsGet = true)]
    //public IEnumerable<StoreLocationDto> VirtualLocations { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public IEnumerable<StoreRuleSettingsDto> RuleSettings { get; set; } = [];    

    [BindProperty(SupportsGet = true)]
    public WarehouseRuleInfo GeneralRuleInfo { get; set; } = new();
    [BindProperty(SupportsGet = true)]
    public InventoryInfo InventoryInfo { get; set; } = new();
    [BindProperty(SupportsGet = true)]
    public string BlockSelect { get; set; } = "";

    public IEnumerable<SkuSafetyStockDto> SkuSafetyStocks { get; set; } = [];

    public List<SelectListItem> OptionBlocks
    {
        get
        {
            var rs = new List<SelectListItem>();
            foreach (var wcsBlock in WcsBlocks)
            {
                rs.Add(new SelectListItem
                {
                    Value = $"{wcsBlock.Id}",
                    Text = string.IsNullOrEmpty(wcsBlock.BlockCode) ? $"{wcsBlock.BlockName}" : $"[{wcsBlock.BlockCode}] {wcsBlock.BlockName}"
                });
            }
            return rs;
        }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (id <= 0) return NotFound();

        Id = id;

        SkuSafetyStocks = await _warehouseService.GetSafetyStockConfig(id);
        var data = await _warehouseService.GetGeneralInfoAsync(id);
        GeneralInfo = data ?? new WarehouseGeneralInfo
        {
            Id = id,
            Name = "Warehouse Name Not Found"
        };

        InventoryInfo = await GetInventoryOverview(id);
        return Page();
    }

    private async Task<InventoryInfo> GetInventoryOverview(int id)
    {
        var condition = new PageSearchRequest
        {
            pageIndex = 1,
            pageSize = 100,
            sqlTitle = "",
            searchObjects = [],
        };

        condition.searchObjects.Add(new SearchObject
        {
            Name = "WarehouseId",
            Operator = Operators.Equal,
            Value = id
        });

        var inbound = await _warehouseService.GetInboundByWarehouse(id, condition);
        var outbound = await _warehouseService.GetOutboundByWarehouse(id, condition);
        var overview = await _warehouseService.GetInventoryOverview(id, condition);

        GeneralRuleInfo = await GetWarehouseRuleInfo(id);
        
        return new InventoryInfo
        {
            InboundInfo = inbound ?? [],
            OutboundInfo = outbound ?? [],
            Overview = overview ?? []
        };
    }

    private async Task<WarehouseRuleInfo> GetWarehouseRuleInfo(int id)
    {
        return new WarehouseRuleInfo
        {
            RuleSettings = await _warehouseService.GetSettingsAsync(id) ?? [],
            Floors = await _warehouseService.GetFloors(id),
        };
    }

    public async Task<IActionResult> OnGetReloadRuleSettings()
    {
        var results = await GetWarehouseRuleInfo(Id);
        GeneralRuleInfo = results;
        return new JsonResult(new { data = results });
    }

    public async Task<IActionResult> OnGetLoadSuppliers()
    {
        var result = await _supplierService.GetAllSuppliers();
        return new JsonResult(new { data = result });
    }

    public async Task<IActionResult> OnGetLoadSkuSelect()
    {
        var result = await _stockService.GetSkuSelect(new());
        return new JsonResult(new { data = result });
    }

    public async Task<IActionResult> OnGetWcsLocations(Guid? blockId = null)
    {
        var result = await _warehouseService.GetWcsLocationsAsnyc(blockId);
        WcsLocations = result ?? [];
        return new JsonResult(new { data = (result ?? []).OrderBy(x => x.Level).ThenBy(x => x.CoordX).ThenBy(x => x.CoordY) });
    }

    public async Task<IActionResult> OnGetWcsBlocks()
    {
        var result = await _locationService.GetWcsBlocksAsnyc();
        WcsBlocks = result ?? [];
        return new JsonResult(new { data = (result ?? []) });
    }

    public async Task<IActionResult> OnGetStoreLocations()
    {
        var locations = await _locationService.GetStoreLocationsAsync(Id);
        var storeLocations = (locations ?? []).Where(x => !x.VirtualLocation) ?? [];
        var data = new WarehouseLocationInfo
        {
            StoreLocations = storeLocations.OrderBy(x => x.Level)
                .ThenBy(x => x.CoordX).ThenBy(x => x.CoordY),
            Floors = await _warehouseService.GetFloors(Id)
        };

        return new JsonResult(data);
    }

    public async Task<IActionResult> OnGetReloadWcsLocations(Guid? blockId = null)
    {
        if (OptionBlocks.Count < 1)
        {
            var rsBlocks = await _locationService.GetWcsBlocksAsnyc();
            WcsBlocks = rsBlocks ?? [];
            if (!blockId.HasValue)
            {
                blockId = (rsBlocks ?? []).FirstOrDefault()?.Id.GuidPretty();
            }
        }
        else if (!blockId.HasValue && OptionBlocks.Count != 0)
        {
            blockId = OptionBlocks.FirstOrDefault()?.Value.GuidPretty();
        }

        var locations = await _warehouseService.GetWcsLocationsAsnyc(blockId);
        WcsLocations = locations ?? [];
        return Partial("Maps/_WcsMapLocations", locations);
    }

    public async Task<IActionResult> OnGetReloadMapLocations()
    {
        var locations = await _locationService.GetStoreLocationsAsync(Id);
        var storeLocations = (locations ?? []).Where(x => !x.VirtualLocation) ?? [];
        var data = new WarehouseLocationInfo
        {
            StoreLocations = storeLocations.OrderBy(x => x.Level)
                .ThenBy(x => x.CoordX).ThenBy(x => x.CoordY),
            Floors = await _warehouseService.GetFloors(Id)
        };

        return Partial("Maps/_MapPalletLocations", data);
    }

    public async Task<IActionResult> OnGetReloadInventory()
    {
        InventoryInfo = await GetInventoryOverview(Id);
        return Partial("Inventory/_InventoryInfo");
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

    public async Task<IActionResult> OnPostDeleteSettings([FromBody] DeleteStoreSettingsRequest request)
    {
        bool success = await _warehouseService.DeleteRuleSettings(Id, request.SettingRuleId ?? 0);
        if (success)
        {
            RuleSettings = await _warehouseService.GetSettingsAsync(Id);
            return new JsonResult(new { success = true, message = "Deleted successfully." });
        }
        else
        {
            return new JsonResult(new { success = false, message = "Failed to delete." });
        }
    }

    public async Task<IActionResult> OnPostSaveWcsLocations([FromBody] CreateStoreLocationRequest request)
    {
        if (request == null)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }

        if (!request.Locations.Any())
        {
            if (!string.IsNullOrEmpty(request.BlockId))
            {
                request.Locations = await _warehouseService.GetWcsLocationsAsnyc(request.BlockId);
            }
            else
            {
                return new JsonResult(new { success = false, message = "No locations provided" });
            }
        }

        var (id, message) = await _warehouseService.SaveWcsLocations(Id, request.Locations, request.BlockId);
        if (id <= 0)
        {
            return new JsonResult(new { success = false, message });
        }

        return new JsonResult(new { success = true });
    }

    public async Task<IActionResult> OnPostCreateSettings([FromBody] CreateStoreSettingsRequest request)
    {
        if (request == null)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }

        request.WarehouseId = Id;

        var (id, message) = await _warehouseService.CreateSettingsAsync(request);

        if (id <= 0)
        {
            return new JsonResult(new { success = false, message });
        }

        //var storeRuleSettings = await _warehouseService.GetSettingsAsync(Id);
        //RuleSettings = storeRuleSettings;
        return new JsonResult(new { success = true });
    }

    public IActionResult OnGetDownloadTemplate()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "TemplateSafetyStockConfig.xlsx");

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return File(stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "TemplateSafetyStockConfig.xlsx");
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
            var inputs = await fileUtil.ReadSheetSkuSafetyStockAsync(ExcelFile, startRow);
            if (inputs != null && inputs.Count > 0)
            {
                (int? data, string? message) = await _warehouseService.ImportExcelSafetyStock(Id, inputs);
                return new JsonResult(data.HasValue && data.Value > 0
                   ? new { success = true, id = data.Value }
                   : new { success = false, message = message ?? "Failed to import excel" });
            }
        }

        return new JsonResult(new { success = false, message = "Invalid request data" });
    }

    public async Task<JsonResult> OnPostUpdateSafetyData([FromBody] SkuSafetyStockDto skuSafety)
    {
        (int? data, string? message) = await _warehouseService.UpdateSkuSafetyStock(Id, skuSafety);
        return new JsonResult(data.HasValue && data.Value > 0
                   ? new { success = true, id = data.Value }
                   : new { success = false, message = message ?? "Failed to Update Safety Stock" });
    }

    public async Task<JsonResult> OnPostDeleteSafetyData(int skuSafetyId)
    {
        if (skuSafetyId < 1)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }

        (int? data, string? message) = await _warehouseService.DeleteSafetyData(Id, skuSafetyId);
        return new JsonResult(data.HasValue && data.Value > 0
                   ? new { success = true, id = data.Value }
                   : new { success = false, message = message ?? "Failed to Update Safety Stock" });
    }

    public async Task<JsonResult> OnPostSaveInboundReceipt([FromBody] SaveInboundReceiptClientDto clientRequest)
    {
        if (clientRequest is null)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }

        if (clientRequest.Items is null || clientRequest.Items.Count == 0)
        {
            return new JsonResult(new { success = false, message = "No item to save." });
        }

        var validItems = clientRequest.Items
            .Where(x => x.SkuId > 0 && x.SkuUomId > 0 && x.Quantity > 0)
            .ToList();

        if (validItems.Count == 0)
        {
            return new JsonResult(new { success = false, message = "Invalid item data." });
        }

        var firstSupplierId = validItems.FirstOrDefault()?.SupplierId ?? 0;


        var receiptNo = await _receiptService.GetNextReceiptCode();

        var palletCode = await _palletService.GetNextPalletCode();

        var request = new CreateReceiptRequest
        {
            ReceiptNo = receiptNo,
            WarehouseId = Id,
            SupplierId = firstSupplierId,
            Type = "MISC",
            IsStored = true,
            IsDraft = false,
            CreatedDate = DateTime.UtcNow,
            Description = string.Empty,
            Details = validItems.Select(x => new CreateReceiptDetailDto
            {
                SkuId = x.SkuId,
                SkuUomId = x.SkuUomId,
                Quantity = x.Quantity,
                LocationId = clientRequest.LocationId > 0 ? clientRequest.LocationId : null,
                ExpiryDate = x.ExpireDate,
                PalletCode = palletCode?.PalletCode,
                AsnId = null,
                SourceNumber = null
            }).ToList()
        };

        var (id, message) = await _receiptService.CreateNewReceiptAsync(request);
        if (id == 0)
        {
            return new JsonResult(new { success = false, message });
        }

        return new JsonResult(new { success = true, id });
    }

    public async Task<JsonResult> OnGetSearchSku(string? keyWord, 
        int pageIndex = SystemConfig.DEFAULT_INDEX, int pageSize = SystemConfig.MAX_PAGE_SIZE)
    {
        var pageSearch = new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            sqlTitle = "",
            searchObjects =
            [
                new() { Name = "SkuName", Value = keyWord ?? "", Text = keyWord ?? "", Operator = Operators.Contains, Label = "SkuName", Group = "Search" },
                new() { Name = "SkuCode", Value = keyWord ?? "", Text = keyWord ?? "", Operator = Operators.Contains, Label = "SkuCode", Group = "Search" },
                new() { Name = "SupplierName", Value = keyWord ?? "", Text = keyWord ?? "", Operator = Operators.Contains, Label = "SupplierName", Group = "Search" },
                new() { Name = "UnitName", Value = keyWord ?? "", Text = keyWord ?? "", Operator = Operators.Contains, Label = "UnitName", Group = "Search" }
            ]
        };

        var data = await _skuService.PageSearch(pageSearch, Id);
        return new JsonResult(data);
    }

    public async Task<JsonResult> OnGetSearchSupplier(string? keyword, 
        int pageIndex = SystemConfig.DEFAULT_INDEX, int pageSize = SystemConfig.MAX_PAGE_SIZE)
    {
        var request = new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            sqlTitle = "",
            searchObjects =
        [
            new() { Name = "SupplierName", Value = keyword ?? "", Text = keyword ?? "", Operator = Operators.Contains, Label = "SupplierName", Group = "Search" },
            new() { Name = "ContactTel",  Value = keyword ?? "", Text = keyword ?? "", Operator = Operators.Contains, Label = "ContactTel",  Group = "Search" },
            new() { Name = "City",        Value = keyword ?? "", Text = keyword ?? "", Operator = Operators.Contains, Label = "City",        Group = "Search" },
            new() { Name = "Address",     Value = keyword ?? "", Text = keyword ?? "", Operator = Operators.Contains, Label = "Address",     Group = "Search" },
            new() { Name = "Email",       Value = keyword ?? "", Text = keyword ?? "", Operator = Operators.Contains, Label = "Email",       Group = "Search" }
        ]
        };

        var data = await _supplierService.PageSearchAsync(request);
        return new JsonResult(data);
    }
}

public sealed class SaveInboundReceiptClientDto
{
    public int? LocationId { get; set; }
    public string? LocationAddress { get; set; }
    public List<SaveInboundReceiptItemClientDto> Items { get; set; } = [];
}

public sealed class SaveInboundReceiptItemClientDto
{
    public string? Sku { get; set; }
    public int SkuId { get; set; }
    public int SkuUomId { get; set; }
    public decimal Quantity { get; set; }
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public DateTime? ExpireDate { get; set; }
}
