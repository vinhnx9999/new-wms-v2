using System.Text.Json.Serialization;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Model.ASN
{
    public class AsnMasterCustomDetailedDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;
        [JsonPropertyName("asn_no")]
        public string AsnNo { get; set; } = string.Empty;
        [JsonPropertyName("asn_batch")]
        public string AsnBatch { get; set; } = string.Empty;

        [JsonPropertyName("po_id")]
        public int? PoId { get; set; }

        [JsonPropertyName("estimated_arrival_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime EstimatedArrivalTime { get; set; }
        [JsonPropertyName("asn_status")]
        public byte AsnStatus { get; set; } = 0;

        [JsonPropertyName("weight")]
        public decimal Weight { get; set; } = 0;

        [JsonPropertyName("volume")]
        public decimal Volume { get; set; } = 0;
        [JsonPropertyName("goods_owner_id")]
        public int GoodsOwnerId { get; set; } = 0;

        [JsonPropertyName("goods_owner_name")]
        public string GoodsOwnerName { get; set; } = string.Empty;

        [JsonPropertyName("warehouse_id")]
        public int WareHouseId { get; set; } = 0;

        [JsonPropertyName("warehouse_name")]
        public string WareHouseName { get; set; } = string.Empty;

        [JsonPropertyName("creator")]
        public string Creator { get; set; } = string.Empty;

        [JsonPropertyName("create_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("last_update_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("detailList")]

        public List<AsnmasterDetailViewModel> DetailList { get; set; } = [];
    }

    public class AsnmasterDetailViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        [JsonPropertyName("asnmaster_id")]
        public int AsnmasterId { get; set; } = 0;

        [JsonPropertyName("asn_status")]
        public byte AsnStatus { get; set; } = 0;

        [JsonPropertyName("spu_id")]
        public int SpuId { get; set; } = 0;


        [JsonPropertyName("spu_code")]
        public string SpuCode { get; set; } = string.Empty;

        [JsonPropertyName("spu_name")]
        public string SpuName { get; set; } = string.Empty;

        [JsonPropertyName("sku_id")]
        public int SkuId { get; set; } = 0;


        [JsonPropertyName("sku_code")]
        public string SkuCode { get; set; } = string.Empty;

        [JsonPropertyName("sku_name")]

        public string SkuName { get; set; } = string.Empty;
        [JsonPropertyName("origin")]

        public string Origin { get; set; } = string.Empty;
        [JsonPropertyName("length_unit")]
        public byte LengthUnit { get; set; } = 0;


        [JsonPropertyName("volume_unit")]
        public byte VolumeUnit { get; set; } = 0;

        [JsonPropertyName("weight_unit")]
        public byte WeightUnit { get; set; } = 0;

        [JsonPropertyName("asn_qty")]
        public int AsnQty { get; set; } = 0;

        [JsonPropertyName("actual_qty")]
        public int ActualQty { get; set; } = 0;

        [JsonPropertyName("weight")]
        public decimal Weight { get; set; } = 0;


        [JsonPropertyName("volume")]
        public decimal Volume { get; set; } = 0;
        [JsonPropertyName("supplier_id")]
        public int SupplierId { get; set; } = 0;


        [JsonPropertyName("supplier_name")]
        public string SupplierName { get; set; } = string.Empty;

        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; } = true;
        [JsonPropertyName("expiry_date")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime ExpiryDate { get; set; }


        [JsonPropertyName("price")]
        public decimal Price { get; set; } = 0;

        [JsonPropertyName("sorted_qty")]
        public int SortedQty { get; set; } = 0;
        [JsonPropertyName("putaway_date")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime PutawayDate { get; set; }

        [JsonPropertyName("uom_id")]
        public int? UomId { get; set; }

        [JsonPropertyName("batch_number")]
        public string? BatchNumber { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("asn_qty_decimal")]
        public decimal AsnQtyDecimal { get; set; } = 0;

        [JsonPropertyName("actual_qty_decimal")]
        public decimal ActualQtyDecimal { get; set; } = 0;

        [JsonPropertyName("goods_location_id")]
        public int GoodsLocationId { get; set; } = 0;

        [JsonPropertyName("goods_location_name")]
        public string? GoodsLocationName { get; set; }

        [JsonPropertyName("pallet_id")]
        public int PalletId { get; set; } = 0;
    }
}
