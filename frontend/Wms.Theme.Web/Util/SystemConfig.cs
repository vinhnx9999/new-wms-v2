namespace Wms.Theme.Web.Util;

public static class SystemConfig
{
    public const int DEFAULT_INDEX = 1;
    /// <summary>
    /// Page size for pagination
    /// </summary>
    public const int PAGE_SIZE = 10;
    /// <summary>
    /// Get all data
    /// </summary>

    public const int GET_ALL = 0;
    public const int MAX_PAGE_SIZE = 100;
}

public enum InventoryRules
{
    //First-Expired, First-Out
    FEFO,
    //First-In, First-Out
    FIFO,
    //Last-In, First-Out
    LIFO    
}
