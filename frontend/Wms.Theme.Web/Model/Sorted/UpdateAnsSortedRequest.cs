using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Sorted
{
    public class UpdateAnsSortedRequest
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;
        [JsonPropertyName("asn_id")]
        public int AnsId { get; set; } = 0;
        [JsonPropertyName("sorted_qty")]
        public int SortedQty { get; set; } = 0;

        [JsonPropertyName("series_number")]
        public string SeriesNumber { get; set; } = string.Empty;
        [JsonPropertyName("putaway_qty")]
        public int PutAwayQty { get; set; } = 0;
        [JsonPropertyName("creator")]
        public string Creator { get; set; } = string.Empty;
        [JsonPropertyName("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("last_update_time")]
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
    }
}
