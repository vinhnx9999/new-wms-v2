using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.WMS.Entities.ViewModels.Receipt.Inbound;
using WMSSolution.WMS.Entities.ViewModels.Receipt.Outbound;
using WMSSolution.WMS.IServices.Receipt;

namespace WMSSolution.WMS.Controllers.Orders;

/// <summary>
/// Orders 
/// </summary>
/// <param name="receiptService"></param>
/// <param name="saleService"></param>
[Route("orders")]
[ApiController]
[ApiExplorerSettings(GroupName = "Base")]
public class OrdersController(IReceiptService receiptService,
    IOutboundReceiptService saleService) : BaseController
{
    private readonly IReceiptService _receiptService = receiptService;
    private readonly IOutboundReceiptService _saleService = saleService;

    /// <summary>
    /// Get Receipt Sharing Url
    /// </summary>
    /// <param name="sharingUrl"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{sharingUrl}")]
    [AllowAnonymous]
    public async Task<ResultModel<InboundReceiptDetailedDto>> GetReceiptSharingUrl(string sharingUrl, CancellationToken cancellationToken)
    {
        var data = await _receiptService.GetReceiptSharingUrl(sharingUrl, cancellationToken);
        if (data == null)
        {
            return ResultModel<InboundReceiptDetailedDto>.Error("Receipt not found");
        }
        return ResultModel<InboundReceiptDetailedDto>.Success(data);
    }

    ///sales/{sharingUrl}
    ///
    [HttpGet("sales/{sharingUrl}")]
    [AllowAnonymous]
    public async Task<ResultModel<OutboundReceiptDetailedDto>> GetSalesBySharingUrl(string sharingUrl, CancellationToken cancellationToken)
    {
        var data = await _saleService.GetReceiptSharingUrl(sharingUrl, cancellationToken);
        if (data == null)
        {
            return ResultModel<OutboundReceiptDetailedDto>.Error("Receipt not found");
        }
        return ResultModel<OutboundReceiptDetailedDto>.Success(data);
    }
}
