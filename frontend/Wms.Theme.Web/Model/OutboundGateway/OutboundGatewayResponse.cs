namespace Wms.Theme.Web.Model.OutboundGateway
{
    public class OutboundGatewayResponse
    {
        /// <summary>
        /// id
        /// </summary>
        public int Id { get; set; } = 0;

        /// <summary>
        /// Gateway name
        /// </summary>
        public string GatewayName { get; set; } = string.Empty;

        /// <summary>
        /// Create time
        /// </summary>
        public DateTime CreateTime { get; set; }

    }
}
