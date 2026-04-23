using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Dispatch
{
    /// <summary>
    /// Request model received from frontend JavaScript submitForm()
    /// Property names use snake_case to match the JS requestData object exactly
    /// </summary>
    public class CreateOutboundOrderRequest
    {
        public string dispatch_no { get; set; } = string.Empty;
        public int goods_owner_id { get; set; }
        public string goods_owner_name { get; set; } = string.Empty;
        public int customer_id { get; set; }
        public string customer_name { get; set; } = string.Empty;

        /// <summary>
        /// Warehouse ID - can be int or GUID string depending on backend
        /// </summary>
        [JsonPropertyName("warehouse_id")]
        public object? warehouse_id { get; set; }

        public string warehouse_name { get; set; } = string.Empty;
        public string estimated_delivery_time { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public List<OutboundDetailItemRequest> detailList { get; set; } = new();
    }

    /// <summary>
    /// Detail item in outbound order request - matches JS map() output
    /// </summary>
    public class OutboundDetailItemRequest
    {
        public int sku_id { get; set; }
        public string sku_code { get; set; } = string.Empty;
        public string sku_name { get; set; } = string.Empty;
        public int spu_id { get; set; }
        public string spu_code { get; set; } = string.Empty;
        public string spu_name { get; set; } = string.Empty;
        public int? uom_id { get; set; }
        public int dispatch_qty { get; set; }
        public decimal dispatch_qty_decimal { get; set; }
        public int actual_qty { get; set; }
        public decimal actual_qty_decimal { get; set; }
        public string batch_number { get; set; } = string.Empty;
        public int goods_location_id { get; set; }
        public string? expiry_date { get; set; }
        public string description { get; set; } = string.Empty;
        public List<SelectedLocationRequest> selected_locations { get; set; } = new();
    }

    /// <summary>
    /// Pallet/location selection for dispatch - matches ManualPickLocationRequest on backend
    /// </summary>
    public class SelectedLocationRequest
    {
        public int pallet_id { get; set; }
        public int location_id { get; set; }
        public decimal pick_qty { get; set; }
    }
}
