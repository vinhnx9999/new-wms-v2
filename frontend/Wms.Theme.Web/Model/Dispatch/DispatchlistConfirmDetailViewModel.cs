using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Dispatch
{
    public class DispatchlistConfirmDetailViewModel
    {


        [JsonPropertyName("dispatchlist_id")]
        public int DispatchlistId { get; set; } = 0;

        /// <summary>
        /// dispatch_no
        /// </summary>

        [JsonPropertyName("dispatch_no")]
        public string DispatchNo { get; set; } = string.Empty;

        /// <summary>
        /// dispatch_status
        /// </summary>
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
        /// spu_description
        /// </summary>
        [JsonPropertyName("spu_description")]
        public string SpuDescription { get; set; } = string.Empty;

        /// <summary>
        /// bar_code
        /// </summary>
        [JsonPropertyName("bar_code")]
        public string BarCode { get; set; } = string.Empty;

        /// <summary>
        /// sku_code
        /// </summary>
        [JsonPropertyName("sku_code")]
        public string SkuCode { get; set; } = string.Empty;

        [JsonPropertyName("sku_name")]
        public string SkuName { get; set; } = string.Empty;
        /// <summary>
        /// quantity available
        /// </summary>
        [JsonPropertyName("qty_available")]
        public int QtyAvailable { get; set; } = 0;

        /// <summary>
        /// confirm order
        /// </summary>
        [JsonPropertyName("confirm")]
        public bool Confirm { get; set; } = false;

        /// <summary>
        /// pick list
        /// </summary>
        [JsonPropertyName("pick_list")]
        public List<DispatchlistConfirmPickDetailViewModel> PickList { get; set; } = new List<DispatchlistConfirmPickDetailViewModel>();

    }
}
