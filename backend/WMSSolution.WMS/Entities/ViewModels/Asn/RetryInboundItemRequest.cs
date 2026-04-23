using Newtonsoft.Json;

namespace WMSSolution.WMS.Entities.ViewModels.Asn
{
    /// <summary>
    /// DTO for retrying inbound item request
    /// </summary>
    public class RetryInboundItemRequest
    {
        /// <summary>
        /// Asnid
        /// </summary>
        [JsonProperty("asn_id")]
        public int AsnId { get; set; }
        /// <summary>
        ///  newLocationid
        /// </summary>
        [JsonProperty("new_location_id")]
        public int NewLocationId { get; set; }
        /// <summary>
        /// palletCode
        /// </summary>
        [JsonProperty("pallet_code")]
        public string PalletCode { get; set; } = default!;
    }
}
