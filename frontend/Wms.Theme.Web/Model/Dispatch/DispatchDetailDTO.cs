using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Dispatch
{
    public class DispatchDetailDTO
    {

        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;


        [JsonPropertyName("dispatch_no")]

        public string DispatchNo { get; set; } = string.Empty;


        [JsonPropertyName("dispatch_status")]
        public byte DispatchStatus { get; set; } = 0;

        /// <summary>
        /// customer_id
        /// </summary>
        [JsonPropertyName("customer_id")]
        public int CustomerId { get; set; } = 0;

        /// <summary>
        /// customer_name
        /// </summary>
        [JsonPropertyName("customer_name")]

        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// sku_id
        /// </summary>
        [JsonPropertyName("sku_id")]
        public int SkuId { get; set; } = 0;

        /// <summary>
        /// qty
        /// </summary>
        [JsonPropertyName("qty")]
        public int Qty { get; set; } = 0;

        /// <summary>
        /// weight
        /// </summary>
        [JsonPropertyName("weight")]
        public decimal Weight { get; set; } = 0;

        /// <summary>
        /// volume
        /// </summary>
        [JsonPropertyName("volume")]
        public decimal Volume { get; set; } = 0;

        /// <summary>
        /// creator
        /// </summary>
        [JsonPropertyName("creator")]

        public string Creator { get; set; } = string.Empty;

        /// <summary>
        /// create_time
        /// </summary>
        [JsonPropertyName("create_time")]

        public DateTime CreateTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// damage_qty
        /// </summary>
        [JsonPropertyName("damage_qty")]
        public int DamageQty { get; set; } = 0;

        /// <summary>
        /// lock_qty
        /// </summary>
        [JsonPropertyName("lock_qty")]
        public int LockQty { get; set; } = 0;

        /// <summary>
        /// picked_qty
        /// </summary>
        [JsonPropertyName("picked_qty")]
        public int PickedQty { get; set; } = 0;

        /// <summary>
        /// unpicked_qty
        /// </summary>
        [JsonPropertyName("unpicked_qty")]
        public int UnpickedQty { get; set; } = 0;

        /// <summary>
        /// intrasit_qty
        /// </summary>
        [JsonPropertyName("intrasit_qty")]
        public int IntrasitQty { get; set; } = 0;

        /// <summary>
        /// package_qty
        /// </summary>
        [JsonPropertyName("package_qty")]
        public int PackageQty { get; set; } = 0;

        /// <summary>
        /// unpackage_qty
        /// </summary>
        [JsonPropertyName("unpackage_qty")]
        public int UnpackageQty { get; set; } = 0;

        /// <summary>
        /// weighing_qty
        /// </summary>
        [JsonPropertyName("weighing_qty")]
        public int WeighingQty { get; set; } = 0;

        /// <summary>
        /// weighing_qty
        /// </summary>
        [JsonPropertyName("unweighing_qty")]
        public int UnweighingQty { get; set; } = 0;

        /// <summary>
        /// actual_qty
        /// </summary>
        [JsonPropertyName("actual_qty")]
        public int ActualQty { get; set; } = 0;

        /// <summary>
        /// sign_qty
        /// </summary>
        [JsonPropertyName("sign_qty")]
        public int SignQty { get; set; } = 0;

        /// <summary>
        /// package_no
        /// </summary>
        [JsonPropertyName("package_no")]
        [MaxLength(32, ErrorMessage = "MaxLength")]
        public string PackageNo { get; set; } = string.Empty;
        /// <summary>
        /// package_person
        /// </summary>
        [JsonPropertyName("package_person")]
        [MaxLength(64, ErrorMessage = "MaxLength")]
        public string PackagePerson { get; set; } = string.Empty;

        /// <summary>
        /// package_time
        /// </summary>
        [JsonPropertyName("package_time")]
        [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
        public DateTime PackageTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// weighing_no
        /// </summary>
        [JsonPropertyName("weighing_no")]

        public string WeighingNo { get; set; } = string.Empty;

        /// <summary>
        /// weighing_person
        /// </summary>
        [JsonPropertyName("weighing_person")]

        public string WeighingPerson { get; set; } = string.Empty;

        /// <summary>
        /// weighing_weight
        /// </summary>
        [JsonPropertyName("weighing_weight")]
        public decimal WeighingWeight { get; set; } = 0;

        /// <summary>
        /// waybill_no
        /// </summary>
        [JsonPropertyName("waybill_no")]

        public string WaybillNo { get; set; } = string.Empty;

        /// <summary>
        /// carrier
        /// </summary>
        [JsonPropertyName("carrier")]
        [MaxLength(256, ErrorMessage = "MaxLength")]
        public string Carrier { get; set; } = string.Empty;

        /// <summary>
        /// freightfee
        /// </summary>
        [JsonPropertyName("freightfee")]
        public decimal Freightfee { get; set; } = 0;

        /// <summary>
        /// last_update_time
        /// </summary>
        [JsonPropertyName("last_update_time")]

        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// tenant_id
        /// </summary>
        [JsonPropertyName("tenant_id")]
        public long TenantId { get; set; } = 0;

        /// <summary>
        /// spu_code
        /// </summary>
        [JsonPropertyName("spu_code")]
        public string SpuCode { get; set; } = string.Empty;

        /// <summary>
        /// spu_name
        /// </summary>
        [JsonPropertyName("spu_name")]
        public string SpuName { get; set; } = string.Empty;

        /// <summary>
        /// sku_code
        /// </summary>
        [JsonPropertyName("sku_code")]
        public string SkuCode { get; set; } = string.Empty;

        /// <summary>
        /// spu_description
        /// </summary>
        /// <summary>
        [JsonPropertyName("spu_description")]
        public string SpuDescription { get; set; } = string.Empty;

        /// <summary>
        /// bar_code
        /// </summary>
        [JsonPropertyName("bar_code")]
        public string BarCode { get; set; } = string.Empty;
        /// <summary>
        /// volume_unit
        /// </summary>
        [JsonPropertyName("volume_unit")]
        public byte VolumeUnit { get; set; } = 0;

        /// <summary>
        /// weight_unit
        /// </summary>
        [JsonPropertyName("weight_unit")]
        public byte WeightUnit { get; set; } = 0;

        /// <summary>
        /// length_unit
        /// </summary>
        /// <summary>
        [JsonPropertyName("length_unit")]
        public byte LengthUnit { get; set; } = 0;

        /// <summary>
        /// sku_name
        /// </summary>
        [JsonPropertyName("sku_name")]
        public string SkuName { get; set; } = string.Empty;

        /// <summary>
        /// unit
        /// </summary>

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// is_todo
        /// </summary>
        [JsonPropertyName("is_todo")]
        public bool IsTodo { get; set; } = false;
        /// <summary>
        /// pick_checker_id
        /// </summary>

        [JsonPropertyName("pick_checker_id")]
        public int PickCheckerId { get; set; } = 0;

        /// <summary>
        /// pick_checker
        /// </summary>

        [JsonPropertyName("pick_checker")]
        public string PickChecker { get; set; } = string.Empty;


    }
}

