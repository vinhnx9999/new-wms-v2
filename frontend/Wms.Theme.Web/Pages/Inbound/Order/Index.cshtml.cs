using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.AsnMaster;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Inbound.Order;

public class IndexModel(IAsnMasterService asnMasterService, IStringLocalizer<SharedResource> localizer) : PageModel
{
    private readonly IAsnMasterService _asnMasterService = asnMasterService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetDataTableAsync(
        int pageIndex = 1,
        int pageSize = SystemConfig.PAGE_SIZE,
        string text = "")
    {
        var searchObjects = new List<SearchObject>();

        // Text search - search in order number, batch, goods owner name
        if (!string.IsNullOrEmpty(text))
        {
            var groupId = $"{Guid.NewGuid()}";
            searchObjects.Add(new SearchObject
            {
                Name = "asn_no",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupId,
            });

            searchObjects.Add(new SearchObject
            {
                Name = "asn_batch",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupId,
            });

            searchObjects.Add(new SearchObject
            {
                Name = "goods_owner_name",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupId,
            });
        }

        if (pageSize <= 1)
        {
            pageSize = SystemConfig.PAGE_SIZE;
        }


        var model = await _asnMasterService.GetAsnMasterListAsync(new ListPageModelRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            sqlTitle = "",
            searchObjects = searchObjects
        });

        var total = model?.Data?.Totals ?? 0;
        var tableModel = new
        {
            rows = model?.Data?.Rows.Select(order => (object)new
            {
                id = order.Id,
                orderNo = order.AsnNo,
                createdDate = order.CreateTime.Convert2LocalDate(),
                etaDate = order.EstimatedArrivalTime.Convert2LocalDate(),
                warehouse = order.WarehouseName,
                executor = order.Creator,
                reference = order.AsnBatch,
                orderType = (string?)null,
                status = order.AsnStatus,
                has_rejected_items = order.has_rejected_items
            }).ToList() ?? [],
            total,
            pageIndex,
            pageSize,
            totalPages = (int)Math.Ceiling((double)total / pageSize)
        };

        return new JsonResult(new { status = true, data = tableModel });
    }

    public async Task<IActionResult> OnPostDeleteAsync([FromBody] int id)
    {
        if (id <= 0)
        {
            return new JsonResult(new { success = false, message = _localizer["InvalidOrderID"].Value });
        }

        var result = await _asnMasterService.DeleteAsnMasterAsync(id);
        if (result?.IsSuccess == true)
        {
            return new JsonResult(new { success = true, message = result.Data ?? _localizer["DeleteSuccess"].Value });
        }

        return new JsonResult(new
        {
            success = false,
            message = result?.ErrorMessage ?? _localizer["DeleteFailed"].Value
        });

    }
}
