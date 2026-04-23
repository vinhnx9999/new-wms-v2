using Newtonsoft.Json;


namespace WMSSolution.WMS.Entities.ViewModels.Dispatchlist.Duy_Phat_Solution
{
    /// <summary>
    /// Dispatch Order Create Request
    /// </summary>
    public class CreateDispatchOrderRequest
    {
        /// <summary>
        /// Dispatch order number (unique identifier for the order)
        /// </summary>
        [JsonProperty("dispatch_no")]
        public string DispatchNo { get; set; } = string.Empty;

        /// <summary>
        /// Customer ID associated with this dispatch order
        /// </summary>
        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }

        /// <summary>
        /// Customer name for display purposes
        /// </summary>
        [JsonProperty("customer_name")]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Expected delivery/dispatch date
        /// </summary>
        [JsonProperty("create_date")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// List of items (SKUs) to be dispatched in this order
        /// </summary>
        [JsonProperty("lines")]
        public List<ManualDispatchLineRequest> Lines { get; set; } = new List<ManualDispatchLineRequest>();
    }
}
