using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Models;
using Wms.Theme.Web.Services.PurchaseOrder;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.PurchaseOrder;

public class IndexModel(IPurchaseOrderService poService, IStringLocalizer<SharedResource> localizer) : PageModel
{
    private readonly IPurchaseOrderService _poService = poService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetDataTable(
        int pageIndex = 1,
        int pageSize = SystemConfig.PAGE_SIZE,
        string text = "",
        string orderDateFrom = "",
        string orderDateTo = "",
        string expectedDeliveryDateFrom = "",
        string expectedDeliveryDateTo = "")
    {
        var conditions = new List<SearchObject>();

        if (!string.IsNullOrWhiteSpace(text))
        {
            conditions.Add(new SearchObject
            {
                Name = "po_no",
                Operator = Operators.Contains,
                Text = text,
                Value = text
            });
        }

        if (!string.IsNullOrWhiteSpace(orderDateFrom))
        {
            conditions.Add(new SearchObject
            {
                Name = "order_date",
                Operator = Operators.GreaterThanOrEqual,
                Text = orderDateFrom,
                Value = orderDateFrom,
                Type = "DATEPICKER"
            });
        }

        if (!string.IsNullOrWhiteSpace(orderDateTo))
        {
            conditions.Add(new SearchObject
            {
                Name = "order_date",
                Operator = Operators.LessThanOrEqual,
                Text = orderDateTo,
                Value = orderDateTo,
                Type = "DATEPICKER"
            });
        }

        if (!string.IsNullOrWhiteSpace(expectedDeliveryDateFrom))
        {
            conditions.Add(new SearchObject
            {
                Name = "expected_delivery_date",
                Operator = Operators.GreaterThanOrEqual,
                Text = expectedDeliveryDateFrom,
                Value = expectedDeliveryDateFrom,
                Type = "DATEPICKER"
            });
        }

        if (!string.IsNullOrWhiteSpace(expectedDeliveryDateTo))
        {
            conditions.Add(new SearchObject
            {
                Name = "expected_delivery_date",
                Operator = Operators.LessThanOrEqual,
                Text = expectedDeliveryDateTo,
                Value = expectedDeliveryDateTo,
                Type = "DATEPICKER"
            });
        }

        var model = await _poService.GetPageAsync(new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            searchObjects = conditions
        });


        return new JsonResult(new
        {
            total = model.Data.Totals,
            data = model.Data.Rows,
        });
    }

    public async Task<IActionResult> OnPostDeleteAsync([FromBody] DeleteRequest request)
    {
        if (request.Id <= 0) return new JsonResult(new { success = false, message = _localizer["InvalidId"].Value });

        var response = await _poService.DeleteAsync(request.Id);

        if (response?.IsSuccess == true)
        {
            return new JsonResult(new { success = true, message = _localizer["DeletedSuccessfully"].Value });
        }

        var errorMessage = response?.ErrorMessage ?? _localizer["FailedToDelete"].Value;
        return new JsonResult(new { success = false, message = errorMessage });
    }

    public async Task<IActionResult> OnPostCloseAsync([FromBody] CloseRequest request)
    {
        if (request.Id <= 0) return new JsonResult(new { success = false, message = _localizer["InvalidId"].Value });

        var response = await _poService.CloseAsync(request.Id);

        if (response?.IsSuccess == true)
        {
            return new JsonResult(new { success = true, message = _localizer["ClosedSuccessfully"].Value });
        }

        var errorMessage = response?.ErrorMessage ?? _localizer["CannotClosePO"].Value;
        return new JsonResult(new { success = false, message = errorMessage });
    }
}

public class DeleteRequest : BodyBaseRequest
{
}

public class CloseRequest : BodyBaseRequest
{
}

