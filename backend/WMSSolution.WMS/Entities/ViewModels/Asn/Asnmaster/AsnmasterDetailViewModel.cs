
using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels.Asn.Asnmaster;

/// <summary>
/// 
/// </summary>
public class AsnmasterDetailViewModel
{

    #region constructor
    /// <summary>
    /// constructor
    /// </summary>
    public AsnmasterDetailViewModel()
    {

    }
    #endregion

    #region Property

    /// <summary>
    /// id
    /// </summary>
    [Display(Name = "id")]
    public int id { get; set; } = 0;

    /// <summary>
    /// asnmaster_id
    /// </summary>
    public int asnmaster_id { get; set; } = 0;

    /// <summary>
    /// asn_status
    /// </summary>
    [Display(Name = "asn_status")]
    public byte asn_status { get; set; } = 0;

    /// <summary>
    /// spu_id
    /// </summary>
    [Display(Name = "spu_id")]
    [Required(ErrorMessage = "Required")]
    public int spu_id { get; set; } = 0;

    /// <summary>
    /// spu_code
    /// </summary>
    [Display(Name = "spu_code")]
    [Required(ErrorMessage = "Required")]
    [MaxLength(32, ErrorMessage = "MaxLength")]
    public string spu_code { get; set; } = string.Empty;

    /// <summary>
    /// spu_name
    /// </summary>
    [Display(Name = "spu_name")]
    [Required(ErrorMessage = "Required")]
    [MaxLength(200, ErrorMessage = "MaxLength")]
    public string spu_name { get; set; } = string.Empty;

    /// <summary>
    /// sku_id
    /// </summary>
    [Display(Name = "sku_id")]
    public int sku_id { get; set; } = 0;

    /// <summary>
    /// sku_code
    /// </summary>
    [Display(Name = "sku_code")]
    [MaxLength(32, ErrorMessage = "MaxLength")]
    public string sku_code { get; set; } = string.Empty;

    /// <summary>
    /// sku_name
    /// </summary>
    [Display(Name = "sku_name")]
    [MaxLength(200, ErrorMessage = "MaxLength")]
    public string sku_name { get; set; } = string.Empty;

    /// <summary>
    /// origin
    /// </summary>
    [Display(Name = "origin")]
    [MaxLength(256, ErrorMessage = "MaxLength")]
    public string origin { get; set; } = string.Empty;

    /// <summary>
    /// length_unit (0=millimeter, 1=centimeter, 2=decimeter, 3=meter)
    /// </summary>
    [Display(Name = "length_unit")]
    public byte length_unit { get; set; } = 0;

    /// <summary>
    /// volume_unit (0=cubic centimeter, 1=cubic decimeter, 2=cubic meter)
    /// </summary>
    [Display(Name = "volume_unit")]
    public byte volume_unit { get; set; } = 0;

    /// <summary>
    /// weight_unit (0=milligram, 1=gram, 2=kilogram)
    /// </summary>
    [Display(Name = "weight_unit")]
    public byte weight_unit { get; set; } = 0;

    /// <summary>
    /// asn_qty
    /// </summary>
    [Display(Name = "asn_qty")]
    public int asn_qty { get; set; } = 0;

    /// <summary>
    /// actual_qty
    /// </summary>
    [Display(Name = "actual_qty")]
    public int actual_qty { get; set; } = 0;

    /// <summary>
    /// weight
    /// </summary>
    [Display(Name = "weight")]
    public decimal weight { get; set; } = 0;

    /// <summary>
    /// volume
    /// </summary>
    [Display(Name = "volume")]
    public decimal volume { get; set; } = 0;

    /// <summary>
    /// supplier_id
    /// </summary>
    [Display(Name = "supplier_id")]
    public int supplier_id { get; set; } = 0;

    /// <summary>
    /// supplier_name
    /// </summary>
    [Display(Name = "supplier_name")]
    [MaxLength(256, ErrorMessage = "MaxLength")]
    public string supplier_name { get; set; } = string.Empty;

    /// <summary>
    /// is_valid
    /// </summary>
    [Display(Name = "is_valid")]
    public bool is_valid { get; set; } = true;

    /// <summary>
    /// expiry_date
    /// </summary>
    [Display(Name = "expiry_date")]
    public DateTime expiry_date { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// price
    /// </summary>
    [Display(Name = "price")]
    public decimal price { get; set; } = 0;

    /// <summary>
    /// sorted_qty
    /// </summary>
    public int sorted_qty { get; set; } = 0;

    /// <summary>
    /// putaway_date
    /// </summary>
    public DateTime putaway_date { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Unit of Measure ID
    /// </summary>
    [Display(Name = "uom_id")]
    public int? uom_id { get; set; }

    /// <summary>
    /// Batch/Lot number for this item
    /// </summary>
    [Display(Name = "batch_number")]
    [MaxLength(64, ErrorMessage = "MaxLength")]
    public string? batch_number { get; set; }

    /// <summary>
    /// description
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// asn qty decimal 
    /// </summary>
    [Required(ErrorMessage = "Required")]
    public decimal asn_qty_decimal { get; set; } = 0;

    /// <summary>
    /// actual qty decimal
    /// </summary>
    [Required(ErrorMessage = "Required")]
    public decimal actual_qty_decimal { get; set; } = 0;

    /// <summary>
    /// goods location id
    /// </summary>
    public int goods_location_id { get; set; } = 0;

    /// <summary>
    /// goods location name
    /// </summary>
    public string? goods_location_name { get; set; }

    /// <summary>
    /// pallet id
    /// </summary>
    public int pallet_id { get; set; } = 0;

    /// <summary>
    /// pallet code
    /// </summary>
    public string? pallet_code { get; set; }
}

#endregion


