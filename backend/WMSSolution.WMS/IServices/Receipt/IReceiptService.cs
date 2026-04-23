using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Shared.Enums;
using WMSSolution.Shared.Excel;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.ViewModels.Dashboard;
using WMSSolution.WMS.Entities.ViewModels.Receipt;
using WMSSolution.WMS.Entities.ViewModels.Receipt.Inbound;

namespace WMSSolution.WMS.IServices.Receipt;
/// <summary>
/// Receipt Service interface
/// </summary>
public interface IReceiptService : IBaseService<InboundReceiptEntity>
{
    /// <summary>
    /// Page inbound receipts 
    /// </summary>
    /// <param name="pageSearch">page args</param>
    /// <param name="currentUser">current user</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    Task<(List<InboundReceiptListResponse> data, int totals)> PageInboundReceiptAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Get receipt detail by ID
    /// </summary>
    /// <param name="id">receipt id</param>
    /// <param name="currentUser">current user</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>detailed receipt or null</returns>
    Task<InboundReceiptDetailedDto?> GetReceiptByIdAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Hanlde update receipt with status draft, only allow update when receipt is in draft status
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool, string)> UpdateAsync(int id, UpdateInboundReceiptRequest request, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Complete a receipt, only allow complete when receipt is in Processing status
    /// </summary>
    /// <param name="id">receipt id</param>
    /// <param name="currentUser">current user</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>status message</returns>
    Task<(bool success, string message)> CancelAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Create new receipt with details
    /// </summary>
    /// <param name="request">receipt data with details</param>
    /// <param name="currentUser">current user</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>created receipt id and message</returns>
    Task<(int id, string message)> CreateAsync(CreateReceiptRequest request, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Generate next receipt number
    /// </summary>
    /// <returns>next Receipt no</returns>
    Task<string> GetNextReceiptNoAsync();
    /// <summary>
    /// Get Inbound Info
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<IEnumerable<OrderItemDTO>> GetInboundInfo(CurrentUser currentUser);
    /// <summary>
    /// Get Inbound By Date
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="finishStatuses"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    Task<IEnumerable<OrderItemDTO>> GetInboundByDate(CurrentUser currentUser,
        ReceiptStatus[] finishStatuses, 
        DateTime dateTime);
    /// <summary>
    /// Get Inbound By RangeDate
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="finishStatuses"></param>
    /// <param name="dateTime"></param>
    /// <param name="today"></param>
    /// <returns></returns>
    Task<IEnumerable<DateOrderItemDTO>> GetInboundByRangeDate(CurrentUser currentUser,
        ReceiptStatus[] finishStatuses, DateTime dateTime, DateTime today);
    /// <summary>
    /// Retry Inbound Details
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool success, string message)> RetryInboundDetailsAsync(RetryInboundRequest request, 
        CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Import Excel Data
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> ImportExcelData(List<InboundOrderExcel> request, 
        CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Revert Inbound
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool success, string message)> RevertInbound(int id, CurrentUser currentUser, 
        CancellationToken cancellationToken);
    /// <summary>
    /// Clone Inbound
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool success, string message)> CloneInboundAsync(int id, CurrentUser currentUser, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Import Excel beginning merchandise
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> ImportExcelBeginningData(List<BeginMerchandiseExcel> request, 
        CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Get Begin Merchandises
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<BeginMerchandiseDto>> GetBeginMerchandiseAsync(CurrentUser currentUser, 
        CancellationToken cancellationToken);
    /// <summary>
    /// Delete Begin Merchandise
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool success, string message)> DeleteBeginMerchandise(int id, 
        CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Save Begin Merchandise
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool success, string message)> SaveBeginMerchandise(CurrentUser currentUser, 
        CancellationToken cancellationToken);
    /// <summary>
    /// Get deleted data for revert
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<InboundReceiptListResponse>> GetDeletedData(CurrentUser currentUser, 
        CancellationToken cancellationToken);
    /// <summary>
    /// Create Bulk Receipts Async
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(int id, string message)> CreateBulkReceiptsAsync(CreateBulkReceiptRequest request, 
        CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Get Share Inbound
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> GetShareInbound(int id, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Get Receipt Sharing Url
    /// </summary>
    /// <param name="sharingUrl"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<InboundReceiptDetailedDto> GetReceiptSharingUrl(string sharingUrl, CancellationToken cancellationToken);
}
