using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Dispatch
{
    public class DispatchListPackageDTO
    {

        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;
        /// <summary>
        /// dispatch_no
        /// </summary>
        [JsonPropertyName("dispatch_no")]
        public string dispatch_no { get; set; } = string.Empty;

        /// <summary>
        /// dispatch_status
        /// </summary>
        [JsonPropertyName("dispatch_status")]
        public byte DispatchStatus { get; set; } = 0;

        [JsonPropertyName("package_qty")]
        public int PackageQty { get; set; } = 0;

        [JsonPropertyName("picked_qty")]
        public int PickedQty { get; set; } = 0;
    }
}
