using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSSolution.Core.Controller;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Excel;
using WMSSolution.WMS.Entities.ViewModels.Receipt;
using WMSSolution.WMS.Entities.ViewModels.Receipt.Inbound;
using WMSSolution.WMS.Entities.ViewModels.Receipt.Outbound;
using WMSSolution.WMS.IServices.Receipt;

namespace WMSSolution.WMS.Controllers.Receipt;

    
/// <summary>    
/// receipt controller
/// </summary>    
[Route("receipt")]
[ApiController]
[ApiExplorerSettings(GroupName = "Base")]
public class ReceiptController(IReceiptService receiptService, IOutboundReceiptService outboundReceiptService) : BaseController
{
    private readonly IReceiptService _receiptService = receiptService;
    private readonly IOutboundReceiptService _outboundReceiptService = outboundReceiptService;

    /// <summary>
    /// Page inbound receipts
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>paged receipts</returns>
    [HttpPost("inbound/list")]
    public async Task<ResultModel<PageData<InboundReceiptListResponse>>> PageInboundReceiptAsync(PageSearch pageSearch, CancellationToken cancellationToken)
    {
        var (data, totals) = await _receiptService.PageInboundReceiptAsync(pageSearch, CurrentUser, cancellationToken);

        return ResultModel<PageData<InboundReceiptListResponse>>.Success(new PageData<InboundReceiptListResponse>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// Get deleted data Inbound for revert
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("inbound/deleted-data")]
    public async Task<ResultModel<IEnumerable<InboundReceiptListResponse>>> GetDeletedDataInbound(CancellationToken cancellationToken)
    {
        var data = await _receiptService.GetDeletedData(CurrentUser, cancellationToken);
        return ResultModel<IEnumerable<InboundReceiptListResponse>>.Success(data);
    }

    /// <summary>
    /// Get Share Inbound
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("share-inbounds/{id}")]
    public async Task<ResultModel<string>> GetShareInbound(int id, CancellationToken cancellationToken)
    {
        var data = await _receiptService.GetShareInbound(id, CurrentUser, cancellationToken);
        return ResultModel<string>.Success(data);
    }

    /// <summary>
    /// Get inbound receipt by ID
    /// </summary>
    /// <param name="id">receipt id</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>detailed receipt</returns>
    [HttpGet("inbound/{id}")]
    public async Task<ResultModel<InboundReceiptDetailedDto>> GetReceiptByIdAsync(int id, CancellationToken cancellationToken)
    {
        var data = await _receiptService.GetReceiptByIdAsync(id, CurrentUser, cancellationToken);
        if (data == null)
        {
            return ResultModel<InboundReceiptDetailedDto>.Error("Receipt not found");
        }
        return ResultModel<InboundReceiptDetailedDto>.Success(data);
    }

    /// <summary>
    /// Create new inbound receipt
    /// </summary>
    /// <param name="request">receipt creation request</param>
    /// <param name="cancellationToken" >cancellation token</param>
    /// <returns>created receipt id and message</returns>
    [HttpPost("inbound/create")]
    public async Task<ResultModel<int>> CreateAsync([FromBody] CreateReceiptRequest request, CancellationToken cancellationToken)
    {
        var (id, message) = await _receiptService.CreateAsync(request, CurrentUser, cancellationToken);
        if (id <= 0)
        {
            return ResultModel<int>.Error(message);
        }
        return ResultModel<int>.Success(id, message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("inbound/create-multi-pallets")]
    public async Task<ResultModel<int>> CreateNewBulkReceiptAsync([FromBody] CreateBulkReceiptRequest request, CancellationToken cancellationToken)
    {
        var (id, message) = await _receiptService.CreateBulkReceiptsAsync(request, CurrentUser, cancellationToken);
        if (id <= 0)
        {
            return ResultModel<int>.Error(message);
        }
        return ResultModel<int>.Success(id, message);
    }

    /// <summary>
    /// Update inbound receipt
    /// </summary>
    /// <param name="id">receipt id</param>
    /// <param name="request">receipt update request</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>update result</returns>
    [HttpPut("inbound/{id}")]
    public async Task<ResultModel<bool>> UpdateAsync([FromRoute] int id, [FromBody] UpdateInboundReceiptRequest request, CancellationToken cancellationToken)
    {
        (bool success, string message) = await _receiptService.UpdateAsync(id, request, CurrentUser, cancellationToken);
        return success ? ResultModel<bool>.Success(success) : ResultModel<bool>.Error(message);
    }

    /// <summary>
    /// return next receipt bumber
    /// </summary>
    /// <returns></returns>
    [HttpGet("inbound/next-receipt-no")]
    public async Task<ResultModel<string>> GetNextReceiptNoAsync()
    {
        var receiptNo = await _receiptService.GetNextReceiptNoAsync();
        return ResultModel<string>.Success(receiptNo);
    }

    /// <summary>
    /// Retry inbound receipt task
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("inbound/retry")]
    public async Task<ResultModel<bool>> RetryInboundAsync([FromBody] RetryInboundRequest request, CancellationToken cancellationToken)
    {
        (bool success, string message) = await _receiptService.RetryInboundDetailsAsync(request, CurrentUser, cancellationToken);
        return success ? ResultModel<bool>.Success(success) : ResultModel<bool>.Error(message);
    }

    /// <summary>
    /// Create new inbound receipt
    /// </summary>
    /// <param name="request">receipt creation request</param>
    /// <param name="cancellationToken" >cancellation token</param>
    /// <returns>created receipt id and message</returns>
    [HttpPost("outbound/create")]
    public async Task<ResultModel<int>> CreateOutboundAsync([FromBody] CreateOutboundReceiptRequest request, CancellationToken cancellationToken)
    {
        var (id, message) = await _outboundReceiptService.CreateAsync(request, CurrentUser, cancellationToken);
        if (id <= 0)
        {
            return ResultModel<int>.Error(message);
        }
        return ResultModel<int>.Success(id, message);
    }

    /// <summary>
    /// return next receipt bumber
    /// </summary>
    /// <returns></returns>
    [HttpGet("outbound/next-receipt-no")]
    public async Task<ResultModel<string>> GetNextOutboundReceiptNoAsync()
    {
        var receiptNo = await _outboundReceiptService.GetNextReceiptNoAsync();
        return ResultModel<string>.Success(receiptNo);
    }


    /// <summary>
    /// Get Share Outbound
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("share-outbounds/{id}")]
    public async Task<ResultModel<string>> GetShareOutbound(int id, CancellationToken cancellationToken)
    {
        var data = await _outboundReceiptService.GetShareOutbound(id, CurrentUser, cancellationToken);
        return ResultModel<string>.Success(data);
    }

    /// <summary>
    /// Page outbound receipts
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="cancellationToken"></param>
    /// <returns>paged receipts</returns>
    [HttpPost("outbound/list")]
    public async Task<ResultModel<PageData<OutboundReceiptListResponse>>> PageOutboundReceiptAsync(PageSearch pageSearch, CancellationToken cancellationToken)
    {
        var (data, totals) = await _outboundReceiptService.PageOutboundReceiptAsync(pageSearch, CurrentUser, cancellationToken);

        return ResultModel<PageData<OutboundReceiptListResponse>>.Success(new PageData<OutboundReceiptListResponse>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// Get deleted data Outbound for revert
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("outbound/deleted-data")]
    public async Task<ResultModel<IEnumerable<OutboundReceiptListResponse>>> GetDeletedDataOutbound(CancellationToken cancellationToken)
    {
        var data = await _outboundReceiptService.GetDeletedData(CurrentUser, cancellationToken);
        return ResultModel<IEnumerable<OutboundReceiptListResponse>>.Success(data);
    }

    /// <summary>
    /// Cancel receipt
    /// </summary>
    /// <param name="id">receipt id</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>cancellation result</returns>
    [HttpPatch("cancel/{id}")]
    public async Task<ResultModel<bool>> CancelAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        var (success, message) = await _receiptService.CancelAsync(id, CurrentUser, cancellationToken);
        return success ? ResultModel<bool>.Success(success, message) : ResultModel<bool>.Error(message);
    }

    /// <summary>
    /// Revert Inbound
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Roles = "SysAdmin,Admin")]
    [HttpPatch("revert/{id}")]
    public async Task<ResultModel<bool>> RevertInboundAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        var (success, message) = await _receiptService.RevertInbound(id, CurrentUser, cancellationToken);
        return success ? ResultModel<bool>.Success(success, message) : ResultModel<bool>.Error(message);
    }

    /// <summary>
    /// Clone Inbound Async
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("clone/{id}")]
    public async Task<ResultModel<bool>> CloneInboundAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        var (success, message) = await _receiptService.CloneInboundAsync(id, CurrentUser, cancellationToken);
        return success ? ResultModel<bool>.Success(success, message) : ResultModel<bool>.Error(message);
    }

    /// <summary>
    /// Clone Outbound
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("outbound/clone/{id}")]
    public async Task<ResultModel<bool>> CloneOutboundAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        var (success, message) = await _outboundReceiptService.CloneOutboundAsync(id, CurrentUser, cancellationToken);
        return success ? ResultModel<bool>.Success(success, message) : ResultModel<bool>.Error(message);
    }

    /// <summary>
    /// Get outbound receipt by ID
    /// </summary>
    /// <param name="id">The ID of the outbound receipt</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The detailed outbound receipt DTO</returns>
    [HttpGet("outbound/{id}")]
    public async Task<ResultModel<OutboundReceiptDetailedDto>> GetOutboundReceiptByIdAsync(int id, CancellationToken cancellationToken)
    {
        var data = await _outboundReceiptService.GetReceiptByIdAsync(id, CurrentUser, cancellationToken);
        if (data == null)
        {
            return ResultModel<OutboundReceiptDetailedDto>.Error("Receipt not found");
        }
        return ResultModel<OutboundReceiptDetailedDto>.Success(data);
    }

    /// <summary>
    /// Cancel receipt
    /// </summary>
    /// <param name="id">receipt id</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>cancellation result</returns>
    [HttpPatch("outbound/cancel/{id}")]
    public async Task<ResultModel<bool>> CancelOutboundAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        var (success, message) = await _outboundReceiptService.CancelAsync(id, CurrentUser, cancellationToken);
        return success ? ResultModel<bool>.Success(success, message) : ResultModel<bool>.Error(message);
    }

    /// <summary>
    /// Revert Outbound
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Roles = "SysAdmin,Admin")]
    [HttpPatch("outbound/revert/{id}")]
    public async Task<ResultModel<bool>> RevertOutboundAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        var (success, message) = await _outboundReceiptService.RevertOutboundAsync(id, CurrentUser, cancellationToken);
        return success ? ResultModel<bool>.Success(success, message) : ResultModel<bool>.Error(message);
    }

    /// <summary>
    /// Update outbound receipt
    /// </summary>
    /// <param name="id">receipt id</param>
    /// <param name="request">receipt update request</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>update result</returns>
    [HttpPut("outbound/{id}")]
    public async Task<ResultModel<bool>> UpdateOutboundAsync([FromRoute] int id, [FromBody] UpdateOutboundReceiptRequest request, CancellationToken cancellationToken)
    {
        (bool success, string message) = await _outboundReceiptService.UpdateAsync(id, request, CurrentUser, cancellationToken);
        return success ? ResultModel<bool>.Success(success) : ResultModel<bool>.Error(message);
    }

    /// <summary>
    /// Import Excel
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("inbound/import-excel")]
    public async Task<ResultModel<int>> ImportExcelData([FromBody] List<InboundOrderExcel> request, CancellationToken cancellationToken)
    {
        var result = await _receiptService.ImportExcelData(request, CurrentUser, cancellationToken);

        if (result <= 0)
        {
            return ResultModel<int>.Error("Failed to Import Excel Inbound Order");
        }
        return ResultModel<int>.Success(result);
    }

    /// <summary>
    /// Import Excel Beginning merchandise Data
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("beginning/import-excel")]
    public async Task<ResultModel<int>> ImportExcelBeginningData([FromBody] List<BeginMerchandiseExcel> request, CancellationToken cancellationToken)
    {
        var result = await _receiptService.ImportExcelBeginningData(request, CurrentUser, cancellationToken);

        if (result <= 0)
        {
            return ResultModel<int>.Error("Failed to Import Excel Beginning Inbound Order");
        }
        return ResultModel<int>.Success(result);
    }

    ///BeginMerchandiseDto
    [HttpGet("beginning")]
    public async Task<ResultModel<IEnumerable<BeginMerchandiseDto>>> GetBeginMerchandiseAsync(CancellationToken cancellationToken)
    {
        var data = await _receiptService.GetBeginMerchandiseAsync(CurrentUser, cancellationToken);
        if (data == null)
        {
            return ResultModel<IEnumerable<BeginMerchandiseDto>>.Error("Receipt not found");
        }
        return ResultModel<IEnumerable<BeginMerchandiseDto>>.Success(data);
    }

    /// <summary>
    /// Delete Begin Merchandise
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("beginning/{id}")]
    public async Task<ResultModel<int>> DeleteBeginMerchandise([FromRoute] int id, CancellationToken cancellationToken)
    {
        var (success, message) = await _receiptService.DeleteBeginMerchandise(id, CurrentUser, cancellationToken);
        return success ? ResultModel<int>.Success(1, message) : ResultModel<int>.Error(message);
    }

    /// <summary>
    /// Save Begin Merchandise
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("beginning")]
    public async Task<ResultModel<int>> SaveBeginMerchandise(CancellationToken cancellationToken)
    {
        var (success, message) = await _receiptService.SaveBeginMerchandise(CurrentUser, cancellationToken);
        return success ? ResultModel<int>.Success(1, message) : ResultModel<int>.Error(message);
    }

    /// <summary>
    /// Import Excel Outbound Data
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("outbound/import-excel")]
    public async Task<ResultModel<int>> ImportExcelOutboundData([FromBody] List<OutboundOrderExcel> request, CancellationToken cancellationToken)
    {
        var result = await _outboundReceiptService.ImportExcelData(request, CurrentUser, cancellationToken);

        if (result <= 0)
        {
            return ResultModel<int>.Error("Failed to Import Excel Outbound Order");
        }
        return ResultModel<int>.Success(result);
    }
}
