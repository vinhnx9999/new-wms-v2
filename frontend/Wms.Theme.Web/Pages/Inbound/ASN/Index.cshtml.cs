using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Wms.Theme.Web.Components;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Models;
using Wms.Theme.Web.Services.Asn;
using Wms.Theme.Web.Services.AsnMaster;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Inbound.ASN;

public class IndexModel(IAsnService asnService, IAsnMasterService asnMasterService, IStringLocalizer<SharedResource> localizer) : PageModel
{
    private readonly IAsnService _asnService = asnService;
    private readonly IAsnMasterService _asnMasterService = asnMasterService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    [BindProperty]
    public DataTableViewModel DataTableView { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int StatusFilter { get; set; } = -1;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetDataTableAsync(int pageIndex = 1,
        int pageSize = SystemConfig.PAGE_SIZE, string text = "",
        int status = -1, DateTime? expectedDeliveryDateFrom = null, DateTime? expectedDeliveryDateTo = null)
    {
        StatusFilter = status;
        var searchObjects = new List<SearchObject>();

        if (!string.IsNullOrEmpty(text))
        {
            var groupID = $"{Guid.NewGuid()}";
            searchObjects.Add(new SearchObject
            {
                Name = "asn_no",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupID,
            });

            searchObjects.Add(new SearchObject
            {
                Name = "asn_batch",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupID,
            });

            searchObjects.Add(new SearchObject
            {
                Name = "goods_owner_name",
                Operator = Operators.Contains,
                Text = text,
                Value = text,
                Group = groupID,
            });
        }

        if (expectedDeliveryDateFrom.HasValue)
        {
            searchObjects.Add(new SearchObject
            {
                Name = "estimated_arrival_time",
                Operator = Operators.GreaterThanOrEqual,
                Value = expectedDeliveryDateFrom.Value.ToString("yyyy-MM-dd"), // Ensure correct format
            });
        }

        if (expectedDeliveryDateTo.HasValue)
        {
            searchObjects.Add(new SearchObject
            {
                Name = "estimated_arrival_time",
                Operator = Operators.LessThanOrEqual,
                Value = expectedDeliveryDateTo.Value.ToString("yyyy-MM-dd"), // Ensure correct format
            });
        }

        //filter default status
        string sqlTitle = AsnStatusConstBinding.ALL; // Default
        sqlTitle = StatusFilter switch
        {
            0 => AsnStatusConstBinding.NEW,
            1 => AsnStatusConstBinding.DELIVERY,
            2 => AsnStatusConstBinding.UNLOAD,
            3 => AsnStatusConstBinding.PUTAWAY,
            4 => AsnStatusConstBinding.COMPLETED,
            8 => AsnStatusConstBinding.CANCELLED,
            _ => "",
        };

        if (pageSize <= 1) pageSize = SystemConfig.PAGE_SIZE;
        var model = await _asnMasterService.GetAsnMasterListAsync(new ListPageModelRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            sqlTitle = sqlTitle,
            searchObjects = searchObjects
        });

        // Map to pure JSON model for AlpineDataTable
        var total = model?.Data?.Totals ?? 0;
        var tableModel = new
        {
            rows = model?.Data?.Rows.Select(asn => (object)new
            {
                id = asn.Id,
                asnNo = asn.AsnNo,
                asnBatch = asn.AsnBatch,
                eta = asn?.EstimatedArrivalTime.Convert2LocalDate() ?? "",
                goodsOwnerName = asn?.GoodsOwnerName,
                asnStatus = asn?.AsnStatus
            }).ToList() ?? [],
            total,
            pageIndex,
            pageSize,
            totalPages = (int)Math.Ceiling((double)total / pageSize)
        };

        return new JsonResult(new { status = true, data = tableModel });
    }


    public async Task<IActionResult> OnPostDeleteAsync([FromBody] DeleteRequest request)
    {
        if (request.Id <= 0)
        {
            return new JsonResult(new { success = false, message = _localizer["InvalidASNID"].Value });
        }

        var response = await _asnService.DeleteAsnAsync(request.Id);
        if (response?.IsSuccess == true)
        {
            return new JsonResult(new { success = true, message = _localizer["ASNDeletedSuccessfully"].Value });
        }

        var errorMessage = response?.ErrorMessage ?? _localizer["FailedToDeleteASN"].Value;
        return new JsonResult(new { success = false, message = errorMessage });
    }

}

public class DeleteRequest : BodyBaseRequest
{

}

