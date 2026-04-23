using Wms.Theme.Web.Model.InboundReceipt;
using Wms.Theme.Web.Model.ShareModel;
using WMSSolution.Shared.Excel;

namespace Wms.Theme.Web.Services.Receipt;

public interface IReceiptService
{
    Task<PageData<InboundReceiptListResponse>> PageSearchReceipt(PageSearchRequest request);
    Task<InboundReceiptDetailedDTO?> GetReceiptDetailAsync(int id);
    Task<(int id, string message)> CreateNewReceiptAsync(CreateReceiptRequest request);
    Task<string> GetNextReceiptCode();
    Task<bool> UpdateReceiptAsync(int id, UpdateInboundReceiptRequest request);
    Task<bool> CancelReceipt(int id);
    Task<bool> RetryInboundTask(RetryInboundRequest request);
    Task<(int? data, string? message)> ImportExcelData(List<InboundOrderExcel> inputOrders);
    Task<bool> RevertReceipt(int id);
    Task<bool> CloneReceipt(int id);
    Task<IEnumerable<InboundReceiptListResponse>> GetDeletedData();
    Task<(int id, string? message)> CreateNewBulkReceiptAsync(CreateBulkReceiptRequest request);
    Task<string> GetShareInbound(int id);
    Task<InboundReceiptDetailedDTO?> GetReceiptSharingUrl(string sharingUrl);
}
