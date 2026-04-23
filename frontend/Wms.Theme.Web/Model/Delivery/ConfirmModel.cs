using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Delivery
{
    public class ConfirmModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Arrival time as string in format dd/mm/yyyy HH:mm from frontend.
        /// </summary>
        [JsonPropertyName("arrival_time")]
        public string ArrivalTime { get; set; } = string.Empty;

        [JsonPropertyName("input_qty")]
        public int InPutQty { get; set; }
    }
}