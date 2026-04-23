using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Dispatch
{
    public class DispatchListSignDTO
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
        /// damage_qty
        /// </summary>
        [JsonPropertyName("damage_qty")]
        public int DamageQty { get; set; } = 0;
    }
}
