using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Dispatch
{
    public class DispatchListWeightDTO
    {

        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;
        [JsonPropertyName("dispatch_no")]
        public string DispatchNo { get; set; } = string.Empty;

        /// <summary>
        /// dispatch_status
        /// </summary>
        [JsonPropertyName("dispatch_status")]
        public byte DispatchStatus { get; set; } = 0;

        /// <summary>
        /// weighing_qty
        /// </summary>
        [JsonPropertyName("weighing_qty")]
        public int WeighingQty { get; set; } = 0;

        /// <summary>
        /// weighing_weight
        /// </summary>
        [JsonPropertyName("weighing_weight")]
        public decimal WeighingWeight { get; set; } = 0;

        /// <summary>
        /// picked_qty
        /// </summary>
        [JsonPropertyName("picked_qty")]
        public int PickedQty { get; set; } = 0;


    }
}
