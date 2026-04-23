using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.ASN
{
    public class GetAsnQrCodeRequest
    {
        [JsonPropertyName("asn_id")]
        public int AsnId { get; set; } = 0;
        [JsonPropertyName("asnmaster_id")]
        public int AsnMasterId { get; set; } = 0;

        [JsonPropertyName("sku_id")]
        public int SkuId { get; set; } = 0;

        [JsonPropertyName("asn_no")]
        public string AsnNo { get; set; } = string.Empty;

        [JsonPropertyName("spu_code")]
        public string SpuCode { get; set; } = string.Empty;

        [JsonPropertyName("spu_name")]
        public string SpuName { get; set; } = string.Empty;
        [JsonPropertyName("sku_code")]
        public string SkuCode { get; set; } = string.Empty;

        [JsonPropertyName("sku_name")]
        public string SkuName { get; set; } = string.Empty;
        [JsonPropertyName("series_number")]
        public string SeriesNumber { get; set; } = string.Empty;
        [JsonPropertyName("location_name")]
        public string location_name { get; set; } = string.Empty;

    }
}
