using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Sorted
{
    public class AddAnsToAnsSortedRequest
    {
        [JsonPropertyName("asn_id")]
        public int Id { get; set; } = 0;
        [JsonPropertyName("is_auto_num")]
        public bool IsAutoNum { get; set; } = false;
        [JsonPropertyName("series_number")]
        public string SeriesNumber { get; set; } = string.Empty;
        [JsonPropertyName("sorted_qty")]
        public int SortedQuantity { get; set; } = 0;
        [JsonPropertyName("expiry_date")]
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow;
    }
}
