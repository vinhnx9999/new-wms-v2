using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Shared.Excel;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.ViewModels.Receipt.Outbound;

namespace WMSSolution.WMS.IServices.Receipt;

/// <summary>
/// Outbound receipt service interface
/// </summary>
public interface IOutboundReceiptService : IBaseService<OutBoundReceiptEntity>
{
    /// <summary>
    /// Creates a new outbound receipt
    /// </summary>
    /// <param name="request">The request object containing outbound receipt details</param>
    /// <param name="currentUser">The current user performing the operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tuple containing the ID of the created receipt and a message</returns>
    Task<(int id, string message)> CreateAsync(CreateOutboundReceiptRequest request, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Generates the next receipt number for a new outbound receipt
    /// </summary>
    /// <returns>The next receipt number</returns>
    Task<string> GetNextReceiptNoAsync();

    /// <summary>
    /// page search outbound Receipt 
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(List<OutboundReceiptListResponse> data, int totals)> PageOutboundReceiptAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Get outbound receipt details by ID
    /// </summary>
    /// <param name="id">The ID of the outbound receipt</param>
    /// <param name="currentUser">The current user performing the operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The detailed outbound receipt DTO</returns>
    Task<OutboundReceiptDetailedDto?> GetReceiptByIdAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels an outbound receipt by ID
    /// </summary>
    /// <param name="id">The ID of the outbound receipt to cancel</param>
    /// <param name="currentUser">The current user performing the operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tuple indicating the success of the operation and a message</returns>
    Task<(bool success, string message)> CancelAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing outbound receipt by ID
    /// </summary>
    /// <param name="id">The ID of the outbound receipt to update</param>
    /// <param name="request">The request object containing updated outbound receipt details</param>
    /// <param name="currentUser">The current user performing the operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tuple indicating the success of the operation and an error message if applicable</returns>
    Task<(bool IsSuccess, string ErrorMessage)> UpdateAsync(int id, UpdateOutboundReceiptRequest request, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Import Excel Data
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> ImportExcelData(List<OutboundOrderExcel> request, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Revert Outbound
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool success, string message)> RevertOutboundAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Clone Outbound
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool success, string message)> CloneOutboundAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Get Deleted Data for revert
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<OutboundReceiptListResponse>> GetDeletedData(CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Get Share Outbound
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> GetShareOutbound(int id, CurrentUser currentUser, CancellationToken cancellationToken);
    Task<OutboundReceiptDetailedDto> GetReceiptSharingUrl(string sharingUrl, CancellationToken cancellationToken);
}
