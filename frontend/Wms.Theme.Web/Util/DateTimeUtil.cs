
using Wms.Theme.Web.Model.ShareModel;

namespace Wms.Theme.Web.Util;

public static class NumberUtil
{
    public static bool IsNumber(this object obj)
    {
        try
        {
            return double.TryParse($"{obj}", out double rs);
        }
        catch
        {
        }
        return false;
    }

    public static string FormatPrice(this decimal obj)
    {
        return $"{obj:N0}";
    }

    public static string FormatPrice(this int obj)
    {
        return $"{obj:N0}";
    }
}

public class SearchUtil
{
    public static PageSearchRequest GetPageSearch(List<SearchObject> searches, int pageIndex = 1, int pageSize = SystemConfig.PAGE_SIZE)
    {
        return new PageSearchRequest
        {
            pageIndex = pageIndex,
            pageSize = pageSize,
            sqlTitle = "",
            searchObjects = searches
        };
    }
}


public static class DateTimeUtil
{
    private const string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    private const string _dateFormat = "yyyy-MM-dd";
    private const int _defaultExpiryDays = 30;

    public static string ConvertDate2LocalTime(this DateTime? utcTime)
    {
        if (utcTime == null) return "";

        DateTime localTime = utcTime.GetValueOrDefault().ToLocalTime();
        return localTime.ToString(_dateTimeFormat);
    }

    public static string Convert2LocalTime(this DateTime utcTime)
    {
        DateTime localTime = utcTime.ToLocalTime();
        return localTime.ToString(_dateTimeFormat);
    }
    public static string Convert2LocalDate(this DateTime utcTime)
    {
        DateTime localTime = utcTime.ToLocalTime();
        return localTime.ToString(_dateFormat);
    }    
    public static bool IsSoonExpired(this DateTime utcTime)
    {
        return utcTime > DateTime.Now && utcTime < DateTime.Now.AddDays(_defaultExpiryDays);
    }

    public static bool IsExpired(this DateTime utcTime)
    {
        return utcTime < DateTime.Now;
    }
}