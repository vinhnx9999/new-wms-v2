using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Shared.Enums;
using WMSSolution.WMS.Entities.Models.OutboundGateway;
using WMSSolution.WMS.Entities.ViewModels.Dashboard;
using WMSSolution.WMS.Entities.ViewModels.OutboundGateway;

namespace WMSSolution.WMS.IServices.OutboundGateway
{
    /// <summary>
    /// Interface of OutboundGatewayController
    /// </summary>
    public interface IOutboundGatewayService : IBaseService<OutboundGatewayEntity>
    {
        /// <summary>
        /// PageSearch Outbound Gateway by using Warehouse
        /// </summary>
        /// <param name="pageSearch"></param>
        /// <param name="warehouseID"></param>
        /// <param name="currentUser"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<(List<OutboundGatewayResponse> data, int total)> PageAsyncByWarehouseId(PageSearch pageSearch, int warehouseID, CurrentUser currentUser, CancellationToken cancellationToken);

        /// <summary>
        /// Adding new Outbound Gateway
        /// </summary>
        /// <returns></returns>
        Task<(int id, string message)> AddAsync(AddOutboundGatewayRequest request, CurrentUser currentUser, CancellationToken cancellationToken);
        /// <summary>
        /// Get Outbound By Range Date
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="finishStatuses"></param>
        /// <param name="fromDate"></param>
        /// <param name="today"></param>
        /// <returns></returns>
        Task<IEnumerable<DateOrderItemDTO>> GetOutboundByRangeDate(CurrentUser currentUser,
            ReceiptStatus[] finishStatuses, DateTime fromDate, DateTime today);
    }
}
