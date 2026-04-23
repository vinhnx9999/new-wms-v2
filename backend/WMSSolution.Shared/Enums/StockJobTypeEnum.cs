namespace WMSSolution.Shared.Enums;

/// <summary>
/// Represents the types of stock job type.
/// </summary>
public enum StockJobTypeEnum
{
    /// <summary>
    /// Stock Adjustment
    /// </summary>
    StockAdjust = 2,
    /// <summary>
    /// Stock Taking
    /// </summary>
    StockTaking = 1
}

public enum ReceiptStatus
{
    DRAFT = 0,
    NEW = 1,
    PROCESSING = 2,
    COMPLETE = 3,
    CANCELED = 4,
}
