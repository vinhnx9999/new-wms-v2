using System.Text;
using System.Text.Json;
namespace Wms.Theme.Web.Util;

public static class StringContentUtil
{
    public static StringContent ContentPretty(this object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static StringContent ContentPretty(this object obj, JsonSerializerOptions options)
    {
        var json = JsonSerializer.Serialize(obj, options);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static Guid GuidPretty(this string strId)
    {
        try
        {
            return string.IsNullOrEmpty(strId) ? Guid.Empty : Guid.Parse(strId);
        }
        catch
        {
            return Guid.Empty;
        }
    }
}
