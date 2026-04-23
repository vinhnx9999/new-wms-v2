namespace WMSSolution.Core.Utility;

/// <summary>
/// DateTime utility class
/// </summary>
public static class DateTimeUtil
{
    private const string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    private const int _defaultExpiryDays = 30;
    /// <summary>
    /// Convert UTC time to local time
    /// </summary>
    /// <param name="utcTime"></param>
    /// <returns></returns>
    public static string Convert2LocalTime(this DateTime utcTime)
    {
        DateTime localTime = utcTime.ToLocalTime();
        return localTime.ToString(_dateTimeFormat);
    }

    /// <summary>
    /// IsSoonExpired
    /// </summary>
    /// <param name="utcTime"></param>
    /// <returns></returns>
    public static bool IsSoonExpired(this DateTime utcTime)
    {
        return utcTime > DateTime.Now && utcTime < DateTime.Now.AddDays(_defaultExpiryDays);
    }

    /// <summary>
    /// IsExpired
    /// </summary>
    /// <param name="utcTime"></param>
    /// <returns></returns>
    public static bool IsExpired(this DateTime utcTime)
    {
        return utcTime < DateTime.Now;
    }
}
