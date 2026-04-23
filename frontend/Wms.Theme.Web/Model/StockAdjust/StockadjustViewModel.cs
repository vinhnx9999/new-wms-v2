using Wms.Theme.Web.Model.StockProcess;

namespace Wms.Theme.Web.Model.StockAdjust;
public class StockadjustViewModel
{
    public StockadjustViewModel()
    {
    }

    #region Property

    /// <summary>
    /// id
    /// </summary>

    public int id { get; set; } = 0;

    /// <summary>
    /// job_type
    /// </summary>
    public byte job_type { get; set; } = 0;

    /// <summary>
    /// job_code
    /// </summary>
    public string job_code { get; set; } = string.Empty;

    /// <summary>
    /// spu_code
    /// </summary>
    public string spu_code { get; set; } = string.Empty;

    /// <summary>
    /// spu_name
    /// </summary>
    public string spu_name { get; set; } = string.Empty;

    /// <summary>
    /// sku_id
    /// </summary>
    public int sku_id { get; set; } = 0;

    /// <summary>
    /// sku_code
    /// </summary>
    public string sku_code { get; set; } = string.Empty;

    /// <summary>
    /// sku_name
    /// </summary>
    public string sku_name { get; set; } = string.Empty;

    /// <summary>
    /// goods_owner_id
    /// </summary>
    public int goods_owner_id { get; set; } = 0;

    /// <summary>
    /// goods owner's name
    /// </summary>
    public string goods_owner_name { get; set; } = string.Empty;

    /// <summary>
    /// goods_location_id
    /// </summary>
    public int goods_location_id { get; set; } = 0;

    /// <summary>
    /// warehouse_name
    /// </summary>
    public string warehouse_name { get; set; } = string.Empty;

    /// <summary>
    /// location_name
    /// </summary>
    public string location_name { get; set; } = string.Empty;

    /// <summary>
    /// qty
    /// </summary>
    public int qty { get; set; } = 0;

    /// <summary>
    /// creator
    /// </summary>
    public string creator { get; set; } = string.Empty;

    /// <summary>
    /// create_time
    /// </summary>

    public DateTime create_time { get; set; }

    /// <summary>
    /// last_update_time
    /// </summary>
    public DateTime last_update_time { get; set; }

    /// <summary>
    /// tenant_id
    /// </summary>
    public long tenant_id { get; set; } = 0;

    /// <summary>
    /// is_update_stock
    /// </summary>
    public bool is_update_stock { get; set; } = false;

    /// <summary>
    /// source_table_id
    /// </summary>
    public int source_table_id { get; set; } = 0;

    /// <summary>
    /// series_number
    /// </summary>
    public string series_number { get; set; } = string.Empty;

    /// <summary>
    /// expiry_date
    /// </summary>
    public DateTime expiry_date { get; set; }

    /// <summary>
    /// price
    /// </summary>
    public decimal price { get; set; } = 0;

    /// <summary>
    /// putaway_date
    /// </summary>
    public DateTime putaway_date { get; set; }
    /// <summary>
    /// process_status
    /// </summary>
    public bool process_status { get; set; } = true;
    /// <summary>
    /// adjust_status
    /// </summary>
    public bool adjust_status { get; set; } = true;
    public string processor { get; set; } = string.Empty;
    /// <summary>
    /// process_time
    /// </summary>
    public DateTime? process_time { get; set; }

    /// <summary>
    /// source detail table
    /// </summary>
    public List<StockprocessdetailViewModel> source_detail_list { get; set; } = new List<StockprocessdetailViewModel>(2);
    /// <summary>
    /// target detail table
    /// </summary>
    public List<StockprocessdetailViewModel> target_detail_list { get; set; } = new List<StockprocessdetailViewModel>(2);

    #endregion Property
}
