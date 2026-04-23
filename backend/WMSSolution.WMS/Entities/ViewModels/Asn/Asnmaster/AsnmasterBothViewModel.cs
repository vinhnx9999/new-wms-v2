using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WMSSolution.WMS.Entities.ViewModels.Asn.Asnmaster;

namespace WMSSolution.WMS.Entities.ViewModels;

/// <summary>
/// Asnmaster ViewModel
/// </summary>
public class AsnmasterBothViewModel
{

    #region constructor
    /// <summary>
    /// constructor
    /// </summary>
    public AsnmasterBothViewModel()
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
    /// asn_no
    /// </summary>
    [Display(Name = "asn_no")]
    [MaxLength(32, ErrorMessage = "MaxLength")]
    public string asn_no { get; set; } = string.Empty;

    /// <summary>
    /// asn_batch
    /// </summary>
    [Display(Name = "asn_batch")]
    [MaxLength(64, ErrorMessage = "MaxLength")]
    public string asn_batch { get; set; } = string.Empty;

    /// <summary>
    /// po_id
    /// </summary>
    [Display(Name = "po_id")]
    public int? po_id { get; set; }

    /// <summary>
    /// estimated_arrival_time
    /// </summary>
    [Display(Name = "estimated_arrival_time")]
    public DateTime estimated_arrival_time { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// asn_status
    /// </summary>
    [Display(Name = "asn_status")]
    public byte asn_status { get; set; } = 0;

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
    /// goods_owner_id
    /// </summary>
    [Display(Name = "goods_owner_id")]
    public int goods_owner_id { get; set; } = 0;

    /// <summary>
    /// goods_owner_name
    /// </summary>
    [Display(Name = "goods_owner_name")]
    public string goods_owner_name { get; set; } = string.Empty;

    /// <summary>
    /// warehouse_id
    /// </summary>
    [Display(Name = "warehouse_id")]
    public int warehouse_id { get; set; } = 0;

    /// <summary>
    /// warehouse_name
    /// </summary>
    [Display(Name = "warehouse_name")]
    public string warehouse_name { get; set; } = string.Empty;

    /// <summary>
    /// creator
    /// </summary>
    [Display(Name = "creator")]
    [MaxLength(64, ErrorMessage = "MaxLength")]
    public string creator { get; set; } = string.Empty;

    /// <summary>
    /// create_time
    /// </summary>
    [Display(Name = "create_time")]
    public DateTime create_time { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// last_update_time
    /// </summary>
    [Display(Name = "last_update_time")]
    public DateTime last_update_time { get; set; } = DateTime.UtcNow;


    /// <summary>
    /// if reject as not null true means has reject qty
    /// </summary>

    [JsonPropertyName("has_rejected_items")]
    public bool? has_rejected_items { get; set; }

    #endregion

    #region details
    /// <summary>
    /// details
    /// </summary>
    public List<AsnmasterDetailViewModel> detailList { get; set; } = [];
    #endregion
}
