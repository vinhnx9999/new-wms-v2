using System.Text.Json.Serialization;

namespace WMSSolution.WMS.Entities.ViewModels.Stockadjust
{
    public class StockAdjustRequest
    {
        public byte job_type { get; set; }
        public string reason { get; set; }

        [JsonPropertyName("items")]
        public List<StockAdjustItemDto> items { get; set; }
    }
}
