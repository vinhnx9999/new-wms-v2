using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Unload
{
    public class UnloadConfirmRequest
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        [JsonPropertyName("unload_time")]
        public DateTime UnLoadTime = DateTime.UtcNow;

        [JsonPropertyName("unload_person_id")]
        public int UnloadPersonId { get; set; } = 0;

        [JsonPropertyName("unload_person")]
        public string UnloadPerson { get; set; } = "";

        [JsonPropertyName("input_qty")]
        public int InPutQty { get; set; } = 0;

    }
}
