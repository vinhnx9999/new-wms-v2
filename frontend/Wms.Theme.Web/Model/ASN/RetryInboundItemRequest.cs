using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.ASN
{
    public class RetryInboundItemRequest
    {
        /// <summary>
        /// ASN Detail ID (ID của dòng chi tiết cần retry)
        /// </summary>
        [JsonPropertyName("asn_id")]
        public int AsnId { get; set; }
        /// <summary>
        ///  newLocationid
        /// </summary>
        [JsonPropertyName("new_location_id")]
        public int NewLocationId { get; set; }
        /// <summary>
        /// palletCode
        /// </summary>
        [JsonPropertyName("pallet_code")]
        public string PalletCode { get; set; } = default!;
    }
}
