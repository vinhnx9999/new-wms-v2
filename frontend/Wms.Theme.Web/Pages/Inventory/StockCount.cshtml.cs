using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.GoodLocation;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Stock;
using Wms.Theme.Web.Pages.Warehouse;
using Wms.Theme.Web.Services.GoodLocations;
using Wms.Theme.Web.Services.Sku;
using Wms.Theme.Web.Services.Stock;
using Wms.Theme.Web.Services.Warehouse;

namespace Wms.Theme.Web.Pages.Inventory;

public class StockCountModel(IWarehouseService warehouseService,
                             IGoodLocationService goodLocationService,
                             IStockService stockService,
                             ISkuService skuService) : PageModel
{
    private readonly IWarehouseService _warehouseService = warehouseService;
    private readonly IGoodLocationService _goodLocationService = goodLocationService;
    private readonly IStockService _stockService = stockService;
    private readonly ISkuService _skuService = skuService;


    private const int PAGE_INDEX = 1;
    private const int PAGE_SIZE = 10;

    [BindProperty(SupportsGet = true)]
    public List<WarehouseInfoDTO> ListWarehouse { get; set; } = default!;

    public async Task OnGet()
    {
        var result = await _warehouseService.GetAllAsync();
        ListWarehouse = result.IsSuccess ? result.Data.Select(x => new WarehouseInfoDTO
        {
            Id = x.id,
            Name = x.WarehouseName,
            WcsBlockId = x.WcsBlockId,
        }).ToList() : [];
    }

    public async Task<JsonResult> OnGetLocationSyncLogs([FromQuery] int warehouseId)
    {
        if (warehouseId <= 0)
        {
            return new JsonResult(new { success = false, message = "Invalid warehouseId" });
        }

        var logs = await _stockService.GetLocationSyncLogsAsync(warehouseId);
        return new JsonResult(new { success = true, data = logs });
    }

    public async Task<JsonResult> OnGetLocationSyncConflictsByTrace(
      [FromQuery] string traceId,
      [FromQuery] string blockId,
      [FromQuery] string warehouseId)
    {
        if (string.IsNullOrWhiteSpace(traceId) ||
            string.IsNullOrWhiteSpace(blockId) ||
            string.IsNullOrWhiteSpace(warehouseId))
        {
            return new JsonResult(new { success = false, message = "Invalid parameters" });
        }

        var conflictKeys = await _stockService.GetLocationSyncConflictsByTraceIdAsync(traceId);
        if (conflictKeys.Count == 0)
        {
            return new JsonResult(new { success = true, rows = new List<object>() });
        }

        var taskLocationWcs = _goodLocationService.GetWcsPalletlocationAsync(blockId);
        var taskLocationWms = _stockService.GetLocationStockInfoAsync(warehouseId);
        await Task.WhenAll(taskLocationWcs, taskLocationWms);

        var rows = BuildCompareRows(taskLocationWcs.Result ?? [], taskLocationWms.Result ?? []);
        var filtered = FilterRowsByConflictKeys(rows, conflictKeys);

        return new JsonResult(new
        {
            success = true,
            traceId,
            rows = filtered,
            summary = new
            {
                total = filtered.Count,
                matched = filtered.Count(x => x.MatchStatus == "OK"),
                mismatched = filtered.Count(x => x.MatchStatus == "ERROR")
            }
        });
    }

    private static List<StockCompareRowDto> FilterRowsByConflictKeys(
      List<StockCompareRowDto> rows,
      List<LocationSyncConflictKeyDto> keys)
    {
        if (rows.Count == 0 || keys.Count == 0) return [];

        return rows.Where(r =>
        {
            var location = string.IsNullOrWhiteSpace(r.WmsLocation) ? r.WcsLocation : r.WmsLocation;
            var reason = r.ReasonCode;
            var wmsHasPallet = !string.IsNullOrWhiteSpace(r.WmsLocation);
            var wcsStatus = (byte)(string.IsNullOrWhiteSpace(r.WcsLocation) ? 0 : 1);

            return keys.Any(k =>
                string.Equals(k.LocationName?.Trim(), location?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(k.Reason?.Trim(), reason?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                k.WmsHasPallet == wmsHasPallet &&
                k.WcsStatus == wcsStatus);
        }).ToList();
    }

    public async Task<JsonResult> OnPostCancelPreviousLogs([FromBody] CancelPreviousLogsRequest request)
    {
        if (request is null || request.WarehouseId <= 0 || string.IsNullOrWhiteSpace(request.TraceId))
        {
            return new JsonResult(new { success = false, message = "Invalid request." });
        }

        var (success, canceledLogs, deletedConflicts, message) =
            await _stockService.CancelPreviousLogsAndDeleteConflictsAsync(request.WarehouseId, request.TraceId);

        return new JsonResult(new
        {
            success,
            message,
            canceledLogs,
            deletedConflicts
        });
    }

    public async Task<JsonResult> OnPostResolveWcsOnlyInbound([FromBody] ResolveWcsOnlyInboundRequest request)
    {
        var result = await _stockService.ResolveWcsOnlyInboundAsync(request);
        if (result)
        {
            return new JsonResult(new
            {
                success = true,
                message = "Successfully"
            });
        }

        return new JsonResult(new
        {
            success = false,
            message = "Not Successfully"
        });
    }

    public async Task<JsonResult> OnPostResolvePalletMergeSameLocation([FromBody] ResolvePalletMergeSameLocationRequest request)
    {
        var result = await _stockService.ResolvePalletMergeSameLocationAsync(request);

        if (result)
        {
            return new JsonResult(new
            {
                success = true,
                message = "Merge thành công."
            });
        }

        return new JsonResult(new
        {
            success = false,
            message = "Merge thất bại."
        });
    }

    public async Task<JsonResult> OnGetSkuSearch([FromQuery] int warehouseId, [FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new JsonResult(new { success = true, data = new List<object>() });
        }

        PageSearchRequest pageSearch = new PageSearchRequest
        {
            pageIndex = PAGE_INDEX,
            pageSize = PAGE_SIZE,
            sqlTitle = "",
            searchObjects = new List<SearchObject>
            {
                new SearchObject
                {
                    Name = "SkuName",
                    Value = keyword ?? "",
                    Text = keyword ?? "",
                    Operator = Operators.Contains,
                    Label = "SkuName",
                    Group = "Search"
                },
                new SearchObject
                {
                    Name = "SkuCode",
                    Value = keyword ?? "",
                    Text = keyword ?? "",
                    Operator = Operators.Contains,
                    Label = "SkuCode",
                    Group = "Search"
                },

            }
        };
        var data = await _skuService.PageSearch(pageSearch, warehouseId);
        return new JsonResult(new { success = true, data = data });
    }

    public async Task<JsonResult> OnPostResolveWmsOnlyClearLocation([FromBody] ResolveWmsOnlyClearLocationRequest request)
    {
        var result = await _stockService.ResolveWmsOnlyClearLocationAsync(request);
        return new JsonResult(new
        {
            success = result,
            message = result ? "Đã xóa toàn bộ hàng hóa tại vị trí." : "Xóa hàng hóa thất bại."
        });
    }


    public async Task<JsonResult> OnPostResolveLocationMismatch([FromBody] ResolveLocationMismatchRequest request)
    {
        var result = await _stockService.ResolveLocationMismatchAsync(request);
        return new JsonResult(new
        {
            success = result,
            message = result ? "Đã cập nhật vị trí WMS theo WCS." : "Cập nhật vị trí thất bại."
        });
    }

    private static string Norm(string? v) => (v ?? string.Empty).Trim().ToUpperInvariant();

    private static List<StockCompareRowDto> BuildCompareRows(
    List<PalletLocactionDTO> wcsData,
    List<LocationStockInfoDTO> wmsData)
    {
        var wcsByPallet = wcsData
            .Where(x => !string.IsNullOrWhiteSpace(x.PalletCode))
            .GroupBy(x => Norm(x.PalletCode))
            .ToDictionary(g => g.Key, g => g.First().CurrentAddress?.Trim() ?? string.Empty);

        var wmsByPallet = wmsData
            .Where(x => !string.IsNullOrWhiteSpace(x.PalletCode))
            .GroupBy(x => Norm(x.PalletCode))
            .ToDictionary(g => g.Key, g => g.ToList());

        var allPallets = wcsByPallet.Keys.Union(wmsByPallet.Keys).OrderBy(x => x).ToList();
        var rows = new List<StockCompareRowDto>();

        foreach (var palletKey in allPallets)
        {
            var hasWcs = wcsByPallet.TryGetValue(palletKey, out var wcsLocation);
            var hasWms = wmsByPallet.TryGetValue(palletKey, out var wmsRows);

            if (hasWcs && hasWms)
            {
                foreach (var wmsRow in wmsRows!)
                {
                    var isMatch = Norm(wcsLocation) == Norm(wmsRow.LocationName);
                    var reason = isMatch ? "MATCHED" : "LOCATION_MISMATCH";

                    var itemRows = (wmsRow.Items ?? [])
                        .Select(item => new StockCompareRowDto
                        {
                            Id = $"{palletKey}-{item.SkuId}-{item.ExpiryDate:yyyyMMdd}-{item.SupplierId}",
                            PalletCode = wmsRow.PalletCode,
                            WcsLocation = wcsLocation!,
                            WmsLocation = wmsRow.LocationName,
                            SkuCode = item.SkuCode,
                            SkuName = item.SkuName,
                            Qty = item.Quantity,
                            SkuId = item.SkuId,
                            SupplierId = item.SupplierId,
                            SupplierName = item.SupplierName,
                            ExpiryDate = item.ExpiryDate,
                            MatchStatus = isMatch ? "OK" : "ERROR",
                            ReasonCode = reason,
                            RowSpan = 1
                        }).ToList();

                    if (itemRows.Count == 0)
                    {
                        itemRows.Add(new StockCompareRowDto
                        {
                            Id = $"{palletKey}-EMPTY",
                            PalletCode = wmsRow.PalletCode,
                            WcsLocation = wcsLocation!,
                            WmsLocation = wmsRow.LocationName,
                            MatchStatus = isMatch ? "OK" : "ERROR",
                            ReasonCode = reason,
                            RowSpan = 1
                        });
                    }

                    itemRows[0].RowSpan = itemRows.Count;
                    rows.AddRange(itemRows);
                }

                continue;
            }

            if (hasWcs && !hasWms)
            {
                rows.Add(new StockCompareRowDto
                {
                    Id = $"{palletKey}-WCS_ONLY",
                    PalletCode = palletKey,
                    WcsPalletCode = palletKey,
                    WcsLocation = wcsLocation!,
                    MatchStatus = "ERROR",
                    ReasonCode = "WCS_ONLY",
                    RowSpan = 1
                });
                continue;
            }

            foreach (var wmsRow in wmsRows!)
            {
                var itemRows = (wmsRow.Items ?? []).Select(item => new StockCompareRowDto
                {
                    Id = $"{palletKey}-{item.SkuId}-{item.ExpiryDate:yyyyMMdd}-{item.SupplierId}",
                    PalletCode = wmsRow.PalletCode,
                    WmsPalletCode = wmsRow.PalletCode,
                    WcsLocation = string.Empty,
                    WmsLocation = wmsRow.LocationName,
                    SkuCode = item.SkuCode,
                    SkuName = item.SkuName,
                    Qty = item.Quantity,
                    SkuId = item.SkuId,
                    SupplierId = item.SupplierId,
                    SupplierName = item.SupplierName,
                    ExpiryDate = item.ExpiryDate,
                    MatchStatus = "ERROR",
                    ReasonCode = "WMS_ONLY",
                    RowSpan = 1
                }).ToList();

                if (itemRows.Count == 0)
                {
                    itemRows.Add(new StockCompareRowDto
                    {
                        Id = $"{palletKey}-WMS_ONLY-EMPTY",
                        PalletCode = wmsRow.PalletCode,
                        WmsPalletCode = wmsRow.PalletCode,
                        WmsLocation = wmsRow.LocationName,
                        MatchStatus = "ERROR",
                        ReasonCode = "WMS_ONLY",
                        RowSpan = 1
                    });
                }

                itemRows[0].RowSpan = itemRows.Count;
                rows.AddRange(itemRows);
            }
        }

        MergeSameLocationCrossPallet(rows);
        return rows;
    }

    private static void MergeSameLocationCrossPallet(List<StockCompareRowDto> rows)
    {
        var wcsOnly = rows.Where(x => x.ReasonCode == "WCS_ONLY" && !string.IsNullOrWhiteSpace(x.WcsLocation))
                          .GroupBy(x => Norm(x.WcsLocation))
                          .ToDictionary(g => g.Key, g => g.ToList());

        var wmsOnly = rows.Where(x => x.ReasonCode == "WMS_ONLY" && !string.IsNullOrWhiteSpace(x.WmsLocation))
                          .GroupBy(x => Norm(x.WmsLocation))
                          .ToDictionary(g => g.Key, g => g.ToList());

        var sameLocs = wcsOnly.Keys.Intersect(wmsOnly.Keys).ToList();
        foreach (var loc in sameLocs)
        {
            var left = wcsOnly[loc];
            var right = wmsOnly[loc];
            var pairCount = Math.Min(left.Count, right.Count);

            for (var i = 0; i < pairCount; i++)
            {
                var wcsRow = left[i];
                var wmsRow = right[i];

                rows.Remove(wcsRow);
                //rows.Remove(wmsRow);

                wmsRow.Id = $"PALLET_MERGE-{loc}-{i}-{wmsRow.Id}";
                wmsRow.ReasonCode = "PALLET_CODE_MERGE_CANDIDATE";
                wmsRow.PalletCode = $"{wcsRow.PalletCode} ⇄ {wmsRow.PalletCode}";
                wmsRow.WcsPalletCode = wcsRow.PalletCode;
                wmsRow.WcsLocation = wcsRow.WcsLocation;
                wmsRow.MatchStatus = "ERROR";
            }
        }
    }

    private sealed class StockCompareRowDto
    {
        public string Id { get; set; } = string.Empty;
        public string PalletCode { get; set; } = string.Empty;
        public string WcsPalletCode { get; set; } = string.Empty;
        public string WmsPalletCode { get; set; } = string.Empty;
        public string WcsLocation { get; set; } = string.Empty;
        public string WmsLocation { get; set; } = string.Empty;
        public string SkuCode { get; set; } = string.Empty;
        public string SkuName { get; set; } = string.Empty;
        public int Qty { get; set; }
        public int SkuId { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int RowSpan { get; set; }
        public string MatchStatus { get; set; } = "ERROR";
        public string ReasonCode { get; set; } = string.Empty;
    }

    public sealed class CancelPreviousLogsRequest
    {
        public int WarehouseId { get; set; }
        public string TraceId { get; set; } = string.Empty;
    }
}





