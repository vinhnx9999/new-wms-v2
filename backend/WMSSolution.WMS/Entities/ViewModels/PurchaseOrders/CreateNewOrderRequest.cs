using System.Text.Json.Serialization;

namespace WMSSolution.WMS.Entities.ViewModels.PurchaseOrders
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateNewOrderRequest
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("id")]
        public int id { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("po_no")]
        public string po_no { get; set; } = string.Empty;

        /// <summary>
        /// ref supplier id
        /// </summary>
        [JsonPropertyName("supplier_id")]
        public int supplier_id { get; set; } = 1;

        /// <summary>
        /// supplier name
        /// </summary>
        [JsonPropertyName("supplier_name")]
        public string supplier_name { get; set; } = string.Empty;

        /// <summary>
        /// order date
        /// </summary>
        [JsonPropertyName("order_date")]
        public DateTime order_date { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// expected delivery date
        /// </summary>
        [JsonPropertyName("expected_delivery_date")]
        public DateTime expected_delivery_date { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// po status 
        /// @1 = new , @2 = processing , @3 completed , @4 canceled
        ///</summary>
        [JsonPropertyName("po_status")]
        public int po_status { get; set; } = 0;

        /// <summary>
        /// staff name
        /// </summary>
        [JsonPropertyName("creator")]
        public string creator { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Details")]
        public List<PurchaseOrderDetailsDTO> Details { get; set; }
    }
}
