namespace Wms.Theme.Web.Model.OutboundGateway
{
    public class AddOutboundGatewayRequest
    {
        public string GatewayName { get; set; } = default!;
        public int WarehouseId { get; set; }
    }
}
