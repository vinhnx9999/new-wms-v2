using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Util
{
    /// <summary>
    /// Custom DateTime converter to handle the API's DateTime format: "yyyy-MM-dd HH:mm:ss"
    /// </summary>
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private static readonly string[] AcceptedFormats =
        [
            // API format (primary)
            DateTimeFormat,

            // UI/Flatpickr formats used in Razor pages (Create.cshtml etc.)
            "dd/MM/yyyy HH:mm",
            "d/M/yyyy H:mm",
            "dd/MM/yyyy HH:mm:ss",
            "d/M/yyyy H:mm:ss",

            // Common ISO-ish formats (defensive)
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.fff",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss.fffZ"
        ];

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();

            if (string.IsNullOrEmpty(dateString))
                return DateTime.MinValue;

            // Try parsing with known formats (invariant is fine for numeric dates)
            if (DateTime.TryParseExact(
                    dateString,
                    AcceptedFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out var result))
            {
                return result;
            }

            // Fallback to standard parsing (try invariant + vi-VN to handle dd/MM reliably)
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var fallbackInvariant))
            {
                return fallbackInvariant;
            }

            if (DateTime.TryParse(dateString, CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.AllowWhiteSpaces, out var fallbackViVn))
            {
                return fallbackViVn;
            }

            return DateTime.MinValue;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(DateTimeFormat));
        }
    }
}