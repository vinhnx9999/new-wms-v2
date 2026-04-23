using System.Text.Json.Serialization;
using System.Text.Json;

namespace Wms.Theme.Web.Model.ShareModel
{
    public class ApiResponse
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public DataResponse? Data { get; set; }
    }

    public class ApiResponse<T>
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public DataResponse<T>? Data { get; set; }
    }

    public class DataResponse
    {
        [JsonPropertyName("rows")]
        public List<object> Rows { get; set; } = [];
        [JsonPropertyName("totals")]
        public int Totals { get; set; }
    }

    public class DataResponse<T>
    {
        [JsonPropertyName("rows")]
        public List<T> Rows { get; set; } = [];
        [JsonPropertyName("totals")]
        public int Totals { get; set; }
    }

    /// <summary>
    /// Simple API response for operations that return a simple success message or string data
    /// </summary>
    public class SimpleApiResponse
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        [JsonConverter(typeof(StringOrPrimitiveJsonConverter))]
        public string? Data { get; set; } // Backend may return string/bool/number here
    }

    /// <summary>
    /// Allows binding JSON primitives (string/bool/number/null) to a string property.
    /// Prevents exceptions when API returns data=true/false.
    /// </summary>
    internal sealed class StringOrPrimitiveJsonConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.True => "true",
                JsonTokenType.False => "false",
                JsonTokenType.Number => reader.TryGetInt64(out var l) ? l.ToString() : reader.GetDouble().ToString(),
                JsonTokenType.Null => null,

                // If backend ever returns object/array, keep the raw JSON text as a string
                JsonTokenType.StartObject or JsonTokenType.StartArray => JsonDocument.ParseValue(ref reader).RootElement.ToString(),
                _ => throw new JsonException($"Unsupported token type {reader.TokenType} for string value.")
            };
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            if (value is null) writer.WriteNullValue();
            else writer.WriteStringValue(value);
        }
    }

    /// <summary>
    /// Generic API response for endpoints that return a direct array in the data field
    /// Can be used for any list of objects: List<GoodLocationDto>, List<AnsSortedResponse>, etc.
    /// </summary>
    /// <typeparam name="T">The type of objects in the data array</typeparam>
    public class ListApiResponse<T>
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<T>? Data { get; set; }
    }
}
