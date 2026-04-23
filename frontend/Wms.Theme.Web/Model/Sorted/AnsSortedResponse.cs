using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Sorted
{
    public class AnsSortedResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        [JsonPropertyName("asn_id")]
        public int AsnId { get; set; } = 0;

        [JsonPropertyName("sorted_qty")]
        public int SortedQty { get; set; } = 0;

        [JsonPropertyName("series_number")]
        public string SeriesNumber { get; set; } = string.Empty;

        [JsonPropertyName("putaway_qty")]
        public int PutawayQty { get; set; } = 0;

        [JsonPropertyName("expiry_date")]
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("creator")]
        public string Creator { get; set; } = string.Empty;

        [JsonPropertyName("create_time")]
        public DateTime Create_time { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("last_update_time")]
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; } = true;

        [JsonPropertyName("tenant_id")]
        public long TenantId { get; set; } = 1;
    }
}
