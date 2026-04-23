using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Model.Delivery;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Sorted;
using Wms.Theme.Web.Model.Unload;
using Wms.Theme.Web.Services.Asn;
using Wms.Theme.Web.Services.GoodLocations;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Inbound.Process;

public class IndexModel(IAsnService asnService, IGoodLocationService goodLocationService, IStringLocalizer<SharedResource> localizer) : PageModel
{
    private readonly IAsnService _asnService = asnService;
    private readonly IGoodLocationService _goodLocationService = goodLocationService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public Dictionary<string, int> TabCounts { get; set; } = new Dictionary<string, int>
    {
        { "delivery", 0 },
        { "unload", 0 },
        { "qc", 0 },
        { "putaway", 0 }
    };
    public void OnGet()
    {

    }

    public async Task<IActionResult> OnGetTotalTab()
    {
        var result = await _asnService.GetTotalRecord();
        var responseData = new
        {
            delivery = result.TryGetValue(0, out int value) ? value : 0,
            unload = result.TryGetValue(1, out int value1) ? value1 : 0,
            qc = result.TryGetValue(2, out int value2) ? value2 : 0,
            putaway = result.TryGetValue(3, out int value3) ? value3 : 0
        };
        return new JsonResult(responseData);
    }


    public async Task<IActionResult> OnGetTabCountsAsync()
    {
        return await OnGetTotalTab();
    }

    public async Task<IActionResult> OnGetAvailableLocationsAsync()
    {
        try
        {
            var locationList = new List<object>();
            return new JsonResult(new { success = true, data = locationList });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnGetDataTableAsync(string handlerName, int pageIndex = 1, 
        int pageSize = SystemConfig.PAGE_SIZE, string text = "")
    {
        var searchObjects = new List<SearchObject>();
        string sqlTitle = "";

        // Universal text search (Grouped OR condition)
        if (!string.IsNullOrWhiteSpace(text))
        {
            var groupID = Guid.NewGuid().ToString();
            searchObjects.Add(new SearchObject
            {
                Name = "asn_no",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupID
            });

            searchObjects.Add(new SearchObject
            {
                Name = "goods_owner_name",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupID
            });
            searchObjects.Add(new SearchObject
            {
                Name = "asn_no",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupID
            });
            searchObjects.Add(new SearchObject
            {
                Name = "supplier_name",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupID
            });
            searchObjects.Add(new SearchObject
            {
                Name = "goods_owners_name",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupID
            });
            searchObjects.Add(new SearchObject
            {
                Name = "sku_code",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupID
            });
            searchObjects.Add(new SearchObject
            {
                Name = "sku_name",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupID
            });
        }

        switch (handlerName)
        {
            case "DeliveryTable":
                sqlTitle = "asn_status:0";
                break;
            case "UnLoadTable":
                sqlTitle = "asn_status:1";
                break;
            case "QcTable":
                sqlTitle = "asn_status:2";
                break;
            case "PutAwayTable":
                // handle for using 3 and 4 for PutAway
                sqlTitle = "asn_status:waiting";
                break;
            default:
                return new JsonResult(new { rows = new List<object>(), pagination = new { total = 0 } });
        }

        var response = await _asnService.GetAsnListAsync(new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            sqlTitle = sqlTitle,
            searchObjects = searchObjects
        });

        var rows = response.Data?.Rows.Select(asn => new Dictionary<string, object>
        {
            ["Id"] = asn.Id,
            ["AsnNo"] = asn.AsnNo,
            ["SupplierName"] = asn.SupplierName,
            ["AsnStatus"] = asn.AsnStatus,
            ["StatusBadge"] = GetStatusBadge(asn.AsnStatus),
            ["EstimatedArrivalTime"] = asn.EstimatedArrivalTime.Convert2LocalTime(),
            ["GoodsOwnerName"] = asn.GoodsOwnerName,
            ["Price"] = asn.Price,
            ["SkuCode"] = asn.SkuCode,
            ["SkuName"] = asn.SkuName,
            ["AsnQty"] = asn.AsnQty,
            ["SortedQty"] = asn.SortedQty,
            ["ActualQty"] = asn.ActualQty,
            ["ExpiryDate"] = asn.ExpiryDate.Convert2LocalDate(),
            ["ArrivalTime"] = asn.ArrivalTime.Convert2LocalDate(),
            ["UnloadTime"] = asn.UnloadTime.Convert2LocalDate(),
        }).ToList();

        return new JsonResult(new
        {
            status = true,
            data = new
            {
                rows = rows ?? [],
                total = response.Data?.Totals ?? 0,
                pageIndex,
                totalPages = (int)Math.Ceiling((double)(response.Data?.Totals ?? 0) / pageSize)
            }
        });
    }

    #region Deliverry tab
    /// <summary>
    /// Handles the confirmation of a delivery.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult.</returns>
    public async Task<IActionResult> OnPostConfirmAsync([FromBody] List<ConfirmModel> confirmData)
    {
        if (confirmData == null || confirmData.Count == 0)
        {
            return new JsonResult(new { success = false, message = _localizer["NoDataValid"].Value });
        }
        var (success, message) = await _asnService.ConfirmForDeliveryAsnAsync(confirmData);

        if (success)
        {
            // Fixed frontend success message
            return new JsonResult(new { success = true, message = _localizer["ConfirmSuccess"].Value });
        }
        else
        {
            var errorMsg = string.IsNullOrEmpty(message) ? _localizer["ConfirmFailed"].Value : message;
            return new JsonResult(new { success = false, message = errorMsg });
        }
    }

    #endregion

    #region UnLoad tab

    /// <summary>
    /// Handles the confirmation of unload for multiple items.
    /// </summary>
    /// <param name="items">List of individual item requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<IActionResult> OnPostConfirmUnloadAsync([FromBody] List<UnloadItemRequest> items)
    {
        if (items == null || items.Count == 0)
            return new JsonResult(new { success = false, message = _localizer["NoAsnSelected"].Value });

        // Validate all items have unload person
        if (items.Any(i => string.IsNullOrWhiteSpace(i.UnloadPerson)))
            return new JsonResult(new { success = false, message = _localizer["UnloadPersonRequired"].Value });

        var confirmRequests = items.Select(item => new UnloadConfirmRequest
        {
            Id = item.Id,
            UnLoadTime = DateTime.UtcNow,
            UnloadPerson = item.UnloadPerson,
            InPutQty = item.InputQty,
        }).ToList();

        var (success, message) = await _asnService.ConfirmForUnLoadAsnAsync(confirmRequests);

        if (success)
        {
            return new JsonResult(new { success = true, message = _localizer["UnloadConfirmSuccess"].Value });
        }
        else
        {
            var errorMsg = string.IsNullOrEmpty(message) ? _localizer["UnloadConfirmFailed"].Value : message;
            return new JsonResult(new { success = false, message = errorMsg });
        }
    }

    public async Task<IActionResult> OnPostRejectUnloadAsync([FromBody] RejectDto data)
    {
        if (data.Ids == null || data.Ids.Count == 0)
            return new JsonResult(new { success = false, message = _localizer["NoAsnSelectedReject"].Value });

        var result = await _asnService.RejectForDeliveryAsnAsync(data.Ids);

        return result
            ? new JsonResult(new { success = true, message = _localizer["RejectSuccess"].Value })
            : new JsonResult(new { success = false, message = _localizer["RejectFailed"].Value });
    }

    #endregion

    #region QC tab

    public async Task<IActionResult> OnPostConfirmQcAsync([FromBody] QcActionDto data)
    {
        if (data.Ids == null || data.Ids.Count == 0) return new JsonResult(new { success = false, message = _localizer["NoAsnSelected"].Value });

        var (success, message) = await _asnService.ConfirmForSortedAsnAsync(data.Ids);

        if (success)
        {
            return new JsonResult(new { success = true, message = _localizer["ConfirmQcSuccess"].Value });
        }
        else
        {
            var errorMsg = string.IsNullOrEmpty(message) ? _localizer["ConfirmQcFailed"].Value : message;
            return new JsonResult(new { success = false, message = errorMsg });
        }
    }

    public async Task<IActionResult> OnPostRejectQcAsync([FromBody] QcActionDto data)
    {
        if (data.Ids == null || data.Ids.Count == 0) return new JsonResult(new { success = false, message = _localizer["NoAsnSelected"].Value });

        var (success, message) = await _asnService.RejectUnloadAsync(data.Ids);

        if (success)
        {
            return new JsonResult(new { success = true, message = _localizer["RejectQcSuccess"].Value });
        }
        else
        {
            var errorMsg = string.IsNullOrEmpty(message) ? _localizer["RejectQcFailed"].Value : message;
            return new JsonResult(new { success = false, message = errorMsg });
        }
    }

    public async Task<IActionResult> OnPostAddQcAsync([FromBody] AddQcDto data)
    {
        if (data.SortedQuantity < 1)
        {
            return new JsonResult(new { success = false, message = _localizer["InvalidQuantity"].Value });
        }

        var request = new AddAnsToAnsSortedRequest
        {
            Id = data.Id,
            IsAutoNum = data.IsAutoNum,
            SortedQuantity = data.SortedQuantity,
            ExpiryDate = data.ExpiryDate
        };

        var (success, message) = await _asnService.AddedAsnToAnsSortedAsync(new List<Model.Sorted.AddAnsToAnsSortedRequest> { request });

        // Ensure message has a default value if empty
        string responseMessage = !string.IsNullOrEmpty(message) ? message : (success ? _localizer["OperationSuccessful"].Value : _localizer["OperationFailed"].Value);

        return success
            ? new JsonResult(new { success = true, message = responseMessage })
            : new JsonResult(new { success = false, message = responseMessage });
    }


    public async Task<JsonResult> OnGetQcDetailsAsync(int asnId)
    {
        var result = await _asnService.GetAnsSortedByAnsIDAsync(asnId);
        return new JsonResult(result ?? []);
    }

    // API POST cập nhật
    public async Task<IActionResult> OnPostUpdateQcAsync([FromBody] UpdateQcDto data)
    {
        var result = await _asnService.UpdateAsnSortedAsync(data.Items);
        return result
            ? new JsonResult(new { success = true, message = _localizer["UpdateQcSuccess"].Value })
            : new JsonResult(new { success = false, message = _localizer["UpdateQcFailed"].Value });
    }

    #endregion

    #region PutAway tab

    public async Task<JsonResult> OnGetAsnPendingPutawayAsync(int asnId)
    {
        var putawayItems = await _asnService.GetAsnPendingPutawayAsync(asnId);
        return new JsonResult(putawayItems ?? []);
    }

    /// <summary>
    /// Get Good Locations available for pallet placement
    /// </summary>
    public async Task<JsonResult> OnGetGoodLocationsAsync()
    {
        var goodLocations = await _goodLocationService.GetAvailableLocationsForPalletAsync();
        return new JsonResult(goodLocations ?? []);
    }

    public async Task<IActionResult> OnPostUpdatePutawayAsync([FromBody] UpdatePutawayRequestWrapper requestData)
    {
        if (requestData?.UpdatePutawayRequests == null || requestData.UpdatePutawayRequests.Count == 0)
        {
            return new JsonResult(new { success = false, message = _localizer["NoPutawayData"].Value });
        }

        foreach (var item in requestData.UpdatePutawayRequests)
        {
            if (item.GoodLocationId <= 0)
                return new JsonResult(new { success = false, message = string.Format(_localizer["ItemNoLocation"].Value, item.SeriesNumber) });

            if (item.PutawayQuantity <= 0)
                return new JsonResult(new { success = false, message = string.Format(_localizer["ItemInvalidQuantity"].Value, item.SeriesNumber) });
        }

        var (result, flag, message) = await _asnService.UpdatePutaway(requestData.UpdatePutawayRequests);
        if (result)
        {
            var successMsg = _localizer["PutawaySuccess"].Value; // Fixed frontend message for success
            if (flag == 1)
            {
                var ids = requestData.UpdatePutawayRequests.Select(r => r.AsnId).Distinct().ToList();
                return new JsonResult(new { success = true, message = successMsg, flag = true, ids });
            }
            return new JsonResult(new { success = true, message = successMsg });
        }
        else
        {
            var errorMsg = string.IsNullOrEmpty(message) ? _localizer["PutawayFailed"].Value : message;
            return new JsonResult(new { success = false, message = errorMsg });
        }
    }

    public async Task<IActionResult> OnPostRejectPutawayAsync([FromBody] List<int> ids)
    {
        var (success, message) = await _asnService.RejectForSortedAsnAsync(ids);
        return success
            ? new JsonResult(new { success = true, message })
            : new JsonResult(new { success = false, message });
    }

    public async Task<IActionResult> OnPostConfirmRobotAsync([FromBody] List<int> ids)
    {
        var (success, message) = await _asnService.ConfirmRobotSuccessAsync(ids);
        return success ?
                new JsonResult(new { success = true, message = _localizer["OperationSuccessful"].Value }) :
                new JsonResult(new { success = false, message });
    }

    public async Task<JsonResult> OnGetShowQrCodeAsync([FromQuery] List<int> id)
    {
        List<GetAsnQrCodeRequest>? result = await _asnService.ShowQrCode(id);
        return new JsonResult(result);
    }

    public class UpdatePutawayRequestWrapper
    {
        public List<UpdatePutawayRequest> UpdatePutawayRequests { get; set; } = [];
    }

    #endregion

    // ASN TEXT EXTENSION SUPPORT FOR STATUS 
    private string GetStatusBadge(int status)
    {
        string badgeClass;
        string statusText;

        switch (status)
        {
            case 0: // New
                badgeClass = "bg-blue-100 text-blue-800";
                statusText = _localizer["New"].Value;
                break;
            case 1: // Delivery
                badgeClass = "bg-yellow-100 text-yellow-800";
                statusText = _localizer["Delivery"].Value;
                break;
            case 2: // Unload
                badgeClass = "bg-yellow-100 text-yellow-800";
                statusText = _localizer["Unload"].Value;
                break;
            case 3: // Putaway
                badgeClass = "bg-purple-100 text-purple-800";
                statusText = _localizer["Putaway"].Value;
                break;
            case 4: // Waiting Robot
                badgeClass = "bg-orange-100 text-orange-800";
                statusText = _localizer["WaitingRobot"].Value;
                break;
            case 5: // Completed
                badgeClass = "bg-green-100 text-green-800";
                statusText = _localizer["Completed"].Value;
                break;
            case 8: // Cancelled
                badgeClass = "bg-red-100 text-red-800";
                statusText = _localizer["Cancelled"].Value;
                break;
            default:
                badgeClass = "bg-gray-100 text-gray-800";
                statusText = status.ToString(); // Or Unknown
                break;
        }

        return $"<span class=\"inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium {badgeClass}\">{statusText}</span>";
    }
}
