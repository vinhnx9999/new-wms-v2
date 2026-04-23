using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.ASN
{
    public class UpdatePutawayRequest
    {
        [JsonPropertyName("asn_id")]
        public int AsnId { get; set; }
        [JsonPropertyName("goods_owner_id")]
        public int GoodOwnerId { get; set; }
        [JsonPropertyName("series_number")]
        public string SeriesNumber { get; set; } = string.Empty;
        [JsonPropertyName("goods_location_id")]
        public int GoodLocationId { get; set; }
        [JsonPropertyName("putaway_qty")]
        public int PutawayQuantity { get; set; }

    }
}
