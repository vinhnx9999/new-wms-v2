using System.Text.Json.Serialization;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Model.ASN
{
    public class AsnApiResponse
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public AsnDataResponse? Data { get; set; }
    }

    public class AsnDataResponse
    {
        [JsonPropertyName("rows")]
        public List<AsnDto> Rows { get; set; } = new List<AsnDto>();

        [JsonPropertyName("totals")]
        public int Totals { get; set; }
    }

    public class AsnDetailApiResponse
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public AsnDto? Data { get; set; }
    }

    public class AsnDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("asnmaster_id")]
        public int AsnmasterId { get; set; }

        [JsonPropertyName("asn_no")]
        public string AsnNo { get; set; } = string.Empty;

        [JsonPropertyName("asn_batch")]
        public string AsnBatch { get; set; } = string.Empty;

        [JsonPropertyName("estimated_arrival_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime EstimatedArrivalTime { get; set; }

        [JsonPropertyName("asn_status")]
        public int AsnStatus { get; set; }

        [JsonPropertyName("spu_id")]
        public int SpuId { get; set; }

        [JsonPropertyName("spu_code")]
        public string SpuCode { get; set; } = string.Empty;

        [JsonPropertyName("spu_name")]
        public string SpuName { get; set; } = string.Empty;

        [JsonPropertyName("sku_id")]
        public int SkuId { get; set; }

        [JsonPropertyName("sku_code")]
        public string SkuCode { get; set; } = string.Empty;

        [JsonPropertyName("sku_name")]
        public string SkuName { get; set; } = string.Empty;

        [JsonPropertyName("origin")]
        public string Origin { get; set; } = string.Empty;

        [JsonPropertyName("length_unit")]
        public int LengthUnit { get; set; }

        [JsonPropertyName("volume_unit")]
        public int VolumeUnit { get; set; }

        [JsonPropertyName("weight_unit")]
        public int WeightUnit { get; set; }

        [JsonPropertyName("asn_qty")]
        public int AsnQty { get; set; }

        [JsonPropertyName("actual_qty")]
        public int ActualQty { get; set; }

        [JsonPropertyName("arrival_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime ArrivalTime { get; set; }

        [JsonPropertyName("unload_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime UnloadTime { get; set; }

        [JsonPropertyName("unload_person_id")]
        public int UnloadPersonId { get; set; }

        [JsonPropertyName("unload_person")]
        public string UnloadPerson { get; set; } = string.Empty;

        [JsonPropertyName("sorted_qty")]
        public int SortedQty { get; set; }

        [JsonPropertyName("shortage_qty")]
        public int ShortageQty { get; set; }

        [JsonPropertyName("more_qty")]
        public int MoreQty { get; set; }

        [JsonPropertyName("damage_qty")]
        public int DamageQty { get; set; }

        [JsonPropertyName("weight")]
        public decimal Weight { get; set; }

        [JsonPropertyName("volume")]
        public decimal Volume { get; set; }

        [JsonPropertyName("supplier_id")]
        public int SupplierId { get; set; }

        [JsonPropertyName("supplier_name")]
        public string SupplierName { get; set; } = string.Empty;

        [JsonPropertyName("goods_owner_id")]
        public int GoodsOwnerId { get; set; }

        [JsonPropertyName("goods_owner_name")]
        public string GoodsOwnerName { get; set; } = string.Empty;

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

        [JsonPropertyName("expiry_date")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime ExpiryDate { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }

    public class AsnUpdateRequest
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("asnmaster_id")]
        public int AsnmasterId { get; set; }

        [JsonPropertyName("asn_no")]
        public string AsnNo { get; set; } = string.Empty;

        [JsonPropertyName("asn_batch")]
        public string AsnBatch { get; set; } = string.Empty;

        [JsonPropertyName("estimated_arrival_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime EstimatedArrivalTime { get; set; }

        [JsonPropertyName("asn_status")]
        public byte AsnStatus { get; set; }

        [JsonPropertyName("spu_id")]
        public int SpuId { get; set; }

        [JsonPropertyName("spu_code")]
        public string SpuCode { get; set; } = string.Empty;

        [JsonPropertyName("spu_name")]
        public string SpuName { get; set; } = string.Empty;

        [JsonPropertyName("sku_id")]
        public int SkuId { get; set; }

        [JsonPropertyName("sku_code")]
        public string SkuCode { get; set; } = string.Empty;

        [JsonPropertyName("sku_name")]
        public string SkuName { get; set; } = string.Empty;

        [JsonPropertyName("origin")]
        public string Origin { get; set; } = string.Empty;

        [JsonPropertyName("length_unit")]
        public byte LengthUnit { get; set; }

        [JsonPropertyName("volume_unit")]
        public byte VolumeUnit { get; set; }

        [JsonPropertyName("weight_unit")]
        public byte WeightUnit { get; set; }

        [JsonPropertyName("asn_qty")]
        public int AsnQty { get; set; }

        [JsonPropertyName("actual_qty")]
        public int ActualQty { get; set; }

        [JsonPropertyName("arrival_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime ArrivalTime { get; set; }

        [JsonPropertyName("unload_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime UnloadTime { get; set; }

        [JsonPropertyName("unload_person_id")]
        public int UnloadPersonId { get; set; }

        [JsonPropertyName("unload_person")]
        public string UnloadPerson { get; set; } = string.Empty;

        [JsonPropertyName("sorted_qty")]
        public int SortedQty { get; set; }

        [JsonPropertyName("shortage_qty")]
        public int ShortageQty { get; set; }

        [JsonPropertyName("more_qty")]
        public int MoreQty { get; set; }

        [JsonPropertyName("damage_qty")]
        public int DamageQty { get; set; }

        [JsonPropertyName("weight")]
        public decimal Weight { get; set; }

        [JsonPropertyName("volume")]
        public decimal Volume { get; set; }

        [JsonPropertyName("supplier_id")]
        public int SupplierId { get; set; }

        [JsonPropertyName("supplier_name")]
        public string SupplierName { get; set; } = string.Empty;

        [JsonPropertyName("goods_owner_id")]
        public int GoodsOwnerId { get; set; }

        [JsonPropertyName("goods_owner_name")]
        public string GoodsOwnerName { get; set; } = string.Empty;

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

        [JsonPropertyName("expiry_date")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime ExpiryDate { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }

    public class AsnUpdateResponse
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class AsnCreateResponse
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public int? CreatedId { get; set; }
    }

    public class AsnDeleteResponse
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// ASN Master DTO - matches the JSON structure from API response
    /// </summary>
    public class AsnMasterDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("asn_no")]
        public string AsnNo { get; set; } = string.Empty;

        [JsonPropertyName("asn_batch")]
        public string AsnBatch { get; set; } = string.Empty;

        [JsonPropertyName("estimated_arrival_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime EstimatedArrivalTime { get; set; }

        [JsonPropertyName("asn_status")]
        public int AsnStatus { get; set; }

        [JsonPropertyName("weight")]
        public decimal Weight { get; set; }

        [JsonPropertyName("volume")]
        public decimal Volume { get; set; }

        [JsonPropertyName("goods_owner_id")]
        public int GoodsOwnerId { get; set; }

        [JsonPropertyName("goods_owner_name")]
        public string GoodsOwnerName { get; set; } = string.Empty;

        [JsonPropertyName("warehouse_name")]
        public string WarehouseName { get; set; } = string.Empty;

        [JsonPropertyName("creator")]
        public string Creator { get; set; } = string.Empty;

        [JsonPropertyName("create_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime CreateTime { get; set; }

        [JsonPropertyName("last_update_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime LastUpdateTime { get; set; }

        [JsonPropertyName("has_rejected_items")]
        public bool? has_rejected_items { get; set; }

        [JsonPropertyName("detailList")]
        public List<AsnDto> DetailList { get; set; } = new List<AsnDto>();
    }

    /// <summary>
    /// ASN Master API Response using shared response pattern
    /// </summary>
    public class AsnMasterApiResponse
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public AsnMasterDataResponse? Data { get; set; }
    }

    public class AsnMasterDataResponse
    {
        [JsonPropertyName("rows")]
        public List<AsnMasterDto> Rows { get; set; } = new List<AsnMasterDto>();

        [JsonPropertyName("totals")]
        public int Totals { get; set; }
    }

    /// <summary>
    /// Response model for ASN Master creation API
    /// </summary>
    public class AsnMasterCreateResponse
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public int? CreatedId { get; set; }  // API returns the created ASN Master ID as a number
    }

    /// <summary>
    /// Response model for ASN Master detail endpoint
    /// </summary>
    public class AsnMasterDetailResponse
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public AsnMasterCustomDetailedDTO? Data { get; set; }
    }
}