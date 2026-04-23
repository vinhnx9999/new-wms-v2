using Wms.Theme.Web.Model.OutboundGateway;
using Wms.Theme.Web.Model.ShareModel;

namespace Wms.Theme.Web.Services.OutboundGateway
{
    public interface IOutboundGatewayService
    {
        Task<List<OutboundGatewayResponse>> PageSearchByWarehouse(PageSearchRequest pageSearch, int warehouseId);
        Task<(int id, string message)> AddAsync(AddOutboundGatewayRequest request);
    }
}
