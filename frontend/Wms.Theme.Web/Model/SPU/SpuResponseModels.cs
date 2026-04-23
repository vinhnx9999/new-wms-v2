using System.Text.Json.Serialization;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Model.SPU
{
    public class SpuDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("spu_code")]
        public string SpuCode { get; set; } = string.Empty;

        [JsonPropertyName("spu_name")]
        public string SpuName { get; set; } = string.Empty;

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("spu_description")]
        public string SpuDescription { get; set; } = string.Empty;

        [JsonPropertyName("supplier_id")]
        public int SupplierId { get; set; }

        [JsonPropertyName("supplier_name")]
        public string SupplierName { get; set; } = string.Empty;

        [JsonPropertyName("brand")]
        public string Brand { get; set; } = string.Empty;

        [JsonPropertyName("origin")]
        public string Origin { get; set; } = string.Empty;

        [JsonPropertyName("length_unit")]
        public int LengthUnit { get; set; }

        [JsonPropertyName("volume_unit")]
        public int VolumeUnit { get; set; }

        [JsonPropertyName("weight_unit")]
        public int WeightUnit { get; set; }

        [JsonPropertyName("creator")]
        public string Creator { get; set; } = string.Empty;

        [JsonPropertyName("create_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime CreateTime { get; set; }

        [JsonPropertyName("last_update_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime LastUpdateTime { get; set; }

        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("detailList")]
        public List<SkuDto> DetailList { get; set; } = new List<SkuDto>();
    }

    public class SkuDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("spu_id")]
        public int SpuId { get; set; }

        [JsonPropertyName("sku_code")]
        public string SkuCode { get; set; } = string.Empty;

        [JsonPropertyName("sku_name")]
        public string SkuName { get; set; } = string.Empty;

        [JsonPropertyName("bar_code")]
        public string BarCode { get; set; } = string.Empty;

        [JsonPropertyName("weight")]
        public decimal Weight { get; set; }

        [JsonPropertyName("lenght")]
        public decimal Length { get; set; }

        [JsonPropertyName("width")]
        public decimal Width { get; set; }

        [JsonPropertyName("height")]
        public decimal Height { get; set; }

        [JsonPropertyName("volume")]
        public decimal Volume { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = string.Empty;

        [JsonPropertyName("cost")]
        public decimal Cost { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("create_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime CreateTime { get; set; }

        [JsonPropertyName("last_update_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime LastUpdateTime { get; set; }

        [JsonPropertyName("detailList")]
        public List<SkuWarehouseDto> DetailList { get; set; } = new List<SkuWarehouseDto>();
    }

    public class SkuWarehouseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("sku_id")]
        public int SkuId { get; set; }

        [JsonPropertyName("warehouse_id")]
        public int WarehouseId { get; set; }

        [JsonPropertyName("warehouse_name")]
        public string WarehouseName { get; set; } = string.Empty;

        [JsonPropertyName("safety_stock_qty")]
        public int SafetyStockQty { get; set; }
    }
}