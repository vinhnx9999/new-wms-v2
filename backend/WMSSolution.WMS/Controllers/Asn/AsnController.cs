using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Asn;
using WMSSolution.WMS.IServices;

namespace WMSSolution.WMS.Controllers.Asn;

/// <summary>
/// asn controller
/// </summary>
/// <remarks>
/// constructor
/// </remarks>
/// <param name="asnService">asn Service</param>
/// <param name="stringLocalizer">Localizer</param>
[Route("asn")]
[ApiController]
[ApiExplorerSettings(GroupName = "WMS")]
public class AsnController(IAsnService asnService, IStringLocalizer<MultiLanguage> stringLocalizer) : BaseController
{
    #region Args

    /// <summary>
    /// asn Service
    /// </summary>
    private readonly IAsnService _asnService = asnService;

    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    #endregion

    #region Arrival list 
    /// <summary>
    /// Arrival list
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <returns></returns>
    [HttpPost("asnmaster/list")]
    public async Task<ResultModel<PageData<AsnmasterBothViewModel>>> PageAsnmasterAsync(PageSearch pageSearch)
    {
        var (data, totals) = await _asnService.PageAsnMasterAsync(pageSearch, CurrentUser);

        return ResultModel<PageData<AsnmasterBothViewModel>>.Success(new PageData<AsnmasterBothViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }
    /// <summary>
    /// This asn is already coming
    /// get Arrival list
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("asnmaster")]
    public async Task<ResultModel<AsnmasterBothViewModel>> GetAsnmasterAsync(int id)
    {
        var data = await _asnService.GetAsnmasterAsync(id, CurrentUser);
        if (data != null && data.id > 0)
        {
            return ResultModel<AsnmasterBothViewModel>.Success(data);
        }
        else
        {
            return ResultModel<AsnmasterBothViewModel>.Error(_stringLocalizer["not_exists_entity"]);
        }
    }

    /// <summary>
    /// add a new record 
    /// BR : new ASN is coming ans_status = 0
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <returns></returns>
    [HttpPost("asnmaster")]
    public async Task<ResultModel<int>> AddAsnmasterAsync(AsnmasterBothViewModel viewModel)
    {
        var (id, msg) = await _asnService.AddAsnmasterAsync(viewModel, CurrentUser);
        return id > 0 ? ResultModel<int>.Success(id) : ResultModel<int>.Error(msg);
    }

    /// <summary>
    /// Save draft ASN master (create ASN master + details only)
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <returns></returns>
    [HttpPost("asnmaster/draft")]
    public async Task<ResultModel<int>> SaveDraftAsync(AsnmasterBothViewModel viewModel)
    {
        var (id, msg) = await _asnService.SaveDraftAsync(viewModel, CurrentUser);
        return id > 0 ? ResultModel<int>.Success(id) : ResultModel<int>.Error(msg);
    }

    /// <summary>
    /// Submit ASN master (create ASN master + details + receipt + integration)
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <returns></returns>
    [HttpPost("asnmaster/submit")]
    public async Task<ResultModel<int>> SubmitOrderAsync(AsnmasterBothViewModel viewModel)
    {
        var (id, msg) = await _asnService.SubmitOrderAsync(viewModel, CurrentUser);
        return id > 0 ? ResultModel<int>.Success(id) : ResultModel<int>.Error(msg);
    }
    /// <summary>
    /// update record
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <returns></returns>
    [HttpPut("asnmaster")]
    public async Task<ResultModel<bool>> UpdateAsnmasterAsync(AsnmasterBothViewModel viewModel)
    {
        var (flag, msg) = await _asnService.UpdateAsnmasterAsync(viewModel, CurrentUser);
        return flag ? ResultModel<bool>.Success(flag) : ResultModel<bool>.Error(msg, 400, flag);
    }

    /// <summary>
    /// Mark ASN master as completed and create WCS tasks/history
    /// </summary>
    /// <param name="id">asn master id</param>
    [HttpPut("asnmaster/complete")]
    public async Task<ResultModel<string>> CompleteAsnmasterAsync(int id)
    {
        var (flag, msg) = await _asnService.UpdateOrderToCompletedAsync(id, CurrentUser);
        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// Retry Inbound with new location
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("submit-inbound-retry")]
    public async Task<ResultModel<string>> SubmitInboundRetryAsync([FromBody] RetryInboundItemRequest request)
    {
        var (flag, msg) = await _asnService.SubmitInboundRetryAsync(request, CurrentUser);
        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    [HttpDelete("asnmaster")]
    public async Task<ResultModel<string>> DeleteAsnmasterAsync(int id)
    {
        var (flag, msg) = await _asnService.DeleteAsnmasterAsync(id);
        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// Get next ASN No (generated)
    /// </summary>
    /// <returns></returns>
    [HttpGet("asnmaster/next-asn-no")]
    public async Task<ResultModel<string>> GetNextAsnNoAsync()
    {
        var nextNo = await _asnService.GetNextAsnNo();
        return ResultModel<string>.Success(nextNo);
    }

    #endregion

    #region Api
    /// <summary>
    /// page search, sqlTitle input asn_status:0 ~ 4
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns></returns>
    [HttpPost("list")]
    public async Task<ResultModel<PageData<AsnViewModel>>> PageAsync(PageSearch pageSearch, CancellationToken cancellationToken)
    {
        var (data, totals) = await _asnService.PageAsync(pageSearch, CurrentUser, cancellationToken);

        return ResultModel<PageData<AsnViewModel>>.Success(new PageData<AsnViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// get a record by id
    /// </summary>
    /// <returns>args</returns>
    [HttpGet]
    public async Task<ResultModel<AsnViewModel>> GetAsync(int id)
    {
        var data = await _asnService.GetAsync(id);
        if (data != null && data.id > 0)
        {
            return ResultModel<AsnViewModel>.Success(data);
        }
        else
        {
            return ResultModel<AsnViewModel>.Error(_stringLocalizer["not_exists_entity"]);
        }
    }
    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ResultModel<int>> AddAsync(AsnViewModel viewModel)
    {
        var (id, msg) = await _asnService.AddAsync(viewModel, CurrentUser);
        return id > 0 ? ResultModel<int>.Success(id) : ResultModel<int>.Error(msg);
    }

    /// <summary>
    /// update a record
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<ResultModel<bool>> UpdateAsync(AsnViewModel viewModel)
    {
        var (flag, msg) = await _asnService.UpdateAsync(viewModel);
        return flag ? ResultModel<bool>.Success(flag) : ResultModel<bool>.Error(msg, 400, flag);
    }

    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<ResultModel<string>> DeleteAsync(int id)
    {
        var (flag, msg) = await _asnService.DeleteAsync(id);
        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// Bulk modify Goodsowner
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <returns></returns>
    [HttpPut("bulk-modify-goods-owner")]
    public async Task<ResultModel<bool>> BulkModifyGoodsownerAsync(AsnBulkModifyGoodsOwnerViewModel viewModel)
    {
        var (flag, msg) = await _asnService.BulkModifyGoodsownerAsync(viewModel);
        return flag ? ResultModel<bool>.Success(flag) : ResultModel<bool>.Error(msg, 400, flag);
    }

    #endregion

    #region New Flow Api
    /// <summary>
    /// Confirm Delivery
    /// change the asn_status from 0 to 1
    /// </summary>
    /// <param name="viewModels">args</param>
    /// <returns></returns>
    [HttpPut("confirm")]
    [AllowAnonymous]
    public async Task<ResultModel<string>> ConfirmAsync([FromBody] List<AsnConfirmInputViewModel> viewModels)
    {
        var (flag, msg) = await _asnService.ConfirmAsync(viewModels);
        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// Cancel confirm, change asn_status 1 to 0
    /// </summary>
    /// <param name="idList">id list</param>
    /// <returns></returns>
    [HttpPut("confirm-cancel")]
    public async Task<ResultModel<string>> ConfirmCancelAsync([FromBody] List<int> idList)
    {
        var (flag, msg) = await _asnService.ConfirmCancelAsync(idList);
        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// Unload
    /// change the asn_status from 1 to 2
    /// </summary>
    /// <param name="viewModels">args</param>
    /// <returns></returns>
    [HttpPut("unload")]
    public async Task<ResultModel<string>> UnloadAsync(List<AsnUnloadInputViewModel> viewModels)
    {
        var (flag, msg) = await _asnService.UnloadAsync(viewModels, CurrentUser);
        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// Cancel unload
    /// change the asn_status from 2 to 1
    /// </summary>
    /// <param name="idList">id list</param>
    /// <returns></returns>
    [HttpPut("unload-cancel")]
    public async Task<ResultModel<string>> UnloadCancelAsync(List<int> idList)
    {
        var (flag, msg) = await _asnService.UnloadCancelAsync(idList);
        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// sorting， add a new asnsort record and update asn sorted_qty
    /// </summary>
    /// <param name="viewModels">args</param>
    /// <returns></returns>
    [HttpPut("sorting")]
    public async Task<ResultModel<string>> SortingAsync(List<AsnsortInputViewModel> viewModels)
    {
        var (flag, msg) = await _asnService.SortingAsync(viewModels, CurrentUser);
        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// get asnsorts list by asn_id
    /// </summary>
    /// <param name="asn_id">asn id</param>
    /// <returns></returns>
    [HttpGet("sorting")]
    public async Task<ResultModel<List<AsnsortViewModel>>> GetAsnsortsAsync(int asn_id)
    {
        var data = await _asnService.GetAsnsortsAsync(asn_id);
        return ResultModel<List<AsnsortViewModel>>.Success(data);
    }

    /// <summary>
    /// update or delete asnsorts data
    /// </summary>
    /// <param name="entities">data</param>
    /// <returns></returns>
    [HttpPut("sorting-modify")]
    public async Task<ResultModel<string>> ModifyAsnsortsAsync(List<AsnsortEntity> entities)
    {
        var (flag, msg) = await _asnService.ModifyAsnsortsAsync(entities, CurrentUser);
        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// Sorted
    /// change the asn_status from 2 to 3
    /// </summary>
    /// <param name="idList">id list</param>
    /// <returns></returns>
    [HttpPut("sorted")]
    public async Task<ResultModel<string>> SortedAsync(List<int> idList)
    {
        var (flag, msg) = await _asnService.SortedAsync(idList);
        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// Cancel sorted
    /// change the asn_status from 3 to 2
    /// </summary>
    /// <param name="idList">id list</param>
    /// <returns></returns>
    [HttpPut("sorted-cancel")]
    public async Task<ResultModel<string>> SortedCancelAsync(List<int> idList)
    {
        var (flag, msg) = await _asnService.SortedCancelAsync(idList);
        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// get pending putaway data by asn_id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("pending-putaway")]
    public async Task<ResultModel<List<AsnPendingPutawayViewModel>>> GetPendingPutawayDataAsync(int id)
    {
        var data = await _asnService.GetPendingPutawayDataAsync(id);
        data ??= [];
        return ResultModel<List<AsnPendingPutawayViewModel>>.Success(data);
    }

    /// <summary>
    /// PutAway
    /// </summary>
    /// <param name="viewModels">args</param>
    /// <returns></returns>
    [HttpPut("putaway")]
    public async Task<ResultModel<string>> PutAwayAsync(List<AsnPutAwayInputViewModel> viewModels)
    {
        var (flag, msg) = await _asnService.PutAwayAsync(viewModels, CurrentUser);
        return flag ? ResultModel<string>.Success(msg + "change_status:") :
                ResultModel<string>.Error(msg + "change_status:");
    }

    #endregion

    #region print series number
    /// <summary>
    /// print series number
    /// </summary>
    /// <param name="input">selected asn id</param>
    /// <returns></returns>
    [HttpPost("print-sn")]
    public async Task<ResultModel<List<AsnPrintSeriesNumberViewModel>>> GetAsnPrintSeriesNumberAsync(List<int> input)
    {
        var data = await _asnService.GetAsnPrintSeriesNumberAsync(input);
        return ResultModel<List<AsnPrintSeriesNumberViewModel>>.Success(data);
    }
    #endregion

    #region count the status records 
    /// <summary>
    /// count the status records
    /// </summary>
    /// <returns></returns>
    [HttpGet("status-count")]
    public async Task<ResultModel<Dictionary<int, int>>> GetAsnStatusTotalRecordAsync()
    {
        var data = await _asnService.GetAsnStatusTotalRecordAsync(CurrentUser);
        return ResultModel<Dictionary<int, int>>.Success(data);
    }

    /// <summary>
    /// confirm robot has success
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpPatch("confirm-robot")]
    public async Task<ResultModel<string>> ConfirmAsnByRobotAsync([FromBody] List<int> ids)
    {
        var tenantId = CurrentUser?.tenant_id ?? 1;
        var flag = await _asnService.ConfirmRobotHasSuccessAsync(ids, tenantId);
        return flag ? ResultModel<string>.Success("Confirm Robot Success") :
                ResultModel<string>.Error("Failed to confirm Robot");
    }

    #endregion
}