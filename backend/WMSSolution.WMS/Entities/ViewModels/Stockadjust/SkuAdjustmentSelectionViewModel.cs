using System.Text.Json.Serialization;

namespace WMSSolution.WMS.Entities.ViewModels.Stockadjust;

/// <summary>
/// 
/// </summary>
public class SkuAdjustmentSelectionViewModel
{
    /// <summary>
    /// sku id
    /// </summary>
    public int sku_id { get; set; } = 0;
    /// <summary>
    /// sku code
    /// </summary>
    public string sku_code { get; set; } = string.Empty;
    /// <summary>
    /// sku name
    /// </summary>
    public string sku_name { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string unit { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string spu_code { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string spu_name { get; set; } = string.Empty;

    // Stock Info
    /// <summary>
    /// 
    /// </summary>
    public int goods_location_id { get; set; } = 0;
    /// <summary>
    /// Location Name
    /// </summary>    
    public string LocationName { get; set; } = string.Empty;
    /// <summary>
    /// Ware houseName
    /// </summary>    
    public string WarehouseName { get; set; } = string.Empty;
    /// <summary>
    /// warehouse_name
    /// </summary>
    public string warehouse_name { get; set; } = string.Empty;
    /// <summary>
    /// location_name
    /// </summary>
    public string location_name { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public int goods_owner_id { get; set; } = 0;
    /// <summary>
    /// 
    /// </summary>
    public string goods_owner_name { get; set; } = string.Empty;

    // Lot / Price / Expiry for identification
    /// <summary>
    /// 
    /// </summary>
    public string series_number { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public DateTime? expiry_date { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public DateTime? putaway_date { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public decimal price { get; set; } = 0;

    // Quantities
    /// <summary>
    /// 
    /// </summary>
    public decimal qty_total { get; set; } = 0;
    /// <summary>
    /// 
    /// </summary>
    public decimal qty_locked { get; set; } = 0;
    /// <summary>
    /// 
    /// </summary>
    public decimal qty_available { get; set; } = 0;
}
