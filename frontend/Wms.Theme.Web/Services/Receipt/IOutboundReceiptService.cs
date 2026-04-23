using Wms.Theme.Web.Model.InboundReceipt;
using Wms.Theme.Web.Model.OutboundReceipt;
using Wms.Theme.Web.Model.ShareModel;
using WMSSolution.Shared.Excel;

namespace Wms.Theme.Web.Services.Receipt;

public interface IOutboundReceiptService
{
    /// <summary>
    /// Cancel outbound receipt by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> CancelReceipt(int id);
    Task<bool> CloneReceipt(int id);

    /// <summary>
    /// Create outbound receipt 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<(int id, string message)> CreateOutboundReceiptAsync(CreateOutboundReceiptRequest request);

    /// <summary>
    /// Get next Receipt code
    /// </summary>
    /// <returns></returns>
    Task<string> GetNextReceiptCode();

    /// <summary>
    /// Get outbound receipt detail by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<OutboundReceiptDetailedDto?> GetReceiptDetailAsync(int id);
    Task<IEnumerable<OutboundReceiptListResponse>> GetDeletedData();
    Task<(int? data, string? message)> ImportExcelData(List<OutboundOrderExcel> inputOrders);

    /// <summary>
    /// Get paginated list of outbound receipts
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<PageData<OutboundReceiptListResponse>> PageSearchReceipt(PageSearchRequest request);
    /// <summary>
    /// Revert Receipt
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> RevertReceipt(int id);

    /// <summary>
    /// Update outbound receipt by ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<bool> UpdateReceiptAsync(int id, UpdateOutboundReceiptRequest request);
    Task<string?> GetShareOutbound(int id);
    /// <summary>
    /// Get Receipt Sharing Url
    /// </summary>
    /// <param name="sharingUrl"></param>
    /// <returns></returns>
    Task<OutboundReceiptDetailedDto> GetReceiptSharingUrl(string sharingUrl);
}
