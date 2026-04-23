using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Dispatch
{
    public class DispatchListDeliveryDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        /// <summary>
        /// dispatch_no
        /// </summary>
        [JsonPropertyName("dispatch_no")]
        public string DispatchNo { get; set; } = string.Empty;

        /// <summary>
        /// dispatch_status
        /// </summary>
        [JsonPropertyName("dispatch_status")]
        public byte DispatchStatus { get; set; } = 0;

        /// <summary>
        /// picked_qty
        /// </summary>
        [JsonPropertyName("picked_qty")]
        public int picked_qty { get; set; } = 0;
    }
}
