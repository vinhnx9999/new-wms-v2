using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Dispatch
{
    public class DispatchAdvancedDTO
    {
        [JsonPropertyName("dispatch_no")]
        public string DispatchNo { get; set; } = string.Empty;

        [JsonPropertyName("dispatch_status")]
        public byte DispatchStatus { get; set; } = 0;

        [JsonPropertyName("customer_id")]
        public int CustomerId { get; set; } = 0;
        [JsonPropertyName("customer_name")]

        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("qty")]
        public int Qty { get; set; } = 0;

        [JsonPropertyName("weight")]
        public decimal Weight { get; set; } = 0;

        [JsonPropertyName("volume")]
        public decimal Volume { get; set; } = 0;

        [JsonPropertyName("tenant_id")]
        public long TenantId { get; set; } = 0;
        [JsonPropertyName("creator")]
        public string Creator { get; set; } = string.Empty;
    }
}
