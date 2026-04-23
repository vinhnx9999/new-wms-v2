using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Dispatch
{
    public class CancelOrderDTO
    {
        [JsonPropertyName("dispatch_no")]
        public string DispatchNo { get; set; } = string.Empty;
        [JsonPropertyName("dispatch_status")]
        public int DispatchStatus { get; set; } = 0;
    }
}
