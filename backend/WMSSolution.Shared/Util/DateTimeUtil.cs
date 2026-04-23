namespace WMSSolution.Shared.Util;

public static class DateTimeUtil
{
    private const string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    private const string _dateFormat = "yyyy-MM-dd";
    private const int _defaultExpiryDays = 30;
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
