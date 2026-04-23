using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.ASN
{
    public class SkuSelectDTO
    {
        [JsonPropertyName("sku_id")]
        public int sku_id { get; set; } = 0;

        [JsonPropertyName("spu_id")]
        public int SpuID { get; set; } = 0;

        [JsonPropertyName("spu_code")]
        public string SpuCode { get; set; } = string.Empty;

        [JsonPropertyName("spu_name")]
        public string SpuName { get; set; } = string.Empty;

        [JsonPropertyName("sku_code")]
        public string SkuCode { get; set; } = string.Empty;

        [JsonPropertyName("sku_name")]
        public string SkuName { get; set; } = string.Empty;

        [JsonPropertyName("supplier_id")]
        public int SupplierId { get; set; } = 0;

        [JsonPropertyName("supplier_name")]
        public string SupplierName { get; set; } = string.Empty;

        [JsonPropertyName("brand")]
        public string Brand { get; set; } = string.Empty;
        [JsonPropertyName("origin")]
        public string Origin { get; set; } = string.Empty;

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = string.Empty;

        [JsonPropertyName("qty_available")]
        public decimal QtyAvailable { get; set; } = 0;

        [JsonPropertyName("cost")]
        public decimal Cost { get; set; } = 0;

        [JsonPropertyName("price")]
        public decimal Price { get; set; } = 0;

        [JsonPropertyName("weight")]
        public decimal Weight { get; set; } = 0;

        [JsonPropertyName("volume")]
        public decimal Volume { get; set; } = 0;

    }
}
