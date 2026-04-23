using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.ASN
{
    public class GetAsnPutawayResponse
    {
        [JsonPropertyName("asn_id")]
        public int AsnId { get; set; }
        [JsonPropertyName("goods_owner_id")]
        public int GoodOwnerId { get; set; }
        [JsonPropertyName("goods_owner_name")]
        public string GoodOwnerName { get; set; } = string.Empty;
        [JsonPropertyName("series_number")]
        public string SeriesNumber { get; set; } = string.Empty;
        [JsonPropertyName("sorted_qty")]
        public int SortedQuantity { get; set; } = 0;

        [JsonPropertyName("asn_no")]
        public string AsnNo { get; set; } = string.Empty;

        [JsonPropertyName("expiry_date")]
        public DateTime ExpiryDate { get; set; } = DateTime.MinValue;

        [JsonPropertyName("sku_name")]
        public string SkuName { get; set; } = string.Empty;

        [JsonPropertyName("goods_location_id")]
        public int GoodLocationId { get; set; }

        [JsonPropertyName("location_name")]
        public string LocationName { get; set; } = string.Empty;
    }
}