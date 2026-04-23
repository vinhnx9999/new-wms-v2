namespace WMSSolution.WMS.Entities.ViewModels.OutboundGateway
{
    /// <summary>
    /// Adding new Outbound Gateway request
    /// </summary>
    public class AddOutboundGatewayRequest
    {
        /// <summary>
        /// required name 
        /// </summary>
        public string GatewayName { get; set; } = default!;
        /// <summary>
        /// required warehouse id
        /// </summary>
        public int WarehouseId { get; set; }
    }
}
