using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Dispatch
{
    public class DispatchListAddViewModel
    {

        [JsonPropertyName("customer_id")]
        public int CustomerId { get; set; } = 0;

        [JsonPropertyName("customer_name")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("sku_id")]
        public int SkuId { get; set; } = 0;

        [JsonPropertyName("qty")]
        public int Qty { get; set; } = 0;

        [JsonPropertyName("weight")]
        public decimal Weight { get; set; } = 0;

        [JsonPropertyName("volume")]
        public decimal Volume { get; set; } = 0;

    }
}

