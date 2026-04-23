using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models.PurchaseOrders;
using WMSSolution.WMS.Entities.ViewModels.PurchaseOrders;

namespace WMSSolution.WMS.IServices.PurchaseOrder
{
    /// <summary>
    /// Purchase Order Service Interface
    /// </summary>
    public interface IPurchaseOrderService : IBaseService<PurchaseOrderEntity>
    {
        /// <summary>
        /// Create new purchase order
        /// </summary>
        /// <param name="request">Purchase order request</param>
        /// <param name="currentUser">Current user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Order id and message</returns>
        Task<(int, string)> CreateNewPoOrder(CreateNewPoRequest request, CurrentUser currentUser, CancellationToken cancellationToken);

        /// <summary>
        /// Get purchase order page list
        /// </summary>
        /// <param name="pageSearch">Page search parameters</param>
        /// <param name="currentUser">Current user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of purchase orders and total count</returns>
        Task<(List<PageSearchPOResponse> data, int totals)> GetPageAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken);

        /// <summary>
        /// Get purchase order by id
        /// </summary>
        /// <param name="id">Purchase order id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="currentUser">Current user  </param>
        /// <returns>Purchase order detail or null if not found</returns>
        Task<PoDetailResponseDto?> GetDetailAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken);

        /// <summary>
        /// Update purchase order
        /// </summary>
        /// <param name="request">Purchase order request</param>
        /// <param name="currentUser">Current user</param>
        /// <returns>Success flag and message</returns>
        Task<(bool, string)> UpdateAsync(CreateNewOrderRequest request, CurrentUser currentUser);

        /// <summary>
        /// Delete purchase order
        /// </summary>
        /// <param name="id">Purchase order id</param>
        /// <param name="currentUser">Current user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success flag and message</returns>
        Task<(bool, string)> DeleteAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken);

        /// <summary>
        /// Get open purchase orders (for import to ASN)
        /// </summary>
        /// <param name="currentUser">Current user</param>
        /// <returns>List of open purchase orders</returns>
        Task<List<PurchaseOrderEntity>> GetOpenPosAsync(CurrentUser currentUser);

        /// <summary>
        /// Close purchase order (Short Close / Cancellation)
        /// </summary>
        /// <param name="id">Purchase order id</param>
        /// <param name="currentUser">Current user</param>
        /// <returns>Success flag and message</returns>
        Task<(bool success, string message)> CloseAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// Generate a new Purchase Order number
        /// </summary>
        /// <param name="currentUser">Current user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New Purchase Order number</returns>
        Task<string> GeneratePoNoAsync(CurrentUser currentUser, CancellationToken cancellationToken);
    }
}
